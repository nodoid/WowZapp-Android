using System;
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;

using LOLAccountManagement;
using LOLApp_Common;
using LOLMessageDelivery;

using WZCommon;

namespace wowZapp.Contacts
{
    public static class EditContactUtil
    {
        public static ContactDB UserContact
        {
            get;
            set;
        }
    }

    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class EditContact : Activity
    {
        Context context;
        DBManager dbm;
        Button blockcontact;
        private ImageView contactpic;
        private int xSize, ySize;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
           
            SetContentView(Resource.Layout.EditContact);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewloginHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
			
            context = header.Context;
            Header.headertext = Application.Context.Resources.GetString(Resource.String.editContactTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            TextView name = FindViewById<TextView>(Resource.Id.txtContactName);
            name.Text = EditContactUtil.UserContact.ContactUser.FirstName + " " + EditContactUtil.UserContact.ContactUser.LastName;
            contactpic = FindViewById<ImageView>(Resource.Id.imgContactPic);
            contactpic.Tag = new Java.Lang.String("profilepic_1");
            dbm = wowZapp.LaffOutOut.Singleton.dbm;

            blockcontact = FindViewById<Button>(Resource.Id.btnBlockUser);
            Button deletecontact = FindViewById<Button>(Resource.Id.btnDeleteUser);

            ImageButton uniback = FindViewById<ImageButton>(Resource.Id.btnUniBack);

            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
			
            uniback.Click += delegate
            {
                Finish();
            };

            blockcontact.Text = Application.Context.Resources.GetString(EditContactUtil.UserContact.Blocked ? Resource.String.editContactUnblock : Resource.String.editContactBlock);

            blockcontact.Click += new EventHandler(blockcontact_Click);
            deletecontact.Click += new EventHandler(deletecontact_Click);
            float size = 100f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                size *= wowZapp.LaffOutOut.Singleton.bigger;
            xSize = (int)ImageHelper.convertDpToPixel(size, context);
            ySize = xSize;
            if (ContactsUtil.contactFilenames.Contains(EditContactUtil.UserContact.ContactAccountGuid))
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("using cache");
                #endif
                using (Bitmap bm = BitmapFactory.DecodeFile (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, EditContactUtil.UserContact.ContactAccountGuid)))
                {
                    using (MemoryStream ms = new MemoryStream ())
                    {
                        bm.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                        byte[] image = ms.ToArray();
                        displayImage(image, contactpic);
                    }
                }
            } else
            {
                if (EditContactUtil.UserContact.ContactUser.Picture.Length > 0)
                {
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("grabbing");
                    #endif
                    LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                    service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
                    service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, EditContactUtil.UserContact.ContactAccountID, new Guid(AndroidData.ServiceAuthToken));
                } else
                {
                    System.Diagnostics.Debug.WriteLine("no pic");
                    using (Bitmap bm = BitmapFactory.DecodeResource (this.Resources, Resource.Drawable.defaultuserimage))
                    {
                        using (MemoryStream ms = new MemoryStream ())
                        {
                            bm.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                            byte[] image = ms.ToArray();
                            displayImage(image, contactpic);
                        }
                    }
                }
            }
        }

        private void displayImage(byte[] image, ImageView contactPic)
        {
            if (image.Length > 0)
            {
                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, xSize, ySize, this.Resources))
                {
                    RunOnUiThread(delegate
                    {
                        contactpic.SetImageBitmap(myBitmap);
                    });
                }
            }
        }
		
        private void blockcontact_Click(object sender, EventArgs e)
        {
            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            RunOnUiThread(() => Toast.MakeText(context, 
			                                  (EditContactUtil.UserContact.Blocked ? Resource.String.editContactBlocking : Resource.String.editContactUnblocking), 
			                                  ToastLength.Short).Show());

            service.ContactsSaveCompleted += Service_ContactsSaveCompleted;
            service.ContactsSaveAsync(ContactDB.ConvertFromContactDB(EditContactUtil.UserContact), new Guid(AndroidData.ServiceAuthToken));
        }

        private void deletecontact_Click(object sender, EventArgs e)
        {
            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            RunOnUiThread(delegate
            {
                Toast.MakeText(context, Resource.String.editContactDeleting, ToastLength.Short).Show();
            });
            Contact toDelete = ContactDB.ConvertFromContactDB(EditContactUtil.UserContact);
            service.ContactsDeleteCompleted += Service_ContactsDeleteCompleted;
            service.ContactsDeleteAsync(toDelete.ContactID, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
        }

        private void Service_ContactsSaveCompleted(object s, ContactsSaveCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)s;
            service.ContactsSaveCompleted -= Service_ContactsSaveCompleted;
            if (e.Error == null)
            {
                Contact result = e.Result;
                if (result.Errors.Count > 0)
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorEditContactsSaveTitle), 
							StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors).ToString());
                    });
                else
                {
                    dbm.InserOrUpdateContacts(new List<Contact>() { result });
                    EditContactUtil.UserContact.Blocked = !EditContactUtil.UserContact.Blocked;
                    blockcontact.Text = Application.Context.Resources.GetString 
						(EditContactUtil.UserContact.Blocked ? Resource.String.editContactUnblock : Resource.String.editContactBlock);
                }
            } else
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorEditContactsSaveTitle),
                        string.Format("{0} {1}", e.Result.ContactUser.FirstName + " " + e.Result.ContactUser.LastName, e.Error.Message));
                });
        }

        private void Service_ContactsDeleteCompleted(object s, ContactsDeleteCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)s;
            service.ContactsDeleteCompleted -= Service_ContactsDeleteCompleted;
            if (e.Error == null)
            {
                List<LOLAccountManagement.GeneralError> result = new List<LOLAccountManagement.GeneralError>();
                result = e.Result;
                if (result.Count > 0)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorEditContactsDeleteTitle),
                            StringUtils.CreateErrorMessageFromGeneralErrors(result).ToString());
                    });
                } else
                {
                    List<MessageDB> contactMessages = dbm.GetAllSentMessagesForUserToOwner(EditContactUtil.UserContact.ContactAccountGuid, AndroidData.CurrentUser.AccountID.ToString());
                    foreach (MessageDB eachMessage in contactMessages)
                    {
                        foreach (MessageStepDB eachMessageStep in eachMessage.MessageStepDBList)
                        {
                            if (eachMessageStep.StepType == MessageStep.StepTypes.Voice)
                            {
                                string voiceFile = GetVoiceRecordingFilename(eachMessage.MessageID, eachMessageStep.StepNumber);
                                if (File.Exists(voiceFile))
                                    File.Delete(voiceFile);
                            }
                            if (eachMessageStep.StepType == MessageStep.StepTypes.Polling)
                            {
                                PollingStepDB pollingStep = dbm.GetPollingStep(eachMessage.MessageGuid, eachMessageStep.StepNumber);
                                if (pollingStep != null)
                                {
                                    List<string> pollFiles = new List<string>();
                                    if (!string.IsNullOrEmpty(pollingStep.PollingData1File))
                                        pollFiles.Add(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, pollingStep.PollingData1File));
                                    if (!string.IsNullOrEmpty(pollingStep.PollingData2File))
                                        pollFiles.Add(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, pollingStep.PollingData2File));
                                    if (!string.IsNullOrEmpty(pollingStep.PollingData3File))
                                        pollFiles.Add(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, pollingStep.PollingData3File));
                                    if (!string.IsNullOrEmpty(pollingStep.PollingData4File))
                                        pollFiles.Add(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, pollingStep.PollingData4File));

                                    pollFiles.ForEach(file => 
                                    { 
                                        if (File.Exists(file))
                                            File.Delete(file);
                                    });
                                }
                                dbm.DeletePollingStep(pollingStep);
                            }
                        }
                    }
                    ContactsUtil.contactFilenames.Remove(EditContactUtil.UserContact.ContactAccountGuid);
                    string filename = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, EditContactUtil.UserContact.ContactAccountGuid);
                    if (File.Exists(filename))
                        File.Delete(filename);
                    dbm.DeleteMessages(contactMessages);
                    dbm.DeleteContactForOwner(EditContactUtil.UserContact);
                    RunOnUiThread(delegate
                    {
                        Finish();
                    });
                }
            } else
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorEditContactsDeleteTitle), e.Error.Message);
                });
        }

        private string GetVoiceRecordingFilename(Guid msgID, int stepNumber)
        {
            return System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, string.Format(LOLConstants.VoiceRecordingFormat, msgID.ToString(), stepNumber));
        }

        private void Service_UserGetImageDataCompleted(object sender, UserGetImageDataCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;

            if (e.Result.Errors.Count == 0)
            {
                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay (e.Result.ImageData, xSize, ySize, this.Resources))
                {
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("saving");
                    #endif
                    RunOnUiThread(() => contactpic.SetImageBitmap(myBitmap));
                    File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, e.Result.AccountID.ToString()), e.Result.ImageData);
                    ContactsUtil.contactFilenames.Add(e.Result.AccountID.ToString());
                }

                dbm.UpdateUserImage(e.Result.AccountID.ToString(), e.Result.ImageData);
            }
        }
		
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
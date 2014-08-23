using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using LOLApp_Common;
using System.IO;
using Android.Util;
using LOLAccountManagement;
using Android.Graphics;

using WZCommon;

namespace wowZapp.Contacts
{
    public static class FindContactResultsUtil
    {
        public static List<ContactDB> contacts
        { get; set; }
    }

    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class FindContactResultsActivity : Activity
    {
        private DBManager db;
        private Context context;
        private LinearLayout listWrapper;
        private List<ContactDB> contacts;
        private List<Guid> profilePicsToBeGrabbed;
        private int profilePicsGrabIndex = 0, xSize, ySize;
        //DON'T TOUCH these, they are not strings for display, just attributes I use to detect checks on ImageView
        private const string ADD_ENABLED = "Enabled", ADD_DISABLED = "Disabled";
        float[] newSizes;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FindContactResults);

            listWrapper = FindViewById<LinearLayout>(Resource.Id.linearResultsWrapper);
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            Button btnDone = FindViewById<Button>(Resource.Id.btnDone);
            context = listWrapper.Context;

            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
			
            Header.headertext = Application.Context.Resources.GetString(Resource.String.findContactbtnSearch);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            db = wowZapp.LaffOutOut.Singleton.dbm;
            contacts = FindContactResultsUtil.contacts;
            profilePicsGrabIndex = 0;
            profilePicsToBeGrabbed = new List<Guid>();
            LinearLayout bottomHolder = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            //xSize = (int)ImageHelper.convertDpToPixel (40f, context);
            //ySize = xSize;

            btnBack.Click += delegate
            {
                Finish();
            };

            btnDone.Click += delegate
            {
                Finish();
            };

            ImageHelper.setupButtonsPosition(btnBack, btnDone, bottomHolder, context, true);

            newSizes = new float[2];
            newSizes [0] = newSizes [1] = 40f;
            newSizes = ImageHelper.getNewSizes(newSizes, context);

            for (int n = 0; n < contacts.Count; n++)
            {
                LinearLayout layout = new LinearLayout(context);
                layout.Orientation = Android.Widget.Orientation.Horizontal;
                layout.SetGravity(GravityFlags.Center);
                layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), 
				                   (int)ImageHelper.convertDpToPixel(10f, context));

                ImageView profilepic = new ImageView(context);
                profilepic.LayoutParameters = new ViewGroup.LayoutParams((int)newSizes [0], (int)newSizes [1]);
                profilepic.Tag = new Java.Lang.String("profilepic_" + contacts [n].ContactAccountID.ToString());
                if (Contacts.ContactsUtil.contactFilenames.Contains(contacts [n].ContactAccountID.ToString()))
                {
                    using (Bitmap bm = BitmapFactory.DecodeFile (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, contacts [n].ContactAccountID.ToString ())))
                    {
                        using (MemoryStream ms = new MemoryStream ())
                        {
                            bm.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                            byte[] image = ms.ToArray();
                            displayImage(image, profilepic);
                        }
                    }
                } else
                {
                    if (contacts [n].ContactUser.Picture.Length > 0)
                    {
                        profilePicsToBeGrabbed.Add(contacts [n].ContactAccountID);
                    } else
                    {
                        profilepic.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.defaultuserimage));
                    }
                }

                layout.AddView(profilepic);

                using (TextView text = new TextView (context))
                {
                    text.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(235f, context), (int)ImageHelper.convertDpToPixel(40f, context));
                    text.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);
                    text.Gravity = GravityFlags.CenterVertical;
                    text.TextSize = 16f;
                    text.SetTextColor(Android.Graphics.Color.White);
                    if (contacts [n].ContactUser.FirstName != "" || contacts [n].ContactUser.LastName != "")
                    {
                        text.Text = contacts [n].ContactUser.FirstName + " " + contacts [n].ContactUser.LastName;
                    } else
                    {
                        text.Text = contacts [n].ContactUser.EmailAddress;
                    }
                    layout.AddView(text);
                }
                ImageView checkbox = new ImageView(context);
                checkbox.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(25f, context), (int)ImageHelper.convertDpToPixel(25f, context));

                if (db.CheckContactExistsForOwner(contacts [n].ContactAccountID.ToString(), AndroidData.CurrentUser.AccountID.ToString()) == true)
                {
                    checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.@checkedbox));
                    checkbox.ContentDescription = ADD_DISABLED;
                } else
                {
                    checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.add));
                    checkbox.ContentDescription = ADD_ENABLED;
                }

                int contactId = new int();
                contactId = n;
                layout.Clickable = true;
                layout.Click += delegate
                {
                    AddContact(checkbox, contactId);
                };
                layout.AddView(checkbox);
                this.listWrapper.AddView(layout);
            }

            if (profilePicsToBeGrabbed.Count > 0)
            {
                profilePicsGrabIndex = 0;
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
                service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [0], new Guid(AndroidData.ServiceAuthToken));
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
                        contactPic.SetImageBitmap(myBitmap);
                    });
                }
            }
        }

        private void Service_UserGetImageDataCompleted(object sender, UserGetImageDataCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;

            if (e.Result.Errors.Count == 0 && e.Result.ImageData.Length > 0)
            {
                RunOnUiThread(delegate
                {
                    using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(e.Result.ImageData, (int)newSizes[0], (int)newSizes[1], this.Resources))
                    {
                        ImageView pic = (ImageView)listWrapper.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID.ToString()));
                        if (myBitmap != null)
                            pic.SetImageBitmap(myBitmap);
                        else
                            pic.SetImageResource(Resource.Drawable.defaultuserimage);
                    }
                });

                db.UpdateUserImage(e.Result.AccountID.ToString(), e.Result.ImageData);
                profilePicsGrabIndex++;
                if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                {
                    service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [profilePicsGrabIndex], new Guid(AndroidData.ServiceAuthToken));
                } else
                {
                    service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                }
            } else
			if (e.Result.Errors.Count == 0 && e.Result.ImageData.Length == 0)
            {
                ImageView pic = (ImageView)listWrapper.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID.ToString()));
                pic.SetBackgroundResource(Resource.Drawable.defaultuserimage);
            }
        }

        private void AddContact(ImageView checkbox, int contactId)
        {
            if (checkbox.ContentDescription == ADD_ENABLED)
            {
                Contact contact = new Contact();
                contact = ContactDB.ConvertFromContactDB(contacts [contactId]);

                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.ContactsSaveCompleted += Service_ContactsSaveCompleted;
                service.ContactsSaveAsync(contact, new Guid(AndroidData.ServiceAuthToken));

                RunOnUiThread(delegate
                {
                    checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.@checkedbox));
                    checkbox.ContentDescription = ADD_DISABLED;
                });
            }
        }

        private void Service_ContactsSaveCompleted(object sender, ContactsSaveCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.ContactsSaveCompleted -= Service_ContactsSaveCompleted;

            if (null == e.Error)
            {

                Contact result = e.Result;
                if (result.Errors.Count > 0)
                {
                    RunOnUiThread(() => GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), 
                                        StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors).ToString()));
                } else
                {
                    if (e.Result.ContactUser.Picture.Length > 0)
                    {
                        Contacts.ContactsUtil.contactFilenames.Add(e.Result.ContactUser.AccountID.ToString());
                        File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, e.Result.ContactUser.AccountID.ToString()), e.Result.ContactUser.Picture);
                    }
                    db.InserOrUpdateContacts(new List<Contact>() { result });
                }
            } else
            {
                RunOnUiThread(delegate
                {
                    string p = string.Format("{0} {1}",
                        string.Format(Application.Context.GetString(Resource.String.errorSavingContactFormat),
                                  e.Result.ContactUser.FirstName + " " + e.Result.ContactUser.LastName), e.Error.Message);
                    GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), p);
                });
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
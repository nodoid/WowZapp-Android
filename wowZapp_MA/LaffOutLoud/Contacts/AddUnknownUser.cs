using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Widget;

using LOLApp_Common;
using LOLAccountManagement;
using LOLMessageDelivery;
using WZCommon;

namespace wowZapp.Contacts
{
    public static class UnknownUser
    {
        public static List<Guid> unknownUsers
		{ get; set; }
        public static Context context
		{ get; set; }
    }

    public class AddUnknownUser
    {
        public AddUnknownUser(List<Guid>users, Context c)
        {
            UnknownUser.unknownUsers = users;
            UnknownUser.context = c;
            startActivity(c);
        }
		
        private void startActivity(Context c)
        {
            Intent i = new Intent(c, typeof(AddNewUserActivity));
            c.StartActivity(i);
        }
    }
	
    [Activity]
    public class AddNewUserActivity : Activity
    {
        private Dialog ModalNewContact;
        private List<ContactDB> contacts;
        private int counter, newUsersToAdd;
        private ImageView modalImage;
        private float[] imageSize;
        private DBManager dbm;
        private Context context;
        private List<User> newUsers;
		
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            dbm = wowZapp.LaffOutOut.Singleton.dbm;
            Context context = UnknownUser.context;
            imageSize = new float[2];
            imageSize [0] = imageSize [1] = 56f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                imageSize = ImageHelper.getNewSizes(imageSize, context);
            contacts = new List<ContactDB>();
            newUsers = new List<User>();
            createNewUsers();
        }
		
        private void end()
        {
            Finish();
        }
		
        private void createNewUsers()
        {	
            newUsersToAdd = UnknownUser.unknownUsers.Count;
            if (newUsersToAdd == 0)
                end();
            counter = 0;
            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            service.UserGetSpecificCompleted += Service_UserGetSpecificDone;
            service.UserGetSpecificAsync(AndroidData.CurrentUser.AccountID, UnknownUser.unknownUsers [counter], new Guid(AndroidData.ServiceAuthToken));
        }
		
        private void Service_UserGetSpecificDone(object s, UserGetSpecificCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)s;
            if (e.Error == null)
            {
                User result = e.Result;
                #if DEBUG
                if (result.Errors.Count > 0)
                    System.Diagnostics.Debug.WriteLine("Error getting user");
                #endif
                newUsers.Add(result);
                counter++;
                if (counter < newUsersToAdd)
                    service.UserGetSpecificAsync(AndroidData.CurrentUser.AccountID, UnknownUser.unknownUsers [counter], new Guid(AndroidData.ServiceAuthToken));
                else
                {
                    service.UserGetSpecificCompleted -= Service_UserGetSpecificDone;
                    counter = 0;
                    AddNewUsers(newUsers);
                }
            }
        }
		
        private void AddNewUsers(List<User> users)
        {
            User user = new User();
            user = users [counter];
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Users to add : {0}", user.FirstName + " " + user.LastName);
#endif
			
            RunOnUiThread(delegate
            {
                ModalNewContact = new Dialog(this, Resource.Style.lightbox_dialog);
                ModalNewContact.SetContentView(Resource.Layout.ModalNewContacts);
				
                ((Button)ModalNewContact.FindViewById(Resource.Id.btnAccept)).Click += delegate
                {
                    if (user.Picture.Length > 0)
                    {
                        Contacts.ContactsUtil.contactFilenames.Add(user.AccountID.ToString());
                        System.IO.File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, user.AccountID.ToString()), 
						                              user.Picture);
                    }
                    DismissModalDialog();
                    counter++;
                    ContactDB contact = new ContactDB();
                    contact.Blocked = false;
                    contact.ContactAccountID = user.AccountID;
                    contact.OwnerAccountID = AndroidData.CurrentUser.AccountID;
                    contact.ContactUser = user;
                    contacts.Add(contact);
                    if (counter < users.Count)
                        AddNewUsers(users);
                    else
                    {
                        DismissModalDialog();
                        returnToSender();
                    }
                };
                ((Button)ModalNewContact.FindViewById(Resource.Id.btnDecline)).Click += delegate
                {
                    DismissModalDialog();
                    counter++;
                    if (counter < users.Count)
                        AddNewUsers(users);
                    else
                    {
                        DismissModalDialog();
                        returnToSender();
                    }
                };
                ((TextView)ModalNewContact.FindViewById(Resource.Id.txtContactName)).Text = user.FirstName + " " + user.LastName;
                modalImage = ((ImageView)ModalNewContact.FindViewById(Resource.Id.imgContact));
                if (Contacts.ContactsUtil.contactFilenames.Contains(user.AccountID.ToString()))
                {
                    Bitmap bm = BitmapFactory.DecodeFile(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, user.AccountID.ToString()));
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    bm.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                    byte[] img = ms.ToArray();
                    displayImage(img, modalImage);
                } else
                {
                    if (user.Picture.Length == 0)
                        RunOnUiThread(() => modalImage.SetBackgroundResource(Resource.Drawable.defaultuserimage));
                    else
                    {
                        if (user.Picture.Length == 0)
                        {
                            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                            service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
                            service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, user.AccountID, new Guid(AndroidData.ServiceAuthToken));
                        } else
                        {
                            Bitmap bm = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.defaultuserimage);
                            System.IO.MemoryStream ms = new System.IO.MemoryStream();
                            bm.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                            byte[] img = ms.ToArray();
                            displayImage(img, modalImage);
                        }
                    }
                }
                ModalNewContact.Show();
            });
        }
		
        private void returnToSender()
        {
            counter = 0;
            Contact contact = new Contact();
            contact = ContactDB.ConvertFromContactDB(contacts [counter]);
				
            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            service.ContactsSaveCompleted += Service_ContactsSaveCompleted;
            service.ContactsSaveAsync(contact, new Guid(AndroidData.ServiceAuthToken));

        }
		
        private void Service_UserGetImageDataCompleted(object sender, UserGetImageDataCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
		
            if (e.Result.Errors.Count == 0 && e.Result.ImageData.Length > 0)
            {
                using (Bitmap userImage = ImageHelper.CreateUserProfileImageForDisplay(e.Result.ImageData, (int)imageSize[0], (int)imageSize[1], this.Resources))
                {
                    if (userImage != null)
                        RunOnUiThread(() => modalImage.SetImageBitmap(userImage));
                    else
                        RunOnUiThread(() => modalImage.SetImageResource(Resource.Drawable.defaultuserimage));
                }
				
                service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                dbm.UpdateUserImage(e.Result.AccountID.ToString(), e.Result.ImageData);
            }
        }
	
        private void DismissModalDialog()
        {
            if (ModalNewContact != null)
                ModalNewContact.Dismiss();
            ModalNewContact = null;
        }
	
        private void displayImage(byte[] image, ImageView contactPic)
        {
            if (image.Length > 0)
            {
                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)imageSize[0], (int)imageSize[1], this.Resources))
                {
                    RunOnUiThread(delegate
                    {
                        contactPic.SetImageBitmap(myBitmap);
                    });
                }
            }
        }
	
        private void Service_ContactsSaveCompleted(object sender, ContactsSaveCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
		
		
            if (null == e.Error)
            {
			
                Contact result = e.Result;
                if (result.Errors.Count > 0)
                {
                    RunOnUiThread(() => GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), 
				                            StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors)));
                } else
                {
                    if (e.Result.ContactUser.Picture.Length > 0)
                    {
                        Contacts.ContactsUtil.contactFilenames.Add(e.Result.ContactUser.AccountID.ToString());
                        System.IO.File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, e.Result.ContactUser.AccountID.ToString()), e.Result.ContactUser.Picture);
                    }
                    dbm.InserOrUpdateContacts(new List<Contact>() { result });
                    counter++;
                    if (counter < contacts.Count)
                    {
                        Contact contact = new Contact();
                        contact = ContactDB.ConvertFromContactDB(contacts [counter]);
                        service.ContactsSaveAsync(contact, new Guid(AndroidData.ServiceAuthToken));
                    } else
                    {
                        service.ContactsSaveCompleted -= Service_ContactsSaveCompleted;
                        end();
                    }
                }
            } else
            {
                service.ContactsSaveCompleted -= Service_ContactsSaveCompleted;
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LOLApp_Common;
using LOLAccountManagement;

using Android.App;
using Android.Widget;
using Android.Views;
using Android.OS;
using Android.Content;
using Android.Util;
using Android.Graphics;

using WZCommon;

namespace wowZapp.Contacts
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class ManageContactsActivity : Activity, IDisposable
    {
        private DBManager db;
        private Context context;

        public ISocialProviderManager Provider
        {
            get;
            private set;
        }

        private List<ContactDB> contactsDB;
        private List<Contact> contacts;
        private ProgressDialog dialog;
        private ImageView imgAddContact;
        private ImageView imgUniBack;
        private LinearLayout listWrapper;
        private EditText txtSearch;
        private List<Guid> profilePicsToBeGrabbed;
        private int profilePicsGrabIndex = 0;
        private const int FIND_CONTACT_ACTIVITY = 1, EDIT_CONTACT_ACTIVITY = 2;
        ViewGroup Parent;
        private Dialog LightboxDialog;
        private float[] imageSize;
        private bool isBack;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ManageContactsMain);
            isBack = base.Intent.GetBooleanExtra("back", false);
            imgAddContact = FindViewById<ImageView>(Resource.Id.btnFindContact);
            imgUniBack = FindViewById<ImageView>(Resource.Id.btnBack);
            listWrapper = FindViewById<LinearLayout>(Resource.Id.linearListWrapper);
            txtSearch = FindViewById<EditText>(Resource.Id.txtContactsSearch);
            context = listWrapper.Context;

            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
			
            Header.headertext = Application.Context.Resources.GetString(Resource.String.contactsTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            profilePicsToBeGrabbed = new List<Guid>();

            db = wowZapp.LaffOutOut.Singleton.dbm;
            contactsDB = new List<ContactDB>();
            contacts = new List<Contact>();

            Parent = listWrapper;
            imgAddContact.Click += delegate
            {
                StartActivityForResult(typeof(FindContactActivity), FIND_CONTACT_ACTIVITY);
            };
            imgAddContact.Tag = 1;
            imgUniBack.Click += delegate
            {
                Finish();
            };
            imgUniBack.Tag = 0;

            ImageView[] buttons = new ImageView[2];
            buttons [0] = imgUniBack;
            buttons [1] = imgAddContact;

            float size = 40f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                size *= wowZapp.LaffOutOut.Singleton.bigger;
            imageSize = new float[2];
            imageSize [0] = imageSize [1] = size;

            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            ImageHelper.setupButtonsPosition(buttons, bottom, context);

            txtSearch.TextChanged += delegate
            {
                FilterContacts();
            };
			
            System.Threading.Tasks.Task.Factory.StartNew(() => LoadContactsFromDB());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case FIND_CONTACT_ACTIVITY:
                    LoadContactsFromDB();
                    break;
                case EDIT_CONTACT_ACTIVITY:
                    LoadContactsFromDB();
                    break;
            }
        }

        private void handleContactClick(TextView text, int id)
        {
            EditContactUtil.UserContact = contactsDB [id];
            Intent i = new Intent(this, typeof(EditContact));
            StartActivityForResult(i, EDIT_CONTACT_ACTIVITY);
        }

        private void LoadContactsFromDB()
        {
            if (contactsDB != null)
                contactsDB.Clear(); 
            if (listWrapper != null)
                RunOnUiThread(() => listWrapper.RemoveAllViews());

            RunOnUiThread(() => ShowLightboxDialog(Application.Resources.GetString(Resource.String.manageContactsRetrievingContacts)));

            contactsDB = db.GetAllContactsForOwner(AndroidData.CurrentUser.AccountID.ToString());
            if (contactsDB.Count > 0)
                sortContacts();

            int m = 0;
            //RunOnUiThread (delegate {
            foreach (ContactDB eachContact in contactsDB)
            {
                eachContact.LastMessageSent = db.GetLastMessageDateTimeForUser(eachContact.ContactAccountGuid, true);

                LinearLayout layout = new LinearLayout(context);
                layout.Orientation = Android.Widget.Orientation.Horizontal;
                layout.SetGravity(GravityFlags.Center);
                layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), (int)ImageHelper.convertDpToPixel(10f, context));

                ImageView profilepic = new ImageView(context);
                profilepic.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(imageSize [0], context), (int)ImageHelper.convertDpToPixel(imageSize [1], context));
                profilepic.Tag = new Java.Lang.String("profilepic_" + eachContact.ContactAccountID.ToString());
				
                if (ContactsUtil.contactFilenames.Contains(eachContact.ContactAccountID.ToString()))
                {
                    string file = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, eachContact.ContactAccountID.ToString());
                    FileInfo f = new FileInfo(file);
                    long s1 = f.Length;
                    if (s1 > 0 && s1 != 2)
                    {
                        using (Bitmap bm = BitmapFactory.DecodeFile (file))
                        {
                            using (MemoryStream ms = new MemoryStream ())
                            {
                                bm.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                                byte[] image = ms.ToArray();
                                displayImage(image, profilepic);
                            }
                        }
                    }
                } else
                {
                    if (eachContact.ContactUser.Picture.Length == 0)
                    {
                        profilePicsToBeGrabbed.Add(eachContact.ContactAccountID);
                    } else
                    {
                        if (eachContact.ContactUser.Picture.Length > 0 && eachContact.ContactUser.Picture.Length != 2)
                            displayImage(eachContact.ContactUser.Picture, profilepic);
                        else
                            RunOnUiThread(() => profilepic.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.defaultuserimage)));
                    }
                }
                RunOnUiThread(() => layout.AddView(profilepic));

                TextView text = new TextView(context);
                text.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(260f, context), (int)ImageHelper.convertDpToPixel(40f, context));
                text.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);
                text.Gravity = GravityFlags.CenterVertical;
                text.TextSize = 16f;
                text.SetTextColor(Android.Graphics.Color.White);
                if (eachContact.ContactUser.FirstName != "" || eachContact.ContactUser.LastName != "")
                {
                    text.Text = eachContact.ContactUser.FirstName + " " + eachContact.ContactUser.LastName;
                } else
                {
                    text.Text = eachContact.ContactUser.EmailAddress;
                }

                int contactId = new int();
                contactId = m;
                layout.Clickable = true;
                layout.Click += delegate
                {
                    handleContactClick(text, contactId);
                };
				
                RunOnUiThread(delegate
                {
                    layout.AddView(text);
                    listWrapper.AddView(layout);
                });
                m++;
                    
            }

            RunOnUiThread(() => DismissLightboxDialog());

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
                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)imageSize[0], (int)imageSize[1], this.Resources))
                {
                    RunOnUiThread(() => contactPic.SetImageBitmap(myBitmap));
                }
            }
        }

        private void Service_UserGetImageDataCompleted(object sender, UserGetImageDataCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;

            if (e.Result.Errors.Count == 0)
            {
                if (e.Result.ImageData.Length > 0 && e.Result.ImageData.Length != 2)
                {
                    using (Bitmap userImage = ImageHelper.CreateUserProfileImageForDisplay(e.Result.ImageData, (int)imageSize[0], (int)imageSize[1], this.Resources))
                    {
                        ImageView pic = (ImageView)listWrapper.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID));
                        RunOnUiThread(() => pic.SetImageBitmap(userImage));
                            
                        if (!Contacts.ContactsUtil.contactFilenames.Contains(e.Result.AccountID.ToString()))
                        {
                            File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, e.Result.AccountID.ToString()), e.Result.ImageData);
                            Contacts.ContactsUtil.contactFilenames.Add(e.Result.AccountID.ToString());
                        }
                    }

                    db.UpdateUserImage(e.Result.AccountID.ToString(), e.Result.ImageData);
                        
                    profilePicsGrabIndex++;
                    if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                        service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [profilePicsGrabIndex], new Guid(AndroidData.ServiceAuthToken));
                    else
                        service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                } else
                {
                    if (e.Result.ImageData.Length == 0)
                    {
                        ImageView pic = (ImageView)listWrapper.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID));
                        RunOnUiThread(() => pic.SetBackgroundResource(Resource.Drawable.defaultuserimage));
                        profilePicsGrabIndex++;
                        if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                            service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [profilePicsGrabIndex], new Guid(AndroidData.ServiceAuthToken));
                        else
                            service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                    } else
                    {
                        if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                            service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [profilePicsGrabIndex], new Guid(AndroidData.ServiceAuthToken));
                        else
                            service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                    }
                }
            } else
            {
                if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                {
                    service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [profilePicsGrabIndex], new Guid(AndroidData.ServiceAuthToken));
                } else
                {
                    service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                }
            }
        }

        private void sortContacts()
        {
            List<ContactDB> dupes = new List<ContactDB>();
            dupes = contactsDB.Distinct().ToList();
            Dictionary<Guid, int> changed = new Dictionary<Guid, int>();
            for (int n = 0; n < dupes.Count; ++n)
            {
                string originalName = dupes [n].ContactUser.FirstName + " " + dupes [n].ContactUser.LastName;
                string firstname = dupes [n].ContactUser.FirstName, surname = dupes [n].ContactUser.LastName;
                if (firstname.Length > 0)
                    dupes [n].ContactUser.FirstName = firstname.Substring(0, 1).ToUpper() + firstname.Substring(1, firstname.Length - 1);
                if (surname.Length > 0)
                    dupes [n].ContactUser.LastName = surname.Substring(0, 1).ToUpper() + surname.Substring(1, surname.Length - 1);
                string newName = dupes [n].ContactUser.FirstName + " " + dupes [n].ContactUser.LastName;
                if (originalName != newName)
                {
                    int m = 0;
                    if (dupes [n].ContactUser.FirstName != firstname && dupes [n].ContactUser.LastName != surname)
                        m = 3;
                    else
                    {
                        if (dupes [n].ContactUser.FirstName != firstname)
                            m = 2;
                        else
                            m = 1;
                    }
                    changed.Add(dupes [n].ContactUser.AccountID, m);
                }
            }
            var c = dupes.OrderBy(person => person.ContactUser.LastName).ThenBy(person => person.ContactUser.FirstName);
			
            List<ContactDB> tmpList = new List<ContactDB>();
            foreach (ContactDB cc in c)
                tmpList.Add(cc);
			
            contactsDB = tmpList;
            if (changed != null)
            {
                int tgv = 0;
                foreach (ContactDB g in contactsDB)
                {
                    if (changed.TryGetValue(g.ContactUser.AccountID, out tgv))
                    {
                        string first = g.ContactUser.FirstName, last = g.ContactUser.LastName;
                        switch (tgv)
                        {
                            case 1: // lastname
                                g.ContactUser.LastName = last.Substring(0, 1).ToLower() + last.Substring(1, last.Length - 1);
                                break;
                            case 2: // first
                                g.ContactUser.FirstName = first.Substring(0, 1).ToLower() + first.Substring(1, first.Length - 1);
                                break;
                            case 3: // both
                                g.ContactUser.LastName = last.Substring(0, 1).ToLower() + last.Substring(1, last.Length - 1);
                                g.ContactUser.FirstName = first.Substring(0, 1).ToLower() + first.Substring(1, first.Length - 1);
                                break;
                        }
                    }
                }
            }
        }

        private void FilterContacts()
        {
            if (txtSearch.Text.Trim() == "")
            {
                for (int i = 1; i < listWrapper.ChildCount; i++)
                {
                    listWrapper.GetChildAt(i).Visibility = ViewStates.Visible;
                }
            } else
            {
                for (int i = 1; i < listWrapper.ChildCount; i++)
                {
                    ViewStates visibility = ViewStates.Gone;
                    LinearLayout temp = (LinearLayout)listWrapper.GetChildAt(i);
                    TextView text = (TextView)temp.GetChildAt(1);

                    string[] lastNameArr = text.Text.Split(' ');
                    string lastNameStr = "";
                    if (lastNameArr.Length > 1)
                        lastNameStr = lastNameArr [1];

                    if (text.Text.ToLower().StartsWith(txtSearch.Text.Trim().ToLower()) ||
                        lastNameStr.ToLower().StartsWith(txtSearch.Text.Trim().ToLower()))
                    {
                        visibility = ViewStates.Visible;
                    }
                    temp.Visibility = visibility;
                }
            }
        }

        public void ShowLightboxDialog(string message)
        {
            LightboxDialog = new Dialog(this, Resource.Style.lightbox_dialog);
            LightboxDialog.SetContentView(Resource.Layout.LightboxDialog);
            ((TextView)LightboxDialog.FindViewById(Resource.Id.dialogText)).Text = message;
            LightboxDialog.Show();
        }

        public void DismissLightboxDialog()
        {
            if (LightboxDialog != null)
                LightboxDialog.Dismiss();

            LightboxDialog = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
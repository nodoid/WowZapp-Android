using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Util;
using LOLApp_Common;
using LOLAccountManagement;
using System.IO;
using LOLMessageDelivery;
using Android.Graphics;

using WZCommon;

namespace wowZapp.Contacts
{
    public static class SelectContactsUtil
    {
        public static List<ContactDB> selectedContacts
        { get; set; }
        public static List<UserDB> selectedUserContacts
        { get; set; }
        public static int messageType
        { get; set; }
    }

    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class SelectContactsActivity : Activity
    {
        private DBManager db;
        private Context context;

        public ISocialProviderManager Provider
        {
            get;
            private set;
        }

        private List<ContactDB> contactsDB;
        private ProgressDialog dialog;
        private LinearLayout listWrapper;
        private Button btnDone;
        private EditText txtSearch;
        private List<Guid> profilePicsToBeGrabbed;
        private int profilePicsGrabIndex = 0;
        private const string CHECKBOX_CHECKED = "Selected", CHECKBOX_UNCHECKED = "Unselected";
        private const int TEXT_MESSAGE = 10, DRAW_MESSAGE = 11, COMIX_MESSAGE = 12,
            VOICE_MESSAGE = 13, COMICON_MESSAGE = 14, POLL_MESSAGE = 15,
            VIDEO_MESSAGE = 16, SFX_MESSAGE = 17, EMOTICON_MESSAGE = 18;
        private Dialog LightboxDialog;
        private float[] imageSizes;
        private float xSize, ySize;
		
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SelectContacts);
            listWrapper = FindViewById<LinearLayout>(Resource.Id.linearListWrapper);
            txtSearch = FindViewById<EditText>(Resource.Id.txtContactsSearch);
            btnDone = FindViewById<Button>(Resource.Id.btnDone);
            context = listWrapper.Context;
            float msize = 100f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                msize *= wowZapp.LaffOutOut.Singleton.bigger;
            xSize = (int)ImageHelper.convertDpToPixel(msize, context);
            ySize = xSize;
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, context);
			
            Header.headertext = Application.Context.Resources.GetString(Resource.String.contactsSelectContacts);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
            profilePicsToBeGrabbed = new List<Guid>();

            db = wowZapp.LaffOutOut.Singleton.dbm;
            contactsDB = new List<ContactDB>();
            SelectContactsUtil.selectedContacts = new List<ContactDB>();
            System.Threading.Tasks.Task.Factory.StartNew(() => LoadContactsFromDB());

            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            btnBack.Click += delegate
            {
                Finish();
            };
            bool jump = false;
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            ImageHelper.setupButtonsPosition(btnBack, btnDone, bottom, context);

            ViewGroup Parent = listWrapper;
            float size = 40f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                size *= wowZapp.LaffOutOut.Singleton.bigger;
            imageSizes = new float[2];
            imageSizes [0] = imageSizes [1] = size;
            txtSearch.TextChanged += delegate
            {
                FilterContacts();
            };
            int pack = 0;
            int messType = 0;
            btnDone.Click += delegate
            {
                LOLMessageDelivery.Message msg = new LOLMessageDelivery.Message();
                msg.FromAccountID = AndroidData.CurrentUser.AccountID;
                List<MessageStep> msgSteps = new List<MessageStep>();
                Messages.ComposeMessageMainUtil.msgSteps = msgSteps;
                Messages.ComposeMessageMainUtil.message = msg;
                Messages.ComposeMessageMainUtil.currentPosition [0] = 1;
                if (Messages.MessageReceivedUtil.FromMessages)
                {
                    Intent t = new Intent(this, typeof(Messages.ComposeMessageChooseContent));
                    context.StartActivity(t);
                    Finish();
                } else
                {
                    switch (SelectContactsUtil.messageType)
                    {
                        case TEXT_MESSAGE:
                            Intent i = new Intent(context, typeof(Messages.ComposeTextMessageActivity));
                            i.PutExtra("CurrentStep", base.Intent.GetIntExtra("CurrentStep", 1));
                            context.StartActivity(i);
                            jump = true;
                            Finish();
                            break;
                        case COMIX_MESSAGE:
                            pack = 2;
                            messType = 1;
                            break;

                        case COMICON_MESSAGE:
                            i = new Intent(context, typeof(Messages.ComposeGenericMessage));
                            pack = 1;
                            messType = 2;
                            break;

                        case SFX_MESSAGE:
                            pack = messType = 5;
                            break;

                        case EMOTICON_MESSAGE:
                            pack = 3;
                            messType = 6;
                            break;

                        case VOICE_MESSAGE:
                            messType = 3;
                            break;
                        case POLL_MESSAGE:
                            Intent i7 = new Intent(context, typeof(Messages.ComposePollChoiceActivity));
                            i7.PutExtra("CurrentStep", base.Intent.GetIntExtra("CurrentStep", 1));
                            context.StartActivity(i7);
                            jump = true;
                            Finish();
                            break;
#if DEBUG
                        case DRAW_MESSAGE:
                            messType = 7;
                            break;
                        case VIDEO_MESSAGE:
                            messType = 4;
                            break;
#endif 
                    }
                    if (!jump)
                    {
                        Intent k = new Intent(context, typeof(Messages.ComposeGenericMessage));
                        k.PutExtra("MessageType", messType);
                        if (pack != 0)
                            k.PutExtra("pack", pack);
                        k.PutExtra("CurrentStep", base.Intent.GetIntExtra("CurrentStep", 1));
                        context.StartActivity(k);
                        Finish();
                    }
                }
            };
            if (SelectContactsUtil.selectedContacts.Count == 0)
                btnDone.Visibility = ViewStates.Invisible;
        }

        private void LoadContactsFromDB()
        {
            if (contactsDB != null)
                contactsDB.Clear();
			
            contactsDB = db.GetAllContactsForOwner(AndroidData.CurrentUser.AccountID.ToString());

            RunOnUiThread(() => ShowLightboxDialog(Application.Resources.GetString(Resource.String.manageContactsRetrievingContacts)));
            for (int n = 0; n < contactsDB.Count; n++)
            {
                LinearLayout layout = new LinearLayout(context);
                layout.Orientation = Android.Widget.Orientation.Horizontal;
                layout.SetGravity(GravityFlags.Center);
                layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, 
										(int)ImageHelper.convertDpToPixel(10f, context), (int)ImageHelper.convertDpToPixel(10f, context));

                ImageView profilepic = new ImageView(context);
                using (profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)imageSizes [0], (int)imageSizes [1]))
                {
                    profilepic.Tag = new Java.Lang.String("profilepic_" + contactsDB [n].ContactAccountID);
                    if (ContactsUtil.contactFilenames.Contains(contactsDB [n].ContactAccountID.ToString()))
                    {
                        string file = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, contactsDB [n].ContactAccountID.ToString());
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
                        if (contactsDB [n].ContactUser.Picture.Length == 0)
                        {
                            profilePicsToBeGrabbed.Add(contactsDB [n].ContactAccountID);
                        } else
                        {
                            if (contactsDB [n].ContactUser.Picture.Length > 0 && contactsDB [n].ContactUser.Picture.Length != 2)
                                displayImage(contactsDB [n].ContactUser.Picture, profilepic);
                            else
                                RunOnUiThread(() => profilepic.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.defaultuserimage)));
                        }
                    }
                }
                RunOnUiThread(() => layout.AddView(profilepic));

                TextView text = new TextView(context);
                using (text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (235f, context), (int)ImageHelper.convertDpToPixel (40f, context)))
                {
                    text.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);
                    text.Gravity = GravityFlags.CenterVertical;
                    text.TextSize = 16f;
                    text.SetTextColor(Android.Graphics.Color.Black);
                    if (contactsDB [n].ContactUser.FirstName != "" || contactsDB [n].ContactUser.LastName != "")
                    {
                        text.Text = contactsDB [n].ContactUser.FirstName + " " + contactsDB [n].ContactUser.LastName;
                    } else
                    {
                        text.Text = contactsDB [n].ContactUser.EmailAddress;
                    }
                }
                RunOnUiThread(() => layout.AddView(text));
				
                ImageView checkbox = new ImageView(context);
                checkbox.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(25f, context), (int)ImageHelper.convertDpToPixel(25f, context));
                checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.checkbox));
                checkbox.ContentDescription = CHECKBOX_UNCHECKED;

                int contactId = new int();
                contactId = n;
                layout.Clickable = true;
                layout.Click += delegate
                {
                    handleContactClick(checkbox, contactId);
                };
                RunOnUiThread(delegate
                {
                    layout.AddView(checkbox);
                    listWrapper.AddView(layout);
                });
            }

            RunOnUiThread(() => DismissLightboxDialog());

            if (profilePicsToBeGrabbed.Count > 0)
            {
                profilePicsGrabIndex = 0;
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
                service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [0], new Guid(AndroidData.ServiceAuthToken));
            }
            //});
        }

        private void displayImage(byte[] image, ImageView contactPic)
        {
            if (image.Length > 0)
            {
                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)imageSizes[0], (int)imageSizes[1], this.Resources))
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
                    using (Bitmap userImage = ImageHelper.CreateUserProfileImageForDisplay(e.Result.ImageData, (int)imageSizes[0], (int)imageSizes[1], this.Resources))
                    {
                        ImageView pic = (ImageView)listWrapper.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID));
                        if (userImage != null)
                            RunOnUiThread(() => pic.SetImageBitmap(userImage));
                        else
                            RunOnUiThread(() => pic.SetImageResource(Resource.Drawable.defaultuserimage));
                        
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
                    /*if (e.Result.ImageData.Length == 0 && e.Result.ImageData.Length != 2)
                    {*/
                    ImageView pic = (ImageView)listWrapper.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID));
                    RunOnUiThread(() => pic.SetBackgroundResource(Resource.Drawable.defaultuserimage));
                    profilePicsGrabIndex++;
                    if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                        service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [profilePicsGrabIndex], new Guid(AndroidData.ServiceAuthToken));
                    else
                        service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                    /*} else
                    {

                        if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                            service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [profilePicsGrabIndex], new Guid(AndroidData.ServiceAuthToken));
                        else
                            service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                    }*/
                }
            } else
            {
                if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                {
                    ImageView pic = (ImageView)listWrapper.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID));
                    RunOnUiThread(() => pic.SetBackgroundResource(Resource.Drawable.defaultuserimage));
                    profilePicsGrabIndex++;
                    if (profilePicsGrabIndex < profilePicsToBeGrabbed.Count)
                        service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [profilePicsGrabIndex], new Guid(AndroidData.ServiceAuthToken));
                } else
                {
                    service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                }
            }
        }

        private void handleContactClick(ImageView checkbox, int contactId)
        {
            if (checkbox.ContentDescription == CHECKBOX_UNCHECKED)
            {
                checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.@checkedbox));
                checkbox.ContentDescription = CHECKBOX_CHECKED;
                SelectContactsUtil.selectedContacts.Add(contactsDB [contactId]);
                btnDone.Visibility = ViewStates.Visible;
            } else
            {
                checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.checkbox));
                checkbox.ContentDescription = CHECKBOX_UNCHECKED;
                SelectContactsUtil.selectedContacts.Remove(contactsDB [contactId]);
                if (SelectContactsUtil.selectedContacts.Count == 0)
                    btnDone.Visibility = ViewStates.Invisible;
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

        public void ShowLightboxDialog(String message)
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
		
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus == true)
            if (Messages.MessageReceivedUtil.FromMessages && Messages.MessageReceivedUtil.FromMessagesDone)
            {
                Messages.MessageReceivedUtil.FromMessagesDone = false;
                Finish();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
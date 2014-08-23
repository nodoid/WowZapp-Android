using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LOLAccountManagement;
using LOLApp_Common;

namespace wowZapp.Contacts
{
//handles actual view rather than the modal
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
    public partial class AboutMe : Activity
    {
        private Context context;
        private TextView fullName, screenName, emailAddress, password, birthday, profDesc, gender;
        private User.Gender currentGender;
        private string[] genders;
        private bool imageChanged = false, imageHasAltered = false, essentialHasAltered = false, nonessesentialHasAltered = false;
        private string MD5Check = "";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.AboutMe);

            string currentUserName = AndroidData.CurrentUser.FirstName + " " + AndroidData.CurrentUser.LastName;
            genders = new string[]
            {
                "Unknown",
                "Male",
                "Female",
                "Alien",
                "Monster"
            };

            int d = 1, m = 1, yr = 1901;
            currentGender = AndroidData.CurrentUser.UserGender;
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewLoginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            context = header.Context;
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, context);
			
            Header.headertext = Application.Context.Resources.GetString(Resource.String.mainTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
			
            ImageView myPic = FindViewById<ImageView>(Resource.Id.imgMe);
			
            if (AndroidData.CurrentUser.Picture.Length == 0)
                myPic.SetBackgroundResource(Resource.Drawable.defaultuserimage);
            else
                MD5Check = generateMD5();

            fullName = FindViewById<TextView>(Resource.Id.txtFullName);
            fullName.Text = currentUserName;
            screenName = FindViewById<TextView>(Resource.Id.txtScreenName);
            screenName.Text = AndroidData.CurrentUser.UserName;
            emailAddress = FindViewById<TextView>(Resource.Id.txtEmail);
            emailAddress.Text = AndroidData.CurrentUser.EmailAddress;
            gender = FindViewById<TextView>(Resource.Id.txtGender);
            switch (currentGender)
            {
                case User.Gender.Alien:
                    gender.Text = genders [3];
                    break;
                case User.Gender.Female:
                    gender.Text = genders [2];
                    break;
                case User.Gender.Male:
                    gender.Text = genders [1];
                    break;
                case User.Gender.Monster:
                    gender.Text = genders [4];
                    break;
                case User.Gender.Unknown:
                    gender.Text = genders [0];
                    break;
            }
            password = FindViewById<TextView>(Resource.Id.txtPassword);
            string pw = "";
            for (int n = 0; n < AndroidData.CurrentUser.Password.Length; ++n)
                pw += "*";
            password.Text = pw;
            birthday = FindViewById<TextView>(Resource.Id.txtDOB);
            birthday.Text = AndroidData.CurrentUser.DateOfBirth.ToShortDateString();
            profDesc = FindViewById<TextView>(Resource.Id.txtProfile);
            profDesc.Text = AndroidData.CurrentUser.Description;
			
            CheckBox quest1 = FindViewById<CheckBox>(Resource.Id.checkedProQ1);
            quest1.Checked = AndroidData.CurrentUser.AllowLocationSearch;
            CheckBox quest2 = FindViewById<CheckBox>(Resource.Id.checkedProQ2);
            quest2.Checked = AndroidData.CurrentUser.ShowLocation;
            CheckBox quest3 = FindViewById<CheckBox>(Resource.Id.checkedProQ3);
            quest3.Checked = AndroidData.CurrentUser.AllowSearch;
			
            Button btnPicChange = FindViewById<Button>(Resource.Id.btnPicChange);
            Button btnChangeName = FindViewById<Button>(Resource.Id.btnChangeName);
            Button btnScreenName = FindViewById<Button>(Resource.Id.btnScreenName);
            Button btnEmail = FindViewById<Button>(Resource.Id.btnEmail);
            Button btnGender = FindViewById<Button>(Resource.Id.btnGender);
            Button btnPassword = FindViewById<Button>(Resource.Id.btnPassword);
            Button btnDOB = FindViewById<Button>(Resource.Id.btnDOB);
            Button btnProfDesc = FindViewById<Button>(Resource.Id.btnProfDesc);
			
            Button btnCancel = FindViewById<Button>(Resource.Id.btnCancel);
            Button btnOK = FindViewById<Button>(Resource.Id.btnAccept);
			
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
			
            btnPicChange.Click += delegate
            {
                generateModal(0);
            };
			
            btnChangeName.Click += delegate
            {
                generateModal(1);
            };
			
            btnScreenName.Click += delegate
            {
                generateModal(2);
            };
			
            btnEmail.Click += delegate
            {
                generateModal(3);
            };
			
            btnGender.Click += delegate
            {
                generateModal(4);
            };
			
            btnPassword.Click += delegate
            {
                generateModal(5);
            };
			
            btnDOB.Click += delegate
            {
                generateModal(6);
            };
			
            btnProfDesc.Click += delegate
            {
                generateModal(7);
            };
			
            btnCancel.Click += delegate
            {
                Finish();
            };
			
            btnBack.Click += delegate
            {
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.AddFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };
			
            btnOK.Click += delegate
            {
                if (checkText(fullName.Text, screenName.Text, emailAddress.Text, password.Text, profDesc.Text))
                {
                    string[] nameSplit = new string[2];
                    if (fullName.Text != currentUserName)
                    {
                        nameSplit = fullName.Text.Split(' ');
                        AndroidData.CurrentUser.FirstName = nameSplit [0];
                        AndroidData.CurrentUser.LastName = nameSplit [1];
                        essentialHasAltered = true;
                    }
                    if (screenName.Text != AndroidData.CurrentUser.UserName)
                    {
                        AndroidData.CurrentUser.UserName = screenName.Text;
                        nonessesentialHasAltered = true;
                    }
                    if (emailAddress.Text != AndroidData.CurrentUser.EmailAddress)
                    {
                        AndroidData.CurrentUser.EmailAddress = emailAddress.Text;
                        essentialHasAltered = true;
                    }
                    if (password.Text != AndroidData.CurrentUser.Password)
                    {
                        AndroidData.CurrentUser.Password = password.Text;
                        essentialHasAltered = true;
                    }
                    if (profDesc.Text != AndroidData.CurrentUser.Description)
                    {
                        AndroidData.CurrentUser.Description = profDesc.Text;
                        nonessesentialHasAltered = true;
                    }

                    if (birthday.Text != AndroidData.CurrentUser.DateOfBirth.ToString())
                    {
                        string[] splitter = new string[3];
                        splitter = birthday.Text.Split('/');
                        d = Convert.ToInt32(splitter [0]);
                        m = Convert.ToInt32(splitter [1]);
                        yr = Convert.ToInt32(splitter [2]);
                        nonessesentialHasAltered = true;
                    }
				
                    if (quest1.Checked != AndroidData.CurrentUser.ShowLocation)
                    {
                        AndroidData.CurrentUser.ShowLocation = quest1.Checked;
                        nonessesentialHasAltered = true;
                    }
                    if (quest2.Checked != AndroidData.CurrentUser.AllowLocationSearch)
                    {
                        AndroidData.CurrentUser.AllowLocationSearch = quest2.Checked;
                        nonessesentialHasAltered = true;
                    }
                    if (quest3.Checked != AndroidData.CurrentUser.AllowSearch)
                    {
                        AndroidData.CurrentUser.AllowSearch = quest3.Checked;
                        nonessesentialHasAltered = true;
                    }
				
                    if (quest1.Checked && quest2.Checked)
                        AndroidData.GeoLocationEnabled = true;
                    else
                        AndroidData.GeoLocationEnabled = false;
				
                    if (imageChanged)
                    {
                        imageHasAltered = true;
                    }

                    switch (gender.Text)
                    {
                        case "Unknown":
                            AndroidData.CurrentUser.UserGender = User.Gender.Unknown;
                            break;
                        case "Male":
                            AndroidData.CurrentUser.UserGender = User.Gender.Male;
                            break;
                        case "Female":
                            AndroidData.CurrentUser.UserGender = User.Gender.Female;
                            break;
                        case "Alien":
                            AndroidData.CurrentUser.UserGender = User.Gender.Alien;
                            break;
                        case "Monster":
                            AndroidData.CurrentUser.UserGender = User.Gender.Monster;
                            break;
                    }

                    if (essentialHasAltered || imageHasAltered || nonessesentialHasAltered)
                    {
                        LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                        if (essentialHasAltered)
                        {
                            service.UserUpdateCompulsoryDetailsCompleted += Service_UpdateCompulsoryCompleted;
                            service.UserUpdateCompulsoryDetailsAsync(AndroidData.CurrentUser.AccountID,
                                                                 AndroidData.CurrentUser.FirstName,
                                                                 AndroidData.CurrentUser.LastName,
                                                                 AndroidData.CurrentUser.EmailAddress,
                                                                 password.Text,
                                                                 AndroidData.CurrentUser.AccountID,
                                                                 new Guid(AndroidData.ServiceAuthToken));
                        }

                        if (imageHasAltered)
                        {
                            byte[] myNewPic = File.ReadAllBytes(AndroidData.imageFileName);
                            service.UserUpdateImageCompleted += Service_UpdateImageCompleted;
                            service.UserUpdateImageAsync(AndroidData.CurrentUser.AccountID, myNewPic, new Guid(AndroidData.ServiceAuthToken));
                        }

                        if (nonessesentialHasAltered)
                        {
                            DateTime t = new DateTime(yr, m, d);
                            service.UserUpdateRestDetailsCompleted += Service_UpdateRestCompleted;
                            service.UserUpdateRestDetailsAsync(AndroidData.CurrentUser.AccountID,
                                                                   t,
                                                                   screenName.Text,
                                                                   profDesc.Text,
                                                                   AndroidData.CurrentUser.ShowLocation,
                                                                   AndroidData.CurrentUser.AllowLocationSearch,
                                                                   AndroidData.CurrentUser.AllowSearch,
                                                                   AndroidData.CurrentUser.UserGender,
                                                                   new Guid(AndroidData.ServiceAuthToken)
                            );
                        }
                    }
                }
            };
        }

        private void Service_UpdateCompulsoryCompleted(object s, UserUpdateCompulsoryDetailsCompletedEventArgs e)
        {

        }

        private void Service_UpdateImageCompleted(object s, UserUpdateImageCompletedEventArgs e)
        {

        }

        private void Service_UpdateRestCompleted(object s, UserUpdateRestDetailsCompletedEventArgs e)
        {

        }

        private string generateMD5()
        {
            string toReturn = "";
            using (var md5 = MD5.Create())
            {
                toReturn = md5.ComputeHash(AndroidData.CurrentUser.Picture).ToString();
            }
            return toReturn;
        }

        private string generateMD5(byte[] image)
        {
            string toReturn = "";
            using (var md5 = MD5.Create())
            {
                toReturn = md5.ComputeHash(image).ToString();
            }
            return toReturn;
        }

        private string generateMD5(string filename)
        {
            string toReturn = "";
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    toReturn = md5.ComputeHash(stream).ToString();
                }
            }
            return toReturn;
        }

        private bool checkText(string fullname, string screenname, string emailaddress, string password, string description)
        {
            if (!String.IsNullOrEmpty(fullname) && !String.IsNullOrEmpty(screenname) && !String.IsNullOrEmpty(emailaddress)
                && !String.IsNullOrEmpty(password) && !String.IsNullOrEmpty(description))
                return true;
            else
            {
                if (String.IsNullOrEmpty(fullname))
                {
                    GeneralUtils.Alert(context, Resources.GetString(Resource.String.errorAboutMeTitle), 
					                    Resources.GetString(Resource.String.errorAboutMeFullName));
                } else
                {
                    if (String.IsNullOrEmpty(screenname))
                    {
                        GeneralUtils.Alert(context, Resources.GetString(Resource.String.errorAboutMeTitle), 
						                    Resources.GetString(Resource.String.errorAboutMeScreename));
                    } else
                    {
                        if (String.IsNullOrEmpty(emailaddress))
                        {
                            GeneralUtils.Alert(context, Resources.GetString(Resource.String.errorAboutMeTitle), 
							                    Resources.GetString(Resource.String.errorAboutMeEmail));
                        } else
                        {
                            if (String.IsNullOrEmpty(password))
                            {
                                GeneralUtils.Alert(context, Resources.GetString(Resource.String.errorAboutMeTitle), 
								                    Resources.GetString(Resource.String.errorAboutMePassword));
                            }
                        }
                    }
                }
                return false;
            }	
        }
    }
}


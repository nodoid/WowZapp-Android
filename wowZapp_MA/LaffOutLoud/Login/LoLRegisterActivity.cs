using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Locations;
using Android.Widget;
using Android.Graphics;
using Android.Views.InputMethods;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using LOLAccountManagement;
using LOLApp_Common;

using WZCommon;

/* this is for tablets only */

namespace wowZapp.Login
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class LoLRegisterActivity : Activity, ILocationListener
    {
        Context context;
        private const int CAMMY = 1, PICCY = 2;
        private ImageView picture;
        private EditText firstName, lastName, email, password, verify, description, username;
        private Button register, takePic, choosePic;
        private ProgressDialog progress;
        private User.Gender gender;
        private DateTime dob;
        private bool q1, q2, q3;
        private LocationManager locationManager;
        private StringBuilder builder;
        private Geocoder geocoder;
        private double[] location;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LoginSignupLoL);

            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            Header.headertext = Application.Context.Resources.GetString(Resource.String.createTitle);
            context = header.Context;
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            q1 = q2 = q3 = false;

            AndroidData.imageFileName = "";
			
            firstName = FindViewById<EditText>(Resource.Id.editFirst);
            lastName = FindViewById<EditText>(Resource.Id.editSurname);
            username = FindViewById<EditText>(Resource.Id.editUsername);
            description = FindViewById<EditText>(Resource.Id.editDescribe);
            password = FindViewById<EditText>(Resource.Id.editPassword);
            verify = FindViewById<EditText>(Resource.Id.editPasswordVerify);
            email = FindViewById<EditText>(Resource.Id.editEmail);
			
            int y = DateTime.Now.Year;
            int m = DateTime.Now.Month;
            int d = DateTime.Now.Day;
			
            Spinner month = FindViewById<Spinner>(Resource.Id.spinMonth);
            Spinner day = FindViewById<Spinner>(Resource.Id.spinDate);
            Spinner year = FindViewById<Spinner>(Resource.Id.spinYear);
			
            var adapterM = ArrayAdapter.CreateFromResource(this, Resource.Array.createMonths, Android.Resource.Layout.SimpleSpinnerItem);
            adapterM.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            month.Adapter = adapterM;
            month.SetSelection(m - 1);
            month.ItemSelected += (sender, e) => {
                m = month.SelectedItemPosition + 1;
            };
			
			
            List<string> dates = new List<string>();
            List<string> years = new List<string>();
            for (int n = 1; n < 32; ++n)
                dates.Add(n.ToString());
            for (int n = 0; n < 75; ++n)
                years.Add((y - 75 + n).ToString());
			
            var yearsAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, years);
            yearsAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            year.Adapter = yearsAdapter;
            year.SetSelection(54);
            y -= 21;
			
            var daysAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, dates);
            daysAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            day.Adapter = daysAdapter;
            day.SetSelection(d - 1);
			
            day.ItemSelected += (sender, e) => {
                string dd = (string)day.GetItemAtPosition(e.Position);
                d = Convert.ToInt32(dd);
            };
            year.ItemSelected += (sender, e) => {
                string yy = (string)year.GetItemAtPosition(e.Position);
                y = Convert.ToInt32(yy);
            };
			
            register = FindViewById<Button>(Resource.Id.btnRegister);
            register.Enabled = false;
            takePic = FindViewById<Button>(Resource.Id.btnTakePic);
            choosePic = FindViewById<Button>(Resource.Id.btnChoosePic);
            picture = FindViewById<ImageView>(Resource.Id.imageView1);

            password.TextChanged += delegate
            {
                register.Enabled = password.Text == verify.Text ? true : false;
            };

            verify.TextChanged += delegate
            {
                register.Enabled = password.Text == verify.Text ? true : false;
            };

            gender = User.Gender.Unknown;

            ImageView female = FindViewById<ImageView>(Resource.Id.imgGenderFemale);
            female.Click += delegate
            {
                gender = User.Gender.Female;
            };
            ImageView male = FindViewById<ImageView>(Resource.Id.imgGenderMale);
            male.Click += delegate
            {
                gender = User.Gender.Male;
            };
            ImageView alien = FindViewById<ImageView>(Resource.Id.imgGenderAlien);
            alien.Click += delegate
            {
                gender = User.Gender.Alien;
            };
            ImageView monster = FindViewById<ImageView>(Resource.Id.imgGenderMonster);
            monster.Click += delegate
            {
                gender = User.Gender.Monster;
            };
            ImageView other = FindViewById<ImageView>(Resource.Id.imgGenderDunno);
            other.Click += delegate
            {
                gender = User.Gender.Unknown;
            };

            CheckBox quest1 = FindViewById<CheckBox>(Resource.Id.checkQ1);
            quest1.Click += delegate
            {
                q1 = !q1;
            };
            CheckBox quest2 = FindViewById<CheckBox>(Resource.Id.checkQ2);
            quest2.Click += delegate
            {
                q2 = !q2;
            };
            CheckBox quest3 = FindViewById<CheckBox>(Resource.Id.checkQ3);
            quest3.Click += delegate
            {
                q3 = !q3;
            };

            ImageButton rotateLeft = FindViewById<ImageButton>(Resource.Id.imgRotateLeft);
            ImageButton rotateRight = FindViewById<ImageButton>(Resource.Id.imgRotateRight);
			
            rotateLeft.Click += delegate
            {
                if (!string.IsNullOrEmpty(AndroidData.imageFileName))
                    picture.SetImageBitmap(ImageHelper.rotateImage(true));
            };
			
            rotateRight.Click += delegate
            {
                if (!string.IsNullOrEmpty(AndroidData.imageFileName))
                    picture.SetImageBitmap(ImageHelper.rotateImage());
            };
			

            bool[] camTest = new bool[2];
            camTest = GeneralUtils.CamRec();

            if (camTest [0] == false || camTest [1] == false)
            {
                takePic.Visibility = Android.Views.ViewStates.Invisible;
                choosePic.Visibility = Android.Views.ViewStates.Invisible;
            }

            takePic.Click += delegate(object s, EventArgs e)
            {
                Intent i = new Intent(this, typeof(CameraVideo.CameraTakePictureActivity));
                i.PutExtra("camera", CAMMY);
                StartActivityForResult(i, CAMMY);
            };

            choosePic.Click += delegate(object s, EventArgs e)
            {
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(imageIntent, "Choose Image"), PICCY);
            };

            register.Click += delegate(object s, EventArgs e)
            {
                DisableAll();
                dob = new DateTime(y, m, d);
                createUser(s, e);
            };
            location = new double[2];
            if (q1 && q2)
            {
                geocoder = new Geocoder(this);
                var locationManager = (LocationManager)GetSystemService(LocationService);
			
                var criteria = new Criteria();
                criteria.Accuracy = Accuracy.NoRequirement;
                criteria.PowerRequirement = Power.NoRequirement;
                string bestProvider = locationManager.GetBestProvider(criteria, true);
			
                Location lastKnownLocation = locationManager.GetLastKnownLocation(bestProvider);
				
                if (lastKnownLocation != null)
                {
                    location [0] = lastKnownLocation.Latitude;
                    location [1] = lastKnownLocation.Longitude;
                }
                AndroidData.GeoLocationEnabled = true;
            } else
            {
                location [0] = location [1] = -999;
                AndroidData.GeoLocationEnabled = false;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case CAMMY:
                    if (resultCode == Result.Ok)
                    {
                        string filename = data.GetStringExtra("filename");
                        if (!string.IsNullOrEmpty(filename))
                        {
                            using (Bitmap bmp = BitmapFactory.DecodeFile(filename))
                            {
                                if (bmp != null)
                                {
                                    using (MemoryStream ms = new MemoryStream ())
                                    {
                                        bmp.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                                        byte[] image = ms.ToArray();
                                        displayImage(image);
                                    }
                                }
                            }
                            AndroidData.imageFileName = filename;
                        }
                    }
                    break;
                case PICCY:
                    if (resultCode == Result.Ok)
                    {
                        string filename = getRealPathFromUri(data.Data);
                        using (Drawable draw = Drawable.CreateFromPath(filename))
                        {
                            picture.SetImageDrawable(draw);
                        }
                        AndroidData.imageFileName = filename;
                    }
                    break;
            }
        }

        private void displayImage(byte[] image)
        {
            if (image.Length > 0)
            {
                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)ImageHelper.convertDpToPixel(125f, context), 
				                                                                      (int)ImageHelper.convertDpToPixel(96f, context), this.Resources))
                {
                    RunOnUiThread(delegate
                    {
                        picture.SetImageBitmap(myBitmap);
                    });
                }
            }
        }

        private void createUser(object s, EventArgs e)
        {
            if (ValidateEntries(firstName.Text, lastName.Text, email.Text, password.Text) != false)
            {
                RunOnUiThread(delegate
                {
                    progress = ProgressDialog.Show(context, Application.Context.Resources.GetString(Resource.String.loginRegistrationLOL),
                        Application.Context.Resources.GetString(Resource.String.loginRegistrationMessage));
                });

                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.AuthenticationTokenGetCompleted += Service_AuthenticationTokenGetCompleted;
                service.AuthenticationTokenGetAsync(AndroidData.NewDeviceID);
            } else
            {
                EnableAll();
            }
        }

        private void Service_AuthenticationTokenGetCompleted(object sender, AuthenticationTokenGetCompletedEventArgs e)
        {
            bool t = false;
            byte[] cropped = null;
            LOLConnectClient service = (LOLConnectClient)sender;
            service.AuthenticationTokenGetCompleted -= Service_AuthenticationTokenGetCompleted;

            if (null == e.Error)
            {
                Guid result = e.Result;

                if (!result.Equals(Guid.Empty))
                {
                    AndroidData.ServiceAuthToken = result.ToString();

                    if (AndroidData.imageFileName != "")
                    {
                        Stream fs = File.Open(AndroidData.imageFileName, FileMode.Open);
                        Android.Graphics.Bitmap bmp = Android.Graphics.BitmapFactory.DecodeStream(fs);
                        MemoryStream stream = new MemoryStream();
                        bmp.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 70, stream);
                        cropped = stream.ToArray();
                        fs.Dispose();
                        stream.Dispose();
                        bmp.Dispose();
                        t = true;
                    }
					
                    service.UserCreateCompleted += Service_UserCreateCompleted;
                    service.UserCreateAsync(AndroidData.NewDeviceID,
                                            DeviceDeviceTypes.Android,
                                            AccountOAuth.OAuthTypes.LOL,
                                            string.Empty, string.Empty,
                                            firstName.Text,
                                            lastName.Text,
                                            email.Text,
                                            password.Text,
                                            t == true ? cropped : new byte[0],
                                            dob,
                                            username.Text, // username
                                            description.Text, // description
                                            location [0], // latitude
                                            location [1], // longitude
                                            q1, // showLocation
                                            q2, // allowLocSearch
                                            q3, // allowSearch
                                            gender, // gender
                                            new Guid(AndroidData.ServiceAuthToken));
                                            
                } else
                {
                    RunOnUiThread(delegate
                    {
                        if (progress != null)
                            progress.Dismiss();
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                                    Application.Context.Resources.GetString(Resource.String.authAccountIDNotToken));
                        EnableAll();
                    });
                }
            } else
            {
                RunOnUiThread(delegate
                {
                    if (progress != null)
                        progress.Dismiss();
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                                Application.Context.Resources.GetString(Resource.String.authAccountIDNotToken));
                    EnableAll();
                });
            }
        }

        private void Service_UserCreateCompleted(object sender, UserCreateCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserCreateCompleted -= Service_UserCreateCompleted;

            if (null == e.Error)
            {
                User createdUser = e.Result;
                if (createdUser.Errors.Count > 0)
                {
                    RunOnUiThread(delegate
                    {
                        if (progress != null)
                            progress.Dismiss();
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
						          Application.Context.Resources.GetString(Resource.String.errorDuplicateUser));
                        EnableAll();
                    });
                } else
                {
                    AndroidData.CurrentUser = createdUser;
                    service.UserLoginCompleted += Service_UserLoginCompleted;
                    service.UserLoginAsync(AndroidData.NewDeviceID,
                                           DeviceDeviceTypes.Android,
                                           AndroidData.CurrentUser.AccountID,
                                           string.Empty, string.Empty,
                                           AccountOAuth.OAuthTypes.LOL,
                                           email.Text,
                                           password.Text,
                                           new Guid(AndroidData.ServiceAuthToken));
                }//end if els
            } else
            {
                RunOnUiThread(delegate
                {
                    if (progress != null)
                        progress.Dismiss();
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                                e.Error.Message + " - " + e.Error.StackTrace);
                    EnableAll();
                });
            }//end if else
        }

        private bool ValidateEntries(string first, string surname, string email, string pass)
        {
            bool rv = true;
            if (string.IsNullOrEmpty(first))
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                               Application.Context.Resources.GetString(Resource.String.alertViewTitleFirstNameEmpty));
                });
                rv = false;
            }

            if (string.IsNullOrEmpty(surname))
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                               Application.Context.Resources.GetString(Resource.String.alertViewTitleLastNameEmpty));
                });
                rv = false;
            }

            if (string.IsNullOrEmpty(email))
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                               Application.Context.Resources.GetString(Resource.String.alertViewEmailEmpty));
                });
                rv = false;
            }

            if (string.IsNullOrEmpty(pass))
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                               Application.Context.Resources.GetString(Resource.String.alertViewTitlePasswordEmpty));
                });
                rv = false;
            }

            if (!StringUtils.IsEmailAddress(email))
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                               Application.Context.Resources.GetString(Resource.String.alertViewMessageNotValidEmailAddress));
                });
                rv = false;
            }

            return rv;
        }

        private void Service_UserLoginCompleted(object sender, UserLoginCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserCreateCompleted -= Service_UserCreateCompleted;
            if (null == e.Error)
            {
                User createdUser = e.Result;
                if (createdUser.Errors.Count > 0)
                {
                    foreach (GeneralError eachError in createdUser.Errors)
                    {
                        if (eachError.ErrorDescription.Contains("already exists"))
                        {
                            RunOnUiThread(delegate
                            {
                                if (progress != null)
                                    progress.Dismiss();
                                GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                                             Application.Context.Resources.GetString(Resource.String.alertViewMessageAccountExists));
                                EnableAll();
                            });
                            return;
                        }
                    } 
                } else
                {
                    AndroidData.IsLoggedIn = true;
                    AndroidData.CurrentUser = createdUser;
                    RunOnUiThread(delegate
                    { 
                        if (progress != null)
                            progress.Dismiss();
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonDone),
                                                         Application.Context.Resources.GetString(Resource.String.createSuccess));
                        EnableAll(); 
                    });
                    AndroidData.user = UserType.ExistingUser;
                    RunOnUiThread(delegate
                    {
                        Intent i = new Intent(this, typeof(Main.HomeActivity));
                        i.AddFlags(ActivityFlags.ClearTop);
                        StartActivity(i);
                    });
                }
            } else
            {
                RunOnUiThread(delegate
                {
                    if (progress != null)
                        progress.Dismiss();
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError), 
                                                     Application.Context.Resources.GetString(Resource.String.createCommsError));
                    EnableAll(); 
                });
            }
        }

        private string getRealPathFromUri(Android.Net.Uri contentUri)
        {
            string[] proj = { MediaStore.Images.ImageColumns.Data };
            ICursor cursor = this.ManagedQuery(contentUri, proj, null, null, null);
            int column_index = cursor.GetColumnIndexOrThrow(MediaStore.Images.ImageColumns.Data);
            cursor.MoveToFirst();
            return cursor.GetString(column_index);
        }

        private void DisableAll()
        {
            firstName.Enabled = false;
            lastName.Enabled = false;
            email.Enabled = false;
            password.Enabled = false;
            takePic.Enabled = false;
            choosePic.Enabled = false;
            register.Enabled = false;
            RunOnUiThread(() => Toast.MakeText(context, Application.Context.Resources.GetString(Resource.String.loginRegistrationMessage), ToastLength.Short).Show());
        }

        private void EnableAll()
        {
            firstName.Enabled = true;
            lastName.Enabled = true;
            email.Enabled = true;
            password.Enabled = true;
            takePic.Enabled = true;
            choosePic.Enabled = true;
            register.Enabled = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
		
        // other stuff needed to satisfy the listener
        public void OnLocationChanged(Location location)
        {
        }
		
        public void OnProviderDisabled(string provider)
        {
        }
		
        public void OnProviderEnabled(string provider)
        {
        }
		
        public void OnStatusChanged(string provider, Android.Locations.Availability status, Bundle extras)
        {
        }
    }
}
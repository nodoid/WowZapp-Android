using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

using LOLApp_Common;
using LOLAccountManagement;

using WZCommon;

namespace wowZapp
{
    [Activity (ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
    public class LOLRegisterPhoneRegisterActivity : Activity, ILocationListener
    {
        private Context context;
        private string username, password, email, first, last, describe;
        private byte[]image;
        private bool q1, q2, q3;
        private double[] location;
        private User.Gender gender;
        private DateTime dob;
        private ProgressDialog progress;
	
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LoginSignupLoLS2);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            Header.headertext = Application.Context.Resources.GetString(Resource.String.createTitle);
            context = header.Context;
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
			
            username = base.Intent.Extras.GetString("username");
            email = base.Intent.Extras.GetString("email");
            password = base.Intent.Extras.GetString("password");
            image = base.Intent.Extras.GetByteArray("image");
			
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
			
            Button register = FindViewById<Button>(Resource.Id.btnRegister);
            ImageView back = FindViewById<ImageView>(Resource.Id.imgBack);
            ImageView female = FindViewById<ImageView>(Resource.Id.imageGenderFemale);
            female.Click += delegate
            {
                gender = User.Gender.Female;
            };
            ImageView male = FindViewById<ImageView>(Resource.Id.imageGenderMale);
            male.Click += delegate
            {
                gender = User.Gender.Male;
            };
            ImageView alien = FindViewById<ImageView>(Resource.Id.imageGenderAlien);
            alien.Click += delegate
            {
                gender = User.Gender.Alien;
            };
            ImageView monster = FindViewById<ImageView>(Resource.Id.imageGenderMonster);
            monster.Click += delegate
            {
                gender = User.Gender.Monster;
            };
            ImageView other = FindViewById<ImageView>(Resource.Id.imageGenderOther);
            other.Click += delegate
            {
                gender = User.Gender.Unknown;
            };
			
            q1 = q2 = q3 = false;
            CheckBox quest1 = FindViewById<CheckBox>(Resource.Id.checkQuest1);
            quest1.Click += delegate
            {
                q1 = !q1;
            };
            CheckBox quest2 = FindViewById<CheckBox>(Resource.Id.checkQuest2);
            quest2.Click += delegate
            {
                q2 = !q2;
            };
            CheckBox quest3 = FindViewById<CheckBox>(Resource.Id.checkQuest3);
            quest3.Click += delegate
            {
                q3 = !q3;
            };
			
            EditText firstname = FindViewById<EditText>(Resource.Id.editFirstName);
            EditText lastname = FindViewById<EditText>(Resource.Id.editSurname);
            EditText description = FindViewById<EditText>(Resource.Id.editDescription); 
			
            back.Click += delegate
            {
                Intent i = new Intent();
                i.PutExtra("email", email);
                i.PutExtra("password", password);
                i.PutExtra("username", username);
                i.PutExtra("image", image);
                SetResult(Result.Ok, i);
                Finish();
            };
			
            register.Click += delegate
            {
                if (!string.IsNullOrEmpty(firstname.Text))
                {
                    describe = description.Text;
                    first = firstname.Text;
                    if (string.IsNullOrEmpty(lastname.Text))
                        last = "null";
                    else
                        last = lastname.Text;
                    dob = new DateTime(y, m, d);
                    createUser();
                } else
                    RunOnUiThread(() => GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.errorCreateTitle), 
					                                         Application.Context.GetString(Resource.String.errorCreateNoFirst)));
            };
            location = new double[2];
            if (q1 && q2)
            {
                var locationManager = (LocationManager)GetSystemService(LocationService);
			
                var criteria = new Criteria();
                criteria.Accuracy = Accuracy.Coarse;
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
			
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            ImageHelper.setupButtonsPosition(back, register, bottom, context);
        }
		
        private void createUser()
        {
            RunOnUiThread(delegate
            {
                progress = ProgressDialog.Show(context, Application.Context.Resources.GetString(Resource.String.loginRegistrationLOL),
					                                Application.Context.Resources.GetString(Resource.String.loginRegistrationMessage));
            });
				
            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            service.AuthenticationTokenGetCompleted += Service_AuthenticationTokenGetCompleted;
            service.AuthenticationTokenGetAsync(AndroidData.NewDeviceID);
        }
		
        private void Service_AuthenticationTokenGetCompleted(object sender, AuthenticationTokenGetCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.AuthenticationTokenGetCompleted -= Service_AuthenticationTokenGetCompleted;
			
            if (null == e.Error)
            {
                Guid result = e.Result;
				
                if (!result.Equals(Guid.Empty))
                {
                    AndroidData.ServiceAuthToken = result.ToString();
					
                    service.UserCreateCompleted += Service_UserCreateCompleted;
                    service.UserCreateAsync(AndroidData.NewDeviceID,
					                         DeviceDeviceTypes.Android,
					                         AccountOAuth.OAuthTypes.LOL,
					                         string.Empty, string.Empty,
					                         first,
					                         last,
					                         email,
					                         password,
					                         image,
					                         dob,
					                         username, // username
					                         describe, // description
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
					                        email,
					                        password,
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
                    GeneralUtils.Alert(context, Resource.String.commonError, Resource.String.createCommsError);
                });
            }
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


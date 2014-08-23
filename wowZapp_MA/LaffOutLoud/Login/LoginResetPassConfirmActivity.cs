using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LOLApp_Common;
using LOLAccountManagement;

namespace wowZapp.Login
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class LoginResetPassConfirmActivity : Activity
    {
        Context c;
        private string email, token, password;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LoginResetPassConfirm);

            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            Header.headertext = Application.Context.Resources.GetString(Resource.String.resetTitle);
            c = header.Context;
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(c);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            email = base.Intent.GetStringExtra("email");
            token = base.Intent.GetStringExtra("token");
            EditText password = FindViewById<EditText>(Resource.Id.editPassword);
            EditText verify = FindViewById<EditText>(Resource.Id.editPasswordVerify);
            Button login = FindViewById<Button>(Resource.Id.btnLogin);
            login.Click += (object s, EventArgs e) => {
                loginNewPass(s, e, password.Text, verify.Text); };
        }

        private bool ValidateFields(string pass, string verify)
        {
            if (string.IsNullOrEmpty(pass) && string.IsNullOrEmpty(verify))
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(c, Application.Context.GetString(Resource.String.commonError),
                                    Application.Context.GetString(Resource.String.alertViewMessagePasswordFieldsEmpty));
                });
                return false;
            }

            if (pass != verify)
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(c, Application.Context.GetString(Resource.String.commonError),
                                    Application.Context.GetString(Resource.String.alertViewTitlePasswordsMismatch));
                });
                return false;
            }
			
            return true;
        }

        private void loginNewPass(object sender, EventArgs e, string pass, string verify)
        {
            if (this.ValidateFields(pass, verify))
            {
                RunOnUiThread(() => Toast.MakeText(c, Resource.String.resetResetting, ToastLength.Short).Show());
                password = pass.Trim();
                token.Trim();
                email.Trim();
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("sending email {0}, token {1}, password {2}, ServiceAuthToken {3}", email, token, password, AndroidData.ServiceAuthToken);
                #endif
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.UserPasswordResetCompleted += Service_UserPasswordResetCompleted;
                service.UserPasswordResetAsync(email, token, password, new Guid(AndroidData.ServiceAuthToken));
            }
        }

        private void Service_UserPasswordResetCompleted(object sender, UserPasswordResetCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserPasswordResetCompleted -= Service_UserPasswordResetCompleted;

            if (null == e.Error)
            {
                User result = e.Result;
                if (result.Errors.Count > 0)
                {
                    string errorMessage = StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors);
                    RunOnUiThread(() => GeneralUtils.Alert(c, Application.Context.GetString(Resource.String.commonError), errorMessage));
                } else
                {
                    RunOnUiThread(() => Toast.MakeText(c, Resource.String.commonLoggingIn, ToastLength.Short).Show());
                    service.UserLoginCompleted += Service_UserLoginCompleted;
                    service.UserLoginAsync(AndroidData.NewDeviceID,
                                           DeviceDeviceTypes.Android,
                                           LOLConstants.DefaultGuid,
                                           string.Empty, string.Empty,
                                           AccountOAuth.OAuthTypes.LOL,
                                           email,
                                           password,
                                           new Guid(AndroidData.ServiceAuthToken));
                }
            } else
            {
                RunOnUiThread(() => GeneralUtils.Alert(c, Application.Context.GetString(Resource.String.commonError),
                                                     Application.Context.GetString(Resource.String.errorResettingPassword)));
            }
        }

        private void Service_UserLoginCompleted(object sender, UserLoginCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserLoginCompleted -= Service_UserLoginCompleted;

            if (null == e.Error)
            {

                User loggedUser = e.Result;
                if (loggedUser.Errors.Count > 0)
                {
                    RunOnUiThread(() => GeneralUtils.Alert(c, Application.Context.GetString(Resource.String.commonError),
                                                         Application.Context.GetString(Resource.String.errorNotLoggedIn)));
                } else
                {
                    AndroidData.IsLoggedIn = true;
                    AndroidData.CurrentUser = loggedUser;
                    StartActivity(typeof(Main.HomeActivity));
                    Finish();
                }
            } else
            {
                RunOnUiThread(() => GeneralUtils.Alert(c, Application.Context.GetString(Resource.String.commonError),
                                                     Application.Context.GetString(Resource.String.errorLoginFailed)));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
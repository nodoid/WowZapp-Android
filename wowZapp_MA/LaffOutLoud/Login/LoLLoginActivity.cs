using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using LOLAccountManagement;
using System.Threading.Tasks;
using LOLAccountManagement.Classes.DtoObjects;
using LOLApp_Common;
using WZCommon;

namespace wowZapp.Login
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class LoLLoginActivity : Activity, IDisposable
    {
        Context c;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LoginLoL);

            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            Header.headertext = Application.Context.Resources.GetString(Resource.String.loginHeader);
            c = header.Context;
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(c);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            email = FindViewById<EditText>(Resource.Id.editEmail);
            password = FindViewById<EditText>(Resource.Id.editPassword);
            Button login = FindViewById<Button>(Resource.Id.btnLogin);
            Button cancel = FindViewById<Button>(Resource.Id.btnCancel);
            Button forgotten = FindViewById<Button>(Resource.Id.btnForgotPassword);
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            btnBack.Click += delegate
            {
                Finish();
            };

            login.Click += delegate(object s, EventArgs e)
            {
                startLogin(s, e);
            };
            forgotten.Click += delegate
            {
                StartActivity(typeof(Login.LoginForgottenPassActivity));
                Finish();
            };
            cancel.Click += delegate
            {
                Finish();
            };
        }
        
        private EditText email, password;

        private bool ValidateEmailField(string email)
        {

            if (!StringUtils.IsEmailAddress(email))
            {
                RunOnUiThread(() => GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.commonError),
                                                Application.Context.Resources.GetString(Resource.String.alertViewMessageNotValidEmailAddress)));
                return false;
            }
            return true;
        }

        private void startLogin(object s, EventArgs e)
        {
            if (ValidateEmailField(email.Text))
            {
                RunOnUiThread(() => Toast.MakeText(c, Resource.String.commonLoggingIn, ToastLength.Short).Show());

                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.AuthenticationTokenGetCompleted += Service_AuthTokenCompleted;
                service.AuthenticationTokenGetAsync(AndroidData.NewDeviceID);
            }
        }

        private void userlogin()
        {
            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            service.UserLoginCompleted += Service_UserLoginCompleted;
            service.UserLoginAsync(AndroidData.NewDeviceID,
                                   DeviceDeviceTypes.Android,
                                   LOLConstants.DefaultGuid,
                                   string.Empty, string.Empty,
                                   AccountOAuth.OAuthTypes.LOL,
                                   email.Text,
                                   password.Text,
                                   new Guid(AndroidData.ServiceAuthToken));
        }

        private void Service_AuthTokenCompleted(object sender, AuthenticationTokenGetCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.AuthenticationTokenGetCompleted -= Service_AuthTokenCompleted;

            if (e.Error != null)
            {
                Guid result = e.Result;
                string error = result.ToString(), errorOut = "";
                if (result == Guid.Empty)
                {
                    errorOut = Application.Context.Resources.GetString(Resource.String.authNoDeviceIDSent);
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.errorAuthError), errorOut);
                    });
                }
            } else
            {
                AndroidData.ServiceAuthToken = e.Result.ToString();
                userlogin();
            }
        }

        private void Service_UserLoginCompleted(object sender, UserLoginCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserLoginCompleted -= Service_UserLoginCompleted;

            if (null == e.Error)
            {
                User result = e.Result;

                if (result.Errors.Count > 0)
                {
                    string errorMessage =
                        StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors);
                    foreach (GeneralError eachError in result.Errors)
                    {
                        ConnectServiceErrors error = StringUtils.GetErrorForErrorNumberStr(eachError.ErrorNumber);
                        string errorType = error.ToString();
                        string err = "";
                        switch (errorType)
                        {
                            case "PasswordsDontMatch":
                                err = Application.Context.Resources.GetString(Resource.String.errorCouldNotValidateEmailPassword);
                                break;
                            case "AccountInformationInvalid":
                                err = Application.Context.Resources.GetString(Resource.String.errorLoginAccountInvalid);
                                break;
                            default:
                                err = Application.Context.Resources.GetString(Resource.String.errorLoginFail);
                                break;
                        }
						
                        RunOnUiThread(() => GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.commonError), err));
                        
#if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("Error message: {0}--{1}--{2}--{3}",
                                          eachError.ErrorTitle,
                                          eachError.ErrorDescription,
                                          eachError.ErrorLocation,
                                          eachError.ErrorNumber);
#endif
                    }
                } else
                {
                    AndroidData.IsLoggedIn = true;
                    AndroidData.CurrentUser = result;
					
                    if (AndroidData.CurrentUser.HasProfileImage &&
                        AndroidData.CurrentUser.Picture.Length == 0)
                    {
						
                        Task<UserImageDTO> task = service.CallAsyncMethod<UserImageDTO, UserGetImageDataCompletedEventArgs>((s, h, b) => {
                            if (b)
                            {
                                s.UserGetImageDataCompleted += h;
                            } else
                            {
                                s.UserGetImageDataCompleted -= h;
                            }//end if else
                        }, (s) => {
								
                            s.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, 
								                        AndroidData.CurrentUser.AccountID, 
								                        new Guid(AndroidData.ServiceAuthToken));
                        });
						
                        try
                        {
							
                            UserImageDTO usrImage = task.Result;
                            if (usrImage.Errors.Count > 0)
                            {
#if(DEBUG)
                                //Console.WriteLine ("Error retrieving user's image! {0}", StringUtils.CreateErrorMessageFromGeneralErrors (usrImage.Errors));
#endif
                            } else
                            {
                                User currentUser = AndroidData.CurrentUser;
                                currentUser.Picture = usrImage.ImageData;
                                AndroidData.CurrentUser = currentUser;
                            }//end if else
                        } catch (Exception ex)
                        {
#if(DEBUG)
                            //Console.WriteLine ("Exception retrieving user's image! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        }//end try catch
						
                    }
                    if (System.Net.ServicePointManager.ServerCertificateValidationCallback == null)
                    {
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = CertValidator.Validator;
                    }
					
                    RunOnUiThread(delegate
                    {
                        StartActivity(typeof(Main.HomeActivity));
                        Finish();
                    });
                }//end if else
            } else
                RunOnUiThread(() => GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.commonError),
                                                Application.Context.Resources.GetString(Resource.String.errorLoginFailed)));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
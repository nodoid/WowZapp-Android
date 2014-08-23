using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LOLApp_Common;

using WZCommon;

namespace wowZapp.Login
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class LoginForgottenPassActivity : Activity
	{
		Context c;
		private string emailAddress;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.LoginForgottenPass);

			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			c = header.Context;
			Header.headertext = Application.Context.Resources.GetString (Resource.String.forgotPassword);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (c);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			EditText editEmailAddress = FindViewById<EditText> (Resource.Id.editEmail);
			Button reset = FindViewById<Button> (Resource.Id.btnReset);
			Button cancel = FindViewById<Button> (Resource.Id.btnCancel);

			reset.Enabled = false;
			string email = "";

			editEmailAddress.TextChanged += (object s, Android.Text.TextChangedEventArgs e) =>
			{
				RunOnUiThread (delegate {
					if (editEmailAddress.Text.Length != 0) {
						reset.Enabled = true;
						email = editEmailAddress.Text;
					} else {
						reset.Enabled = false;
						email = "";
					}
				});
			};

			cancel.Click += delegate {
				Finish ();
			};
			reset.Click += (object s, EventArgs e) =>
			{
				resetPass (s, e, email);
			};
		}

		private void resetPass (object sender, EventArgs e, string email)
		{
			if (ValidateEmailAddress (email)) {
				Toast.MakeText (c, Resource.String.passwordRecoveryRequestingResetCode, ToastLength.Short).Show ();
				this.emailAddress = email;
				LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
				/*service.UserGetResetTokenCompleted += Service_UserGetResetTokenCompleted;
				service.UserGetResetTokenAsync (emailAddress, new Guid (AndroidData.ServiceAuthToken));*/
				service.AuthenticationTokenGetCompleted += Service_AuthenticationTokenGetCompleted;
				service.AuthenticationTokenGetAsync (AndroidData.NewDeviceID);
			}
		}
		
		private void Service_AuthenticationTokenGetCompleted (object s, AuthenticationTokenGetCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)s;
			service.AuthenticationTokenGetCompleted -= Service_AuthenticationTokenGetCompleted;
		
			if (e.Error == null) {
				Guid result = e.Result;
				if (!result.Equals (Guid.Empty)) {
					AndroidData.ServiceAuthToken = result.ToString ();
					service.UserGetResetTokenCompleted += Service_UserGetResetTokenCompleted;
					service.UserGetResetTokenAsync (emailAddress, new Guid (AndroidData.ServiceAuthToken));
				} else {
					RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
					                                    Application.Context.GetString (Resource.String.errorAuthError)));
				}
			} else
				RunOnUiThread (() => Finish ());
		}

		private void Service_UserGetResetTokenCompleted (object sender, UserGetResetTokenCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)sender;
			service.UserGetResetTokenCompleted -= Service_UserGetResetTokenCompleted;
			if (null == e.Error) {
				ConnectServiceErrors error = StringUtils.GetErrorForErrorNumberStr (e.Result.ErrorNumber);

				if (error == ConnectServiceErrors.None || string.IsNullOrEmpty (e.Result.ErrorNumber)) {
					RunOnUiThread (delegate {
						Intent i = new Intent (this, typeof(LoginResetPassActivity));
						i.PutExtra ("email", emailAddress);
						StartActivity (i);
						Finish ();
					});
				} else {
					string err = "";
					if (error == ConnectServiceErrors.UnableToResetPassword)
						err = Application.Context.GetString (Resource.String.errorUnableToSendVerificationCode);
					else
						err = error.ToString ();
					RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError), err));
				}
			} else {
				RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
                                                     Application.Context.GetString (Resource.String.errorRequestingPasswordResetCode)));
			}//end if else
		}

		private bool ValidateEmailAddress (string email)
		{
			bool rv = true;
			RunOnUiThread (delegate {
				if (string.IsNullOrEmpty (email)) {
					GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
                                     Application.Context.GetString (Resource.String.alertViewEmailEmpty));
					rv = false;
				}

				if (!StringUtils.IsEmailAddress (email)) {
					GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
                                     Application.Context.GetString (Resource.String.alertViewEmailInvalid));
					rv = false;
				}
			});
			return rv;
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
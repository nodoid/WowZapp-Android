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
	public class LoginResetPassActivity : Activity
	{
		private string email, verifyCode;
		Context c;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.email = base.Intent.GetStringExtra ("email");
			SetContentView (Resource.Layout.LoginResetPass);

			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			Header.headertext = Application.Context.Resources.GetString (Resource.String.resetTitle);
			c = header.Context;
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (c);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			TextView emailAddresser = FindViewById<TextView> (Resource.Id.txtEmailAddress);
			c = emailAddresser.Context;
			emailAddresser.Text = email;
			EditText resetCode = FindViewById<EditText> (Resource.Id.editCode);
			Button btnReset = FindViewById<Button> (Resource.Id.btnReset);
			Button btnCancel = FindViewById<Button> (Resource.Id.btnCancel);
			btnCancel.Click += delegate {
				Finish ();
			};
			btnReset.Click += (object s, EventArgs e) => {
				validateReset (s, e, resetCode.Text); };
		}

		private void validateReset (object sender, EventArgs e, string code)
		{
			if (ValidateField (code)) {
				RunOnUiThread (() => Toast.MakeText (c, Resource.String.verificationCodeSendingVerificationCode, ToastLength.Short).Show ());
				this.verifyCode = code;
				LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
				service.UserValidateResetTokenCompleted += Service_UserValidateResetTokenCompleted;
				service.UserValidateResetTokenAsync (this.email,
                                                    this.verifyCode,
                                                    new Guid (AndroidData.ServiceAuthToken));
			}
		}

		private void Service_UserValidateResetTokenCompleted (object sender, UserValidateResetTokenCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)sender;
			service.UserValidateResetTokenCompleted -= Service_UserValidateResetTokenCompleted;
			if (null == e.Error) {
				ConnectServiceErrors error = StringUtils.GetErrorForErrorNumberStr (e.Result.ErrorNumber);
				if (error == ConnectServiceErrors.None || string.IsNullOrEmpty (e.Result.ErrorNumber)) {
					Intent i = new Intent (this, typeof(LoginResetPassConfirmActivity));
					i.PutExtra ("email", this.email);
					i.PutExtra ("token", this.verifyCode);
					StartActivity (i);
				} else {
					RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
                                                         e.Result.ToString ()));
				}
			} else {
				RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
                                                     Application.Context.GetString (Resource.String.verificationCodeResetError)));
			}
		}

		private bool ValidateField (string s)
		{
			bool rv = true;
			if (string.IsNullOrEmpty (s)) {
				RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
                                 Application.Context.GetString (Resource.String.alertViewTitleFieldEmpty)));
				rv = false;
			}
			return rv;
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
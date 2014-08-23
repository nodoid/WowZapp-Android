using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using LOLApp_Common;
using LOLAccountManagement;

using WZCommon;

namespace wowZapp.Login
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class LoginButtonsActivity : Activity
	{
		private SocialAuthController authController;
		private Context c;
                
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.LoginButtons);

			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			c = header.Context;

			if (AndroidData.user == UserType.NewUser) {
				Header.headertext = Resources.GetString (Resource.String.loginNewUserHeader);
			} else {
				Header.headertext = Resources.GetString (Resource.String.loginHeader);
			}

			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (c);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;
			ImageButton google = FindViewById<ImageButton> (Resource.Id.imgNewUserGoogle);
			google.Clickable = true;
			google.Click += new EventHandler (startSignup_Click);

			ImageButton facebook = FindViewById<ImageButton> (Resource.Id.imgNewUserFacebook);
			facebook.Clickable = true;
			facebook.Click += new EventHandler (startSignup_Click);

			ImageButton twitter = FindViewById<ImageButton> (Resource.Id.imgNewUsertwitter);
			twitter.Clickable = true;
			twitter.Click += new EventHandler (startSignup_Click);

			ImageButton lol = FindViewById<ImageButton> (Resource.Id.imgNewUserLOL);
			lol.Clickable = true;
			lol.Click += new EventHandler (startSignup_Click);

			ImageButton linkedin = FindViewById<ImageButton> (Resource.Id.imgLinkedIn);
			linkedin.Clickable = true;
			linkedin.Click += new EventHandler (startSignup_Click);

			ImageButton youtube = FindViewById<ImageButton> (Resource.Id.imgYouTube);
			youtube.Clickable = true;
			youtube.Click += new EventHandler (startSignup_Click);
            
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Clickable = true;
			btnBack.Click += delegate {
				Finish ();
			};
		}

		private void startSignup_Click (object s, EventArgs e)
		{
			ImageButton i = (ImageButton)s;

			switch (i.Id) {
			case Resource.Id.imgYouTube:
				this.authController = new SocialAuthController (AccountOAuth.OAuthTypes.YouTube, c); 
				break;

			case Resource.Id.imgLinkedIn:
				this.authController = new SocialAuthController (AccountOAuth.OAuthTypes.LinkedIn, c); 
				break;

			case Resource.Id.imgNewUserFacebook:
				this.authController = new SocialAuthController (AccountOAuth.OAuthTypes.FaceBook, c);
				break;

			case Resource.Id.imgNewUserGoogle:
				this.authController = new SocialAuthController (AccountOAuth.OAuthTypes.Google, c);
				break;

			case Resource.Id.imgNewUsertwitter:
				this.authController = new SocialAuthController (AccountOAuth.OAuthTypes.Twitter, c);
				break;

			case Resource.Id.imgNewUserLOL:
				if (AndroidData.user == UserType.NewUser) {
					if (wowZapp.LaffOutOut.Singleton.ScreenYHeight > 800)
						StartActivity (typeof(LoLRegisterActivity));
					else
						StartActivity (typeof(LOLRegisterPhoneActivity));
					FinishActivity (0);
				} else {
					StartActivity (typeof(LoLLoginActivity));
					FinishActivity (0);
				}
				break;
			}
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
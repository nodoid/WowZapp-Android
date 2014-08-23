using System;

using Android.App;
using Android.Widget;
using Android.Content;
using Android.Webkit;
using Android.OS;

using LOLApp_Common;
using LOLAccountManagement;

using System.Threading;

namespace wowZapp.Photoalbums
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class PhotoAuthController : Activity, IDisposable
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.GetConnectedAuth);
			
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewUserHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			Context context = header.Context;
			Header.fontsize = 36f;
			
			int t = base.Intent.GetIntExtra ("type", 0);
			
			switch (t) {
			case 1:
				this.ProviderType = AccountOAuth.OAuthTypes.FaceBook;
				this.Provider = new LFacebookManager (LOLConstants.FacebookAPIKey, LOLConstants.FacebookAppSecret);
				Header.headertext = Application.Context.GetString (Resource.String.commonFacebookHeaderView);
				break;
				
			case 3:
				this.ProviderType = AccountOAuth.OAuthTypes.LinkedIn;
				this.Provider = new LLinkedInManager (LOLConstants.LinkedInConsumerKey, LOLConstants.LinkedInConsumerSecret, string.Empty, string.Empty);
				Header.headertext = Application.Context.GetString (Resource.String.commonLinkedInHeaderView);
				break;
				
			case 2:
				this.ProviderType = AccountOAuth.OAuthTypes.Google;
				HttpHelper helper = new HttpHelper ();
				this.Provider = new LGooglePlusManager (LOLConstants.GooglePlusConsumerKey, LOLConstants.GooglePlusConsumerSecret, helper);
				Header.headertext = Application.Context.GetString (Resource.String.commonGooglePlusHeaderView);
				break;
			}
			
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;
			
			CookieSyncManager.CreateInstance (this);
			CookieSyncManager.Instance.StartSync ();
			
			string url = Provider.BrowserAuthUrl;
			string cookieString = "cookieName=''";
			
			CookieManager cookieManager = CookieManager.Instance;
			cookieManager.SetAcceptCookie (true);
			cookieManager.SetCookie (url, cookieString);
			
			WebView webView = FindViewById<WebView> (Resource.Id.webView1);
			webView.Settings.JavaScriptEnabled = true;
			webView.Settings.SavePassword = true;
			webView.Settings.SaveFormData = true;
			ThreadPool.QueueUserWorkItem (delegate {
				RunOnUiThread (delegate {
					webView.LoadUrl (url);
					webView.SetWebViewClient (new dealWithSACWebView (this, this.ProviderType, this.Provider, btnBack));
				});
				//Finish();
			});
			//Finish();
		}
		
		public AccountOAuth.OAuthTypes ProviderType {
			get;
			private set;
		}//end DeviceOAuth.OAuthTypes NetworkProvider
		
		public ISocialProviderManager Provider {
			get;
			private set;
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
	
	class dealWithSACWebView : WebViewClient
	{
		WebView webView;
		Activity parent;
		private Context c;
		public AccountOAuth.OAuthTypes ProviderType { get; private set; }
		public ISocialProviderManager Provider { get; private set; }
		public event AccessTokenReceivedHandler AccessTokenReceived;
		AccountOAuth accountAuth;
		
		public dealWithSACWebView (Activity f, AccountOAuth.OAuthTypes ProviderType, ISocialProviderManager Provider, ImageButton btnBack)
		{
			this.parent = f;
			this.Provider = Provider;
			this.ProviderType = ProviderType;
			string authUrl = this.Provider.BrowserAuthUrl;
			btnBack.Click += delegate {
				parent.Finish ();
			};
			switch (ProviderType) {
			case AccountOAuth.OAuthTypes.Google:
				AccessTokenReceived += (occurred, expires, accessToken, accessTokenSecret, refreshToken) =>
				{
					AndroidData.GooglePlusAccessToken = accessToken;
					AndroidData.GooglePlusRefreshToken = refreshToken;
					AndroidData.GoogleAccessTokenExpiration = expires;
					ShowAfterLoginScreen ();
				};
				break;
			case AccountOAuth.OAuthTypes.FaceBook:
				LFacebookManager lFace = new LFacebookManager (LOLConstants.FacebookAPIKey, LOLConstants.FacebookAppSecret);
				AccessTokenReceived += (occurred, expires, accessToken, accessTokenSecret, refreshToken) =>
				{
					try {
						if (lFace.RefreshAccessToken ()) {
							AndroidData.FacebookAccessToken = lFace.AccessToken;
							AndroidData.FacebookAccessTokenExpiration =
								lFace.AccessTokenExpirationTime ?? DateTime.Now;
						}//end if
					} catch {
						AndroidData.FacebookAccessToken = accessToken;
						AndroidData.FacebookAccessTokenExpiration = expires;
					}
					ShowAfterLoginScreen ();
				};
				break;
			case AccountOAuth.OAuthTypes.LinkedIn:
				AccessTokenReceived += (occurred, expires, accessToken, accessTokenSecret, refreshToken) =>
				{
					AndroidData.LinkedInAccessToken = accessToken;
					AndroidData.LinkedInAccessTokenSecret = accessTokenSecret;
					ShowAfterLoginScreen ();
				};
				break;
			}
		}
		
		private string urlString;
		
		public override void OnPageFinished (WebView view, string url)
		{
			webView = view;
			c = webView.Context;
			this.urlString = url;
			try {
				switch (this.ProviderType) {
				case AccountOAuth.OAuthTypes.FaceBook:
					ThreadPool.QueueUserWorkItem (delegate {
						parent.RunOnUiThread (delegate {
							this.AuthorizeFacebook ();
						});
					});
					break;
					
				case AccountOAuth.OAuthTypes.LinkedIn:
					ThreadPool.QueueUserWorkItem (delegate {
						parent.RunOnUiThread (delegate {
							this.AuthorizeLinkedIn ();
						});
					});
					break;
					
				case AccountOAuth.OAuthTypes.Google:
					ThreadPool.QueueUserWorkItem (delegate {
						parent.RunOnUiThread (delegate {
							this.AuthorizeGooglePlus ();
						});
					});
					break;
				}//end switch
				//parent.Finish();
			} catch {
				parent.RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
				                                 Application.Context.GetString (Resource.String.alertViewMessageConnectionProblem)));
			}
		}
		
		public void OnReceivedError (WebView view, int errorCode, String description, String failingUrl)
		{
			// This is because on first install, it fails to load
			// https://m.facebook.com subdomain.
			if (errorCode != -999) {
				parent.RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
				                                 Application.Context.GetString (Resource.String.alertViewMessageConnectionProblem)));
			}
			
			/*if (null != this.failingUrl)
            {
                this.FailedToLoadUrl(this.webView, new BrowserEventArgs(DateTime.Now, e));
            }//end if*/
		}
		
		
		private void AuthorizeFacebook ()
		{
			try {
				string urlString = this.webView.Url.ToString ();
				
				if (urlString.StartsWith (LOLConstants.LinkFacebookLoginSuccessUrl)) {
					if (this.Provider.UserAccepted (urlString)) {
						if (this.Provider.CheckForAccessToken (urlString)) {
							if (null != this.AccessTokenReceived) {
								this.AccessTokenReceived (DateTime.Now, this.Provider.AccessTokenExpirationTime ?? DateTime.Now,
								                         this.Provider.AccessToken, string.Empty, string.Empty);
							}
							parent.RunOnUiThread (delegate {
								GeneralUtils.Alert (c, Application.Context.Resources.GetString (Resource.String.loginAuthGood), "Facebook");
							});
						} else {
							parent.RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
							                                 Application.Context.GetString (Resource.String.alertViewMessageAccessTokenProblem)));
						}
					} else {
						parent.RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
						                                 Application.Context.GetString (Resource.String.alertViewUserCancelledAuth)));
					}
				}
			} catch (Exception ex) {
				parent.RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError), ex.ToString ()));
			}
			//parent.Finish();
		}
		
		private void AuthorizeLinkedIn ()
		{
			try {
				
				string requestString = this.webView.Url.ToString ();
				
				if (!string.IsNullOrEmpty (requestString) && requestString.StartsWith (LOLConstants.LinkLinkedInCallbackUrl)) {
					string queryString = requestString.Substring (LOLConstants.LinkLinkedInCallbackUrl.Length + 1);
					
					if (this.Provider.UserAccepted (queryString)) {
						if (this.Provider.CheckForAccessToken (requestString)) {
							if (null != this.AccessTokenReceived) {
								this.AccessTokenReceived (DateTime.Now, DateTime.MinValue, this.Provider.AccessToken,
								                         this.Provider.AccessTokenSecret, string.Empty);
							}
							parent.RunOnUiThread (delegate {
								GeneralUtils.Alert (c, Application.Context.Resources.GetString (Resource.String.loginAuthGood), "LinkedIn");
							});
						} else {
							parent.RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
							                                 Application.Context.GetString (Resource.String.alertViewMessageAccessTokenProblem)));
						}
					} else {
						parent.RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError),
						                                 Application.Context.GetString (Resource.String.alertViewUserCancelledAuth)));
					}
				}
			} catch (Exception ex) {
				parent.RunOnUiThread (() => GeneralUtils.Alert (c, Application.Context.GetString (Resource.String.commonError), ex.ToString ()));
			}
			//parent.Finish();
		}
		
		private void AuthorizeGooglePlus ()
		{
			parent.RunOnUiThread (delegate {
				try {
					string requestString = this.webView.Url.ToString ();
					
					if (!string.IsNullOrEmpty (requestString) && requestString.StartsWith (LOLConstants.LinkGooglePlusCallbackUrl)) {
						if (this.Provider.UserAccepted (requestString)) {
							if (this.Provider.CheckForAccessToken (requestString)) {
								if (null != this.AccessTokenReceived) {
									this.AccessTokenReceived (DateTime.Now, this.Provider.AccessTokenExpirationTime ?? DateTime.Now,
									                         this.Provider.AccessToken, string.Empty, this.Provider.RefreshToken);
								}
								parent.RunOnUiThread (delegate {
									GeneralUtils.Alert (c, Application.Context.Resources.GetString (Resource.String.loginAuthGood), "Google+");
								});
							} else {
								parent.RunOnUiThread (() => Toast.MakeText (c, Resource.String.alertViewMessageAccessTokenProblem, ToastLength.Short).Show ());
							}
						} else {
							parent.RunOnUiThread (() => Toast.MakeText (c, Resource.String.alertViewUserCancelledAuth, ToastLength.Short).Show ());
						}
					}
				} catch (Exception ex) {
					parent.RunOnUiThread (() => Toast.MakeText (c, ex.ToString (), ToastLength.Short).Show ());
				}
			});
		}
		
		private void ShowAfterLoginScreen ()
		{
			Intent resultData = new Intent ();
			parent.SetResult (Result.Ok, resultData);
			parent.Finish ();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using LOLAccountManagement;
using LOLApp_Common;
using WZCommon;

namespace wowZapp.Photoalbums
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class GetPhotoAlbumActivity : Activity
	{
		private AccountOAuth.OAuthTypes providerType;
		private MediaPickType mediaType;
		private Context context;
		private Dialog LightboxDialog;
		private LinearLayout listWrapper;

		public ISocialProviderManager Provider
		{ get; private set; }
		
		private bool gg, gl, gf;
		private const int FACEBOOK_BACK = 1, GOOGLE_BACK = 2, LINKEDIN_BACK = 3, PHOTO_PICK = 4;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.PhotoAlbumNames);
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewUserHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			Header.headertext = Application.Context.Resources.GetString (Resource.String.photoalbumAlbumsTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;
			
			listWrapper = FindViewById<LinearLayout> (Resource.Id.linearListWrapper);
			context = listWrapper.Context;
			
			bool media = base.Intent.GetBooleanExtra ("media", false);
			mediaType = media == true ? MediaPickType.Video : MediaPickType.StillImage;
			
			int socialNetwork = base.Intent.GetIntExtra ("network", -1);
			// socialnetworks - same as in get contacts....
			
			switch (socialNetwork) {
			case 1:
				providerType = AccountOAuth.OAuthTypes.Google;
				if (string.IsNullOrEmpty (AndroidData.GooglePlusAccessToken)) {
					this.Provider = new LGooglePlusManager (LOLConstants.GooglePlusConsumerKey, LOLConstants.GooglePlusConsumerSecret, new HttpHelper ());
					gg = false;
				} else {
					this.Provider = new LGooglePlusManager (LOLConstants.GooglePlusConsumerKey, LOLConstants.GooglePlusConsumerSecret, AndroidData.GooglePlusAccessToken, AndroidData.GooglePlusRefreshToken, 
					                                       AndroidData.GoogleAccessTokenExpiration, new HttpHelper ());
					gg = true;
				}
				break;
				
			case 2:
				providerType = AccountOAuth.OAuthTypes.LinkedIn;
				if (string.IsNullOrEmpty (AndroidData.LinkedInAccessToken)) {
					this.Provider = new LLinkedInManager (LOLConstants.LinkedInConsumerKey, LOLConstants.LinkedInConsumerSecret, string.Empty, string.Empty);
					gl = false;
				} else {
					this.Provider = new LLinkedInManager (LOLConstants.LinkedInConsumerKey, LOLConstants.LinkedInConsumerSecret, AndroidData.LinkedInAccessToken, AndroidData.LinkedInAccessTokenSecret);
					gl = true;
				}
				break;
				
			case 3:
				providerType = AccountOAuth.OAuthTypes.FaceBook;
				if (string.IsNullOrEmpty (AndroidData.FacebookAccessToken)) {
					gf = false;
					this.Provider = new LFacebookManager (LOLConstants.FacebookAPIKey, LOLConstants.FacebookAppSecret);
				} else {
					this.Provider = new LFacebookManager (LOLConstants.FacebookAPIKey, LOLConstants.FacebookAppSecret, AndroidData.FacebookAccessToken, AndroidData.FacebookAccessTokenExpiration);
					gf = true;
				}
				break;
			}
			
			ImageButton btnHome = FindViewById<ImageButton> (Resource.Id.btnHome);
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			
			btnBack.Click += delegate {
				Finish ();
			};
			btnHome.Click += delegate {
				Intent i = new Intent (this, typeof(Main.HomeActivity));
				i.SetFlags (ActivityFlags.ClearTop);
				StartActivity (i);
			};

			btnBack.Tag = 0;
			btnHome.Tag = 1;
			LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			ImageButton[] buttons = new ImageButton[2];
			buttons [0] = btnBack;
			buttons [1] = btnHome;
			ImageHelper.setupButtonsPosition (buttons, bottom, context);

			System.Threading.Tasks.Task.Factory.StartNew (() => SetDataSources ());
		}

		private void SetDataSources ()
		{
			ThreadPool.QueueUserWorkItem (delegate {
				getAlbums ();
			});
		}

		private void getAlbums ()
		{
			try {
				switch (providerType) {
				case AccountOAuth.OAuthTypes.FaceBook:
					if (gf == false) {
						Intent auth = new Intent (this, typeof(PhotoAuthController));
						auth.PutExtra ("type", FACEBOOK_BACK);
						StartActivityForResult (auth, FACEBOOK_BACK);
					} else
					if (mediaType == MediaPickType.StillImage)
						GetFacebookAlbums ();
					else
						GetFacebookVideos ();
					break;
				case AccountOAuth.OAuthTypes.Google:
					if (gg == false) {
						Intent auth = new Intent (this, typeof(PhotoAuthController));
						auth.PutExtra ("type", GOOGLE_BACK);
						StartActivityForResult (auth, GOOGLE_BACK);
					} else
					if (mediaType == MediaPickType.StillImage)
						GetGooglePlusAlbums ();
					else
						GetGooglePlusVideos ();
					break;
				case AccountOAuth.OAuthTypes.LinkedIn:
					if (gl == false) {
						Intent auth = new Intent (this, typeof(PhotoAuthController));
						auth.PutExtra ("type", LINKEDIN_BACK);
						StartActivityForResult (auth, LINKEDIN_BACK);
					} else
					if (mediaType == MediaPickType.StillImage)
						GetLinkedInAlbums ();
					else
						GetLinkedInVideos ();
					break;
				}
			} catch (Exception ex) {
				RunOnUiThread (delegate {
					string m = string.Format ("{0} {1}",
					                         string.Format (Application.Context.GetString (Resource.String.errorDownloadingFriendsFormat),
					              this.Provider.ProviderType), ex.Message);
					GeneralUtils.Alert (context, Application.Context.GetString (Resource.String.commonError), m);
				});
			}
		}

		private void GetGooglePlusAlbums ()
		{
		}
		private void GetLinkedInAlbums ()
		{
		}
		private void GetGooglePlusVideos ()
		{
		}
		private void GetLinkedInVideos ()
		{
		}
        
		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);	
			switch (requestCode) {
			case FACEBOOK_BACK:
				
				if (resultCode == Result.Ok) {
					this.Provider = new LFacebookManager (LOLConstants.FacebookAPIKey, 
					                                     LOLConstants.FacebookAppSecret, 
					                                     AndroidData.FacebookAccessToken, 
					                                     AndroidData.FacebookAccessTokenExpiration);
					this.GetFacebookAlbums ();
				}//end if
				break;
				
			case GOOGLE_BACK:
				if (resultCode == Result.Ok) {
					this.Provider = new LGooglePlusManager (LOLConstants.GooglePlusConsumerKey, 
					                                       LOLConstants.GooglePlusConsumerSecret, 
					                                       AndroidData.GooglePlusAccessToken, 
					                                       AndroidData.GooglePlusRefreshToken, 
					                                       AndroidData.GoogleAccessTokenExpiration, new HttpHelper ());
					this.GetGooglePlusAlbums ();
				}//end if
				break;
				
			case LINKEDIN_BACK:
				if (resultCode == Result.Ok) {
					this.Provider = new LLinkedInManager (LOLConstants.LinkedInConsumerKey, 
					                                     LOLConstants.LinkedInConsumerSecret, 
					                                     AndroidData.LinkedInAccessToken, 
					                                     AndroidData.LinkedInAccessTokenSecret);
					this.GetLinkedInAlbums ();
				}//end if
				break;
			case PHOTO_PICK:
				if (resultCode == Result.Ok) {
					Intent resultData = new Intent ();
					resultData.PutExtra ("filename", PhotoPickerUtil.imageFilename);
					SetResult (Result.Ok, resultData);
					Finish ();
				} else
					Finish ();
				break;
			}
		}
		
		private void GetFacebookAlbums ()
		{
			LFacebookManager fMan = (LFacebookManager)Provider;
			RunOnUiThread (() => ShowLightboxDialog (Application.Resources.GetString (Resource.String.photoalbumModalTitle)));
			
			if (DateTime.Now.CompareTo (AndroidData.FacebookAccessTokenExpiration) > -1) {
				try {
					if (fMan.RefreshAccessToken ()) {
						AndroidData.FacebookAccessToken = fMan.AccessToken;
						AndroidData.FacebookAccessTokenExpiration = fMan.AccessTokenExpirationTime.Value;
					}
				} catch (Exception e) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("error refressing facebook access token {0} {1}", e.Message, e.StackTrace);
#endif
					RunOnUiThread (() => DismissLightboxDialog ());
					return;
				}
			}
			
			string responseStr = string.Empty;
			try {
				responseStr = fMan.GetPhotoAlbums ();
			} catch (Exception ex) {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Error getting users photo albums {0}, {1}", ex.Message, ex.StackTrace);
#endif
				RunOnUiThread (() => DismissLightboxDialog ());
				return;
			}
			List<PhotoAlbumInfo> photoAlbumList = Parsers.ParseFacebookPhotoAlbumsResponse (responseStr);
			propogatePhotoListView (photoAlbumList);
		}

		private void GetFacebookVideos ()
		{
			LFacebookManager fMan = (LFacebookManager)Provider;
			RunOnUiThread (() => ShowLightboxDialog (Application.Resources.GetString (Resource.String.videoalbumModalTitle)));
			
			if (DateTime.Now.CompareTo (AndroidData.FacebookAccessTokenExpiration) > -1) {
				try {
					if (fMan.RefreshAccessToken ()) {
						AndroidData.FacebookAccessToken = fMan.AccessToken;
						AndroidData.FacebookAccessTokenExpiration = fMan.AccessTokenExpirationTime.Value;
					}
				} catch (Exception e) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("error refressing facebook access token {0} {1}", e.Message, e.StackTrace);
#endif
					RunOnUiThread (() => DismissLightboxDialog ());
					return;
				}
			}
			
			string responseStr = string.Empty;
			try {
				responseStr = fMan.GetAllVideos ();
			} catch (Exception ex) {
				#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Error getting user video albums - {0}, {1}", ex.Message, ex.StackTrace);
#endif
				RunOnUiThread (() => DismissLightboxDialog ());
				return;
			}
			
			List<VideoAlbumInfo> vidInfo = Parsers.ParseFacebookVideoAlbumsResponse (responseStr);
			propogateVideoListView (vidInfo);
		}
		
		private void propogatePhotoListView (List<PhotoAlbumInfo> photoAlbums)
		{
			RunOnUiThread (delegate {
				if (photoAlbums.Count != 0) {
					for (int n = 0; n < photoAlbums.Count; ++n) {
						using (LinearLayout layout = new LinearLayout (context)) {
							layout.Orientation = Android.Widget.Orientation.Horizontal;
							layout.SetGravity (GravityFlags.Center);
							layout.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (10f, context));
						
							using (TextView text = new TextView (context)) {
								text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (235f, context), (int)ImageHelper.convertDpToPixel (40f, context));
								text.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
								text.Gravity = GravityFlags.CenterVertical;
								text.TextSize = 16f;
								text.SetTextColor (Android.Graphics.Color.White);
								text.Text = photoAlbums [n].Name;
								int contactId = new int ();
								contactId = n;
								layout.Clickable = true;
								layout.Click += (object s, EventArgs e) => {
									handleClick (photoAlbums [contactId]); 
									#if DEBUG
									Console.WriteLine ("control returned");
#endif
								};
								layout.AddView (text);
							}
							listWrapper.AddView (layout);
						}
					}
				}
				
				RunOnUiThread (() => DismissLightboxDialog ());
			});
		}
	
		private void handleClick (PhotoAlbumInfo photoAlbum)
		{
			/*RunOnUiThread (delegate {
				PhotoPicker picker = new PhotoPicker (photoAlbum, Provider, context);
			});*/
			PhotoPickerUtil.albumInfo = photoAlbum;
			PhotoPickerUtil.Provider = Provider;
			Intent i = new Intent (this, typeof(PhotoPickerActivity));
			StartActivityForResult (i, PHOTO_PICK);
			#if DEBUG
			System.Diagnostics.Debug.WriteLine ("moooooo!");
			#endif
		}
		
		private void handleVideoClick (VideoAlbumInfo videoAlbum)
		{
		}
		
		private void propogateVideoListView (List<VideoAlbumInfo> videoAlbums)
		{
			RunOnUiThread (delegate {
				if (videoAlbums.Count != 0) {
					for (int n = 0; n < videoAlbums.Count; ++n) {
						using (LinearLayout layout = new LinearLayout (context)) {
							layout.Orientation = Android.Widget.Orientation.Horizontal;
							layout.SetGravity (GravityFlags.Center);
							layout.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (10f, context));
						
							using (ImageView profilepic = new ImageView (context)) {
								profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (40f, context), (int)ImageHelper.convertDpToPixel (40f, context));
								profilepic.Tag = new Java.Lang.String ("thumbpic_" + n.ToString ());
								layout.AddView (profilepic);
							}
							using (TextView text = new TextView (context)) {
								text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (235f, context), (int)ImageHelper.convertDpToPixel (40f, context));
								text.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
								text.Gravity = GravityFlags.CenterVertical;
								text.TextSize = 16f;
								text.SetTextColor (Android.Graphics.Color.White);
								text.Text = videoAlbums [n].Description;
								layout.AddView (text);
							}
							using (ImageView checkbox = new ImageView (context)) {
								checkbox.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.checkbox));
								checkbox.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (25f, context), (int)ImageHelper.convertDpToPixel (25f, context));
								//checkbox.ContentDescription = CHECKBOX_UNCHECKED;
						
								int contactId = new int ();
								contactId = n;
								layout.Clickable = true;
								layout.Click += (object s, EventArgs e) => {
									handleVideoClick (videoAlbums [n]); };
								layout.AddView (checkbox);
							}
							this.listWrapper.AddView (layout);
						}
					}
				}
				
				RunOnUiThread (() => DismissLightboxDialog ());
			});
		}
	
		public void ShowLightboxDialog (string message)
		{
			LightboxDialog = new Dialog (this, Resource.Style.lightbox_dialog);
			LightboxDialog.SetContentView (Resource.Layout.LightboxDialog);
			((TextView)LightboxDialog.FindViewById (Resource.Id.dialogText)).Text = message;
			LightboxDialog.Show ();
		}
		
		public void DismissLightboxDialog ()
		{
			if (LightboxDialog != null)
				LightboxDialog.Dismiss ();
			
			LightboxDialog = null;
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
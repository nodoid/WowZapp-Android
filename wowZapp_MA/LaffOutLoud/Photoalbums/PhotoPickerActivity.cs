using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LOLApp_Common;
using WZCommon;

namespace wowZapp.Photoalbums
{
	public static class PhotoPickerUtil
	{
		public static PhotoAlbumInfo albumInfo
		{ get; set; }
		public static ISocialProviderManager Provider
		{ get; set; }
		public static List<string> names
		{ get; set; }
		public static string imageFilename
		{ get; set; }
		public static int fromWhere
		{ get; set; }
		public static int position
		{ get; set; }
	}

	/*public class PhotoPicker
	{
		public PhotoPicker (PhotoAlbumInfo info, ISocialProviderManager provider, Context c)
		{
			PhotoPickerUtil.albumInfo = info;
			PhotoPickerUtil.Provider = provider;
			Intent i = new Intent (c, typeof(PhotoPickerActivity));
			c.StartActivity (i);
		}	
	}*/

	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
	public class PhotoPickerActivity : Activity
	{
		private Dialog previewDialog;
		private Dialog LightboxDialog;
		private List<PhotoInfo> photoList;
		private LinearLayout viewWrapper;
		private Context context;
		private ProgressBar progress;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.PictureAlbums);
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewUserHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			Header.headertext = PhotoPickerUtil.albumInfo.Name;
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;
			viewWrapper = FindViewById<LinearLayout> (Resource.Id.listPhotos);
			context = viewWrapper.Context;
			
			if (PhotoPickerUtil.names == null)
				PhotoPickerUtil.names = new List<string> ();
			else
				PhotoPickerUtil.names.Clear ();
			
			ImageButton btnHome = FindViewById<ImageButton> (Resource.Id.btnHome);
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Tag = 0;
			btnHome.Tag = 1;
			LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			ImageButton[] buttons = new ImageButton[2];
			buttons [0] = btnBack;
			buttons [1] = btnHome;
			ImageHelper.setupButtonsPosition (buttons, bottom, context);
			
			btnBack.Click += delegate {
				Finish ();
			};
			btnHome.Click += delegate {
				Intent i = new Intent (this, typeof(Main.HomeActivity));
				i.SetFlags (ActivityFlags.ClearTop);
				StartActivity (i);
			};
			
			ThreadPool.QueueUserWorkItem (delegate {
				GetAlbumPhotos ();
			});
		}
		
		private void GetAlbumPhotos ()
		{
			string message = string.Format ("{0} {1}", Application.Resources.GetString (Resource.String.onlineModalDownloadFor), PhotoPickerUtil.albumInfo.Name);
			RunOnUiThread (() => ShowLightboxDialog (message));
			string responseStr = string.Empty;
			try {
				responseStr = PhotoPickerUtil.Provider.GetAlbumPhotos (PhotoPickerUtil.albumInfo.ID);
			} catch (Exception e) {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Error downloading photo info for album {0}. Message {1}-{2}", PhotoPickerUtil.albumInfo.Name, e.Message, e.StackTrace);
#endif
				RunOnUiThread (() => DismissLightboxDialog ());
				return;
			}
			photoList = Parsers.ParseFacebookPhotosResponse (responseStr);
			int index = 0;
			LinearLayout layout = new LinearLayout (context);
			layout.Orientation = Android.Widget.Orientation.Horizontal;
			layout.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context));
			layout.SetGravity (GravityFlags.CenterHorizontal);
			RunOnUiThread (delegate {
				foreach (PhotoInfo eachPhoto in photoList) {
					eachPhoto.AlbumID = PhotoPickerUtil.albumInfo.ID;
				
					try {
						eachPhoto.ThumbImage = PhotoPickerUtil.Provider.GetImage (eachPhoto.SmallUrl);
					} catch (Exception ex) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Error downloading photo {0}. Message {1} {2}", eachPhoto.ID, ex.Message, ex.StackTrace);
						#endif
						continue;
					}

#if DEBUG
					System.Diagnostics.Debug.WriteLine ("SmallUrl = {0}", eachPhoto.SmallUrl);
#endif
					ImageView contentpackpic = new ImageView (context);
					contentpackpic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (100f, context), (int)ImageHelper.convertDpToPixel (100f, context));
					contentpackpic.SetPadding (0, 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
					string itemId = string.Empty;
					itemId = eachPhoto.SmallUrl;
					contentpackpic.Click += delegate {
						ShowModalPreviewDialog (itemId);
					};
					
					using (MemoryStream stream = new MemoryStream (eachPhoto.ThumbImage)) {
						Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromStream (stream, "Profile");
						contentpackpic.SetImageDrawable (draw);
						layout.AddView (contentpackpic);
					}

					if (index == photoList.Count || layout.ChildCount == 3) {
						viewWrapper.AddView (layout);
						layout = new LinearLayout (context);
						layout.Orientation = Android.Widget.Orientation.Horizontal;
						layout.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, 0, (int)ImageHelper.convertDpToPixel (10f, context));
						layout.SetGravity (GravityFlags.CenterHorizontal);
					}
					index++;
				}
			});
			DismissLightboxDialog ();
		}
		
		private void ShowModalPreviewDialog (string imgUrl)
		{
			previewDialog = new Dialog (this, Resource.Style.lightbox_dialog);
			previewDialog.SetContentView (Resource.Layout.ModalLargePreviewDialog);

			//string imgUrl = photoList[item].SmallUrl;
			byte[] imgBuff = null;
			ThreadPool.QueueUserWorkItem (delegate {
				
				try {
					RunOnUiThread (delegate {
						imgBuff = PhotoPickerUtil.Provider.GetImage (imgUrl);
						using (MemoryStream stream = new MemoryStream (imgBuff)) {
							Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromStream (stream, "Preview");
							((ImageView)previewDialog.FindViewById (Resource.Id.imgItemPic)).SetImageDrawable (draw);
						}
					});
				} catch (Exception ex) {
					#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Error downloading image {0}. Message {1}-{2}", imgUrl, ex.Message, ex.StackTrace);
					#endif
				}
			});
			
			((Button)previewDialog.FindViewById (Resource.Id.btnAdd)).Click += delegate {
				string filename = Path.GetRandomFileName ();
				filename = Path.Combine (wowZapp.LaffOutOut.Singleton.ContentDirectory, filename);
				File.WriteAllBytes (filename, imgBuff);
				PhotoPickerUtil.names.Insert (PhotoPickerUtil.position, filename);
				EndThis (true);
			};
			
			((Button)previewDialog.FindViewById (Resource.Id.btnCancel)).Click += delegate {
				RunOnUiThread (() => DismissModalPreviewDialog ());
				EndThis ();
			};
			
			previewDialog.Show ();
		}
		
		private void EndThis (bool content = false)
		{
			Intent i = null;
			switch (PhotoPickerUtil.fromWhere) {
			case 0: // animation
				i = new Intent ();
				break;
			case 1: // photopoll picker
				i = new Intent (this, typeof(Messages.ComposePhotoPollActivity));
				break;
			}
			i.PutExtra ("returned", PhotoPickerUtil.fromWhere);
			if (PhotoPickerUtil.fromWhere == 0) {
				SetResult (Result.Ok, i);
				i.PutExtra ("filename", PhotoPickerUtil.names [0]);
			} else {
				i.SetFlags (ActivityFlags.ClearTop);
				StartActivity (i);
			}
			Finish ();
		}
		
		public void DismissModalPreviewDialog ()
		{
			if (previewDialog != null)
				previewDialog.Dismiss ();
			
			previewDialog = null;
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


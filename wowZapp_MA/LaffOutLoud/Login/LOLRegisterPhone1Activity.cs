using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Provider;
using Android.Database;

using LOLAccountManagement;
using LOLApp_Common;

/* This is for PHONES only */

namespace wowZapp.Login
{
	[Activity (ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
	public class LOLRegisterPhoneActivity : Activity
	{
		Context context;
		private const int CAMMY = 1, PICCY = 2, NEXT = 10;
		private ImageView picture;
		private EditText username, email, password, verify;
		private byte[] userImage;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.LoginSignupLoLS1);
			
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			Header.headertext = Application.Context.Resources.GetString (Resource.String.createTitle);
			context = header.Context;
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;
			
			EditText username = FindViewById<EditText> (Resource.Id.editUsername);
			EditText email = FindViewById<EditText> (Resource.Id.editEmail);
			EditText password = FindViewById<EditText> (Resource.Id.editPassword);
			EditText verify = FindViewById<EditText> (Resource.Id.editPasswordVerify);
			
			Button takePic = FindViewById<Button> (Resource.Id.btnTakePic);
			Button choosePic = FindViewById<Button> (Resource.Id.btnChoosePic);
			picture = FindViewById<ImageView> (Resource.Id.imageView1);
			
			ImageView next = FindViewById<ImageView> (Resource.Id.imgNext);
			next.Click += delegate {
				if (verifyDetails (email.Text, password.Text, verify.Text, username.Text)) {
					Intent s = new Intent (this, typeof(LOLRegisterPhoneRegisterActivity));
					s.PutExtra ("email", email.Text);
					s.PutExtra ("password", password.Text);
					s.PutExtra ("username", username.Text);
					if (userImage == null) {
						using (Bitmap bmp = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.defaultuserimage)) {
							using (MemoryStream ms = new System.IO.MemoryStream ()) {
								bmp.Compress (Bitmap.CompressFormat.Jpeg, 80, ms);
								userImage = new byte[ms.Length];
								userImage = ms.ToArray ();
							}
						}
					} else
						using (Bitmap bmp = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.defaultuserimage)) {
							using (MemoryStream ms = new System.IO.MemoryStream ()) {
								bmp.Compress (Bitmap.CompressFormat.Jpeg, 80, ms);
								userImage = ms.ToArray ();
							}
						}
					s.PutExtra ("image", userImage);	
					StartActivityForResult (s, NEXT);		
				}
			};
			
			bool[] camTest = new bool[2];
			camTest = GeneralUtils.CamRec ();
			
			if (camTest [0] == false || camTest [1] == false) {
				takePic.Visibility = ViewStates.Invisible;
				choosePic.Visibility = ViewStates.Invisible;
			}
			
			takePic.Click += delegate(object s, EventArgs e) {
				Intent i = new Intent (this, typeof(CameraVideo.CameraTakePictureActivity));
				i.PutExtra ("camera", CAMMY);
				StartActivityForResult (i, CAMMY);
			};
			
			choosePic.Click += delegate(object s, EventArgs e) {
				var imageIntent = new Intent ();
				imageIntent.SetType ("image/*");
				imageIntent.SetAction (Intent.ActionGetContent);
				StartActivityForResult (Intent.CreateChooser (imageIntent, "Choose Image"), PICCY);
			};
		}
		
		private bool verifyDetails (string email, string password, string verify, string username)
		{
			bool rv = true;
			if (string.IsNullOrEmpty (email)) {
				rv = false;
				alert (Application.Context.GetString (Resource.String.errorCreateEmail));
			}
			if (string.IsNullOrEmpty (username)) {
				rv = false;
				alert (Application.Context.GetString (Resource.String.errorCreateUsername));
			}
			if (string.IsNullOrEmpty (password)) {
				rv = false;
				alert (Application.Context.GetString (Resource.String.errorCreatePassword));
			}
			if (string.IsNullOrEmpty (verify)) {
				rv = false;
				alert (Application.Context.GetString (Resource.String.errorCreateVerify));
			}
			if (password != verify) {
				rv = false;
				alert (Application.Context.GetString (Resource.String.errorCreateNoMatch));
			}
			if (!StringUtils.IsEmailAddress (email)) {
				rv = false;
				alert (Application.Context.GetString (Resource.String.errorCreateEmailInvalid));
			}
			return rv;
		}
		
		private void alert (string message)
		{
			RunOnUiThread (() => GeneralUtils.Alert (context, Application.Context.GetString (Resource.String.errorCreateTitle), message));
		}
		
		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			switch (requestCode) {
			case CAMMY:
				if (resultCode == Result.Ok) {
					string filename = data.GetStringExtra ("filename");
					if (!string.IsNullOrEmpty (filename)) {
						using (Bitmap bmp = BitmapFactory.DecodeFile(filename)) {
							if (bmp != null) {
								using (MemoryStream ms = new MemoryStream ()) {
									bmp.Compress (Bitmap.CompressFormat.Jpeg, 80, ms);
									if (userImage == null)
										userImage = new byte[ms.Length];
									userImage = ms.ToArray ();
									displayImage (userImage);
								}
							}
						}
						AndroidData.imageFileName = filename;
					}
				}
				break;
			case PICCY:
				if (resultCode == Result.Ok) {
					string filename = getRealPathFromUri (data.Data);
					using (Bitmap bmp = BitmapFactory.DecodeFile(filename)) {
						if (bmp != null) {
							using (MemoryStream ms = new MemoryStream ()) {
								bmp.Compress (Bitmap.CompressFormat.Jpeg, 80, ms);
								if (userImage == null)
									userImage = new byte[ms.Length];
								userImage = ms.ToArray ();
								displayImage (userImage);
							}
						}
					}
					AndroidData.imageFileName = filename;
				}
				break;
			case NEXT:
				username.Text = data.GetStringExtra ("username");
				email.Text = data.GetStringExtra ("email");
				password.Text = verify.Text = data.GetStringExtra ("password");
				userImage = data.GetByteArrayExtra ("image");
				displayImage (userImage);
				break;
			}
		}
		
		private void displayImage (byte[] image)
		{
			if (image.Length > 0) {
				using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)ImageHelper.convertDpToPixel(125f, context), 
				                                                                      (int)ImageHelper.convertDpToPixel(96f, context), this.Resources)) {
					RunOnUiThread (delegate {
						picture.SetImageBitmap (myBitmap);
					});
				}
			}
		}
		
		private string getRealPathFromUri (Android.Net.Uri contentUri)
		{
			string[] proj = { MediaStore.Images.ImageColumns.Data };
			ICursor cursor = this.ManagedQuery (contentUri, proj, null, null, null);
			int column_index = cursor.GetColumnIndexOrThrow (MediaStore.Images.ImageColumns.Data);
			cursor.MoveToFirst ();
			return cursor.GetString (column_index);
		}
	}
}


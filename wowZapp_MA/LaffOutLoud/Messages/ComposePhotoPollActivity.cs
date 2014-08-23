using System;
using System.IO;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Android.Speech;

using LOLMessageDelivery;
using LOLApp_Common;

using WZCommon;

namespace wowZapp.Messages
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class ComposePhotoPollActivity : Activity
	{
		private EditText txtPhotoPollMessage;
		private ImageView[] imgPollPhoto;
		private byte[] imgData1, imgData2, imgData3, imgData4;
		private Context c;
		private Dialog selectDialog, ModalPreviewDialog;
		private int elsewhere;
		private float[] newSizes;
		private ImageView[] images;
		private const int VOICE = 5;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ComposePhotoPoll);
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewLoginHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			c = header.Context;
			Header.headertext = Application.Context.Resources.GetString (Resource.String.pollPhotoTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (c);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			if (Photoalbums.PhotoPickerUtil.names == null)
				Photoalbums.PhotoPickerUtil.names = new List<string> ();

			elsewhere = base.Intent.GetIntExtra ("returned", 0);
			imgPollPhoto = new ImageView[4];
			txtPhotoPollMessage = FindViewById<EditText> (Resource.Id.txtPhotoPollMessage);
			
			if (wowZapp.LaffOutOut.Singleton.resizeFonts) {
				float fontSize = txtPhotoPollMessage.TextSize;
				txtPhotoPollMessage.SetTextSize (Android.Util.ComplexUnitType.Dip, ImageHelper.getNewFontSize (fontSize, c));
			}
			
			imgPollPhoto [0] = FindViewById<ImageView> (Resource.Id.imgPollPhoto1);
			imgPollPhoto [1] = FindViewById<ImageView> (Resource.Id.imgPollPhoto2);
			imgPollPhoto [2] = FindViewById<ImageView> (Resource.Id.imgPollPhoto3);
			imgPollPhoto [3] = FindViewById<ImageView> (Resource.Id.imgPollPhoto4);

			imgPollPhoto [0].Click += delegate {
				LaunchImagePicker (1);
			};
			imgPollPhoto [1].Click += delegate {
				LaunchImagePicker (2);
			};
			imgPollPhoto [2].Click += delegate {
				LaunchImagePicker (3);
			};
			imgPollPhoto [3].Click += delegate {
				LaunchImagePicker (4);
			};
				

			Button btnAdd = FindViewById<Button> (Resource.Id.btnAdd);
			btnAdd.Click += delegate {
				AddToPoll ();
			};
			
			ImageView recordVoice = FindViewById<ImageView> (Resource.Id.imgRecordPollMsg);
			string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
			if (rec != "android.hardware.microphone")
				recordVoice.SetBackgroundResource (Resource.Drawable.nomicrophone2);
			else
				recordVoice.Click += (object sender, EventArgs e) => voiceRecord (sender, e);
				
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Tag = 0;
			ImageButton btnHome = FindViewById<ImageButton> (Resource.Id.btnHome);
			btnHome.Tag = 1;
			
			LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			ImageButton[] buttons = new ImageButton[2];
			buttons [0] = btnBack;
			buttons [1] = btnHome;
			ImageHelper.setupButtonsPosition (buttons, bottom, c);
			
			if (elsewhere != 0)
				LaunchFromPhone ();
			
			newSizes = new float[4];
			newSizes [0] = newSizes [1] = newSizes [2] = newSizes [3] = 200f;
			cpcutil.pollIsDone = false;
			
			if (wowZapp.LaffOutOut.Singleton.resizeFonts) { 
				images = new ImageView[4];
				for (int n = 0; n < imgPollPhoto.Length; ++n)
					images [n] = imgPollPhoto [n];
				newSizes [0] *= wowZapp.LaffOutOut.Singleton.bigger;
				newSizes [1] = newSizes [2] = newSizes [3] = newSizes [0];
			}
			
			btnBack.Click += delegate {
				Finish ();
			};
			btnHome.Click += delegate {
				Intent i = new Intent (this, typeof(Main.HomeActivity));
				i.AddFlags (ActivityFlags.ClearTop);
				StartActivity (i);
			};
		}
		
		public override void OnWindowFocusChanged (bool hasFocus)
		{
			base.OnWindowFocusChanged (hasFocus);
			if (hasFocus && images != null && !cpcutil.pollIsDone) {
				cpcutil.pollIsDone = true;
				RunOnUiThread (() => ImageHelper.resizeWidget (images, c));
			}
		}
		
		private void voiceRecord (object s, EventArgs e)
		{
			Intent voiceIntent = new Intent (RecognizerIntent.ActionRecognizeSpeech);
			voiceIntent.PutExtra (RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
			voiceIntent.PutExtra (RecognizerIntent.ExtraPrompt, Application.Context.GetString (Resource.String.messageSpeakNow));
			voiceIntent.PutExtra (RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
			voiceIntent.PutExtra (RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
			voiceIntent.PutExtra (RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
			voiceIntent.PutExtra (RecognizerIntent.ExtraMaxResults, 1);
			voiceIntent.PutExtra (RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
			StartActivityForResult (voiceIntent, VOICE);
		}
		
		private void LaunchFromPhone ()
		{
			elsewhere = 0;
			switch (Photoalbums.PhotoPickerUtil.position) {
			case 0:
				using (Drawable draw = Drawable.CreateFromPath (Photoalbums.PhotoPickerUtil.names[0])) {
					imgPollPhoto [0].SetImageDrawable (draw);
					imgData1 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [0]);
				}
				break;
			case 1:
				using (Drawable draw = Drawable.CreateFromPath (Photoalbums.PhotoPickerUtil.names[1])) {
					imgPollPhoto [1].SetImageDrawable (draw);
					imgData2 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [1]);
				}
				break;
			case 2:
				using (Drawable draw = Drawable.CreateFromPath (Photoalbums.PhotoPickerUtil.names[2])) {
					imgPollPhoto [2].SetImageDrawable (draw);
					imgData3 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [2]);
				}
				break;
			case 3:
				using (Drawable draw = Drawable.CreateFromPath (Photoalbums.PhotoPickerUtil.names[3])) {
					imgPollPhoto [3].SetImageDrawable (draw);
					imgData4 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [3]);
				}
				break;
			}
				
			if (Photoalbums.PhotoPickerUtil.position != 0) {
				int n = 0;
				foreach (string filename in Photoalbums.PhotoPickerUtil.names) {
					using (Drawable draw = Drawable.CreateFromPath (filename)) {
						imgPollPhoto [n].SetImageDrawable (draw);
						if (n == 0)
							imgData1 = File.ReadAllBytes (filename);
						if (n == 1)
							imgData2 = File.ReadAllBytes (filename);
						if (n == 2)
							imgData3 = File.ReadAllBytes (filename);
						if (n == 3)
							imgData4 = File.ReadAllBytes (filename);	
						n++;
					}
				}
			}
		}

		private void LaunchImagePicker (int step)
		{
			selectDialog = new Dialog (this, Resource.Style.lightbox_dialog);
			selectDialog.SetContentView (Resource.Layout.ModalPoll);
			Photoalbums.PhotoPickerUtil.position = step - 1;
			((Button)selectDialog.FindViewById (Resource.Id.btnTop)).Click += delegate {
				DismissModalPreviewDialog ();
				ImagePicker (step);
			};
			
			((Button)selectDialog.FindViewById (Resource.Id.btnMiddle)).Click += delegate {
				DismissModalPreviewDialog ();
				Intent cameraIntent = new Intent (this, typeof(CameraVideo.CameraTakePictureActivity));
				StartActivityForResult (cameraIntent, step);
			};
			
			((Button)selectDialog.FindViewById (Resource.Id.btnBottom)).Click += delegate {
				DismissModalPreviewDialog ();
				ModalPreviewDialog = new Dialog (this, Resource.Style.lightbox_dialog);
				ModalPreviewDialog.SetContentView (Resource.Layout.ModalOnlineSelector);
				ModalPreviewDialog.Show ();
				((ImageView)ModalPreviewDialog.FindViewById (Resource.Id.imgItemPic)).Click += delegate {
					RunOnUiThread (() => DismissLightboxDialog ());
					Intent i = new Intent (this, typeof(Photoalbums.GetPhotoAlbumActivity));
					i.PutExtra ("media", false);
					i.PutExtra ("network", 3);
					Photoalbums.PhotoPickerUtil.fromWhere = 1;
					StartActivityForResult (i, step);
				};
			};
			
			((Button)selectDialog.FindViewById (Resource.Id.btnCancel)).Click += delegate {
				DismissModalPreviewDialog ();
			};
			
			selectDialog.Show ();
		}

		private void ImagePicker (int step)
		{
			var imageIntent = new Intent ();
			imageIntent.SetType ("image/*");
			imageIntent.SetAction (Intent.ActionGetContent);
			StartActivityForResult (Intent.CreateChooser (imageIntent, "Choose Image"), step);
		}

		private void DismissLightboxDialog ()
		{
			if (ModalPreviewDialog != null)
				ModalPreviewDialog.Dismiss ();
			
			ModalPreviewDialog = null;
		}

		private void DismissModalPreviewDialog ()
		{
			if (selectDialog != null)
				selectDialog.Dismiss ();
			
			selectDialog = null;
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			switch (requestCode) {
			case 1:
				if (resultCode == Result.Ok) {
					if (data.Data == null) {
						string filename = data.GetStringExtra ("filename");
						Photoalbums.PhotoPickerUtil.names.Insert (0, filename);
						using (Drawable draw = Drawable.CreateFromPath(filename)) {
							imgPollPhoto [0].SetImageDrawable (draw);
							imgData1 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [0]);
						}
					} else {
						Photoalbums.PhotoPickerUtil.names.Insert (0, getRealPathFromUri (data.Data));
						using (Drawable draw = Drawable.CreateFromPath (Photoalbums.PhotoPickerUtil.names[0])) {
							imgPollPhoto [0].SetImageDrawable (draw);
							imgData1 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [0]);
						}
					}
				}
				break;

			case 2:
				if (resultCode == Result.Ok) {
					if (data.Data == null) {
						string filename = data.GetStringExtra ("filename");
						Photoalbums.PhotoPickerUtil.names.Insert (1, filename);
						using (Drawable draw = Drawable.CreateFromPath(filename)) {
							imgPollPhoto [1].SetImageDrawable (draw);
							imgData2 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [1]);
						}
					} else {
						Photoalbums.PhotoPickerUtil.names.Insert (1, getRealPathFromUri (data.Data));
						using (Drawable draw = Drawable.CreateFromPath (Photoalbums.PhotoPickerUtil.names[1])) {
							imgPollPhoto [1].SetImageDrawable (draw);
							imgData2 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [1]);
						}
					}
				}
				break;

			case 3:
				if (resultCode == Result.Ok) {
					if (data.Data == null) {
						string filename = data.GetStringExtra ("filename");
						Photoalbums.PhotoPickerUtil.names.Insert (1, filename);
						using (Drawable draw = Drawable.CreateFromPath(filename)) {
							imgPollPhoto [2].SetImageDrawable (draw);
							imgData3 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [2]);
						}
					} else {
						Photoalbums.PhotoPickerUtil.names.Insert (2, getRealPathFromUri (data.Data));
						using (Drawable draw = Drawable.CreateFromPath (Photoalbums.PhotoPickerUtil.names[2])) {
							imgPollPhoto [2].SetImageDrawable (draw);
							imgData3 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [2]);
						}
					}
				}
				break;

			case 4:
				if (resultCode == Result.Ok) {
					if (data.Data == null) {
						string filename = data.GetStringExtra ("filename");
						Photoalbums.PhotoPickerUtil.names.Insert (1, filename);
						using (Drawable draw = Drawable.CreateFromPath(filename)) {
							imgPollPhoto [3].SetImageDrawable (draw);
							imgData4 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [3]);
						}
					} else {
						Photoalbums.PhotoPickerUtil.names.Insert (3, getRealPathFromUri (data.Data));
						using (Drawable draw = Drawable.CreateFromPath (Photoalbums.PhotoPickerUtil.names[3])) {
							imgPollPhoto [3].SetImageDrawable (draw);
							imgData4 = File.ReadAllBytes (Photoalbums.PhotoPickerUtil.names [3]);
						}
					}
				}
				break;
			case 5:
				if (resultCode == Result.Ok) {
					IList<String> matches = data.GetStringArrayListExtra (RecognizerIntent.ExtraResults);
					string textInput = matches [0].ToString ();
					if (textInput.Length > 200)
						textInput = textInput.Substring (0, 500);
					txtPhotoPollMessage.Text = textInput;
				}
				break;
			}
		}

		private void AddToPoll ()
		{
			MessageStepDB msgStep = new MessageStepDB ();
			int CurrentStep = base.Intent.GetIntExtra ("CurrentStep", 1);
			msgStep.StepType = MessageStep.StepTypes.Polling;

			PollingStep pollStep = new PollingStep ();
			pollStep.PollingQuestion = txtPhotoPollMessage.Text;
			pollStep.PollingData1 = imgData1;
			pollStep.PollingData2 = imgData2;
			pollStep.PollingData3 = imgData3;
			pollStep.PollingData4 = imgData4;

			if (CurrentStep > ComposeMessageMainUtil.msgSteps.Count) {
				msgStep.StepNumber = ComposeMessageMainUtil.msgSteps.Count + 1;
				ComposeMessageMainUtil.msgSteps.Add (msgStep);
			} else {
				msgStep.StepNumber = CurrentStep;
				ComposeMessageMainUtil.msgSteps [CurrentStep - 1] = msgStep;
			}

			pollStep.StepNumber = msgStep.StepNumber;

			if (ComposeMessageMainUtil.pollSteps != null) {
				ComposeMessageMainUtil.pollSteps [pollStep.StepNumber - 1] = pollStep;
			} else {
				ComposeMessageMainUtil.pollSteps = new PollingStep[6];
				ComposeMessageMainUtil.pollSteps [0] = new PollingStep ();
				ComposeMessageMainUtil.pollSteps [1] = new PollingStep ();
				ComposeMessageMainUtil.pollSteps [2] = new PollingStep ();
				ComposeMessageMainUtil.pollSteps [3] = new PollingStep ();
				ComposeMessageMainUtil.pollSteps [4] = new PollingStep ();
				ComposeMessageMainUtil.pollSteps [5] = new PollingStep ();
				ComposeMessageMainUtil.pollSteps [pollStep.StepNumber - 1] = pollStep;
			}

			if (CurrentStep == 1) {
				StartActivity (typeof(ComposeMessageMainActivity));
				Finish ();
			} else {
				Finish ();
			}
		}

		private string getRealPathFromUri (Android.Net.Uri contentUri)
		{
			string[] proj = { MediaStore.Images.ImageColumns.Data };
			string realpath = string.Empty;
			using (ICursor cursor = this.ManagedQuery (contentUri, proj, null, null, null)) {
				int column_index = cursor.GetColumnIndexOrThrow (MediaStore.Images.ImageColumns.Data);
				cursor.MoveToFirst ();
				realpath = cursor.GetString (column_index);
			}
			return realpath;
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
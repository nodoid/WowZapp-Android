using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;

namespace wowZapp.Messages
{
	public static class cpcutil
	{
		public static bool pollIsDone
		{ get; set; }
		public static bool genericIsDone
		{ get; set; }
		public static bool textIsDone
		{ get; set; }
		public static bool animationIsDone
		{ get; set; }
		public static bool audioIsDone
		{ get; set; }
		public static bool videoIsDone
		{ get; set; }
	}

	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class ComposePollChoiceActivity : Activity
	{
		private Context c;
		private ImageView[] images;
		private float[] newSizes;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ComposePollChoice);
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewLoginHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			c = header.Context;
			Header.headertext = Application.Context.Resources.GetString (Resource.String.pollCreateNewPoll);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (c);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			ImageView btnPhoto = FindViewById<ImageView> (Resource.Id.imgPhotoPoll);
			ImageView btnText = FindViewById<ImageView> (Resource.Id.imgTextPoll);
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Tag = 0;
			ImageButton btnHome = FindViewById<ImageButton> (Resource.Id.btnHome);
			btnHome.Tag = 1;
			LinearLayout bottomHolder = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			
			ImageButton[] buttons = new ImageButton[2];
			buttons [0] = btnBack;
			buttons [1] = btnHome;
			ImageHelper.setupButtonsPosition (buttons, bottomHolder, c);
			
			cpcutil.pollIsDone = false;
			newSizes = new float[2];
			newSizes [0] = newSizes [1] = 200f;
			
			if (wowZapp.LaffOutOut.Singleton.resizeFonts) { 
				images = new ImageView[2];
				images [0] = btnPhoto;
				images [1] = btnText;
				newSizes [0] *= wowZapp.LaffOutOut.Singleton.bigger;
				newSizes [1] = newSizes [0];
			}

			int step = base.Intent.GetIntExtra ("CurrentStep", 1);

			btnPhoto.Click += delegate {
				Intent i = new Intent (btnPhoto.Context, typeof(ComposePhotoPollActivity));
				i.PutExtra ("CurrentStep", step);
				btnPhoto.Context.StartActivity (i);
				Finish ();
			};

			btnText.Click += delegate {
				Intent i = new Intent (btnText.Context, typeof(ComposeTextPollActivity));
				i.PutExtra ("CurrentStep", step);
				btnText.Context.StartActivity (i);
				Finish ();
			};

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

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
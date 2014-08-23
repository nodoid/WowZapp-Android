using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;

using LOLApp_Common;

namespace wowZapp.Messages
{
	public static class AnimationEditUtil
	{
		public static Dictionary<int, FrameInfo> frameItems
{ get; set; }
		public static Dictionary<int, AnimationAudioInfo> audioItems
{ get; set; }
		public static List<UndoInfo> undoItems
{ get; set; }
	}

	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class ComposeAnimationActivity : Activity
	{
		private const int ANIMATION = 1;
		private ImageButton[] images;
		private Context context;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ComposeAnimationMessage);
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewloginHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			Header.headertext = Application.Context.Resources.GetString (Resource.String.animationTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;
			//cpcutil.animationIsDone = false;
			//ImageButton createAnimation = FindViewById<ImageButton> (Resource.Id.btnCreate);
			Button addContacts = FindViewById<Button> (Resource.Id.btnAdd);
			Button sendMessage = FindViewById<Button> (Resource.Id.btnSend);
			ImageButton back = FindViewById<ImageButton> (Resource.Id.btnBack);
			back.Tag = 0;
			ImageButton home = FindViewById<ImageButton> (Resource.Id.btnHome);
			home.Tag = 1;
			LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			context = bottom.Context;
			ImageButton [] buttons = new ImageButton[2];
			buttons [0] = back;
			buttons [1] = home;
			ImageHelper.setupButtonsPosition (buttons, bottom, header.Context);

			if (wowZapp.LaffOutOut.Singleton.resizeFonts) {
				images = new ImageButton[1];
				images [0] = createAnimation;
			}

			createAnimation.Click += delegate {
				Intent i = new Intent (this, typeof(Animations.CreateAnimationActivity));
				i.PutExtra ("CurrentStep", base.Intent.GetIntExtra ("CurrentStep", 1));
				StartActivityForResult (i, ANIMATION);
			};
			
			back.Click += delegate {
				Finish ();
			};
			home.Click += delegate {
				Intent i = new Intent (this, typeof(Main.HomeActivity));
				i.SetFlags (ActivityFlags.ClearTop);
				StartActivity (i);
			};
		}

		public override void OnWindowFocusChanged (bool hasFocus)
		{
			base.OnWindowFocusChanged (hasFocus);
			if (hasFocus && images != null && !cpcutil.animationIsDone) {
				cpcutil.genericIsDone = true;
				RunOnUiThread (() => ImageHelper.resizeWidget (images, context));
			}
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
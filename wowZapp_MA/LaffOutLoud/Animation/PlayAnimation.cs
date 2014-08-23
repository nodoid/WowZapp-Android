using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Webkit;

using LOLMessageDelivery.Classes.LOLAnimation;

namespace wowZapp.Animations
{
	[Activity (ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
	public class PlayAnimation : Activity
	{
		private WebView animationView;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.PlayAnimation);
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewLoginHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			Header.headertext = Application.Context.Resources.GetString (Resource.String.animationPlaybackTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;
			
			animationView = FindViewById<WebView> (Resource.Id.animationView);
			
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Tag = 0;
			btnBack.Click += delegate {
				Finish ();
			};
			ImageButton btnPlay = FindViewById<ImageButton> (Resource.Id.btnPlay);
			btnPlay.Tag = 1;
			LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			ImageButton[] buttons = new ImageButton[2];
			buttons [0] = btnBack;
			buttons [1] = btnPlay;
			ImageHelper.setupButtonsPosition (buttons, bottom, header.Context);
		}
		
		private void playAudio (int audioID)
		{
		
		}
	}
}


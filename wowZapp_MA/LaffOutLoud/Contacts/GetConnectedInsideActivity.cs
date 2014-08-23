using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace wowZapp.Contacts
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public partial class GetConnectedInsideActivity : Activity, IDisposable
	{
		private bool enableBackButton;
		private bool disableContinueButton;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.GetConnectedButtons);

			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			ImageView btns = FindViewById<ImageView> (Resource.Id.newGetConnectedImage);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			
			Header.headertext = Application.Context.Resources.GetString (Resource.String.getconnectedTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			enableBackButton = base.Intent.GetBooleanExtra ("backButton", false);
			disableContinueButton = base.Intent.GetBooleanExtra ("continueButton", false);
			ImageView Google = FindViewById<ImageView> (Resource.Id.imgGetConnectedGoogleFriends);
			ImageView FaceBook = FindViewById<ImageView> (Resource.Id.imgGetConnectedFacebook);
			ImageView LinkedIn = FindViewById<ImageView> (Resource.Id.imgGetConnectedLinkedIn);
			Button btnGetConnectedContinue = FindViewById<Button> (Resource.Id.btnGetConnectedContinue);
			Google.Click += delegate(object s, EventArgs e) {
				button_Click (s, e, 1);
			};
			LinkedIn.Click += delegate(object s, EventArgs e) {
				button_Click (s, e, 2);
			};
			FaceBook.Click += delegate(object s, EventArgs e) {
				button_Click (s, e, 3);
			};
			btnGetConnectedContinue.Click += new EventHandler (btnGetConnectedContinue_Click);
			
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Click += delegate {
				Finish ();
			};
		}

		private const int CHOOSE_FRIENDS = 1;

		private void button_Click (object sender, EventArgs e, int type)
		{
			Intent i = new Intent (this, typeof(GetConnectedActivity));
			i.PutExtra ("type", type);
			i.PutExtra ("back", enableBackButton);
			StartActivityForResult (i, CHOOSE_FRIENDS);
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			switch (requestCode) {
			case CHOOSE_FRIENDS:
				if (resultCode == Result.Ok)
					Finish ();
				break;
			}
		}

		private void btnGetConnectedContinue_Click (object s, EventArgs e)
		{
			Finish ();
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
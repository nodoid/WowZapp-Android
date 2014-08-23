using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;

namespace wowZapp.Messages
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class ViewContactMessages : Activity
	{
		TextView header;
		Context context;

		private const int ANIMATE = 1, CONTACTS = 2, CONTENT = 3;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ViewContactMessageLists);
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewUserHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			context = header.Context;
			Header.headertext = Application.Context.Resources.GetString (Resource.String.messageListHeaderViewTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);

			ImageButton animate = FindViewById<ImageButton> (Resource.Id.btnAnimate);
			animate.Tag = 0;
			ImageButton contacts = FindViewById<ImageButton> (Resource.Id.btnContacts);
			contacts.Tag = 1;
			ImageButton home = FindViewById<ImageButton> (Resource.Id.btnHome);
			home.Tag = 2;
			ImageButton addcontent = FindViewById<ImageButton> (Resource.Id.btnAddContent);
			addcontent.Tag = 3;
			
			LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			ImageButton[] buttons = new ImageButton[4];
			buttons [0] = animate;
			buttons [1] = contacts;
			buttons [2] = home;
			buttons [3] = addcontent;
			ImageHelper.setupButtonsPosition (buttons, bottom, context);

			contacts.Click += delegate {
				StartActivityForResult (typeof(Contacts.SelectContactsActivity), CONTACTS);
			};
			home.Click += delegate {
				Intent i = new Intent (this, typeof(Main.HomeActivity));
				i.SetFlags (ActivityFlags.ClearTop);
				StartActivity (i);
			};

			animate.Click += delegate {
				StartActivityForResult (typeof(Messages.ComposeAnimationActivity), ANIMATE);
			};

			addcontent.Click += delegate {
				StartActivityForResult (typeof(ComposeMessageChooseContent), CONTENT);
			};
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
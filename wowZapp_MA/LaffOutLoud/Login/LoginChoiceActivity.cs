using System;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;

namespace wowZapp.Login
{
    public static class LoginUtil
    {
        public static bool isDone
{ get; set; }
    }
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class LoginChoiceActivity : Activity
    {
        private Button[] buttons;
        private Context context;
	
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LoginFirstScreen);

            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            context = header.Context;
            Header.headertext = Application.Context.Resources.GetString(Resource.String.loginHeader);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
            Button btnNewUser = FindViewById<Button>(Resource.Id.btnloginnewUser);
            Button btnExistingUser = FindViewById<Button>(Resource.Id.btnloginexistingUser);
            LoginUtil.isDone = false;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
            {
                float font = ImageHelper.convertDpToPixel(30f, context);
                float xsize = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth / 400f; // scale
                font *= xsize;
			
                buttons = new Button[2];
                buttons [0] = btnNewUser;
                buttons [1] = btnExistingUser;
			
            }
            btnNewUser.Click += delegate(object s, EventArgs e)
            {
                AndroidData.user = WZCommon.UserType.NewUser;
                if (wowZapp.LaffOutOut.Singleton.ScreenYHeight > 800)
                    StartActivity(typeof(LoLRegisterActivity));
                else
                    StartActivity(typeof(LOLRegisterPhoneActivity));
            };
            btnExistingUser.Click += delegate(object s, EventArgs e)
            {
                AndroidData.user = WZCommon.UserType.ExistingUser;
                StartActivityForResult(typeof(LoLLoginActivity), 1); 
            };
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus == true && wowZapp.LaffOutOut.Singleton.resizeFonts && !LoginUtil.isDone)
            {
                LoginUtil.isDone = true;
                RunOnUiThread(() => ImageHelper.resizeWidget(buttons, context, Android.Views.GravityFlags.Center));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
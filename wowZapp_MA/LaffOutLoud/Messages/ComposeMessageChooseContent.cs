using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace wowZapp.Messages
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class ComposeMessageChooseContent : Activity
    {
        private Context c;
        private const int DUMMY = 1;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ComposeMessageContentSelect);

            LinearLayout linear = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            c = linear.Context;
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewloginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            Header.headertext = Application.Context.Resources.GetString(Resource.String.messageStartMessageTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(c);
            int step = base.Intent.GetIntExtra("CurrentStep", 1);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
            ImageButton startTextMessage = FindViewById<ImageButton>(Resource.Id.btnTextMessage);
            startTextMessage.Click += delegate
            {
                Intent i = new Intent(c, typeof(ComposeTextMessageActivity));
                i.PutExtra("CurrentStep", step);
                i.PutExtra("MessageType", 8);
                StartActivityForResult(i, DUMMY);
                //Finish ();
            };

            ImageButton startDrawMessage = FindViewById<ImageButton>(Resource.Id.btnDrawMessage);
#if DEBUG
            startDrawMessage.Click += delegate
            {
                Intent i = new Intent(c, typeof(ComposeGenericMessage));
                i.PutExtra("CurrentStep", step);
                i.PutExtra("MessageType", 7);
                StartActivityForResult(i, DUMMY);
            };
#endif

            ImageButton startComixMessage = FindViewById<ImageButton>(Resource.Id.btnComixMessage);
            startComixMessage.Click += delegate
            {
                Intent i = new Intent(c, typeof(ComposeGenericMessage));
                i.PutExtra("pack", 2);
                i.PutExtra("CurrentStep", step);
                i.PutExtra("MessageType", 1);
                StartActivityForResult(i, DUMMY);
                //Finish ();
            };

            ImageButton startComiconMessage = FindViewById<ImageButton>(Resource.Id.btnComiconMessage);
            startComiconMessage.Click += delegate
            {
                Intent i = new Intent(c, typeof(ComposeGenericMessage));
                i.PutExtra("pack", 1);
                i.PutExtra("CurrentStep", step);
                i.PutExtra("MessageType", 2);
                StartActivityForResult(i, DUMMY);
                //Finish ();
            };

            ImageButton startPollMessage = FindViewById<ImageButton>(Resource.Id.btnPollMessage);
            startPollMessage.Click += delegate
            {
                Intent i = new Intent(c, typeof(ComposePollChoiceActivity));
                i.PutExtra("CurrentStep", step);
                StartActivityForResult(i, DUMMY);
                //Finish ();
            };

            ImageButton startSFXMessage = FindViewById<ImageButton>(Resource.Id.btnSFXMessage);
            startSFXMessage.Click += delegate
            {
                Intent i = new Intent(c, typeof(ComposeGenericMessage));
                i.PutExtra("pack", 5);
                i.PutExtra("CurrentStep", step);
                i.PutExtra("MessageType", 5);
                StartActivityForResult(i, DUMMY);
                //Finish ();
            };

            ImageButton startEmoticonMessage = FindViewById<ImageButton>(Resource.Id.btnEmotMessage);
            startEmoticonMessage.Click += delegate
            {
                Intent i = new Intent(c, typeof(ComposeGenericMessage));
                i.PutExtra("pack", 3);
                i.PutExtra("CurrentStep", step);
                i.PutExtra("MessageType", 6);
                StartActivityForResult(i, DUMMY);
                //Finish ();
            };

            bool[] checkCameraRecord = new bool[3];
            checkCameraRecord = GeneralUtils.CamRecLong();

            ImageButton startVoiceMessage = FindViewById<ImageButton>(Resource.Id.btnVoiceMessage);
            if (checkCameraRecord [1] == false)
                startVoiceMessage.SetBackgroundResource(Resource.Drawable.nomicrophone2);
            else
                startVoiceMessage.Click += delegate
                {
                    Intent i = new Intent(c, typeof(ComposeGenericMessage));
                    i.PutExtra("CurrentStep", step);
                    i.PutExtra("MessageType", 3);
                    StartActivityForResult(i, DUMMY);
                    //Finish ();
                };

            ImageButton startVideoMessage = FindViewById<ImageButton>(Resource.Id.btnVideoMessage);
            if (checkCameraRecord [0] == false || checkCameraRecord [2] == false)
                startVideoMessage.SetBackgroundResource(Resource.Drawable.novideo2);
#if DEBUG
            else
                startVideoMessage.Click += delegate
                {
                    Intent i = new Intent(c, typeof(ComposeGenericMessage));
                    i.PutExtra("CurrentStep", step);
                    i.PutExtra("MessageType", 4);
                    StartActivityForResult(i, DUMMY);
                };
#endif

            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            btnBack.Click += delegate
            {
                Intent resultData = new Intent();
                resultData.PutExtra("back", true);
                SetResult(Result.Ok, resultData);
                Finish();
            };
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus == true)
            {
                if (Messages.MessageReceivedUtil.FromMessages && MessageReceivedUtil.FromMessagesDone)
                {
                    /*Intent t = new Intent (this, typeof(MessageReceivedActivity));
					t.PutExtra ("message", false);
					t.SetFlags (ActivityFlags.SingleTop);
					StartActivity (t);*/
                    Finish();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (ComposeGenericResults.success)
            {
                ComposeGenericResults.success = false;
                RunOnUiThread(() => Finish());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
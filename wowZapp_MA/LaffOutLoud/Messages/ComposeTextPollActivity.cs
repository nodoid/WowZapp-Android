using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Speech;

using System;
using System.Collections.Generic;

using LOLMessageDelivery;

namespace wowZapp.Messages
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class ComposeTextPollActivity : Activity
    {
        private EditText txtPollMessage, txtPollOption1, txtPollOption2, txtPollOption3, txtPollOption4;
        int VOICE = 0;
		
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ComposeTextPoll);

            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewLoginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            Context context = header.Context;
            Header.headertext = Application.Context.Resources.GetString(Resource.String.pollTextTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            txtPollMessage = FindViewById<EditText>(Resource.Id.txtPollMessage);
            txtPollOption1 = FindViewById<EditText>(Resource.Id.txtPollOption1);
            txtPollOption2 = FindViewById<EditText>(Resource.Id.txtPollOption2);
            txtPollOption3 = FindViewById<EditText>(Resource.Id.txtPollOption3);
            txtPollOption4 = FindViewById<EditText>(Resource.Id.txtPollOption4);
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
            {
                float fontSize = txtPollMessage.TextSize;
                txtPollMessage.SetTextSize(Android.Util.ComplexUnitType.Dip, ImageHelper.getNewFontSize(fontSize, context));
            }

            Button btnCreatePoll = FindViewById<Button>(Resource.Id.btnCreateButton);
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            btnBack.Tag = 0;
            ImageButton btnHome = FindViewById<ImageButton>(Resource.Id.btnHome);
            btnHome.Tag = 1;
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            ImageButton[] buttons = new ImageButton[2];
            buttons [0] = btnBack;
            buttons [1] = btnHome;
            ImageHelper.setupButtonsPosition(buttons, bottom, context);
            
            ImageView imgMicrophone = FindViewById<ImageView>(Resource.Id.imgMicrophone);
            
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
                imgMicrophone.SetImageResource(Resource.Drawable.nomicrophone2);
            else
                imgMicrophone.Click += new EventHandler(recordVoice);
            
            txtPollMessage.TextChanged += delegate
            {
                if (txtPollMessage.Text.Length > 200)
                    txtPollMessage.Text = txtPollMessage.Text.Substring(0, 200);
            };
			
            txtPollOption1.TextChanged += new System.EventHandler<Android.Text.TextChangedEventArgs>(limitText);
            txtPollOption2.TextChanged += new System.EventHandler<Android.Text.TextChangedEventArgs>(limitText);
            txtPollOption3.TextChanged += new System.EventHandler<Android.Text.TextChangedEventArgs>(limitText);
            txtPollOption4.TextChanged += new System.EventHandler<Android.Text.TextChangedEventArgs>(limitText);
			
            btnBack.Click += delegate
            {
                Finish();
            };
            btnHome.Click += delegate
            {
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.AddFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };

            btnCreatePoll.Click += delegate
            {
                CreateTextPoll();
            };
        }

        private void limitText(object s, Android.Text.TextChangedEventArgs e)
        {
            EditText et = (EditText)s;
            if (et.Text.Length > 100)
                et.Text = et.Text.Substring(0, 100);
        }
		
        private void recordVoice(object s, EventArgs e)
        {
            Intent voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            StartActivityForResult(voiceIntent, VOICE);
        }
		
        protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultVal == Result.Ok)
                {
                    IList<String> matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    string textInput = matches [0].ToString();
                    if (textInput.Length > 200)
                        textInput = textInput.Substring(0, 200);
                    txtPollMessage.Text = textInput;
                }
            }
			
            base.OnActivityResult(requestCode, resultVal, data);
        }

        private void CreateTextPoll()
        {

            MessageStep msgStep = new MessageStep();
            int CurrentStep = base.Intent.GetIntExtra("CurrentStep", 1);
            msgStep.StepType = MessageStep.StepTypes.Polling;

            PollingStep pollStep = new PollingStep();
            pollStep.PollingQuestion = txtPollMessage.Text;
            pollStep.PollingAnswer1 = txtPollOption1.Text;
            pollStep.PollingAnswer2 = txtPollOption2.Text;
            pollStep.PollingAnswer3 = txtPollOption3.Text;
            pollStep.PollingAnswer4 = txtPollOption4.Text;

            if (CurrentStep > ComposeMessageMainUtil.msgSteps.Count)
            {
                msgStep.StepNumber = ComposeMessageMainUtil.msgSteps.Count + 1;
                ComposeMessageMainUtil.msgSteps.Add(msgStep);
            } else
            {
                msgStep.StepNumber = CurrentStep;
                ComposeMessageMainUtil.msgSteps [CurrentStep - 1] = msgStep;
            }

            pollStep.StepNumber = msgStep.StepNumber;

            if (ComposeMessageMainUtil.pollSteps != null)
            {
                ComposeMessageMainUtil.pollSteps [pollStep.StepNumber - 1] = pollStep;
            } else
            {
                ComposeMessageMainUtil.pollSteps = new PollingStep[6];
                ComposeMessageMainUtil.pollSteps [0] = new PollingStep();
                ComposeMessageMainUtil.pollSteps [1] = new PollingStep();
                ComposeMessageMainUtil.pollSteps [2] = new PollingStep();
                ComposeMessageMainUtil.pollSteps [3] = new PollingStep();
                ComposeMessageMainUtil.pollSteps [4] = new PollingStep();
                ComposeMessageMainUtil.pollSteps [5] = new PollingStep();
                ComposeMessageMainUtil.pollSteps [pollStep.StepNumber - 1] = pollStep;
            }

            //if (CurrentStep == 1) {
            StartActivity(typeof(ComposeMessageMainActivity));
            Finish();
            //} else {
            //	Finish ();
            //}
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
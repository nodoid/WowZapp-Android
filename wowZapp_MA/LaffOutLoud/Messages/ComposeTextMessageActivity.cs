using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LOLApp_Common;
using LOLMessageDelivery;
using Android.Speech;

using WZCommon;

namespace wowZapp.Messages
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class ComposeTextMessageActivity : Activity
	{
		int VOICE = 0;
		private Context context;
		private EditText text;
		private DBManager dbm;
		private MessageManager mmg;
		private int CurrentStepInc;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ComposeTextMessageScreen);
			string passedIn = base.Intent.GetStringExtra ("text");
			
			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewloginHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			context = header.Context;
			Header.headertext = Application.Context.Resources.GetString (Resource.String.sendMessageTextTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			dbm = wowZapp.LaffOutOut.Singleton.dbm;
			mmg = wowZapp.LaffOutOut.Singleton.mmg;
			CurrentStepInc = base.Intent.GetIntExtra ("CurrentStep", 0); 

			Button addContent = FindViewById<Button> (Resource.Id.btnAddContent);
			Button sendMessage = FindViewById<Button> (Resource.Id.btnMessageSend);
			ImageView useVoice = FindViewById<ImageView> (Resource.Id.imgVoice);
			text = FindViewById<EditText> (Resource.Id.editTextMessage);
			
			if (wowZapp.LaffOutOut.Singleton.resizeFonts) {
				float fSize = text.TextSize;
				text.SetTextSize (Android.Util.ComplexUnitType.Dip, ImageHelper.getNewFontSize (fSize, context));
			}
			
			if (!string.IsNullOrEmpty (passedIn))
				text.Text = passedIn;
				
			string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
			if (rec != "android.hardware.microphone")
				useVoice.SetBackgroundResource (Resource.Drawable.nomicrophone2);
			else
				useVoice.Click += new EventHandler (recordVoice);

			text.TextChanged += delegate {
				if (text.Text.Length > 500)
					text.Text = text.Text.Substring (0, 500);
			};

			sendMessage.Click += delegate {
				SendTextMessage ();
			};

			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Click += delegate {
				Finish ();
			};

			addContent.Click += delegate {
				if (!string.IsNullOrEmpty (text.Text)) {
					MessageStep msgStep = new MessageStep ();
					msgStep.MessageText = text.Text;
					msgStep.StepType = MessageStep.StepTypes.Text;

					if (CurrentStepInc > ComposeMessageMainUtil.msgSteps.Count) {
						msgStep.StepNumber = ComposeMessageMainUtil.msgSteps.Count + 1;
						ComposeMessageMainUtil.msgSteps.Add (msgStep);
					} else {
						msgStep.StepNumber = CurrentStepInc;
						ComposeMessageMainUtil.msgSteps [CurrentStepInc - 1] = msgStep;
					}

					if (CurrentStepInc == 1) {
						StartActivity (typeof(ComposeMessageMainActivity));
						Finish ();
					} else {
						Finish ();
					}
					ComposeGenericResults.success = true;
				} else
					Finish ();
			};
		}

		private void SendTextMessage ()
		{
			List<MessageStep> msgSteps = new List<MessageStep> ();

			MessageStepDB msgStep = new MessageStepDB ();
			msgStep.MessageText = text.Text;
			msgStep.StepNumber = 1;
			msgStep.StepType = MessageStep.StepTypes.Text;
			//msgSteps.Add (msgStep);

			List<Guid> toAccounts = new List<Guid> ();
			
			if (Contacts.SelectContactsUtil.selectedUserContacts == null) {
				foreach (ContactDB toAccount in Contacts.SelectContactsUtil.selectedContacts) {
					toAccounts.Add (toAccount.ContactUser.AccountID);
				}
			} else {
				foreach (UserDB toAccount in Contacts.SelectContactsUtil.selectedUserContacts)
					toAccounts.Add (toAccount.AccountID);
				Contacts.SelectContactsUtil.selectedUserContacts.Clear ();
			}
			MessageDB message = new MessageDB ();
			message.FromAccountID = AndroidData.CurrentUser.AccountID;
			message.MessageStepDBList = new List<MessageStepDB> (){msgStep};
			ContentInfo contentInfo = new ContentInfo (MessageDB.ConvertFromMessageDB (message), toAccounts);
			mmg.QueueMessage (contentInfo, true, context);
			if (Messages.MessageReceivedUtil.FromMessages)
				Messages.MessageReceivedUtil.FromMessagesDone = true;
				
			Finish ();
		}

		private void recordVoice (object s, EventArgs e)
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

		protected override void OnActivityResult (int requestCode, Result resultVal, Intent data)
		{
			if (requestCode == VOICE) {
				if (resultVal == Result.Ok) {
					IList<String> matches = data.GetStringArrayListExtra (RecognizerIntent.ExtraResults);
					string textInput = text.Text + matches [0].ToString ();
					if (textInput.Length > 500)
						textInput = textInput.Substring (0, 500);
					text.Text = textInput;
				}
			}

			base.OnActivityResult (requestCode, resultVal, data);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
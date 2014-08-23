using System;
using System.Reflection;
using System.Collections.Generic;
using LOLAccountManagement;
using LOLApp_Common;
using LOLMessageDelivery;
using System.Threading;
using System.IO;
using System.Linq;

using Android.Content;
using Android.App;
using Android.OS;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using Android.Views;

using Android.Webkit;
using WZCommon;

namespace wowZapp.Messages
{
	public static class MessagePlaybackUtil
	{
		public static List<MessageStepDB> messageSteps
		{ get; set; }
		public static Dictionary<int, ContentPackItem> contentPackItems
		{ get; set; }
		public static Dictionary<int, string> voiceRecordings
		{ get; set; }
		public static Dictionary<int, byte[]> voiceRecordingData
        { get; set; }
		public static Dictionary<int, PollingStep> pollSteps
		{ get; set; }
		public static Dictionary<int, LOLMessageSurveyResult> pollResults
		{ get; set; }
		public static bool isContentInfo
        { get; set; }
		public static bool markAsRead
		{ get; set; }
		public static Context context
        { get; set; }
		public static Dictionary<MessageStep.StepTypes, int> stepIterationsForTypes
        { get; set; }
		public static List<UserDB> recipients
        { get; set; }
		/*public static Dictionary<int, Animate> animation
        { get; set; }*/
	}

	public static class MessagePollUtil
	{
		public static PollingScreenType pollScreenType
        { get; set; }
		public static List<UserDB> recipients
        { get; set; }
		public static MessageStepDB messageStep
        { get; set; }
		public static PollingStep pollStep
        { get; set; }
		public static Context context
        { get; set; }
		public static bool isPhoto
        { get; set; }
		public static string messageText
        { get; set; }
	}


	public class MessagePlaybackController
	{
		private void setupTypes (Context context)
		{
			MessagePlaybackUtil.context = context;
			MessagePlaybackUtil.stepIterationsForTypes = new Dictionary<MessageStep.StepTypes,int> () {
                { MessageStep.StepTypes.Animation, 5 }, { MessageStep.StepTypes.Comicon, 5 },
                { MessageStep.StepTypes.Polling, 5 }, { MessageStep.StepTypes.SoundFX, 5 },
                { MessageStep.StepTypes.Text, 5 },
                { MessageStep.StepTypes.Video, 5 }, { MessageStep.StepTypes.Voice, 5 },
                { MessageStep.StepTypes.Comix, 3 }, { MessageStep.StepTypes.Emoticon, 3 }
            };
			MessagePlaybackUtil.isContentInfo = false;
		}

		public MessagePlaybackController (List<MessageStepDB> msgSteps, 
										  Dictionary<int, ContentPackItem> contentPackItems, 
										  Dictionary<int, string> voiceRecordings, 
										  Dictionary<int, PollingStep> pollSteps,
										  Dictionary<int, LOLMessageSurveyResult> pollResults, bool markAsRead, List<UserDB>recipients, Context context)
		{
			MessagePlaybackUtil.messageSteps = msgSteps;
			MessagePlaybackUtil.contentPackItems = contentPackItems;
			MessagePlaybackUtil.voiceRecordings = voiceRecordings;
			MessagePlaybackUtil.pollSteps = pollSteps;
			MessagePlaybackUtil.pollResults = pollResults;
			MessagePlaybackUtil.markAsRead = markAsRead;
			MessagePlaybackUtil.recipients = recipients;
			setupTypes (context);
			Intent i = new Intent (context, typeof(MessagePlayback));
			context.StartActivity (i);
		}

		public MessagePlaybackController (List<MessageStepDB> msgSteps, 
										  Dictionary<int, ContentPackItem> contentPackItems, 
										  Dictionary<int, string> voiceRecordings, 
										  bool markAsRead, Context context)
		{
			setupTypes (context);
			MessagePlaybackUtil.messageSteps = msgSteps;
			MessagePlaybackUtil.contentPackItems = contentPackItems;
			MessagePlaybackUtil.voiceRecordings = voiceRecordings;
			MessagePlaybackUtil.markAsRead = markAsRead;
			Intent i = new Intent (context, typeof(MessagePlayback));
			context.StartActivity (i);
		}

		public MessagePlaybackController (ContentInfo contentInfo, Dictionary<int, ContentPackItem> contentPackItem, Context c)
		{
			MessagePlaybackUtil.voiceRecordingData = contentInfo.VoiceRecordings;
			MessagePlaybackUtil.messageSteps = MessageDB.ConvertFromMessage (contentInfo.Message).MessageStepDBList;
			MessagePlaybackUtil.contentPackItems = contentPackItem;
			MessagePlaybackUtil.pollSteps = contentInfo.PollingSteps;
			setupTypes (c);
			Intent i = new Intent (c, typeof(MessagePlayback));
			c.StartActivity (i);
		}
	}

	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal", NoHistory=true)]
	public partial class MessagePlayback : Activity
	{
		private LinearLayout linView;
		private ViewGroup parent;
		private ImageView preview;
		private TextView title;
		private Context context;
		private ProgressBar progress;
		private System.Timers.Timer t;
		private Dictionary<int, View> stepViews;
		private bool done, isPlaying;
		private int counter, co, increments;
		private string from, pJSTemplate;
		private DBManager dbm;
		private Guid stepID;
		private ProgressBar voteTL, voteTR, voteBL, voteBR;
		private ProgressBar res1, res2, res3, res4;

		const int TEXT_POLL_Q = 1, PIC_POLL_Q = 2, TEXT_POLL_A = 10, PIC_POLL_A = 11;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			Window.AddFlags (WindowManagerFlags.KeepScreenOn);
			SetContentView (Resource.Layout.previewSoundFX);
        
			if (MessageReceivedUtil.userFrom != null) {
				UserDB user = UserDB.ConvertFromUser (MessageReceivedUtil.userFrom);
				Header.headertext = user.FirstName + " " + user.LastName;
				MessageReceivedUtil.userFrom = null;
			} else {
				if (MessagePlaybackUtil.recipients != null) {
					for (int i = 0; i < MessagePlaybackUtil.recipients.Count; ++i) {
						if (MessagePlaybackUtil.recipients [i] != null) {
							Header.headertext = MessagePlaybackUtil.recipients [i].FirstName + " " + MessagePlaybackUtil.recipients [i].LastName;
							break;
						}
					}
					//}
				} else
					Header.headertext = "Ann Onymouse";
			}
			linView = FindViewById<LinearLayout> (Resource.Id.linearHolder);
			context = linView.Context;

			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewUserHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			parent = linView;
			//preview = FindViewById<ImageView> (Resource.Id.imgComicon);
			progress = FindViewById<ProgressBar> (Resource.Id.prgPreview);
			co = base.Intent.GetIntExtra ("position", 0);
			counter = MessagePlaybackUtil.messageSteps.Count;
			dbm = wowZapp.LaffOutOut.Singleton.dbm;
			isPlaying = false;

			stepID = Guid.Empty;

#if DEBUG
			System.Diagnostics.Debug.WriteLine ("number of steps = {0}", counter);
#endif
			linView.RemoveAllViewsInLayout ();
			t = new System.Timers.Timer ();
			t.Interval = 2500;
			t.Elapsed += new System.Timers.ElapsedEventHandler (t_Elapsed);

			increments = 100 / (counter + 1);
			
			if (co != 0) {
				RunOnUiThread (() => progress.Progress = co * increments); 
				increments *= co + co;
			}

			if (MessagePlaybackUtil.markAsRead) {
				ThreadPool.QueueUserWorkItem (delegate {
					Guid messageID = MessagePlaybackUtil.messageSteps [0].MessageID;

					LOLMessageClient service = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
					service.MessageMarkReadCompleted += Service_MessageMarkReadCompleted;
					service.MessageMarkReadAsync (messageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, new Guid (AndroidData.ServiceAuthToken), messageID);
				});
			}

			ImageButton btnPreAdd = FindViewById<ImageButton> (Resource.Id.imgAdd);
			btnPreAdd.Tag = 1;
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Tag = 0;
			btnBack.Click += delegate {
				Window.ClearFlags (WindowManagerFlags.KeepScreenOn);
				Finish ();
			};
			LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			ImageButton[] buttons = new ImageButton[2];
			buttons [0] = btnBack;
			buttons [1] = btnPreAdd;
			ImageHelper.setupButtonsPosition (buttons, bottom, context);
						
			if (MessageReceivedUtil.readOnly)
				btnPreAdd.Visibility = ViewStates.Invisible;
			else
				btnPreAdd.Click += delegate {
					StartActivity (typeof(ComposeMessageChooseContent));
				};
#if DEBUG
			int m = 0;
			foreach (MessageStep eachMessageStep in MessagePlaybackUtil.messageSteps)
				System.Diagnostics.Debug.WriteLine ("step {0} = {1}", m++, eachMessageStep.StepType.ToString ());
#endif
			
			RunOnUiThread (delegate {
				PrepareViews (co);
			});
		}

		private void t_Elapsed (object s, System.Timers.ElapsedEventArgs e)
		{
			if (co < counter - 1) {
				RunOnUiThread (delegate {
					linView.RemoveAllViewsInLayout ();
					progress.Progress = increments;
					increments += increments;
					co++;
					if (t.Interval != 2500)
						t.Interval = 2500;
					if (co >= counter) {
						RunOnUiThread (delegate {
							t.Stop ();
							Window.ClearFlags (WindowManagerFlags.KeepScreenOn);
							Finish ();
						});
					} else
						PrepareViews (co);
				});
			} else {
				RunOnUiThread (delegate {
					t.Stop ();
					Window.ClearFlags (WindowManagerFlags.KeepScreenOn);
					Finish ();
				});
			}
		}

		private void disableAudio ()
		{
			progress.Visibility = ViewStates.Invisible;
		}

		private void Service_MessageMarkReadCompleted (object sender, MessageMarkReadCompletedEventArgs e)
		{
			LOLMessageClient service = (LOLMessageClient)sender;
			service.MessageMarkReadCompleted -= Service_MessageMarkReadCompleted;

			if (null == e.Error) {

				if (e.Result.ErrorNumber == "0" || string.IsNullOrEmpty (e.Result.ErrorNumber)) {
					Guid messageID = (Guid)e.UserState;
#if(DEBUG)
					System.Diagnostics.Debug.WriteLine ("Marked message as read!");
#endif
					dbm.MarkMessageRead (messageID.ToString (), AndroidData.CurrentUser.AccountID.ToString ());
				}//end if
			} else {
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Exception in message mark as read! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
			}//end if else
		}

		private byte[] getLocalVoiceRecording (string filename)
		{
			//string dataFile = System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ContentDirectory, filename);
			if (!File.Exists (filename)) {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("File {0} doesn't exist", filename);
#endif
				return new byte[0];
			}
			bool rv = false;
			byte[] dataBuffer = null;
			RunOnUiThread (delegate {
				dataBuffer = File.ReadAllBytes (filename);
				rv = dataBuffer == null ? true : false;
			});
			return rv == true ? new byte[0] : dataBuffer;
		}

		private void restartTimerFromPoll ()
		{
			Intent i = new Intent (this, typeof(MessagePlayback));
			i.PutExtra ("position", co + 1);
			StartActivity (i);
			Finish ();
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			switch (requestCode) {
			case TEXT_POLL_A:
			case TEXT_POLL_Q:
			case PIC_POLL_A:
			case PIC_POLL_Q:
				if (resultCode == Result.Ok) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("co = {0}", co);
#endif
					restartTimerFromPoll ();
				}
				break;
			}
		}

		public string JSTemplate {
			get {
				
				if (string.IsNullOrEmpty (this.pJSTemplate)) {
					try {
						Assembly ass;
						try {
							ass = Assembly.GetExecutingAssembly ();
							using (StreamReader sr = new StreamReader (ass.GetManifestResourceStream ("wowZapp.animlib.JavaScript.js"))) {
								pJSTemplate = sr.ReadToEnd ();
							}
						} catch {
							// error occured, do nothing and feel bad.
							return "";
						}
					} catch (FileNotFoundException) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Failed to load the animlib JS file (missing)");
						#endif
					} catch (IOException) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Failed to load the animlib JS file (io error");
						#endif
					}
				}//end if
				
				return this.pJSTemplate;
			}
			
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
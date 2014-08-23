using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Views;

using LOLApp_Common;
using LOLMessageDelivery;

using WZCommon;

namespace wowZapp.Messages
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class ComposeAudioMessageActivity : Activity
    {
        ProgressBar progress;
        private DBManager dbm;
        private MessageManager mmg;
        private int time, up, rectime, CurrentStepInc;
        private Context context;
        private AudioRecorder ar;
        private AudioPlayer ap;
        private string path;
        private bool isRecording, isPlayback;
        private System.Timers.Timer timer;
        private string filename;
        public EventHandler OnGlobalReceived;
        ImageButton btnRecord, btnPlay;
        ImageButton[] images;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.CreateAudioMessage);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewLoginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            Header.headertext = Application.Context.Resources.GetString(Resource.String.audioTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
            CurrentStepInc = base.Intent.GetIntExtra("CurrentStep", 1); 
            isPlayback = base.Intent.GetBooleanExtra("playback", false);
            filename = base.Intent.GetStringExtra("filename");
            time = 15;
            up = 0;
            btnRecord = FindViewById<ImageButton>(Resource.Id.btnRecord);
            btnPlay = FindViewById<ImageButton>(Resource.Id.btnPlay);
            Button btnSend = FindViewById<Button>(Resource.Id.btnSend);
            Button btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
            
            progress = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            context = progress.Context;
            mmg = wowZapp.LaffOutOut.Singleton.mmg;
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            isRecording = false;

            string filename2 = Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "voice_msg_");
            filename2 += CurrentStepInc.ToString() + ".3gp";
            path = string.Format(filename2, CurrentStepInc);
			
            if (string.IsNullOrEmpty(filename))
                filename = path;

            if (File.Exists(filename))
                File.Delete(filename);

            ar = new AudioRecorder(filename);
            ap = new AudioPlayer(context);

#if DEBUG
            System.Diagnostics.Debug.WriteLine("Filename audio being saved as = {0}", filename);
#endif

            btnRecord.Click += new EventHandler(btnRecord_Click);
            
            btnSend.Click += delegate
            {
                SendAudioMessage();
            };

            btnAdd.Click += delegate
            {
                AddToMessages();
            };

            dbm = wowZapp.LaffOutOut.Singleton.dbm;

            if (isPlayback == true)
            {
                btnRecord.Visibility = ViewStates.Gone;
                btnPlay.Click += new EventHandler(btnPlay_Click);
            } else
            {
                btnPlay.Click += new EventHandler(btnStop_Click);
                btnPlay.SetBackgroundResource(Resource.Drawable.stopbutton);
            }

            rectime = 16;
			
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            btnBack.Tag = 0;
            ImageButton btnHome = FindViewById<ImageButton>(Resource.Id.btnHome);
            btnHome.Tag = 1;
			
            btnBack.Click += delegate
            {
                ar.cancelOut();
                Finish();
            };
            btnHome.Click += delegate
            {
                ar.cancelOut();
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.AddFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };
			
            ImageButton[] buttons = new ImageButton[2];
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            buttons [0] = btnBack;
            buttons [1] = btnHome;
            ImageHelper.setupButtonsPosition(buttons, bottom, context);
        }

        private void timer_Elapsed(object s, System.Timers.ElapsedEventArgs e)
        {
            time--;
            up++;
            int user = rectime == 16 ? 16 : rectime;
            if (up == user)
            {
                if (isRecording)
                    ar.RecordStop();
                isRecording = false;
                timer.Stop();
                RunOnUiThread(delegate
                {
                    if (!isPlayback)
                    {
                        btnPlay.SetBackgroundResource(Resource.Drawable.playred);
                        btnPlay.Click -= btnStop_Click;
                        btnPlay.Click += new EventHandler(btnPlay_Click);
                    } else
                    {
                        btnPlay.SetBackgroundResource(Resource.Drawable.stopbutton);
                        btnPlay.Click -= btnPlay_Click;
                        btnPlay.Click += btnStop_Click;
                    }
                    up = 0;
                    rectime = 0;
                    progress.Progress = up;
                });
            } else
            {
                progress.Progress = up;
                if (rectime <= 15)
                    rectime++;
            }
        }

        private void btnStop_Click(object s, EventArgs e)
        {
            if (isRecording == true)
            {
                ar.RecordStop();
                timer.Stop();
                isRecording = false;
                //RunOnUiThread (delegate {
                btnPlay.SetBackgroundResource(Resource.Drawable.playred);
                btnPlay.Click -= btnStop_Click;
                btnPlay.Click += new EventHandler(btnPlay_Click);
                up = 0;
                progress.Progress = up;
                //});
            } else
            {
                ap.stopPlayer();
                timer.Stop();
                isPlayback = false;
                RunOnUiThread(() => progress.Progress = 0);
            }
        }

        private void btnRecord_Click(object s, EventArgs e)
        {
            if (isRecording == false)
            {
                ar.RecordStart();
                timer.Start();
                isRecording = true;
            }
        }

        private void btnPlay_Click(object s, EventArgs e)
        {
            if (isRecording == false)
            {
                timer.Start();
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("off to play the file - filename being passed = {0}, ap = {1}", filename, ap);
                #endif
                ap.playFromFile(filename);
                isPlayback = true;
            }
        }

        private void AddToMessages()
        {
            ar.EndRecording();
            MessageStep msgStep = new MessageStep();
            msgStep.StepType = MessageStep.StepTypes.Voice;
			
            if (CurrentStepInc > ComposeMessageMainUtil.msgSteps.Count)
            {
                msgStep.StepNumber = ComposeMessageMainUtil.msgSteps.Count + 1;
                ComposeMessageMainUtil.msgSteps.Add(msgStep);
            } else
            {
                msgStep.StepNumber = CurrentStepInc;
                ComposeMessageMainUtil.msgSteps [CurrentStepInc - 1] = msgStep;
            }
			
            if (CurrentStepInc == 1)
            {
                StartActivity(typeof(ComposeMessageMainActivity));
                Finish();
            } else
            {
                Finish();
            }
        }

        private void SendAudioMessage()
        {
            ar.EndRecording();
            MessageStepDB messageStep = new MessageStepDB();
            messageStep.StepNumber = 1;
            messageStep.StepType = MessageStep.StepTypes.Voice;

            MessageDB message = new MessageDB();
            message.FromAccountID = AndroidData.CurrentUser.AccountID;
            message.MessageStepDBList = new List<MessageStepDB>() { messageStep };

            List<Guid> recipientGuids = new List<Guid>();
            recipientGuids = Contacts.SelectContactsUtil.selectedContacts.Select(s => s.ContactAccountID).ToList();
			
            ContentInfo contentInfo = new ContentInfo(MessageDB.ConvertFromMessageDB(message), recipientGuids);

            try
            {
                byte[] audioFile = File.ReadAllBytes(filename);
                contentInfo.VoiceRecordings.Add(messageStep.StepNumber, audioFile);
#if DEBUG
                System.Diagnostics.Debug.WriteLine("audioFile.Length = {0}", audioFile.Length);
#endif
            } catch (IOException e)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Exception thrown (audioFile read) : {0} - {1}", e.Message, e.StackTrace);
#endif
                return;
            }

#if DEBUG
            File.Delete(Android.OS.Environment.ExternalStorageDirectory + "/wz/audio.3gp");
            File.Copy(path, Android.OS.Environment.ExternalStorageDirectory + "/wz/audio.3gp", true);
#endif

            RunOnUiThread(delegate
            {
                mmg.QueueMessage(contentInfo, false, context);
				
                if (null != wowZapp.LaffOutOut.Singleton.OnGlobalReceived)
                    wowZapp.LaffOutOut.Singleton.OnGlobalReceived(this, new MessageStepCreatedEventArgs(messageStep, null, path, null, null));
					
                Finish();
            });
        }
		
        private string GetVoiceRecordingFilename(Guid msgID, int stepNumber)
        {
            return Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, string.Format(LOLConstants.VoiceRecordingFormat, msgID.ToString(), stepNumber));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Webkit;

using LOLMessageDelivery;
using LOLAccountManagement;
using LOLApp_Common;
using LOLMessageDelivery.Classes.LOLAnimation;

using WZCommon;

namespace wowZapp.Messages
{
    public partial class MessagePlayback
    {
        private Button doner;
	
        private void PrepareViews(int co)
        {
            if (co >= counter || done)
                RunOnUiThread(delegate
                {
                    t.Stop();
                    Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
                    Finish();
                });
				
            if (!done)
                t.Start();	
            float[] newSizes = new float[2];
            newSizes [0] = 200f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
            { 
                newSizes [0] *= wowZapp.LaffOutOut.Singleton.bigger;
                newSizes [1] = newSizes [0];
            }
            this.stepViews = new Dictionary<int, View>();
            string path = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "tempaudio.wav");
            string videoPath = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "tempvideo.mp4");
			
#if DEBUG
            System.Diagnostics.Debug.WriteLine("co = {0}, counter = {1}", co, counter);
#endif
            MessageStep eachMessageStep = null;
            if (co != counter)
            {
                eachMessageStep = MessagePlaybackUtil.messageSteps [co];
			
                ContentPackItem packItem = null;
                MessagePlaybackUtil.contentPackItems.TryGetValue(eachMessageStep.ContentPackItemID, out packItem);
			
#if DEBUG
                System.Diagnostics.Debug.WriteLine("co = {0}, messagestep type = {1}, contentPackID = {2}, packItem = {3}", co, eachMessageStep, eachMessageStep.ContentPackItemID, packItem != null ? "yes" : "no");
#endif
			
                switch (eachMessageStep.StepType)
                {
                    case MessageStep.StepTypes.Text:
                        RunOnUiThread(delegate
                        {
                            using (TextView textMessage = new TextView (context))
                            {
                                textMessage.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(250f, context), (int)ImageHelper.convertDpToPixel(150f, context));
                                textMessage.SetBackgroundResource(Resource.Drawable.bubblesolidleft);
                                textMessage.Gravity = GravityFlags.Top;
                                textMessage.SetPadding((int)ImageHelper.convertDpToPixel(20f, context), (int)ImageHelper.convertDpToPixel(15f, context),
						                        (int)ImageHelper.convertDpToPixel(20f, context), (int)ImageHelper.convertDpToPixel(15f, context));
                                textMessage.SetTextColor(Color.Black);
                                textMessage.SetTextSize(Android.Util.ComplexUnitType.Pt, 12f);
                                textMessage.Text = MessagePlaybackUtil.messageSteps [co].MessageText;
                                textMessage.Visibility = ViewStates.Visible;
                                linView.AddView(textMessage);
                                this.stepViews [eachMessageStep.StepNumber] = textMessage;
                            }
                        });
                        break;
				
                    case MessageStep.StepTypes.Comix:
                        if (packItem != null)
                        {
                            RunOnUiThread(delegate
                            {
                                using (ImageView preview = new ImageView (context))
                                {
                                    preview.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
                                    preview.SetScaleType(ImageView.ScaleType.FitXy);
                                    Android.Graphics.Drawables.Drawable imgDataC = Android.Graphics.Drawables.Drawable.CreateFromStream(new MemoryStream(packItem.ContentPackItemIcon), "Comix");
                                    preview.SetImageDrawable(imgDataC);
                                    linView.AddView(preview);
                                    this.stepViews [eachMessageStep.StepNumber] = preview;
                                }
                            });
                        }
                        break;
				
                    case MessageStep.StepTypes.Comicon:
                        if (packItem != null)
                        {
                            RunOnUiThread(delegate
                            {
                                using (Android.Graphics.Drawables.Drawable imgDataS = Android.Graphics.Drawables.Drawable.CreateFromStream (new MemoryStream (packItem.ContentPackItemIcon), "Comicon"))
                                {
                                    using (ImageView preview = new ImageView (context))
                                    {
                                        preview.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
                                        preview.SetScaleType(ImageView.ScaleType.FitXy);
                                        preview.SetImageDrawable(imgDataS);
                                        linView.AddView(preview);
                                        this.stepViews [eachMessageStep.StepNumber] = preview;
                                        byte[] audio = packItem.ContentPackData;
                                        System.IO.File.WriteAllBytes(path, audio);
                                        AudioPlayer csfx = new AudioPlayer(context);
                                        t.Interval = csfx.findDuration(path) + 1000;
                                        csfx.playFromFile(path);
                                    }
                                }
                            });
                        }
                        break;
                    case MessageStep.StepTypes.SoundFX:
                        if (packItem != null)
                        {
                            RunOnUiThread(delegate
                            {
                                using (ImageView preview = new ImageView (context))
                                {
                                    preview.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
                                    preview.SetScaleType(ImageView.ScaleType.FitXy);
                                    preview.SetImageResource(Resource.Drawable.audiofile);
                                    linView.AddView(preview);
                                    byte[] audio = packItem.ContentPackData;
                                    System.IO.File.WriteAllBytes(path, audio);
                                    AudioPlayer csfx = new AudioPlayer(context);
                                    t.Interval = csfx.findDuration(path) + 1000;
                                    csfx.playFromFile(path);
                                    this.stepViews [eachMessageStep.StepNumber] = preview;
                                }
                            });
                        }
                        break;
                    case MessageStep.StepTypes.Emoticon:
                        if (packItem != null)
                        {
                            //RunOnUiThread (delegate {
                            t.Interval = 5000;
                            string base64String = System.Convert.ToBase64String(packItem.ContentPackData, 0, packItem.ContentPackData.Length);
                            using (WebView wv = new WebView (context))
                            {
                                string url = "<img src=\"data:image/gif;base64," + base64String + "\" width=\"" + ((int)newSizes [0]).ToString() + "\" height=\"" + ((int)newSizes [1]).ToString() + "\" />";
                                wv.LoadDataWithBaseURL(null, url, "text/html", "UTF-8", null);
                                wv.VerticalScrollBarEnabled = false;
                                wv.HorizontalScrollBarEnabled = false;
                                wv.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(200f, context), (int)ImageHelper.convertDpToPixel(200f, context));
                                wv.SetBackgroundColor(Color.Transparent);
                                RunOnUiThread(() => linView.AddView(wv));
                            }
                            //});
                        }
                        break;
                    case MessageStep.StepTypes.Voice:
                        if (MessagePlaybackUtil.voiceRecordings != null)
                        {
                            RunOnUiThread(delegate
                            {
                                using (ImageView preview = new ImageView (context))
                                {
                                    preview.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
                                    preview.SetScaleType(ImageView.ScaleType.FitXy);
                                    preview.SetImageResource(Resource.Drawable.microphone);
                                    linView.AddView(preview);
                                    this.stepViews [eachMessageStep.StepNumber] = preview;
                                }
                                //byte[] audio = getLocalVoiceRecording (MessagePlaybackUtil.voiceRecordings [eachMessageStep.StepNumber]);
                                AudioPlayer voice = new AudioPlayer(context);
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("audio file filename = {0}, stepNumber = {1}", MessagePlaybackUtil.voiceRecordings [eachMessageStep.StepNumber], eachMessageStep.StepNumber);
#endif
                                t.Interval = voice.findDuration(MessagePlaybackUtil.voiceRecordings [eachMessageStep.StepNumber]) + 1000;
                                string audioPath = MessagePlaybackUtil.voiceRecordings [eachMessageStep.StepNumber];
                                if (!File.Exists(audioPath))
                                {
                                    #if DEBUG
                                    System.Diagnostics.Debug.WriteLine("audio file doesn't exist for playback");
                                    #endif
                                } else
                                    voice.playFromFile(audioPath);
                                isPlaying = true;
                            });
                        }
                        break;
                    case MessageStep.StepTypes.Video:
                        if (packItem != null)
                        {
                            RunOnUiThread(delegate
                            {
                                SurfaceView preview = new SurfaceView(context);
                                preview.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
                                linView.AddView(preview);
                                byte[] audio = packItem.ContentPackData;
                                System.IO.File.WriteAllBytes(videoPath, audio);
                                videoPlay voice = new videoPlay(preview, videoPath);
                                t.Interval = voice.videoDuration(path) + 1000;
                                voice.videoStart();
                                this.stepViews [eachMessageStep.StepNumber] = preview;
                                isPlaying = true;
                            });
                        }
                        break;
                    case MessageStep.StepTypes.Animation:
					/*if (eachMessageStep.AnimationData != null) {
						string animationHtml = 
							Translator.MakePlaybackCode (eachMessageStep.AnimationData, (int)newSizes [0], (int)newSizes [1], RenderPlatformTypes.Android, JSTemplate);
						t.Interval = Convert.ToInt32 (eachMessageStep.AnimationData.Duration) + 100;
						using (WebView wv = new WebView (context)) {
							wv.LoadDataWithBaseURL (null, animationHtml, "text/html", "UTF-8", null);
							wv.VerticalScrollBarEnabled = false;
							wv.HorizontalScrollBarEnabled = false;
							wv.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (200f, context), (int)ImageHelper.convertDpToPixel (200f, context));
							wv.SetBackgroundColor (Color.Transparent);
							RunOnUiThread (() => linView.AddView (wv));
						}
					}*/
                        break;
                    case MessageStep.StepTypes.Polling:
                        if (MessagePlaybackUtil.pollSteps.Count > 0)
                        {
                            PollingStep pollStep = MessagePlaybackUtil.pollSteps [eachMessageStep.StepNumber];
                            if (string.IsNullOrEmpty(pollStep.PollingAnswer1))
                            {
                                t.Stop();
                                MessagePollUtil.pollStep = pollStep;
                                MessagePollUtil.recipients = MessagePlaybackUtil.recipients;
                                MessagePollUtil.isPhoto = true;
                                LOLMessageSurveyResult pollResult;
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("PollResults = {0}", MessagePlaybackUtil.pollResults == null ? "null" : "something");
#endif
                                if (MessagePlaybackUtil.pollResults.TryGetValue(eachMessageStep.StepNumber, out pollResult))
                                    MessagePollUtil.pollScreenType = PollingScreenType.Results;
                                else
                                    MessagePollUtil.pollScreenType = PollingScreenType.Vote;	
						
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("polltype = {0}", MessagePollUtil.pollScreenType);
#endif
						
                                RunOnUiThread(delegate
                                {
                                    SetContentView(Resource.Layout.PhotoPoll);
                                    ImageView topleft = FindViewById<ImageView>(Resource.Id.imgPoll1);
                                    topleft.Tag = 1;
                                    ImageView topright = FindViewById<ImageView>(Resource.Id.imgPoll2);
                                    topright.Tag = 2;
                                    ImageView bottomleft = FindViewById<ImageView>(Resource.Id.imgPoll3);
                                    bottomleft.Tag = 3;
                                    ImageView bottomright = FindViewById<ImageView>(Resource.Id.imgPoll4);
                                    bottomright.Tag = 4;
                                    voteTL = FindViewById<ProgressBar>(Resource.Id.pbVoteTL);
                                    voteTR = FindViewById<ProgressBar>(Resource.Id.pbVoteTR);
                                    voteBL = FindViewById<ProgressBar>(Resource.Id.pbVoteBL);
                                    voteBR = FindViewById<ProgressBar>(Resource.Id.pbVoteBR);
                                    TextView textPoll = FindViewById<TextView>(Resource.Id.txtPollText);
                                    textPoll.Text = MessagePollUtil.pollStep.PollingQuestion;
                                    doner = FindViewById<Button>(Resource.Id.btnDone);
                                    if (MessagePollUtil.pollScreenType == PollingScreenType.Results)
                                    {
                                        doner.Click += delegate
                                        {
                                            Intent i = new Intent(this, typeof(MessagePlayback));
                                            i.PutExtra("position", co + 1);
                                            StartActivity(i);
                                            Finish();
                                        };
                                    } else
                                        doner.Visibility = ViewStates.Invisible;
							
                                    ImageButton btnUniBack = FindViewById<ImageButton>(Resource.Id.btnUniBack);
                                    btnUniBack.Visibility = ViewStates.Invisible;
							
                                    if (MessagePollUtil.pollScreenType == PollingScreenType.Vote)
                                    {
                                        voteTL.Visibility = voteTR.Visibility = voteBL.Visibility = voteBR.Visibility = ViewStates.Invisible;
                                        topleft.Click += new EventHandler(Photo_Click);
                                        topright.Click += new EventHandler(Photo_Click);
                                        bottomleft.Click += new EventHandler(Photo_Click);
                                        bottomright.Click += new EventHandler(Photo_Click);
                                    } else 
							if (MessagePollUtil.pollScreenType == PollingScreenType.Results)
                                    {
                                        LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                                        service.PollingStepGetResultsCompleted += Service_PollingStepGetResultsCompleted;
                                        service.PollingStepGetResultsAsync(eachMessageStep.StepID, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
                                    }
							
                                    if (MessagePollUtil.pollStep.PollingData1 != null)
                                    {
                                        if (MessagePollUtil.pollStep.PollingData1.Length != 0)
                                        {
#if DEBUG
                                            System.Diagnostics.Debug.WriteLine("in A");
#endif
                                            using (Drawable imgDataA = Drawable.CreateFromStream (new MemoryStream (MessagePollUtil.pollStep.PollingData1), "Poll1"))
                                            {
                                                topleft.SetImageDrawable(imgDataA);
                                            }
                                        }
                                    }
                                    if (MessagePollUtil.pollStep.PollingData2 != null)
                                    {
                                        if (MessagePollUtil.pollStep.PollingData2.Length != 0)
                                        {
#if DEBUG
                                            System.Diagnostics.Debug.WriteLine("in B");
#endif
                                            using (Drawable imgDataB = Drawable.CreateFromStream (new MemoryStream (MessagePollUtil.pollStep.PollingData2), "Poll2"))
                                            {
                                                topright.SetImageDrawable(imgDataB);
                                            }
                                        }
                                    }
                                    if (MessagePollUtil.pollStep.PollingData3 != null)
                                    {
                                        if (MessagePollUtil.pollStep.PollingData3.Length != 0)
                                        {
#if DEBUG
                                            System.Diagnostics.Debug.WriteLine("in C");
#endif
                                            using (Drawable imgDataC = Drawable.CreateFromStream (new MemoryStream (MessagePollUtil.pollStep.PollingData3), "Poll3"))
                                            {
                                                bottomleft.SetImageDrawable(imgDataC);
                                            }
                                        }
                                    }
                                    if (MessagePollUtil.pollStep.PollingData4 != null)
                                    {
                                        if (MessagePollUtil.pollStep.PollingData4.Length != 0)
                                        {
#if DEBUG
                                            System.Diagnostics.Debug.WriteLine("in D");
#endif
                                            using (Drawable imgDataD = Drawable.CreateFromStream (new MemoryStream (MessagePollUtil.pollStep.PollingData4), "Poll4"))
                                            {
                                                bottomright.SetImageDrawable(imgDataD);
                                            }
                                        }
                                    }
                                });
                            } else
                            {
                                MessagePollUtil.isPhoto = false;
                                MessagePollUtil.pollStep = pollStep;
                                MessagePollUtil.recipients = MessagePlaybackUtil.recipients;
                                LOLMessageSurveyResult pollResult;
                                if (MessagePlaybackUtil.pollResults.TryGetValue(eachMessageStep.StepNumber, out pollResult))
                                    MessagePollUtil.pollScreenType = PollingScreenType.Results;
                                else
                                    MessagePollUtil.pollScreenType = PollingScreenType.Vote;
                                t.Stop();
						
                                RunOnUiThread(delegate
                                {
                                    SetContentView(Resource.Layout.TextPoll);
                                    Button opt1 = FindViewById<Button>(Resource.Id.btnOpt1);
                                    opt1.Tag = 1;
                                    Button opt2 = FindViewById<Button>(Resource.Id.btnOpt2);
                                    opt2.Tag = 2;
                                    Button opt3 = FindViewById<Button>(Resource.Id.btnOpt3);
                                    opt3.Tag = 3;
                                    Button opt4 = FindViewById<Button>(Resource.Id.btnOpt4);
                                    opt4.Tag = 4;
                                    res1 = FindViewById<ProgressBar>(Resource.Id.pbRes1);
                                    res2 = FindViewById<ProgressBar>(Resource.Id.pbRes2);
                                    res3 = FindViewById<ProgressBar>(Resource.Id.pbRes3);
                                    res4 = FindViewById<ProgressBar>(Resource.Id.pbRes4);
                                    TextView pollQ = FindViewById<TextView>(Resource.Id.txtPollText);
                                    pollQ.Text = MessagePollUtil.pollStep.PollingQuestion;
							
                                    Button finished = FindViewById<Button>(Resource.Id.btnDone);
                                    finished.Click += delegate
                                    { 
                                        Intent i = new Intent(this, typeof(MessagePlayback));
                                        i.PutExtra("position", co + 1);
                                        StartActivity(i);
                                        Finish();
                                    };
							
                                    doner = FindViewById<Button>(Resource.Id.btnDone);
                                    if (MessagePollUtil.pollScreenType == PollingScreenType.Results)
                                    {
                                        doner.Click += delegate
                                        {
                                            Intent i = new Intent(this, typeof(MessagePlayback));
                                            i.PutExtra("position", co + 1);
                                            StartActivity(i);
                                            Finish();
                                        };
                                    } else
                                        doner.Visibility = ViewStates.Invisible;
							
                                    ImageButton btnUniBack = FindViewById<ImageButton>(Resource.Id.btnUniBack);
                                    btnUniBack.Visibility = ViewStates.Invisible;
							
                                    opt1.Text = MessagePollUtil.pollStep.PollingAnswer1;
                                    opt2.Text = MessagePollUtil.pollStep.PollingAnswer2;
                                    opt3.Text = MessagePollUtil.pollStep.PollingAnswer3;
                                    opt4.Text = MessagePollUtil.pollStep.PollingAnswer4;
							
                                    if (MessagePollUtil.pollScreenType == PollingScreenType.Vote)
                                    {
                                        res1.Visibility = res2.Visibility = res3.Visibility = res4.Visibility = ViewStates.Invisible;
                                        opt1.Click += new EventHandler(PollButton_Click);
                                        opt2.Click += new EventHandler(PollButton_Click);
                                        opt3.Click += new EventHandler(PollButton_Click);
                                        opt4.Click += new EventHandler(PollButton_Click);
                                    } else
                                    {
                                        LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                                        service.PollingStepGetResultsCompleted += Service_PollingStepGetResultsCompleted;
                                        service.PollingStepGetResultsAsync(eachMessageStep.StepID, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
                                    }
                                });
                            }
                        }
                        break;
                }
            }
            if (File.Exists(path))
                File.Delete(path);
			
            if (File.Exists(videoPath))
                File.Delete(videoPath);
        }
		
        private void Service_PollingStepGetResultsCompleted(object sender, PollingStepGetResultsCompletedEventArgs e)
        {
            LOLMessageClient service = (LOLMessageClient)sender;
            service.PollingStepGetResultsCompleted -= Service_PollingStepGetResultsCompleted;
			
            if (e.Error == null)
            {
                LOLMessageSurveyResult pollResult;
                RunOnUiThread(() => doner.Visibility = ViewStates.Visible);
				
                doner.Click += delegate
                {
                    RunOnUiThread(() => restartTimerFromPoll());
                };
				
                if (MessagePollUtil.isPhoto)
                {
                    RunOnUiThread(() => voteTL.Visibility = voteTR.Visibility = voteBL.Visibility = voteBR.Visibility = ViewStates.Visible);
                    MessagePlaybackUtil.pollResults.TryGetValue(0, out pollResult);
                    RunOnUiThread(() => voteTL.Progress = (int)(pollResult.Answer1Percent * 100));
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("voteTL = {0}, raw = {1}, count = {2}, replied = {3}", voteTL.Progress, pollResult.Answer1Percent, pollResult.Answer1Count, pollResult.Responses);
#endif
                    MessagePlaybackUtil.pollResults.TryGetValue(1, out pollResult);
                    RunOnUiThread(() => voteTR.Progress = (int)(pollResult.Answer2Percent * 100));
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("voteTR = {0}, raw = {1}", voteTR.Progress, pollResult.Answer2Percent);
#endif
                    MessagePlaybackUtil.pollResults.TryGetValue(2, out pollResult);
                    RunOnUiThread(() => voteBL.Progress = (int)(pollResult.Answer3Percent * 100));
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("voteBL = {0}, raw = {1}", voteBL.Progress, pollResult.Answer3Percent);
#endif
                    MessagePlaybackUtil.pollResults.TryGetValue(3, out pollResult);
                    RunOnUiThread(() => voteBR.Progress = (int)(pollResult.Answer4Percent * 100));
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("voteBR = {0}, raw = {1}", voteBR.Progress, pollResult.Answer4Percent);
#endif
                } else
                {
                    RunOnUiThread(() => res1.Visibility = res2.Visibility = res3.Visibility = res4.Visibility = ViewStates.Visible);
                    MessagePlaybackUtil.pollResults.TryGetValue(0, out pollResult);
                    RunOnUiThread(() => res1.Progress = (int)(pollResult.Answer1Percent * 100));
                    MessagePlaybackUtil.pollResults.TryGetValue(1, out pollResult);
                    RunOnUiThread(() => res2.Progress = (int)(pollResult.Answer2Percent * 100));
                    MessagePlaybackUtil.pollResults.TryGetValue(2, out pollResult);
                    RunOnUiThread(() => res3.Progress = (int)(pollResult.Answer3Percent * 100));
                    MessagePlaybackUtil.pollResults.TryGetValue(3, out pollResult);
                    RunOnUiThread(() => res4.Progress = (int)(pollResult.Answer4Percent * 100));
                }
            }
        }
		
        void Photo_Click(object sender, EventArgs e)
        {
            ImageView iv = (ImageView)sender;
            int option = (int)iv.Tag;
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(MessagePlaybackUtil.messageSteps [co].StepID, AndroidData.CurrentUser.AccountID, option, new Guid(AndroidData.ServiceAuthToken));
        }
		
        private void PollButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int option = (int)btn.Tag;
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(MessagePlaybackUtil.messageSteps [co].StepID, AndroidData.CurrentUser.AccountID, option, new Guid(AndroidData.ServiceAuthToken));
        }
		
        void service_PollingStepResponseCompleted(object sender, PollingStepResponseCompletedEventArgs e)
        {
            LOLMessageClient myService = (LOLMessageClient)sender;
            myService.PollingStepResponseCompleted -= service_PollingStepResponseCompleted;
			
            if (e.Error == null)
            {
                LOLMessageDelivery.GeneralError result = e.Result;
                if (!string.IsNullOrEmpty(result.ErrorNumber) && result.ErrorNumber != "0")
                {
                    RunOnUiThread(() => Toast.MakeText(context, Application.Context.Resources.GetString(Resource.String.errorUploadPollingMessage), ToastLength.Short).Show());
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Error in voting - {0}", result.ErrorDescription);
#endif	
                } else
                {
                    MessagePollUtil.pollStep.HasResponded = true;
                    dbm.SetPollingStepHasResponded(MessagePollUtil.pollStep.MessageID.ToString(), MessagePollUtil.pollStep.StepNumber);
                    //RunOnUiThread (() => restartTimerFromPoll ());
                    LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                    service.PollingStepGetResultsCompleted += Service_PollingStepGetResultsCompleted;
                    service.PollingStepGetResultsAsync(MessagePlaybackUtil.messageSteps [co].StepID, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
                }
            } else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Exception sending vote {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
            }
        }
    }
}


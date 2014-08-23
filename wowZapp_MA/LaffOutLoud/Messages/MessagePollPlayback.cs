using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using LOLApp_Common;

using System.IO;
using System.Linq;

using WZCommon;

namespace wowZapp.Messages
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class MessagePollPlayback : Activity
    {
        //PhotoPoll Views
        private ImageView topleft, topright, bottomleft, bottomright;
        private ProgressBar voteTL, voteTR, voteBL, voteBR;
        private TextView textPoll;
        private Button done;

        //TextPoll Views
        private Button opt1, opt2, opt3, opt4;
        private ProgressBar res1, res2, res3, res4;
        private Button finished;

        //Shared Views & Guid
        private ImageView btnUniBack;
        private Guid stepID;

        //private Context context;
        //private DBManager dbm;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            stepID = MessagePlaybackUtil.messageSteps.Where(s => s.StepNumber == MessagePollUtil.pollStep.StepNumber).Select(s => s.StepID).FirstOrDefault();
            
            if (MessagePollUtil.isPhoto)
            {
                SetContentView(Resource.Layout.PhotoPoll);
                topleft = FindViewById<ImageView>(Resource.Id.imgPoll1);
                topright = FindViewById<ImageView>(Resource.Id.imgPoll2);
                bottomleft = FindViewById<ImageView>(Resource.Id.imgPoll3);
                bottomright = FindViewById<ImageView>(Resource.Id.imgPoll4);
                voteTL = FindViewById<ProgressBar>(Resource.Id.pbVoteTL);
                voteTR = FindViewById<ProgressBar>(Resource.Id.pbVoteTR);
                voteBL = FindViewById<ProgressBar>(Resource.Id.pbVoteBL);
                voteBR = FindViewById<ProgressBar>(Resource.Id.pbVoteBR);
                textPoll = FindViewById<TextView>(Resource.Id.txtPollText);
                textPoll.Text = MessagePollUtil.pollStep.PollingQuestion;
                done = FindViewById<Button>(Resource.Id.btnDone);
                done.Click += new EventHandler(btnUniBack_Click);
                btnUniBack = FindViewById<ImageView>(Resource.Id.btnUniBack);
                btnUniBack.Click += new EventHandler(btnUniBack_Click);

                if (MessagePollUtil.pollScreenType == PollingScreenType.Vote)
                {
                    voteTL.Visibility = voteTR.Visibility = voteBL.Visibility = voteBR.Visibility = ViewStates.Invisible;
                    topleft.Click += new EventHandler(topleft_Click);
                    topright.Click += new EventHandler(topright_Click);
                    bottomleft.Click += new EventHandler(bottomleft_Click);
                    bottomright.Click += new EventHandler(bottomright_Click);
                } else
                {
                    LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                    service.PollingStepGetResultsCompleted += Service_PollingStepGetResultsCompleted;
                    service.PollingStepGetResultsAsync(stepID, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
                }

                if (MessagePollUtil.pollStep.PollingData1.Length != 0)
                {
                    Drawable imgDataA = Drawable.CreateFromStream(new MemoryStream(MessagePollUtil.pollStep.PollingData1), "Poll1");
                    topleft.SetImageDrawable(imgDataA);
                    imgDataA.Dispose();
                }
                if (MessagePollUtil.pollStep.PollingData2.Length != 0)
                {
                    Drawable imgDataB = Drawable.CreateFromStream(new MemoryStream(MessagePollUtil.pollStep.PollingData2), "Poll2");
                    topright.SetImageDrawable(imgDataB);
                    imgDataB.Dispose();
                }
                if (MessagePollUtil.pollStep.PollingData3.Length != 0)
                {
                    Drawable imgDataC = Drawable.CreateFromStream(new MemoryStream(MessagePollUtil.pollStep.PollingData3), "Poll3");
                    bottomleft.SetImageDrawable(imgDataC);
                    imgDataC.Dispose();
                }
                if (MessagePollUtil.pollStep.PollingData4.Length != 0)
                {
                    Drawable imgDataD = Drawable.CreateFromStream(new MemoryStream(MessagePollUtil.pollStep.PollingData2), "Poll4");
                    bottomright.SetImageDrawable(imgDataD);
                    imgDataD.Dispose();
                }
            } else
            {
                SetContentView(Resource.Layout.TextPoll);
                opt1 = FindViewById<Button>(Resource.Id.btnOpt1);
                opt2 = FindViewById<Button>(Resource.Id.btnOpt2);
                opt3 = FindViewById<Button>(Resource.Id.btnOpt3);
                opt4 = FindViewById<Button>(Resource.Id.btnOpt4);
                res1 = FindViewById<ProgressBar>(Resource.Id.pbRes1);
                res2 = FindViewById<ProgressBar>(Resource.Id.pbRes2);
                res3 = FindViewById<ProgressBar>(Resource.Id.pbRes3);
                res4 = FindViewById<ProgressBar>(Resource.Id.pbRes4);
                TextView pollQ = FindViewById<TextView>(Resource.Id.txtPollText);
                pollQ.Text = MessagePollUtil.pollStep.PollingQuestion;

                finished = FindViewById<Button>(Resource.Id.btnDone);
                finished.Click += delegate
                {
                    returnToSender();
                };

                btnUniBack = FindViewById<ImageView>(Resource.Id.btnUniBack);
                btnUniBack.Click += delegate
                {
                    returnToSender();
                };

                opt1.Text = MessagePollUtil.pollStep.PollingAnswer1;
                opt2.Text = MessagePollUtil.pollStep.PollingAnswer2;
                opt3.Text = MessagePollUtil.pollStep.PollingAnswer3;
                opt4.Text = MessagePollUtil.pollStep.PollingAnswer4;

                if (MessagePollUtil.pollScreenType == PollingScreenType.Vote)
                {
                    res1.Visibility = res2.Visibility = res3.Visibility = res4.Visibility = ViewStates.Invisible;
                    opt1.Click += new EventHandler(opt1_Click);
                    opt2.Click += new EventHandler(opt2_Click);     
                    opt3.Click += new EventHandler(opt3_Click);
                    opt4.Click += new EventHandler(opt4_Click);
                } else
                {
                    LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                    service.PollingStepGetResultsCompleted += Service_PollingStepGetResultsCompleted;
                    service.PollingStepGetResultsAsync(stepID, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
                }
            }
        }

        void bottomright_Click(object sender, EventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(stepID, AndroidData.CurrentUser.AccountID, 4, new Guid(AndroidData.ServiceAuthToken));
        }

        void bottomleft_Click(object sender, EventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(stepID, AndroidData.CurrentUser.AccountID, 3, new Guid(AndroidData.ServiceAuthToken));
        }

        void topright_Click(object sender, EventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(stepID, AndroidData.CurrentUser.AccountID, 2, new Guid(AndroidData.ServiceAuthToken));
        }

        void topleft_Click(object sender, EventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(stepID, AndroidData.CurrentUser.AccountID, 1, new Guid(AndroidData.ServiceAuthToken));
        }

        void opt4_Click(object sender, EventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(stepID, AndroidData.CurrentUser.AccountID, 4, new Guid(AndroidData.ServiceAuthToken));
        }

        void opt3_Click(object sender, EventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(stepID, AndroidData.CurrentUser.AccountID, 3, new Guid(AndroidData.ServiceAuthToken));
        }

        void opt2_Click(object sender, EventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(stepID, AndroidData.CurrentUser.AccountID, 2, new Guid(AndroidData.ServiceAuthToken));
        }

        void opt1_Click(object sender, EventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepResponseCompleted += new EventHandler<PollingStepResponseCompletedEventArgs>(service_PollingStepResponseCompleted);
            service.PollingStepResponseAsync(stepID, AndroidData.CurrentUser.AccountID, 1, new Guid(AndroidData.ServiceAuthToken));
        }

        void service_PollingStepResponseCompleted(object sender, PollingStepResponseCompletedEventArgs e)
        {
            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            service.PollingStepGetResultsCompleted += Service_PollingStepGetResultsCompleted;
            service.PollingStepGetResultsAsync(stepID, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
        }

        private void Service_PollingStepGetResultsCompleted(object sender, PollingStepGetResultsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (MessagePollUtil.isPhoto == true)
                {
                    RunOnUiThread(delegate
                    {
                        voteTL.Visibility = voteTR.Visibility = voteBL.Visibility = voteBR.Visibility = ViewStates.Visible;
                        voteTL.Progress = (int)(e.Result.Answer1Percent * 100);
                        voteTR.Progress = (int)(e.Result.Answer2Percent * 100);
                        voteBL.Progress = (int)(e.Result.Answer3Percent * 100);
                        voteBR.Progress = (int)(e.Result.Answer4Percent * 100);
                    });
                } else
                {
                    RunOnUiThread(delegate
                    {
                        res1.Visibility = res2.Visibility = res3.Visibility = res4.Visibility = ViewStates.Visible;
                        res1.Progress = (int)(e.Result.Answer1Percent * 100);
                        res2.Progress = (int)(e.Result.Answer2Percent * 100);
                        res3.Progress = (int)(e.Result.Answer3Percent * 100);
                        res4.Progress = (int)(e.Result.Answer4Percent * 100);
                    });
                }
            }
        }

        private void btnUniBack_Click(object s, EventArgs e)
        {
            returnToSender();
        }

        private void returnToSender()
        {
            RunOnUiThread(delegate
            {
                Intent resultData = new Intent();
                SetResult(Result.Ok, resultData);
                Finish();
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
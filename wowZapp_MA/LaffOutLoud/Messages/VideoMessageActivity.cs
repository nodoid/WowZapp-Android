using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace wowZapp.Messages
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class VideoMessageActivity : Activity
    {
        private bool isRecording, isBackCamera, isPlaying, isLocked;
        private Context context;
        private ProgressBar progress;
        private int time, up;
        private TextView seconds;
        private System.Timers.Timer timer;
        private videoRecord video;
        private videoPlay vidplay;
        private string path;
        private Android.Hardware.Camera camera;
        private Android.Hardware.Camera.CameraInfo cameraInfo;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.VideoMessage);

            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewloginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            context = header.Context;
            Header.headertext = Application.Context.Resources.GetString(Resource.String.videoTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            bool isRecord = base.Intent.GetBooleanExtra("record", true); // true = record
            string filename = base.Intent.GetStringExtra("filename"); // must be full pathname - not just the filename
            
            string toplay = isRecord != false ? "null" : filename;
            time = 20;
            up = 0;
            progress = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            seconds = FindViewById<TextView>(Resource.Id.textTimeLeft);
            ImageButton btnRecord = FindViewById<ImageButton>(Resource.Id.btnRecord);
            ImageButton btnPause = FindViewById<ImageButton>(Resource.Id.btnPause);
            ImageButton btnStop = FindViewById<ImageButton>(Resource.Id.btnStop);
            ImageButton btnPlay = FindViewById<ImageButton>(Resource.Id.btnPlay);
            ImageButton btnSwitchCamera = FindViewById<ImageButton>(Resource.Id.imageRotate);
            SurfaceView surface = FindViewById<SurfaceView>(Resource.Id.surfaceView1);

            path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "myvideo.mp4");

            if (isRecord == false)
            {
                btnRecord.Visibility = ViewStates.Gone;
                progress.Visibility = ViewStates.Gone;
                seconds.Text = "Playback mode";
                isPlaying = true;
                vidplay = new videoPlay(surface, toplay);
            } else
                btnRecord.Click += new EventHandler(btnRecord_Click);

            int cameras = Android.Hardware.Camera.NumberOfCameras;
            if (cameras == 1)
                btnSwitchCamera.Visibility = ViewStates.Gone;
            else
            //{
                //isBackCamera = false;
                btnSwitchCamera.Click += new EventHandler(btnSwitchCamera_Click);
            // }

            btnPause.Click += new EventHandler(btnPause_Click);
            btnPlay.Click += delegate
            {
                videoPlay vp = new videoPlay(surface, path);
            };
            btnStop.Click += new EventHandler(btnStop_Click);

            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

            cameraInfo = new Android.Hardware.Camera.CameraInfo();

            isRecording = isLocked = false;
            correctCameras(0);
            video = new videoRecord(surface, path);
        }

        private void correctCameras(int cameras)
        {
            int result = 0;
            Android.Hardware.Camera.CameraInfo cameraInfo = new Android.Hardware.Camera.CameraInfo();
            Android.Hardware.Camera.GetCameraInfo(cameras, cameraInfo);
            int rotation = WindowManager.DefaultDisplay.Orientation;
            int degrees = rotation;

            if (cameraInfo.Facing == Android.Hardware.CameraFacing.Front)
            {
                result = (cameraInfo.Orientation + degrees) % 360;
                result = (360 - result) % 360;
                try
                {
                    if (isLocked == true)
                        camera.Unlock();
                    camera = Android.Hardware.Camera.Open(cameras);
                } catch (Java.Lang.RuntimeException ex)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                                         Application.Context.Resources.GetString(Resource.String.photoNoConnection));
                    });
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("Exception flung {0}", ex.ToString());
                    #endif
                }
            } else
            {
                result = (cameraInfo.Orientation - degrees + 360) % 360;
                try
                {
                    if (isLocked == true)
                        camera.Unlock();
                    camera = Android.Hardware.Camera.Open(cameras);
                } catch (Java.Lang.RuntimeException ex)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                                          Application.Context.Resources.GetString(Resource.String.photoNoConnection));
                    });
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("Exception flung {0}", ex.ToString());
                    #endif
                }
            }

            if (camera != null)
            {
                RunOnUiThread(delegate
                {
                    camera.SetDisplayOrientation(result);
                    camera.StartPreview();
                    camera.Lock();
                });
                isLocked = true;
            }
        }

        private void btnSwitchCamera_Click(object s, EventArgs e)
        {
            if (isBackCamera == false)
            {
                try
                {
                    RunOnUiThread(delegate
                    {
                        correctCameras(1);
                    });
                } catch (Java.Lang.RuntimeException)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.videoErrorTitle),
                            Application.Context.Resources.GetString(Resource.String.videoFailToConnect));
                    });
                    return;
                }
                isBackCamera = true;
            } else
            {
                try
                {
                    RunOnUiThread(delegate
                    {
                        correctCameras(0);
                    });
                } catch (Java.Lang.RuntimeException)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.videoErrorTitle),
                            Application.Context.Resources.GetString(Resource.String.videoFailToConnect));
                    });
                    return;
                }
                isBackCamera = false;
            }
        }

        private void timer_Elapsed(object s, System.Timers.ElapsedEventArgs e)
        {
            time--;
            up++;
            if (up == 21)
            {
                video.StopRecording();
                RunOnUiThread(() => seconds.Text = "Times up!");
                isRecording = false;
                timer.Stop();
            } else
            {
                RunOnUiThread(() => seconds.Text = time.ToString() + " seconds");
                progress.Progress = up;
            }
        }

        private void btnRecord_Click(object s, EventArgs e)
        {
            if (isRecording != true)
            {
                RunOnUiThread(delegate
                {
                    if (video.StartRecording() == true)
                    {
                        timer.Start();
                        isRecording = true;
                    }
                });
            }
        }

        private void btnStop_Click(object s, EventArgs e)
        {
            if (isPlaying != false)
            {
                vidplay.videoStop();
                isPlaying = false;
            }

            if (isRecording != false)
            {
                video.StopRecording();
                timer.Stop();
                isRecording = false;
            }
        }

        private void btnPause_Click(object s, EventArgs e)
        {
            if (isRecording != false)
            {
                video.StopRecording();
                timer.Stop();
                isRecording = true;
            } else
            {
                video.StartRecording();
                timer.Start();
                isRecording = false;
            }

            if (isPlaying != false)
            {
                vidplay.videoStop();
                isPlaying = true;
            } else
            {
                vidplay.videoStart();
                isPlaying = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
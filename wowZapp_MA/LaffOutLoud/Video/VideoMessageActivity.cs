using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;

namespace wowZapp.VideoMessage
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
    public class VideoMessageActivity : Activity, ISurfaceHolderCallback
    {
        private MediaRecorder mediaRecorder;
        private MediaPlayer mediaPlayer;
        private ISurfaceHolder holder;
        private SurfaceView surface;
        private bool recording = false, playing = false, hasRecorded = false;
        private System.Timers.Timer recordTimer;
        private ProgressBar progress;
        private int counter = 0, timeRemain = 20, fps = 0, camID;
        private TextView timeLeft;
        private string timeString;
        private Android.Hardware.Camera camera;
        private const int EDIT = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.VideoMessage);
            SurfaceView surface = FindViewById<SurfaceView>(Resource.Id.surfaceView1);

            double surfaceSize = wowZapp.LaffOutOut.Singleton.ScreenYHeight * .66;

            ImageButton record = FindViewById<ImageButton>(Resource.Id.btnRecord);
            ImageButton stop = FindViewById<ImageButton>(Resource.Id.btnStop);
            ImageButton pause = FindViewById<ImageButton>(Resource.Id.btnPause);
            ImageButton play = FindViewById<ImageButton>(Resource.Id.btnPlay);
            ImageButton edit = FindViewById<ImageButton>(Resource.Id.btnEdit);
            recordTimer = new System.Timers.Timer();
            recordTimer.Interval = 1000;
            recordTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => timerElapsed(sender, e);
            mediaPlayer = new Android.Media.MediaPlayer();
            progress = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            timeLeft = FindViewById<TextView>(Resource.Id.textTimeLeft);
            Context context = timeLeft.Context;
            timeString = context.Resources.GetString(Resource.String.videoSeconds);
            timeLeft.Text = timeRemain.ToString() + " " + timeString;

            //CamcorderProfile camProf = new CamcorderProfile();
            //fps = camProf.VideoFrameRate;
            camera = null;

            ImageButton returnBack = FindViewById<ImageButton>(Resource.Id.imgBack);
            returnBack.Tag = 0;
            ImageButton flipView = FindViewById<ImageButton>(Resource.Id.imageRotate);
            flipView.Tag = 1;
            ImageButton returnHome = FindViewById<ImageButton>(Resource.Id.imgHome);
            returnHome.Tag = 2;

            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
     
            ImageButton[] btn = new ImageButton[3];
            btn [0] = returnBack;
            btn [2] = returnHome;
            btn [1] = flipView;
     
            ImageHelper.setupButtonsPosition(btn, bottom, context);

            record.Click += delegate
            {
                if (!recording && !playing)
                    startRecording();
            };

            stop.Click += delegate
            {
                if (recording)
                    stopRecording();
                if (playing)
                    stopPlaying();
            };  

            play.Click += delegate
            {
                if (!recording)
                    startPlayback();
            };

            edit.Click += delegate
            {
                Intent i = new Intent(this, typeof(EditVideo));
                i.PutExtra("filename", System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(),
                           "myvideooutputfile.mp4"));
                i.PutExtra("fps", fps);
                i.PutExtra("duration", counter);
                StartActivityForResult(i, EDIT);
            };

            returnBack.Click += delegate
            {
                Finish();
            };

            returnHome.Click += delegate
            {
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.AddFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };

            holder = surface.Holder;
            holder.AddCallback(this);
            holder.SetType(Android.Views.SurfaceType.PushBuffers);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            var outputFile = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(),
                                                    "myvideooutputfile.mp4");
            if (!recording)
            {
                mediaRecorder = new MediaRecorder();
                //correctCamera();
                //if (camera != null)
                //    mediaRecorder.SetCamera(camera);

                mediaRecorder.SetAudioSource(AudioSource.Mic);
                mediaRecorder.SetVideoSource(VideoSource.Camera);
                mediaRecorder.SetOutputFormat(OutputFormat.ThreeGpp);
                mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
                mediaRecorder.SetVideoEncoder(VideoEncoder.Mpeg4Sp);
            
                if (System.IO.File.Exists(outputFile))
                    System.IO.File.Delete(outputFile);
                System.IO.File.Create(outputFile);

                CamcorderProfile camProf = CamcorderProfile.Get(CamcorderQuality.Low);
                fps = camProf.VideoFrameRate;

                mediaRecorder.SetOutputFile(outputFile);
                mediaRecorder.SetPreviewDisplay(holder.Surface);
                mediaRecorder.Prepare();
            } else
            {
                mediaPlayer.SetDisplay(holder);
                mediaPlayer.SetDataSource(outputFile);
                mediaPlayer.Prepared += new EventHandler(mediaPlayer_Prepared);
                mediaPlayer.PrepareAsync();
            }
        }

        private void correctCamera()
        {
            int cameraCount = 0;
            Android.Hardware.Camera.CameraInfo cameraInfo = new Android.Hardware.Camera.CameraInfo();
            cameraCount = Android.Hardware.Camera.NumberOfCameras;
            int result = 0;
            for (int camIdx = 0; camIdx < cameraCount; camIdx++)
            {
                Android.Hardware.Camera.GetCameraInfo(camIdx, cameraInfo);
                int rotation = WindowManager.DefaultDisplay.Orientation;
                int degrees = 0;
                camID = camIdx;
                switch (rotation)
                {
                    case 0:
                        degrees = 0;
                        break;
                    case 90:
                        degrees = 90;
                        break;
                    case 180:
                        degrees = 180;
                        break;
                    case 270:
                        degrees = 270;
                        break;
                }
                
                /*if (cameraInfo.Facing == Android.Hardware.CameraFacing.Front)
                {
                    result = (cameraInfo.Orientation + degrees) % 360;
                    result = (360 - result) % 360;
                    try
                    {
                        camera = Android.Hardware.Camera.Open(camIdx);
                    } catch (Java.Lang.RuntimeException ex)
                    {
                        RunOnUiThread(() => GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.commonError),
                                                               Application.Context.Resources.GetString(Resource.String.photoNoConnection)));
                    }
                } else*/
                if (cameraInfo.Facing == Android.Hardware.CameraFacing.Back)
                {
                    result = (cameraInfo.Orientation - degrees + 360) % 360;
                    result = (360 - result) % 360;
                    camera = Android.Hardware.Camera.Open((int)Android.Hardware.CameraFacing.Back);
                    break;
                }
            }
            if (camera != null)
                camera.SetDisplayOrientation(result);
            //return camera;
        }

        private void mediaPlayer_Prepared(object sender, EventArgs e)
        {
            mediaPlayer.Start();
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            //stopRecording();
            mediaRecorder.Release();
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format fmt, int j, int k)
        {
        }

        private void startRecording()
        {
            mediaRecorder.Start();
            recordTimer.Start();
            recording = true;
        }
        
        private void stopRecording()
        {
            mediaRecorder.Stop();
            recordTimer.Stop();
            recording = false;
        }

        private void startPlayback()
        {
            playing = true;
            recording = true;
            SurfaceCreated(holder);
        }

        private void stopPlaying()
        {
            mediaPlayer.Stop();
            recording = false;
            playing = false;
        }

        private void timerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (counter < 20)
            {
                counter++;
                progress.Progress = counter;
                RunOnUiThread(() => timeLeft.Text = (timeRemain - counter).ToString() + " " + timeString);
                hasRecorded = true;
            } else
            {
                stopRecording();
                counter = 0;
            }
        }
    }
}
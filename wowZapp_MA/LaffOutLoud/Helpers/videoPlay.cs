using System;

using Android.App;
using Android.Views;
using Android.Media;

using Java.IO;

namespace wowZapp
{
    [Activity(ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class videoPlay : Activity, ISurfaceHolderCallback, IDisposable
    {
        MediaPlayer mp;
        string playFilename;

        public videoPlay() { }

        public videoPlay(SurfaceView surface, string filename = "")
        {
            var holder = surface.Holder;
            holder.AddCallback(this);
            holder.SetType(SurfaceType.PushBuffers);

            if (filename == "")
                playFilename = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "myLOLvideo.mp4");
            else
                playFilename = filename;

            mp = new MediaPlayer();
        }

        public int videoDuration(string filename)
        {
            MediaPlayer video = new MediaPlayer();
            FileInputStream fs = new FileInputStream(filename);
            FileDescriptor fd = fs.FD;
            video.SetDataSource(fd);
            video.Prepare();
            int length = video.Duration;
            video.Reset();
            video.Release();
            return length;
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                mp.SetDisplay(holder);
                mp.SetDataSource(playFilename);
                mp.Prepared += new EventHandler(videoPrepared);
                mp.PrepareAsync();
            }
            catch
            {
                throw new Exception();
            }
        }

        private void videoPrepared(object s, EventArgs e)
        {
            videoStart();
        }

        public void videoStart()
        {
            mp.Start();
        }

        public void videoStop()
        {
            mp.Stop();
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            mp.Release();
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format fmt, int width, int height)
        { }
    }
}
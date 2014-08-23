using System;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Media;

namespace wowZapp
{
	public class videoRecord : Java.Lang.Object, ISurfaceHolderCallback, IDisposable
	{
		MediaRecorder mediaRecord;
		private string videoFilename;
		private bool isPaused;
		private Context context;
		ISurfaceHolder holder;

		public videoRecord (SurfaceView surface, string filename = "")
		{
			if (filename == "")
				videoFilename = System.IO.Path.Combine (Android.OS.Environment.ExternalStorageDirectory.ToString (), "myLOLvideo.mp4");
			else
				videoFilename = filename;
			isPaused = false;

			context = surface.Context;
			holder = surface.Holder;
			holder.AddCallback (this);
			holder.SetType (Android.Views.SurfaceType.PushBuffers);
		}

		public void Dispose ()
		{
			this.Dispose ();
		}

		public void SurfaceCreated (ISurfaceHolder holder)
		{
			mediaRecord = new MediaRecorder ();
			mediaRecord.SetAudioSource (AudioSource.Mic);
			mediaRecord.SetVideoSource (VideoSource.Camera);
			mediaRecord.SetOutputFormat (OutputFormat.Default);
			mediaRecord.SetAudioEncoder (AudioEncoder.Default);
			mediaRecord.SetVideoEncoder (VideoEncoder.Default);
            
			System.IO.File.Create (videoFilename);

			mediaRecord.SetOutputFile (videoFilename);
			mediaRecord.SetPreviewDisplay (holder.Surface);
			mediaRecord.Prepare ();
		}

		public void SurfaceDestroyed (ISurfaceHolder holder)
		{
			StopRecording ();
			mediaRecord.Release ();
		}

		public void SurfaceChanged (ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
		{
		}

		public void OnPreviewFrame (byte[] data, Android.Hardware.Camera camera)
		{
		}

		public bool StartRecording ()
		{
			try {
				mediaRecord.Start ();
			} catch (Java.Lang.RuntimeException) {
				GeneralUtils.Alert (context, Application.Context.Resources.GetString (Resource.String.videoErrorTitle),
                    Application.Context.Resources.GetString (Resource.String.videoFailToStart));
				return false;
			}
			return true;
		}

		public void StopRecording ()
		{
			try {
				mediaRecord.Stop ();
			} catch (Java.Lang.IllegalStateException) {
				#if DEBUG
				System.Diagnostics.Debug.WriteLine ("You can't stop what isn't started!!!!");
				#endif
			}
		}
	}
}
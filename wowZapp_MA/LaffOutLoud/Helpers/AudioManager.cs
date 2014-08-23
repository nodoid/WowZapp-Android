using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.Net;
using System;
using System.Threading;
using Java.IO;

namespace wowZapp
{
    public class AudioPlayer : Java.Lang.Object, MediaPlayer.IOnCompletionListener
    {
        MediaPlayer mp;
        Context context;

        public AudioPlayer()
        {
        }

        public AudioPlayer(Context c)
        {
            this.context = c;
            mp = new MediaPlayer();
        }

        public void Dispose()
        {
            this.Dispose();
        }

        public void playFromFile(string filename)
        {
            if (filename.Length == 0)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("filename length = 0");
                #endif
                GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorAudioProblemTitle), 
                    Application.Context.Resources.GetString(Resource.String.errorEmptyFilename));
                return;
            }
            Android.Net.Uri uri = Android.Net.Uri.Parse(filename);
#if DEBUG
            System.Diagnostics.Debug.WriteLine("URI = {0}", uri);
#endif
            if (mp != null)
                playAudio(uri);
            else
            {
                GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorAudioProblemTitle),
                    Application.Context.Resources.GetString(Resource.String.errorPlayingFile));
                return;
            }
        }
        
        public void playFromURL(string url)
        {
            if (url.Length == 0)
            {
                GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorAudioProblemTitle),
                    Application.Context.Resources.GetString(Resource.String.errorEmptyURL));
                return;
            }
            if (checkForNetwork() == false)
            {
                GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorAudioProblemTitle),
                    Application.Context.Resources.GetString(Resource.String.errorNetFault));
                return;
            }
            Android.Net.Uri uri = Android.Net.Uri.Parse(url);
            if (mp != null)
                playAudio(uri);
            else
            {
                GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorAudioProblemTitle),
                    Application.Context.Resources.GetString(Resource.String.errorPlayingURL));
                return;
            }
        }

        public void playFromAssets(AssetManager am, string assetsName)
        {
            if (assetsName.Length == 0)
            {
                GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorAudioProblemTitle),
                    Application.Context.Resources.GetString(Resource.String.errorEmptyURL));
                return;
            }

            AssetFileDescriptor afd = am.OpenFd(assetsName);
            mp.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
            afd.Close();
            mp.Prepare();
            mp.Start();
        }

        public int findDuration(string filename)
        {
            MediaPlayer wav = new MediaPlayer();
            FileInputStream fs = new FileInputStream(filename);
            FileDescriptor fd = fs.FD;
            wav.SetDataSource(fd);
            wav.Prepare();
            int length = wav.Duration;
            wav.Reset();
            wav.Release();
            return length;
        }

        public void pausePlayer()
        {
            OnPause(mp);
        }

        public void stopPlayer()
        {
            OnStop(mp);
        }

        public void startPlayer()
        {
            OnRestart(mp);
        }

        private void playAudio(Android.Net.Uri name)
        {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("uri passed in = {0}", name);
            #endif
            try
            {
                mp.Reset();
                mp.SetDataSource(context, name);
                mp.SetOnCompletionListener(this);
                mp.Prepare();
                mp.Start();
            } catch (Exception ex)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("Exception caught in playAudio - {0} : {1}", ex.Message, ex.StackTrace);
                #endif
            }
        }

        public void OnCompletion(MediaPlayer player)
        {
            try
            {
                player.Stop();
                player.Reset();
                player.Release();
            } catch (Exception ex)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("Exception in audio {0}", ex.Message);
                #endif
            }
        }

        public void OnStart(MediaPlayer player)
        {
            player.Start();
        }

        public void OnPause(MediaPlayer player)
        {
            player.Pause();
        }

        public void OnStop(MediaPlayer player)
        {
            player.Stop();
        }

        public void OnRestart(MediaPlayer player)
        {
            player.Start();
        }

        private bool checkForNetwork()
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
            NetworkInfo networkMob = connectivityManager.GetNetworkInfo(Android.Net.ConnectivityType.Mobile);
            NetworkInfo networkWifi = connectivityManager.GetNetworkInfo(Android.Net.ConnectivityType.Wifi);

            if (networkMob.IsConnected || networkWifi.IsConnected)
                return true;
            else
                return false;
        }
    }

    public class AudioRecorder : Java.Lang.Object, IDisposable
    {
        MediaRecorder recorder;
        System.IO.FileStream f;
        private bool recording;
        private string outputFilename, tempFilename;
        bool failed = false;

        public void Dispose()
        {
            this.Dispose();
        }

        public AudioRecorder(string filename)
        {
            if (filename == "")
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("filename passed in is NULL");
                #endif
                return;
            }
			
            if (System.IO.File.Exists(filename))
                System.IO.File.Delete(filename);
				
            recorder = new MediaRecorder();
            recorder.SetAudioSource(AudioSource.Mic);
            recorder.SetOutputFormat(OutputFormat.ThreeGpp);
            recorder.SetAudioEncoder(AudioEncoder.Aac);
            tempFilename = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "tempfile.3gp");
            outputFilename = filename;
#if (DEBUG)
            System.Diagnostics.Debug.WriteLine("filename = {0}", filename);
#endif
            f = System.IO.File.Create(tempFilename);
            recorder.SetOutputFile(tempFilename);
            recorder.Prepare();
        }

        public void cancelOut()
        {
            f.Close();
            if (failed == false)
            {
                recorder.Reset();
                recorder.Release();
            }
            System.IO.File.Delete(tempFilename);
        }

        public void RecordStart()
        {
            OnStart(recorder);
        }

        public void RecordStop()
        {
            OnStop(recorder);
        }

        public void EndRecording()
        {
            OnCompletion(recorder);
        }

        public void OnStop(MediaRecorder recorder)
        {
            if (!failed)
            {
                f.Close();
                recorder.Stop();
                System.IO.File.Move(tempFilename, outputFilename);
                System.IO.File.Delete(tempFilename);
            }
        }

        public void OnStart(MediaRecorder recorder)
        {
            try
            {
                recorder.Start();
            } catch (Java.Lang.RuntimeException e)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("Runtime Exception thrown : {0}, {1}", e.Message, e.StackTrace);
                #endif
                failed = true;
            } catch (Java.Lang.Exception es)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("Exception thown : {0}, {1}", es.Message, es.StackTrace);
                #endif
                failed = true;
            }
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("failed = {0}", failed);
            #endif
        }

        public void OnCompletion(MediaRecorder recorder)
        {
            f.Close();
			
            if (!failed)
            {
                recorder.Reset();
                recorder.Release();
                try
                {
                    System.IO.File.Move(tempFilename, outputFilename);
                    System.IO.File.Delete(tempFilename);
                } catch
                {
                }
            }
        }
    }
}
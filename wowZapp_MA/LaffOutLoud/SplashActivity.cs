using System;
using System.IO;

using Android.App;
using Android.OS;
using Android.Widget;
using LOLApp_Common;
using System.Text;
using System.Net;

using LOLAccountManagement;
using wowZapp.Main;
using wowZapp.Login;

using WZCommon;

namespace wowZapp
{
    [Activity(Theme = "@style/Theme.Splash", NoHistory = true, MainLauncher = true, Icon = "@drawable/icolol",
              ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : Activity
    {


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            LaffOutOut l = new LaffOutOut(Application.Context);

            if (wowZapp.LaffOutOut.Singleton == null)
                Toast.MakeText(Application.Context, "Singleton is null. Bah!", ToastLength.Long);
            else
                Toast.MakeText(Application.Context, "Starting normally - phew!", ToastLength.Long);

            wowZapp.LaffOutOut.Singleton.ScreenXWidth = WindowManager.DefaultDisplay.Width;
            wowZapp.LaffOutOut.Singleton.ScreenYHeight = WindowManager.DefaultDisplay.Height;
				
            wowZapp.LaffOutOut.Singleton.resizeFonts = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth == 480f ? false : true;
				
            wowZapp.LaffOutOut.Singleton.bigger = (((float)wowZapp.LaffOutOut.Singleton.ScreenXWidth - 480f) / 100f) / 2f;
			
            AndroidData.IsAppActive = true;
            int timeout = 2500;
			
            if (string.IsNullOrEmpty(wowZapp.LaffOutOut.Singleton.ContentDirectory))
                wowZapp.LaffOutOut.Singleton.ContentDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

            if (!Directory.Exists(wowZapp.LaffOutOut.Singleton.ContentDirectory))
            {
                try
                {
                    Directory.CreateDirectory(wowZapp.LaffOutOut.Singleton.ContentDirectory);
/*#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Created lol data directory - {0}", wowZapp.LaffOutOut.Singleton.ContentDirectory);
#endif*/
                } catch (IOException e)
                {
                    Toast.MakeText(this, "Unable to create data directory", ToastLength.Short).Show();
                }
            } 
            /*#if DEBUG
			else
				System.Diagnostics.Debug.WriteLine ("Lol data directory - ", wowZapp.LaffOutOut.Singleton.ContentDirectory);
			#endif

#if DEBUG
			System.Diagnostics.Debug.WriteLine ("DeviceID before = {0}", AndroidData.DeviceID);
#endif*/

            if (AndroidData.DeviceID != null)
            {
                int t = AndroidData.DeviceID.Length, r = 0;
                string dupe = AndroidData.DeviceID;
                for (int m = 0; m < t; ++m)
                {
                    if (dupe [m] == '0')
                        r++;
                }
                if (r == t)
                {
                    AndroidData.NewDeviceID = createNewDeviceID();
                } else
                    AndroidData.NewDeviceID = AndroidData.DeviceID;
            } else
                AndroidData.NewDeviceID = createNewDeviceID();
            //});

#if DEBUG
            //System.Diagnostics.Debug.WriteLine ("DeviceID after = {0}", AndroidData.NewDeviceID);
#endif
            //grabCerts();
            string path = Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "INSTALL");
            if (!File.Exists(path))
            {
                AndroidData.LastConvChecked = new DateTime(1900, 1, 1);
                AndroidData.IsNewInstall = true;
                AndroidData.user = WZCommon.UserType.NewUser;

                try
                {
                    File.Create(path).Close();
                } catch (IOException)
                {
                    Toast.MakeText(Application.Context, Resource.String.debugFailToCreateInstall, ToastLength.Short).Show();
                }
            } else
            {
                AndroidData.IsNewInstall = false;
                AndroidData.user = WZCommon.UserType.ExistingUser;
            }
            //});

            Handler handler = new Handler();
            handler.PostDelayed(new Action(() =>
            {
                if (AndroidData.IsLoggedIn)
                {
                    StartActivity(typeof(HomeActivity));
                } else
                {
                    StartActivity(typeof(LoginChoiceActivity));
                }//end if else
                Finish();
            }), timeout);
        }

        private string createNewDeviceID()
        {
            string file = Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "snu.snu");

            if (File.Exists(file))
            {
                TextReader tr = new StreamReader(file);
                string device = tr.ReadLine();
                return device;
            }
            StringBuilder sb = new StringBuilder();
            RunOnUiThread(delegate
            {
                string[] allowed = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                int next = 0;
                Random r = new Random();
                for (int n = 0; n < 15; ++n)
                {
                    next = r.Next(10);
                    sb.Append(allowed [next]);
                }
            
#if DEBUG
                System.Diagnostics.Debug.WriteLine("New ID = {0}", sb.ToString());
#endif
                if (File.Exists(file))
                    File.Delete(file);
                using (TextWriter tw = File.CreateText(file))
                {
                    tw.WriteLine(sb.ToString());
                }
            });
			
            return sb.ToString();
        }
    }
}


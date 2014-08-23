using System;
using System.Runtime.InteropServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace wowZapp
{
    public static class GeneralUtils
    {
        [DllImport("/system/lib/libc.so")]
        static extern int __system_property_get(string name, StringBuilder value);
		
        public static bool[] CamRec()
        {
            bool[] res = new bool[2];
            int cameraCount = Android.Hardware.Camera.NumberOfCameras;
            res [0] = cameraCount != 0 ? true : false;
			
            StringBuilder v = new StringBuilder(93);
            int r = __system_property_get("ro.hardware", v);
            res [1] = v.ToString() == "goldfish" ? false : true;
            return res;
        }
	
        public static bool[] CamRecLong()
        {
            bool[] res = new bool[3];
            int cameraCount = Android.Hardware.Camera.NumberOfCameras;
            res [0] = cameraCount != 0 ? true : false;
			
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            res [1] = rec == "android.hardware.microphone" ? true : false;
			
            StringBuilder v = new StringBuilder(93);
            int r = __system_property_get("ro.hardware", v);
            res [2] = v.ToString() == "goldfish" ? false : true;
            return res;
        }

        public static void Alert(Context context, string title, string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.modalOK, (object o, Android.Content.DialogClickEventArgs e) =>
            {
                builder.Dispose();
            });
            AlertDialog alert = builder.Create();
            alert.Show();
        }
		
        public static void Alert(Context context, int title, int message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(Application.Context.Resources.GetString(message));
            builder.SetTitle(Application.Context.Resources.GetString(title));
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.modalOK, (object o, Android.Content.DialogClickEventArgs e) =>
            {
                builder.Dispose();
            });
            AlertDialog alert = builder.Create();
            alert.Show();
        }
		
        public static void Alert(Context c, string title, string message, Activity parent)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(c);
            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.modalOK, (object o, Android.Content.DialogClickEventArgs e) =>
            {
                builder.Dispose();
            });
            AlertDialog alert = builder.Create();
            alert.Show();
        }

        public static bool AlertRV(Context context, string title, string Message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(Message);
            builder.SetTitle(title);
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.modalOK, (object o, Android.Content.DialogClickEventArgs e) =>
            {
                builder.Dispose();
            });
            AlertDialog alert = builder.Create();
            alert.Show();
            return true;
        }
    }
}


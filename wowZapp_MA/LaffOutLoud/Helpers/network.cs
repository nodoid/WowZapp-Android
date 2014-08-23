using Android.Net;

namespace wowZapp
{
	public static class network
	{
		public static bool checkForNetwork {
			get {
				ConnectivityManager connectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService (Android.Content.Context.ConnectivityService);
				NetworkInfo networkMob = connectivityManager.GetNetworkInfo (Android.Net.ConnectivityType.Mobile);
				NetworkInfo networkWifi = connectivityManager.GetNetworkInfo (Android.Net.ConnectivityType.Wifi);
				if (networkMob.IsConnected || networkWifi.IsConnected) // only return true is there is actually a connection
					return true;
				else
					return false;
			}
		}
	}
}


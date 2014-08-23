using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Locations;

namespace wowZapp
{
	[Activity]			
	public class geolocation : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			// get the geolocation, then the address from it
			// to ensure something is in there, if location is dead, you're at the home of football - Anfield
		
			List<double> location = new List<double> ();
			List<string> address = new List<string> ();
		
			var geocoder = new Geocoder (this, Java.Util.Locale.Default);
			var locationManager = (LocationManager)GetSystemService (LocationService);
			
			var criteria = new Criteria () { Accuracy = Accuracy.NoRequirement };
			string bestProvider = locationManager.GetBestProvider (criteria, true);
			
			Location lastKnownLocation = locationManager.GetLastKnownLocation (bestProvider);
			if (lastKnownLocation != null) {
				location.Add (lastKnownLocation.Latitude);
				location.Add (lastKnownLocation.Longitude);
			} else {
				if (AndroidData.GeoLocation == null) {
					location.Add (53.430808);
					location.Add (-2.961709);
				} else
					location = AndroidData.GeoLocation;
			}
			
			// get the address for where you are

			var addr = geocoder.GetFromLocation (location [0], location [1], 1);
			
			address.Add (addr [0].Premises);
			address.Add (addr [0].Locality);
			address.Add (addr [0].CountryName);
			
			AndroidData.GeoLocation = location;
			AndroidData.GeoLocationAddress = address;
			AndroidData.GeoLocationUpdate = System.DateTime.Now;
			
			Finish ();
		}
	}
}


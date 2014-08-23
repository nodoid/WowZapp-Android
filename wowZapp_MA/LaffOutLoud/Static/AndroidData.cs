using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Telephony;
using Android.Provider;
using Android.Content.Res;
using Android.Util;

using LOLApp_Common;
using LOLAccountManagement;
using System.IO;
using WZCommon;

namespace wowZapp
{
    public class AndroidDataTypes
    {
        public class Contact
        {
            private string Phone;
            private string Name;

            public Contact(string phone, string name)
            {
                this.Phone = phone;
                this.Name = name;
            }
        }
    }

    public static class AndroidTablet
    {
        public static bool isTablet(Context context)
        {
            TelephonyManager manager = (TelephonyManager)context.GetSystemService(Context.TelephonyService);
            return manager.PhoneType == PhoneType.None ? true : false;
        }
		
        public static bool isTabletDevice(Context activityContext)
        {
            bool xlarge = ((activityContext.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) == ScreenLayout.SizeXlarge);

            if (xlarge)
            {
                Android.Util.DisplayMetrics metrics = new Android.Util.DisplayMetrics();
                Activity activity = (Activity)activityContext;
                activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
                if (metrics.DensityDpi == DisplayMetricsDensity.Default
                    || metrics.DensityDpi == DisplayMetricsDensity.High
                    || metrics.DensityDpi == DisplayMetricsDensity.Medium  
                    || metrics.DensityDpi == DisplayMetricsDensity.Xhigh)
                    return true;
            }
            return false;
        }
    }

    public static class AndroidData
    {
        private static TelephonyManager TeleManager;
	   
	
        /*public static void GrabContacts (Activity a)
		{
			String key, value;
			String[] columns = new String[] 
			{
				ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName,
				ContactsContract.CommonDataKinds.Phone.Number 
			};
			Android.Net.Uri mContacts = ContactsContract.CommonDataKinds.Phone.ContentUri;
			Android.Database.AbstractCursor cur = (Android.Database.AbstractCursor)a.ManagedQuery (mContacts, 
				columns, null, null, null);

			List<Contact> Contacts = new List<Contact> ();

			if (cur == null)
				return;
			if (cur.MoveToFirst ()) {
				do {
					value = cur.GetString (cur.GetColumnIndex (ContactsContract.CommonDataKinds.Phone.Number));
					key = cur.GetString (cur.GetColumnIndex (ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName));
					if (key != null && value != null) {
						
						Contacts.Add (new LOLAccountManagement.Contact () { });
						AndroidDataTypes.Contact A = new AndroidDataTypes.Contact (key, value);
					}
					
				} while (cur.MoveToNext());
				lC.ContactsSaveListAsync (Contacts, AuthToken);
			}
		}*/
		
        private static AppSettings pLOLappSettings;
        public static AppSettings LOLAppSettings
        {
            get
            {

                if (null == pLOLappSettings)
                {

                    if (File.Exists(AppSettingsFile))
                    {

                        pLOLappSettings = Serializer.XmlDeserializeObject<AppSettings>(AppSettingsFile);

                    } else
                    {

                        pLOLappSettings = new wowZapp.AppSettings();

                        Serializer.XmlSerializeObject<AppSettings>(pLOLappSettings, AppSettingsFile);

                    }//end if else

                }//end if

                return pLOLappSettings;

            }
            set
            {

                if (null == value)
                {
                    throw new ArgumentNullException("value is null!");
                }//end if

                pLOLappSettings = value;
                if (File.Exists(AppSettingsFile))
                {
                    File.Delete(AppSettingsFile);
                }//end if

                Serializer.XmlSerializeObject<AppSettings>(pLOLappSettings, AppSettingsFile);

            }//end get set

        }//end static AppSettings AppSettings

        private static string pAppSettingsFile;
        public static string AppSettingsFile
        {
            get
            {

                if (string.IsNullOrEmpty(pAppSettingsFile))
                {
                    if (string.IsNullOrEmpty(wowZapp.LaffOutOut.Singleton.ContentDirectory))
                        wowZapp.LaffOutOut.Singleton.ContentDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    pAppSettingsFile = Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "AppSettings.xml");
                }//end if

                return pAppSettingsFile;

            }//end get

        }//end static string AppSettingsFile



        public static List<string> SupportedSpeechKitLanguages
        {
            get
            {
                return new List<string>() {
					"en_us",
					"en_gb",
					"en_au",
					"fr_fr",
					"fr_ca",
					"it_it",
					"de_de",
					"es_es",
					"es_mx",
					"es_us",
					"ja_jp",
					"cn_ma",
					"zh_hk",
					"zh_TW",
					"nl_nl",
					"ko_kr",
					"sv_se",
					"no_no",
					"da_DK",
					"pl_PL",
					"pt_PT",
					"pt_BR"
				};
            }
        }

        public static void ClearSocialNetworkInfo()
        {
            AndroidData.FacebookAccessToken = string.Empty;
            AndroidData.FacebookAccessTokenExpiration = DateTime.Now;
            AndroidData.GooglePlusAccessToken = string.Empty;
            AndroidData.GooglePlusRefreshToken = string.Empty;
            AndroidData.GoogleAccessTokenExpiration = DateTime.Now;
            AndroidData.LinkedInAccessToken = string.Empty;
            AndroidData.LinkedInAccessTokenSecret = string.Empty;
            AndroidData.TwitterAccessToken = string.Empty;
            AndroidData.TwitterAccessTokenSecret = string.Empty;
            AndroidData.TwitterScreenName = string.Empty;
            AndroidData.TwitterUserId = string.Empty;
            AndroidData.YouTubeAccessToken = string.Empty;
            AndroidData.YouTubeRefreshToken = string.Empty;
            AndroidData.YouTubeAccessTokenExpiration = DateTime.Now;
        }

        private static Guid authToken;
        public static Guid AuthToken
        {
            get { return authToken; }
        }

        //public static LOLConnectClient lC { get; set; }

        public static string DeviceID
        {
            get
            {
                if (TeleManager == null)
                {
                    //ModalMessage.modal m = new ModalMessage.modal(Resource.String.errorNoGetDeviceID);
                    //m.showAlertDialog();
                    //m.Dispose();
                }
                return TeleManager.DeviceId;
            }
        }

        public static void DestroyReferences()
        {
            TeleManager = null;
        }

        public static void SetTeleManager(Android.Content.Context a)
        {
            TeleManager = (Android.Telephony.TelephonyManager)a.GetSystemService(Context.TelephonyService);
        }
		
        public static string Phonenumber
        {
            get
            {
                if (TeleManager == null)
                {
                    //ModalMessage.modal m = new ModalMessage.modal(Resource.String.errorNoGetDeviceID);
                    //m.showAlertDialog();
                    //m.Dispose();
                }
                return TeleManager.Line1Number;
            }
        }

        public static string DeviceType
        {
            get
            {
                if (TeleManager == null)
                {
                    //ModalMessage.modal m = new ModalMessage.modal(Resource.String.errorDeviceType);
                    //m.showAlertDialog();
                    //m.Dispose();
                }
                return TeleManager.PhoneType.ToString();
            }
        }

        public static string NewDeviceID
        {
            get
            {
                return LOLAppSettings.NewDeviceID;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.NewDeviceID = value;
                LOLAppSettings = settings;
            }//end get set
        }



        public static DateTime LastConvChecked
        {
            get { return LOLAppSettings.LastConvChecked;}
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.LastConvChecked = value;
                LOLAppSettings = settings;
            }
        }


        public static string ServiceAuthToken
        {
            get
            {
                return LOLAppSettings.ServiceAuthToken;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.ServiceAuthToken = value;
                LOLAppSettings = settings;
            }//end get set
        }





        private static UserType usr;
        public static UserType user
        {
            get
            {
                if (usr != WZCommon.UserType.ExistingUser)
                    return WZCommon.UserType.NewUser;
                else
                    return WZCommon.UserType.ExistingUser;
            }
            set
            {
                usr = value;
            }
        }






        public static bool IsLoggedIn
        {
            get
            {
                return LOLAppSettings.IsLoggedIn;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.IsLoggedIn = value;
                LOLAppSettings = settings;
            }//end get set
        }




        private static bool isActive;
        public static bool IsAppActive
        {
            get { return isActive; }
            set { isActive = value; }
        }


        private static bool hasImages;
        public static bool HasImages
        {
            get { return hasImages;}
            set { hasImages = value;}
        }


        public static User CurrentUser
        {
            get
            {
                return LOLAppSettings.CurrentUser;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.CurrentUser = value;
                LOLAppSettings = settings;
            }//end get set

        }//end static User CurrentUser


        public static int Skin
        {
            get{ return LOLAppSettings.Skin;}
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.Skin = value;
                LOLAppSettings = settings;
            }
        }

        public static string FacebookAccessToken
        {
            get
            {
                return LOLAppSettings.FacebookAccessToken;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.FacebookAccessToken = value;
                LOLAppSettings = settings;
            }//end get set
        }

        public static DateTime FacebookAccessTokenExpiration
        {
            get
            {
                return LOLAppSettings.FacebookAccessTokenExpiration;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.FacebookAccessTokenExpiration = value;
                LOLAppSettings = settings;
            }//end get set
        }

        public static string TwitterAccessToken
        {
            get
            {
                return LOLAppSettings.TwitterAccessToken;
            }
            set
            {

                AppSettings settings = LOLAppSettings;
                settings.TwitterAccessToken = value;
                LOLAppSettings = settings;

            }//end get set
        }

        public static string TwitterAccessTokenSecret
        {
            get
            {
                return LOLAppSettings.TwitterAccessTokenSecret;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.TwitterAccessTokenSecret = value;
                LOLAppSettings = settings;
            } //end get set
        }

        public static string TwitterUserId
        {
            get
            {
                return LOLAppSettings.TwitterUserId;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.TwitterUserId = value;
                LOLAppSettings = settings;
            } //end get set
        }

        public static string TwitterScreenName
        {
            get
            {
                return LOLAppSettings.TwitterScreenName;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.TwitterScreenName = value;
                LOLAppSettings = settings;
            }//end get set
        }

        public static string LinkedInAccessToken
        {
            get
            {
                return LOLAppSettings.LinkedInAccessToken;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.LinkedInAccessToken = value;
                LOLAppSettings = settings;
            }//end get set
        }

        public static string LinkedInAccessTokenSecret
        {
            get
            {
                return LOLAppSettings.LinkedInAccessTokenSecret;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.LinkedInAccessTokenSecret = value;
                LOLAppSettings = settings;
            }//end get set
        }

        public static string GooglePlusAccessToken
        {
            get
            {
                return LOLAppSettings.GooglePlusAccessToken;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.GooglePlusAccessToken = value;
                LOLAppSettings = settings;
            }//end get set
        }

        public static string GooglePlusRefreshToken
        {
            get
            {
                return LOLAppSettings.GooglePlusRefreshToken;
            }
            set
            {

                AppSettings settings = LOLAppSettings;
                settings.GooglePlusRefreshToken = value;
                LOLAppSettings = settings;

            }//end get set
        }

        public static DateTime GoogleAccessTokenExpiration
        {
            get
            {
                return LOLAppSettings.GoogleAccessTokenExpiration;
            }
            set
            {

                AppSettings settings = LOLAppSettings;
                settings.GoogleAccessTokenExpiration = value;
                LOLAppSettings = settings;

            }//end get set
        }

        public static string YouTubeAccessToken
        {
            get
            {
                return LOLAppSettings.YouTubeAccessToken;
            }
            set
            {

                AppSettings settings = LOLAppSettings;
                settings.YouTubeAccessToken = value;
                LOLAppSettings = settings;

            }//end get set
        }

        public static string YouTubeRefreshToken
        {
            get
            {
                return LOLAppSettings.YouTubeRefreshToken;
            }
            set
            {

                AppSettings settings = LOLAppSettings;
                settings.YouTubeRefreshToken = value;
                LOLAppSettings = settings;

            }//end get set
        }

        public static DateTime YouTubeAccessTokenExpiration
        {
            get
            {
                return LOLAppSettings.YouTubeAccessTokenExpiration;
            }
            set
            {

                AppSettings settings = LOLAppSettings;
                settings.YouTubeAccessTokenExpiration = value;
                LOLAppSettings = settings;

            }//end get set
        }
        
        public static DateTime? LastContactUpdate
        {
            get
            {
                return LOLAppSettings.LastContactUpdateDate;
            }
            set
            {

                AppSettings settings = LOLAppSettings;
                settings.LastContactUpdateDate = value;
                LOLAppSettings = settings;

            }//end get set
        }

        public static bool IsNewInstall
        {
            get
            {
                return LOLAppSettings.IsNewInstall;
            }
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.IsNewInstall = value;
                LOLAppSettings = settings;
            }//end get set
        }

        public static bool GeoLocationEnabled
        {
            get { return LOLAppSettings.GeoLocationEnabled;}
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.GeoLocationEnabled = value;
                LOLAppSettings = settings;
            }
        }

        public static List<double> GeoLocation
        {
            get { return LOLAppSettings.GeoLocation;}
            set
            {
                AppSettings settings = LOLAppSettings;
                if (settings.GeoLocation == null)
                    settings.GeoLocation = new List<double>();
                settings.GeoLocation = value;
                LOLAppSettings = settings;
            }
        }

        public static DateTime GeoLocationUpdate
        {
            get { return LOLAppSettings.GeoLocationUpdate;}
            set
            {
                AppSettings settings = LOLAppSettings;
                settings.GeoLocationUpdate = value;
                LOLAppSettings = settings;
            }
        }

        public static List<string> GeoLocationAddress
        {
            get { return LOLAppSettings.GeoLocationAddress;}
            set
            {
                AppSettings settings = LOLAppSettings;
                if (LOLAppSettings.GeoLocationAddress == null)
                    LOLAppSettings.GeoLocationAddress = new List<string>();
                settings.GeoLocationAddress = value;
                LOLAppSettings = settings;
            }
        }

        //NOTE: Not sure if we need to keep these
        private static string ImageFileName;
        public static string imageFileName
        {
            get { return ImageFileName; }
            set { ImageFileName = value; }
        }
    }

    public class AppSettings
    {

        public AppSettings()
        {
        }//end ctor

        public bool GeoLocationEnabled
        {
            get;
            set;
        }

        public List<double> GeoLocation
        {
            get;
            set;
        }

        public List<string> GeoLocationAddress
        {
            get;
            set;
        }

        public DateTime GeoLocationUpdate
        {
            get;
            set;
        }

        public int Skin
        {
            get;
            set;
        }

        public bool IsNewInstall
        {
            get;
            set;
        }//end bool IsNewInstall

        public DateTime LastConvChecked
        {
            get;
            set;
        }


        public bool IsLoggedIn
        {
            get;
            set;
        }//end bool IsLoggedIn



        public string FacebookAccessToken
        {
            get;
            set;
        }//end string FacebookAccessToken



        public DateTime FacebookAccessTokenExpiration
        {
            get;
            set;
        }//end DateTime FacebookAccessTokenExpiration



        public string TwitterAccessToken
        {
            get;
            set;
        }//end string TwitterAccessToken



        public string TwitterAccessTokenSecret
        {
            get;
            set;
        }//end string TwitterAccessTokenSecret



        public string TwitterUserId
        {
            get;
            set;
        }//end string TwitterUserId



        public string TwitterScreenName
        {
            get;
            set;
        }//end string TwitterScreenName



        public string LinkedInAccessToken
        {
            get;
            set;
        }//end string LinkedInAccessToken



        public string LinkedInAccessTokenSecret
        {
            get;
            set;
        }//end string LinkedInAccessTokenSecret




        public string GooglePlusAccessToken
        {
            get;
            set;
        }//end string GooglePlusAccessToken



        public string GooglePlusRefreshToken
        {
            get;
            set;
        }//end string GooglePlusRefreshToken



        public DateTime GoogleAccessTokenExpiration
        {
            get;
            set;
        }//end string GoogleAccessTokenExpiration



        public string YouTubeAccessToken
        {
            get;
            set;
        }//end string YouTubeAccessToken



        public string YouTubeRefreshToken
        {
            get;
            set;
        }//end string YouTubeRefreshToken



        public DateTime YouTubeAccessTokenExpiration
        {
            get;
            set;
        }//end DateTime YouTubeAccessTokenExpiration



        public string NewDeviceID
        {
            get;
            set;
        }//end string NewDeviceID



        public string ServiceAuthToken
        {
            get;
            set;
        }//end Guid ServiceAuthToken



        public User CurrentUser
        {
            get;
            set;
        }//end User CurrentUser



        public DateTime? LastContactUpdateDate
        {
            get;
            set;
        }//end DateTime? LastContactUpdateDate

    }//end class AppSettings
}
// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Text;
using LOLAccountManagement;

namespace LOLApp_Common
{
    public class LOLConstants
    {

        //
        // Values
        //
        public static DateTime UnixTime
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0);
            }//end get

        }//end static DateTime UnixTime



        public const float DefaultProfilePicSize = 512f;



        private static EndpointAddress pLOLConnectEndpoint;
        public static EndpointAddress LOLConnectEndpoint
        {
            get
            {
                if (null == pLOLConnectEndpoint)
                {
                    pLOLConnectEndpoint = new EndpointAddress(LinkLOLConnectServiceUrl);
                }//end if
                return pLOLConnectEndpoint;
            }//end get

        }//end static EndpointAddress LOLConnectEndpoint



        private static EndpointAddress pLOLMessageEndpoint;
        public static EndpointAddress LOLMessageEndpoint
        {
            get
            {
                if (null == pLOLMessageEndpoint)
                {
                    pLOLMessageEndpoint = new EndpointAddress(LinkLOLMessengerServiceUrl);
                }//end if
                return pLOLMessageEndpoint;
            }//end get

        }//end static EndpointAddress LOLMessengerEndpoint




        private static BasicHttpBinding pDefaultHttpBinding;
        public static BasicHttpBinding DefaultHttpBinding
        {
            get
            {
                if (null == pDefaultHttpBinding)
                {
                    pDefaultHttpBinding = new BasicHttpBinding() {

						Name = "basicHttpBinding",
						MaxReceivedMessageSize = 2147483647,
						MaxBufferSize = 2147483647,
						MaxBufferPoolSize = 2147483647,
						AllowCookies = false,
						BypassProxyOnLocal = false,
						HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
						MessageEncoding = WSMessageEncoding.Text,
						TextEncoding = Encoding.UTF8,
						TransferMode = TransferMode.Buffered,
						UseDefaultWebProxy = true

					};

                    pDefaultHttpBinding.ReaderQuotas.MaxDepth = 32;
                    pDefaultHttpBinding.ReaderQuotas.MaxStringContentLength = 2147483647;
                    pDefaultHttpBinding.ReaderQuotas.MaxArrayLength = 2147483647;
                    pDefaultHttpBinding.ReaderQuotas.MaxBytesPerRead = 2147483647;
                    pDefaultHttpBinding.ReaderQuotas.MaxNameTableCharCount = 2147483647;
                    pDefaultHttpBinding.OpenTimeout = new TimeSpan(0, 1, 0);
                    pDefaultHttpBinding.CloseTimeout = new TimeSpan(0, 1, 0);
                    pDefaultHttpBinding.SendTimeout = new TimeSpan(0, 1, 0);
                }//end if

                return pDefaultHttpBinding;

            }//end get

        }//end static BasicHttpBinding DefaultHttpBinding

        public static Guid DefaultGuid
        {
            get
            {
                return new Guid("00000000-0000-0000-0000-000000000000");
            }//end get

        }//end static Guid DefaultGuid

        //
        // Login screen
        //
        public const float HeaderViewHeight = 55f;

        //
        // Angles
        //
        public const double RadToDeg = 180.0 / Math.PI;
        public const double DegToRad = Math.PI / 180.0;

        //
        // File system
        //
        public const string ImageCacheFolder = "ImageCache";
        public const string ContentPackIconFormat = "icon_{0:0000}.cp";
        public const string ContentPackAdFormat = "ad_{0:0000}.cp";
        public const string ContentPackItemIconFormat = "icon_{0:0000}.cpi";
        public const string ContentPackItemDataFormat = "data_{0:0000}.cpi";
        public const string VoiceRecordingFormat = "voice_{0}_{1:0000}.3gp";
        public const string PollingStepDataFormat = "pollimage{0:00}_{1}_{2:0000}.pol";
        public const string AnimationObjectDataFormat = "animation{0}_{1}_{2:0000}.anm";

        //
        // Links
        //
        //public const string AuthFinishedServer = "http://software.tavlikos.com";
        public const string AuthFinishedServer = "http://www.opensporksoftware.com";
        // Facebook
        public const string LinkFacebookAuthUrl = "https://www.facebook.com/dialog/oauth?client_id={0}" +
            "&redirect_uri=http://www.wowzapp.com/mobileloginsuccess/index.html" +
            "&scope=publish_stream,user_photos,user_about_me,email,friends_about_me,user_birthday,friends_birthday,user_videos" +
            "&response_type=token" +
            "&display=touch";
		
        public const string LinkFacebookLoginSuccessUrl = "http://www.wowzapp.com/mobileloginsuccess/index.html";
        // Twitter
        public const string LinkTwitterRequestTokenUrl = "https://api.twitter.com/oauth/request_token";
        public const string LinkTwitterAccessTokenUrl = "https://api.twitter.com/oauth/access_token";
        public const string LinkTwitterAuthUrl = "https://twitter.com/oauth/";
        public const string LinkTwitterCallbackUrl = "http://www.wowzapp.com/mobileloginsuccess/index.html";
        // LinkedIn
        public const string LinkLinkedInCallbackUrl = "http://www.opensporksoftware.com";
        public const string LinkLinkedInRequestTokenUrl = "https://api.linkedin.com/uas/oauth/requestToken?scope=r_basicprofile+r_emailaddress+r_network+rw_nus";
        public const string LinkLinkedInAuthUrl = "https://www.linkedin.com/uas/oauth/";
        public const string LinkLinkedInAccessTokenUrl = "https://api.linkedin.com/uas/oauth/accessToken";
        // Google+
        public const string LinkGooglePlusAuthUrl = "https://accounts.google.com/o/oauth2/auth";
        public const string LinkGooglePlusRequestTokenUrl = "https://accounts.google.com/o/oauth2/token";
        public const string LinkGooglePlusCallbackUrl = "https://www.wowzapp.com/oauth2callback";
        // YouTube
        public const string LinkYouTubeAuthUrl = "https://accounts.google.com/o/oauth2/auth";
        public const string LinkYouTubeCallbackUrl = "https://www.wowzapp.com/oauth2callback";
        // LOLConnect service
        public const string LinkLOLConnectServiceUrl = "http://50.57.231.101/LolAccounts/LOLConnect.svc";
        public const string LinkLOLMessengerServiceUrl = "http://50.57.231.101/LolMessages/LOLConnect.svc";
        // LOL app website
        public const string LinkLOLAppWebsiteUrl = "http://www.wowzapp.com";
		
		
		
        //
        // Social networks
        //
        public const string FacebookAPIKey = "443836379006145";
        public const string FacebookAppSecret = "3dbffc1a9d262790c7991917cede7077";
        public const string TwitterConsumerKey = "ZFsXUrxQRfgrMloXqChLzA";
        public const string TwitterConsumerSecret = "oNNL7AAifwnrxVfk6XCdReaXFcCYut3mfxRVHVw7A";
        public const string LinkedInConsumerKey = "7tbj0wubvkp4";
        public const string LinkedInConsumerSecret = "JHwHpnotpi0CGb6n";
        public const string GooglePlusConsumerKey = "785007815134-r82ht2ubd0sj80rl0880ivpshpoejg90.apps.googleusercontent.com";
        public const string GooglePlusConsumerSecret = "hmkuAeFYl3h52GZ_onDzVwAu";



        //
        // Regular expressions
        //
        public const string RegexEmailMatch = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}$";



        //
        // Database
        //
        public const string DBClauseSyncOff = "PRAGMA SYNCHRONOUS=OFF;";
        public const string DBClauseVacuum = "VACUUM;";
		
    }
}


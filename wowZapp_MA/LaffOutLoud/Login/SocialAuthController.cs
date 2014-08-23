using System;

using Android.App;
using Android.Widget;
using Android.Content;
using Android.Webkit;
using Android.OS;
using Android.Runtime;

using LOLApp_Common;
using LOLAccountManagement;

using WZCommon;

using System.Threading;

namespace wowZapp.Login
{
    public class SocialAuthController
    {
        public SocialAuthController(AccountOAuth.OAuthTypes providerType, Context c)
        {
            this.ProviderType = providerType;
            int type = 0;
            switch (providerType)
            {
                case AccountOAuth.OAuthTypes.FaceBook:
                    type = 1;
                    break;
                case AccountOAuth.OAuthTypes.Twitter:
                    type = 2;
                    break;
                case AccountOAuth.OAuthTypes.LinkedIn:
                    type = 3;
                    break;
                case AccountOAuth.OAuthTypes.Google:
                    type = 4;
                    break;
                case AccountOAuth.OAuthTypes.YouTube:
                    type = 5;
                    break;
            }
            Intent i = new Intent(c, typeof(WebViewer));
            i.PutExtra("type", type);
            c.StartActivity(i);
        }

        public AccountOAuth.OAuthTypes ProviderType { get; private set; }
    }

    [Activity(ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class WebViewer : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LoginSocialNetwork);
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            Context context = header.Context;
            Header.fontsize = 36f;
            
            int t = base.Intent.GetIntExtra("type", 0);
			
            switch (t)
            {
                case 1:
                    this.ProviderType = AccountOAuth.OAuthTypes.FaceBook;
                    this.Provider = new LFacebookManager(LOLConstants.FacebookAPIKey, LOLConstants.FacebookAppSecret);
                    Header.headertext = Application.Context.Resources.GetString(Resource.String.commonFacebookHeaderView);
                    break;

                case 2:
                    this.ProviderType = AccountOAuth.OAuthTypes.Twitter;
                    this.Provider = new LTwitterManager(LOLConstants.TwitterConsumerKey, LOLConstants.TwitterConsumerSecret, AndroidData.TwitterAccessToken, AndroidData.TwitterAccessTokenSecret, 
                        AndroidData.TwitterScreenName, AndroidData.TwitterUserId);
                    Header.headertext = Application.Context.Resources.GetString(Resource.String.commonTwitterHeaderView);
                    break;

                case 3:
                    this.ProviderType = AccountOAuth.OAuthTypes.LinkedIn;
                    this.Provider = new LLinkedInManager(LOLConstants.LinkedInConsumerKey, LOLConstants.LinkedInConsumerSecret, AndroidData.LinkedInAccessToken, AndroidData.LinkedInAccessTokenSecret);
                    Header.headertext = Application.Context.Resources.GetString(Resource.String.commonLinkedInHeaderView);
                    break;

                case 4:
                    this.ProviderType = AccountOAuth.OAuthTypes.Google;
                    HttpHelper helper = new HttpHelper();
                    this.Provider = new LGooglePlusManager(LOLConstants.GooglePlusConsumerKey, LOLConstants.GooglePlusConsumerSecret, helper);
                    Header.headertext = Application.Context.Resources.GetString(Resource.String.commonGooglePlusHeaderView);
                    break;

                case 5:
                    this.ProviderType = AccountOAuth.OAuthTypes.YouTube;
                    this.Provider = new LYouTubeManager(LOLConstants.GooglePlusConsumerKey, LOLConstants.GooglePlusConsumerSecret);
                    Header.headertext = Application.Context.Resources.GetString(Resource.String.commonYouTubeHeaderView);
                    break;
            }

            ImageHelper.fontSizeInfo(context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            CookieSyncManager.CreateInstance(this);
            CookieSyncManager.Instance.StartSync();

            string url = Provider.BrowserAuthUrl;
            string cookieString = "cookieName=''";

            CookieManager cookieManager = CookieManager.Instance;
            cookieManager.SetAcceptCookie(true);
            cookieManager.SetCookie(url, cookieString);

            WebView webView = FindViewById<WebView>(Resource.Id.webView1);
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.SavePassword = true;
            webView.Settings.SaveFormData = true;

            ThreadPool.QueueUserWorkItem(delegate
            {
                RunOnUiThread(delegate
                {
                    webView.LoadUrl(url);
                    webView.SetWebViewClient(new dealWithWebView(this, this.ProviderType, this.Provider, btnBack));
                });
            });
        }

        public AccountOAuth.OAuthTypes ProviderType { get; private set; }
        public ISocialProviderManager Provider { get; private set; }
    }

    class dealWithWebView : WebViewClient
    {
        WebView webView;
        Activity parent;
        private Context c;
        public AccountOAuth.OAuthTypes ProviderType { get; private set; }
        public ISocialProviderManager Provider { get; private set; }
        public event AccessTokenReceivedHandler AccessTokenReceived;

        public dealWithWebView(Activity f, AccountOAuth.OAuthTypes ProviderType, ISocialProviderManager Provider, ImageButton btnBack)
        {
            this.parent = f;
            this.Provider = Provider;
            this.ProviderType = ProviderType;
            string authUrl = this.Provider.BrowserAuthUrl;
            user = AndroidData.user;
            btnBack.Click += delegate
            {
                parent.Finish();
            };
            switch (ProviderType)
            {
                case AccountOAuth.OAuthTypes.Google:
                    AccessTokenReceived += (occurred, expires, accessToken, accessTokenSecret, refreshToken) =>
                    {
                        AndroidData.GooglePlusAccessToken = accessToken;
                        AndroidData.GooglePlusRefreshToken = refreshToken;
                        AndroidData.GoogleAccessTokenExpiration = expires;
                        startUserLogin(Provider);
                    };
                    break;

                case AccountOAuth.OAuthTypes.FaceBook:
                    LFacebookManager lFace = new LFacebookManager(LOLConstants.FacebookAPIKey, LOLConstants.FacebookAppSecret);
                    AccessTokenReceived += (occurred, expires, accessToken, accessTokenSecret, refreshToken) =>
                    {
                        try
                        {
                            if (lFace.RefreshAccessToken())
                            {
                                AndroidData.FacebookAccessToken = lFace.AccessToken;
                                AndroidData.FacebookAccessTokenExpiration = lFace.AccessTokenExpirationTime ?? DateTime.Now;
                            }
                        } catch
                        {
                            AndroidData.FacebookAccessToken = accessToken;
                            AndroidData.FacebookAccessTokenExpiration = expires;
                        }
                        startUserLogin(Provider);
                    };
                    break;

                case AccountOAuth.OAuthTypes.LinkedIn:
                    AccessTokenReceived += (occurred, expires, accessToken, accessTokenSecret, refreshToken) =>
                    {
                        AndroidData.LinkedInAccessToken = accessToken;
                        AndroidData.LinkedInAccessTokenSecret = accessTokenSecret;
                        startUserLogin(Provider);
                    };
                    break;

                case AccountOAuth.OAuthTypes.Twitter:
                    LTwitterManager lTwit = new LTwitterManager(LOLConstants.TwitterConsumerKey, LOLConstants.TwitterConsumerSecret, AndroidData.TwitterAccessToken, AndroidData.TwitterAccessTokenSecret, 
                        AndroidData.TwitterScreenName, AndroidData.TwitterUserId);
                    AccessTokenReceived += (occurred, expires, accessToken, accessTokenSecret, refreshToken) =>
                    {
                        AndroidData.TwitterAccessToken = accessToken;
                        AndroidData.TwitterAccessTokenSecret = accessTokenSecret;
                        AndroidData.TwitterScreenName = lTwit.ScreenName;
                        AndroidData.TwitterUserId = lTwit.UserId;
                        startUserLogin(Provider);
                    };
                    break;

                case AccountOAuth.OAuthTypes.YouTube:
                    AccessTokenReceived += (occurred, expires, accessToken, accessTokenSecret, refreshToken) =>
                    {
                        AndroidData.YouTubeAccessToken = accessToken;
                        AndroidData.YouTubeRefreshToken = refreshToken;
                        AndroidData.YouTubeAccessTokenExpiration = expires;
                        startUserLogin(Provider);
                    };
                    break;
            }
        }

        private string urlString;

        public override void OnPageFinished(WebView view, string url)
        {
            webView = view;
            c = webView.Context;
            this.urlString = url;

            try
            {
                switch (this.ProviderType)
                {

                    case AccountOAuth.OAuthTypes.FaceBook:
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            parent.RunOnUiThread(delegate
                            {
                                this.AuthorizeFacebook();
                            });
                        });
                        break;

                    case AccountOAuth.OAuthTypes.Twitter:
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            parent.RunOnUiThread(delegate
                            {
                                this.AuthorizeTwitter();
                            });
                        });
                        break;

                    case AccountOAuth.OAuthTypes.LinkedIn:
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            parent.RunOnUiThread(delegate
                            {
                                this.AuthorizeLinkedIn();
                            });
                        });
                        break;

                    case AccountOAuth.OAuthTypes.Google:
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            parent.RunOnUiThread(delegate
                            {
                                this.AuthorizeGooglePlus();
                            });
                        });
                        break;

                    case AccountOAuth.OAuthTypes.YouTube:
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            parent.RunOnUiThread(delegate
                            {
                                this.AuthorizeYouTube();
                            });
                        });
                        break;
                }
            } catch
            {
                parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewMessageConnectionProblem, ToastLength.Short).Show());
            }
        }

        public void OnReceivedError(WebView view, int errorCode, String description, String failingUrl)
        {
            if (errorCode != -999)
            {
                parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewMessageConnectionProblem, ToastLength.Short).Show());
            }
        }

        private void AuthorizeFacebook()
        {
            try
            {
                string urlString = this.webView.Url.ToString();

                if (urlString.StartsWith(LOLConstants.LinkFacebookLoginSuccessUrl))
                {
                    if (this.Provider.UserAccepted(urlString))
                    {
                        if (this.Provider.CheckForAccessToken(urlString))
                        {
                            if (null != this.AccessTokenReceived)
                            {
                                this.AccessTokenReceived(DateTime.Now, this.Provider.AccessTokenExpirationTime ?? DateTime.Now,
									                     this.Provider.AccessToken, string.Empty, string.Empty);
                            }
                            parent.RunOnUiThread(delegate
                            {
                                GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.loginAuthGood), "Facebook", parent);
                            });
                        } else
                        {
                            parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewMessageAccessTokenProblem, ToastLength.Short).Show());
                        }
                    } else
                    {
                        parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewUserCancelledAuth, ToastLength.Short).Show());
                    }
                }
            } catch (Exception ex)
            {
                parent.RunOnUiThread(() => Toast.MakeText(c, ex.ToString(), ToastLength.Short).Show());
            }
        }

        private void AuthorizeTwitter()
        {

            try
            {
                string requestString = this.webView.Url.ToString();

                if (!string.IsNullOrEmpty(requestString) && requestString.StartsWith(LOLConstants.LinkTwitterCallbackUrl))
                {
                    string queryString = requestString.Substring(LOLConstants.LinkTwitterCallbackUrl.Length + 1);
                    if (this.Provider.UserAccepted(queryString))
                    {
                        if (this.Provider.CheckForAccessToken(requestString))
                        {
                            if (null != this.AccessTokenReceived)
                            {
                                this.AccessTokenReceived(DateTime.Now, DateTime.MinValue, this.Provider.AccessToken, 
									this.Provider.AccessTokenSecret, string.Empty);
                            }
                            parent.RunOnUiThread(delegate
                            {
                                GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.loginAuthGood), "Twitter", parent);
                            });
                        } else
                        {
                            parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewMessageAccessTokenProblem, ToastLength.Short).Show());
                        }
                    } else
                    {
                        parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewUserCancelledAuth, ToastLength.Short).Show());
                    }
                }
            } catch (Exception ex)
            {
                parent.RunOnUiThread(() => Toast.MakeText(c, ex.ToString(), ToastLength.Short).Show());
            }
        }

        private void AuthorizeLinkedIn()
        {
            try
            {

                string requestString = this.webView.Url.ToString();

                if (!string.IsNullOrEmpty(requestString) && requestString.StartsWith(LOLConstants.LinkLinkedInCallbackUrl))
                {
                    string queryString = requestString.Substring(LOLConstants.LinkLinkedInCallbackUrl.Length + 1);

                    if (this.Provider.UserAccepted(queryString))
                    {
                        if (this.Provider.CheckForAccessToken(requestString))
                        {
                            if (null != this.AccessTokenReceived)
                            {
                                this.AccessTokenReceived(DateTime.Now, DateTime.MinValue, this.Provider.AccessToken, 
									this.Provider.AccessTokenSecret, string.Empty);
                            }
                            parent.RunOnUiThread(delegate
                            {
                                GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.loginAuthGood), "LinkedIn", parent);
                            });
                        } else
                        {
                            parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewMessageAccessTokenProblem, ToastLength.Short).Show());
                        }
                    } else
                    {
                        parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewUserCancelledAuth, ToastLength.Short).Show());
                    }
                }
            } catch (Exception ex)
            {
                parent.RunOnUiThread(() => Toast.MakeText(c, ex.ToString(), ToastLength.Short).Show());
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Linked in connection problem : {0}", ex.ToString());
#endif
            }
        }

        private void AuthorizeGooglePlus()
        {
            try
            {
                string requestString = this.webView.Url.ToString();

                if (!string.IsNullOrEmpty(requestString) && requestString.StartsWith(LOLConstants.LinkGooglePlusCallbackUrl))
                {
                    if (this.Provider.UserAccepted(requestString))
                    {
                        if (this.Provider.CheckForAccessToken(requestString))
                        {
                            if (null != this.AccessTokenReceived)
                            {
                                this.AccessTokenReceived(DateTime.Now, this.Provider.AccessTokenExpirationTime ?? DateTime.Now,
									this.Provider.AccessToken, string.Empty, this.Provider.RefreshToken);
                            }
                            parent.RunOnUiThread(delegate
                            {
                                GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.loginAuthGood), "Google+", parent);
                            });
                        } else
                        {
                            parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewMessageAccessTokenProblem, ToastLength.Short).Show());
                        }
                    } else
                    {
                        parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewUserCancelledAuth, ToastLength.Short).Show());
                    }
                }
            } catch (Exception ex)
            {
                parent.RunOnUiThread(() => Toast.MakeText(c, ex.ToString(), ToastLength.Short).Show());
            }
        }

        private void AuthorizeYouTube()
        {
            try
            {
                string requestString = this.webView.Url.ToString();

                if (!string.IsNullOrEmpty(requestString) && requestString.StartsWith(LOLConstants.LinkYouTubeCallbackUrl))
                {
                    if (this.Provider.UserAccepted(requestString))
                    {
                        if (this.Provider.CheckForAccessToken(requestString))
                        {
                            if (null != this.AccessTokenReceived)
                            {
                                this.AccessTokenReceived(DateTime.Now, this.Provider.AccessTokenExpirationTime ?? DateTime.Now, this.Provider.AccessToken, string.Empty, this.Provider.RefreshToken);
                            }
                            parent.RunOnUiThread(delegate
                            {
                                GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.loginAuthGood), "YouTube", parent);
                            });
                        } else
                        {
                            parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewMessageAccessTokenProblem, ToastLength.Short).Show());
                        }
                    } else
                    {
                        parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.alertViewUserCancelledAuth, ToastLength.Short).Show());
                    }
                }
            } catch (Exception ex)
            {
                parent.RunOnUiThread(() => Toast.MakeText(c, ex.ToString(), ToastLength.Short).Show());
            }
        }

        private void ShowAfterLoginScreen()
        {
            if (AndroidData.user == UserType.NewUser)
            {
                AndroidData.user = UserType.ExistingUser;
                Intent i = new Intent(parent, typeof(Contacts.GetConnectedButtonsActivity));
                i.PutExtra("backButton", false);
                i.PutExtra("continueButton", false);
                System.Threading.Tasks.Task.Factory.StartNew(() => parent.Finish())
					.ContinueWith(task =>
                {
                    parent.StartActivity(i);
                });
            } else
            {
                Intent ii = new Intent(parent, typeof(Main.HomeActivity));
                System.Threading.Tasks.Task.Factory.StartNew(() => parent.Finish())
					.ContinueWith(task =>
                {
                    ii.AddFlags(ActivityFlags.ClearTop);
                    parent.StartActivity(ii);
                });
            }
        }

        private void startUserLogin(ISocialProviderManager provider)
        {
            parent.RunOnUiThread(() => Toast.MakeText(c, Resource.String.commonLoggingIn, ToastLength.Short).Show());
            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding,
				LOLConstants.LOLConnectEndpoint);
            service.AuthenticationTokenGetCompleted += Service_AuthenticationTokenGetCompleted;
            service.AuthenticationTokenGetAsync(AndroidData.NewDeviceID, provider);
        }

        UserType user;

        private void Service_AuthenticationTokenGetCompleted(object sender, AuthenticationTokenGetCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.AuthenticationTokenGetCompleted -= Service_AuthenticationTokenGetCompleted;

            string activityMessage = user == UserType.NewUser ?
                 Application.Context.Resources.GetString(Resource.String.loginRegisteringUser) :
                 Application.Context.Resources.GetString(Resource.String.loginLoggingIn);

            parent.RunOnUiThread(delegate
            {
                Toast.MakeText(c, activityMessage, ToastLength.Short).Show(); 
            });

            if (null == e.Error)
            {
                Guid result = e.Result;
                if (!result.Equals(Guid.Empty))
                {
                    AndroidData.ServiceAuthToken = result.ToString();

                    AccountOAuth accountAuth = new AccountOAuth();
                    User userObj = null;
                    ISocialProviderManager provider = (ISocialProviderManager)e.UserState;

                    try
                    {
                        userObj = provider.GetUserInfoObject(accountAuth);
                    } catch (Exception ex)
                    {
                        parent.RunOnUiThread(delegate
                        {
                            GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.loginAuthError),
                                string.Format("{0} {1}", Application.Context.Resources.GetString(Resource.String.errorCouldNotGetUserInfoFormat), ex.Message), parent);
                        });
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Problem with authenticating using {0}. Message = {1}. StackTrace = {2}", accountAuth.OAuthType.ToString(), ex.Message, ex.StackTrace);
#endif
                        return;
                    }//end try catch

                    if (user == UserType.NewUser)
                    {
                        service.UserCreateCompleted += Service_UserCreateCompleted;
                        service.UserCreateAsync(AndroidData.NewDeviceID,
                                        DeviceDeviceTypes.Android,
                                        accountAuth.OAuthType,
                                        accountAuth.OAuthID,
                                        accountAuth.OAuthToken,
                                        userObj.FirstName,
                                        userObj.LastName,
                                        userObj.EmailAddress, string.Empty,
                                        userObj.Picture,
                                        userObj.DateOfBirth,
                                        string.Empty, // username
                                        string.Empty, // description
                                        0, // latitude
                                        0, // longtitude
                                        false, //showLocation
                                        false, // allowLocationSearch
                                        false, // allowSearch
                                        User.Gender.Male, // gender
                                        new Guid(AndroidData.ServiceAuthToken), accountAuth);

                    } else
                    {
                        service.UserLoginCompleted += Service_UserLoginCompleted;
                        service.UserLoginAsync(AndroidData.NewDeviceID,
                                               DeviceDeviceTypes.Android,
                                               LOLConstants.DefaultGuid,
                                               accountAuth.OAuthID,
                                               accountAuth.OAuthToken,
                                               accountAuth.OAuthType,
                                               userObj.EmailAddress,
                                               string.Empty,
                                               new Guid(AndroidData.ServiceAuthToken));
                    }//end if else
                } else
                {
                    parent.RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.loginAuthError), Application.Context.Resources.GetString(Resource.String.authNotAuthenticated), parent);
                    });
                }//end if else
            } else
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception in AuthenticationTokenGetCompleted! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
                parent.RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.loginAuthError), Application.Context.Resources.GetString(Resource.String.authNotAuthenticated), parent);
                });
            }//end if else
        }

        private void Service_UserCreateCompleted(object sender, UserCreateCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserCreateCompleted -= Service_UserCreateCompleted;
            if (null == e.Error)
            {
                User createdUsr = e.Result;
                AccountOAuth accountAuth = (AccountOAuth)e.UserState;

                if (createdUsr.Errors.Count > 0)
                {

                    string errorMessage =
                        StringUtils.CreateErrorMessageFromGeneralErrors(createdUsr.Errors);
                    foreach (GeneralError eachError in createdUsr.Errors)
                    {
#if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("User error: {0}--{1}--{2}--{3}",
                                          eachError.ErrorTitle, eachError.ErrorDescription,
                                          eachError.ErrorLocation, eachError.ErrorNumber);
#endif
                    }//end foreach

                    parent.RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.errorUserCreation), errorMessage, parent);
                    });

                } else
                {
                    AndroidData.CurrentUser = createdUsr;

                    service.UserLoginCompleted += Service_UserLoginCompleted;
                    service.UserLoginAsync(AndroidData.NewDeviceID,
                                               DeviceDeviceTypes.Android,
                                               LOLConstants.DefaultGuid,
                                               accountAuth.OAuthID,
                                               accountAuth.OAuthToken,
                                               accountAuth.OAuthType,
                                               AndroidData.CurrentUser.EmailAddress,
                                               string.Empty,
                                               new Guid(AndroidData.ServiceAuthToken));
                }//end if else
            } else
            {
                parent.RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.errorUserCreation), string.Format("{0} {1}",
                        Application.Context.Resources.GetString(Resource.String.errorUserCreationFailed), e.Error.Message), parent);
                });
            }
        }//end void Service_UserCreateCompleted

        private void Service_UserLoginCompleted(object sender, UserLoginCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserLoginCompleted -= Service_UserLoginCompleted;
            if (null == e.Error)
            {
                User result = e.Result;
                if (result.Errors.Count > 0)
                {
                    string errorMessage =
                        StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors);
                    parent.RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.errorUserLoginProblem), errorMessage, parent);
                    });
                } else
                {
                    AndroidData.IsLoggedIn = true;
                    // If it was a new user, it has already been set in UserCreate
                    if (user == UserType.ExistingUser)
                    {
                        AndroidData.CurrentUser = result;
                    }//end if

                    parent.RunOnUiThread(delegate
                    {
                        this.ShowAfterLoginScreen();
                    });
                }//end if else
            } else
            {
                parent.RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(c, Application.Context.Resources.GetString(Resource.String.errorUserLoginProblem), Application.Context.Resources.GetString(Resource.String.errorUserLoginFailed), parent);
                });
            }
        }   
    }
}
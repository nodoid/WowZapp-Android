// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using OAuthLib;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.IO;
using LOLAccountManagement;
using System.Xml.Linq;
using System.Text;


namespace LOLApp_Common
{
    public class LLinkedInManager : ISocialProviderManager
    {

		#region Constructors

        public LLinkedInManager(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            this.ConsumerKey = consumerKey;
            this.ConsumerSecret = consumerSecret;
            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;
            this.ProviderType = AccountOAuth.OAuthTypes.LinkedIn;
            if (null == ServicePointManager.ServerCertificateValidationCallback)
            {
                ServicePointManager.ServerCertificateValidationCallback = CertValidator.Validator;
            }//end if
        }

		#endregion Constructors



		#region Fields

        private string pBrowserAuthUrl;
        private OAuthTokenResponse accessTokenResponse;
        private OAuthTokenResponse authTokenResponse;

		#endregion Fields


        #region ISocialProviderManager implementation
        
        public AccountOAuth.OAuthTypes ProviderType
        {
            get;
            private set;
        }//end AccountOAuth.OAuthTypes ProviderType
        
        
        
        
        public bool CheckForAccessToken(string url)
        {
            if (null != this.accessTokenResponse)
            {
                this.AccessToken = this.accessTokenResponse.Token;
                this.AccessTokenSecret = this.accessTokenResponse.TokenSecret;
                
                return true;
            } else
            {
                return false;
            }//end if else
        }
        
        
        
        
        public bool UserAccepted(string queryString)
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(queryString);
            string requestToken = nvc ["oauth_token"];
            string verifier = nvc ["oauth_verifier"];
            
            this.accessTokenResponse = 
                OAuthUtility.GetAccessToken(this.ConsumerKey, 
                                            this.ConsumerSecret, 
                                            requestToken, this.authTokenResponse.TokenSecret, 
                                            verifier, LOLConstants.LinkLinkedInAccessTokenUrl, 
                                            HTTPVerb.POST, ServiceType.LinkedIn);
            
            return true;
        }
        
        
        
        
        
        public string ApiKey
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
        
        
        
        
        public string AccessToken
        {
            get;
            private set;
        }
        
        
        
        
        public string AccessTokenSecret
        {
            get;
            private set;
        }
        
        
        
        public string RefreshToken
        {
            get
            {
                throw new NotImplementedException();
            }//end get
            
        }//end string RefreshToken
        
        
        
        
        public DateTime? AccessTokenExpirationTime
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
        
        
        
        
        public string BrowserAuthUrl
        {
            get
            {
                if (string.IsNullOrEmpty(this.pBrowserAuthUrl))
                {
                    this.pBrowserAuthUrl = this.GetBrowserAuthUrl();
                }//end if
                
                return this.pBrowserAuthUrl;
            }//end get
        }
        
        
        
        
        public string ConsumerKey
        {
            get;
            private set;
        }
        
        
        
        
        public string ConsumerSecret
        {
            get;
            private set;
        }
        
        
        
        
        public string GetBrowserAuthUrl()
        {
            
            this.authTokenResponse = 
                OAuthUtility.GetRequestToken(LOLConstants.LinkedInConsumerKey, 
                                             LOLConstants.LinkedInConsumerSecret, 
                                             LOLConstants.LinkLinkedInCallbackUrl,
                                             LOLConstants.LinkLinkedInRequestTokenUrl, HTTPVerb.POST);
            
            Uri authorizationUri = 
                OAuthUtility.BuildAuthorizationUri(authTokenResponse.Token, true, LOLConstants.LinkLinkedInAuthUrl);
            
            return authorizationUri.AbsoluteUri;
            
        }//end string GetBrowserAuthUrl
        
        #endregion
        
        
        
        
        #region Public methods
        
        public string GetUserInfo()
        {
            
            string requestUrl = "http://api.linkedin.com/v1/people/~:(id,first-name,last-name,picture-url,date-of-birth,site-standard-profile-request,email-address)";
            
            OAuthTokens authTokens = new OAuthTokens() {
                
                ConsumerKey = this.ConsumerKey,
                ConsumerSecret = this.ConsumerSecret,
                AccessToken = this.AccessToken,
                AccessTokenSecret = this.AccessTokenSecret
                
            };
            
            WebRequestBuilder builder = new WebRequestBuilder(new Uri(requestUrl), OAuthLib.HTTPVerb.GET, authTokens, string.Empty);
            
            try
            {
                
                HttpWebResponse response = builder.ExecuteRequest();
                
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }//end using sr
                
            } catch (Exception ex)
            {
                throw ex;
            }//end try catch
            
        }//end string GetUserInfo
        
        
        
        public string GetUserProfileUrl()
        {
            
            string responseStr = this.GetUserInfo();
            XDocument xDoc = XDocument.Parse(responseStr);
            XElement personElement = xDoc.Element(XName.Get("person"));
            
            if (null != personElement)
            {
                XElement profElement = personElement.Element(XName.Get("site-standard-profile-request"));
                if (null != profElement)
                {
                    return profElement.Element(XName.Get("url")).Value;
                }//end if
                
            }//end if
            return string.Empty;
            
        }//end string GetUserProfileUrl
        
        
        
        
        public User GetUserInfoObject(AccountOAuth accountOAuth)
        {
            
            try
            {
                
                User toReturn = new User();
                string userInfoResponse = this.GetUserInfo();
                
                XDocument xDoc = XDocument.Parse(userInfoResponse);
                XElement personElement = xDoc.Element(XName.Get("person"));
                
                
                accountOAuth.OAuthID = personElement.Element(XName.Get("id")).Value;
                accountOAuth.OAuthToken = this.AccessToken;
                accountOAuth.OAuthType = this.ProviderType;
                
                XElement firstNameElement = personElement.Element(XName.Get("first-name"));
                XElement lastNameElement = personElement.Element(XName.Get("last-name"));
                XElement birthDayElement = personElement.Element(XName.Get("date-of-birth"));
                XElement profilePicElement = personElement.Element(XName.Get("picture-url"));
                XElement emailAddressElement = personElement.Element(XName.Get("email-address"));
                
                toReturn.FirstName = firstNameElement != null ? firstNameElement.Value : string.Empty;
                toReturn.LastName = lastNameElement != null ? lastNameElement.Value : string.Empty;
                toReturn.DateOfBirth = birthDayElement != null ? DateTime.Parse(birthDayElement.Value) : new DateTime(1900, 1, 1);
                toReturn.Picture = profilePicElement != null ? this.GetUserImage(profilePicElement.Value) : new byte[0];
                toReturn.EmailAddress = emailAddressElement != null ? emailAddressElement.Value : string.Empty;
                
                return toReturn;
                
            } catch (Exception ex)
            {
                
                throw ex;
                
            }//end try catch
            
        }//end User GetUserInfoObject
        
        
        
        
        
        public string GetUserFriends(int startIndex, int count)
        {
            
            string requestUrl = string.Format("http://api.linkedin.com/v1/people/~/connections:(id,first-name,last-name,picture-url,date-of-birth)?start={0}&count={1}", startIndex, count);
            
            OAuthTokens authTokens = new OAuthTokens() {
                
                ConsumerKey = this.ConsumerKey,
                ConsumerSecret = this.ConsumerSecret,
                AccessToken = this.AccessToken,
                AccessTokenSecret = this.AccessTokenSecret
                
            };
            
            WebRequestBuilder builder = new WebRequestBuilder(new Uri(requestUrl), OAuthLib.HTTPVerb.GET, authTokens, string.Empty);
            
            try
            {
                
                HttpWebResponse response = builder.ExecuteRequest();
                
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }//end using sr
                
            } catch (Exception ex)
            {
                throw ex;
            }//end try catch
            
        }//end string GetUserFriends
        
        
        
        
        public string GetAllUserFriends()
        {
            
            string requestUrl = "http://api.linkedin.com/v1/people/~/connections:(id,first-name,last-name,picture-url,date-of-birth)";
            
            OAuthTokens authTokens = new OAuthTokens() {
                
                ConsumerKey = this.ConsumerKey,
                ConsumerSecret = this.ConsumerSecret,
                AccessToken = this.AccessToken,
                AccessTokenSecret = this.AccessTokenSecret
                
            };
            
            WebRequestBuilder builder = new WebRequestBuilder(new Uri(requestUrl), OAuthLib.HTTPVerb.GET, authTokens, string.Empty);
            
            try
            {
                HttpWebResponse response = builder.ExecuteRequest();
                
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }//end using sr
                
            } catch (Exception ex)
            {
                throw ex;
            }//end try catch
            
        }//end string GetallUserFriends
        
        
        
        
        public byte[] GetUserImage(string imgUrl)
        {
            
            try
            {
                
                HttpWebRequest request = WebRequest.Create(imgUrl) as HttpWebRequest;
                request.Method = "GET";
                request.KeepAlive = true;
                
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    using (Stream responseStream = request.GetResponse().GetResponseStream())
                    {
                        
                        while (true)
                        {
                            
                            int readCount = responseStream.Read(buffer, 0, buffer.Length);
                            if (readCount == 0)
                            {
                                break;
                            }//end if
                            
                            ms.Write(buffer, 0, readCount);
                            
                        }//end while
                        
                    }//end using responseStream
                    
                    return ms.ToArray();
                    
                }//end using ms
                
            } catch (Exception ex)
            {
                
                throw ex;
                
            }//end try catch
            
        }//end byte[] GetUserImage
        
        
        
        
        
        /// <summary>
        /// Share something on LinkedIn
        /// </summary>
        /// <returns>
        /// True if it succeeds. Otherwise throws an exception.
        /// </returns>
        /// <param name='message'>
        /// The message to send.
        /// </param>
        /// <param name='userID'>
        /// Not needed for LinkedIn. Just pass string.Empty.
        /// </param>
        public bool PostToFeed(string message, string userID)
        {
            
            // Share
            //string requestUrl = string.Format("http://api.linkedin.com/v1/people/~/shares");
            // Activity
            string requestUrl = "http://api.linkedin.com/v1/people/~/person-activities";
            
            OAuthTokens authTokens = new OAuthTokens() {
                
                ConsumerKey = this.ConsumerKey,
                ConsumerSecret = this.ConsumerSecret,
                AccessToken = this.AccessToken,
                AccessTokenSecret = this.AccessTokenSecret
                
            };
            
            WebRequestBuilder builder = new WebRequestBuilder(new Uri(requestUrl), HTTPVerb.POST, authTokens, string.Empty);
            byte[] postDataBuffer = Encoding.UTF8.GetBytes(message);
            
            try
            {
                
                HttpWebRequest request = builder.PrepareRequest();
                request.ContentType = "application/xml";
                request.ContentLength = postDataBuffer.Length;
                
                using (Stream s = request.GetRequestStream())
                {
                    s.Write(postDataBuffer, 0, postDataBuffer.Length);
                }//end using s
                
                using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    #if(DEBUG)
                    Console.WriteLine("Response: {0}", sr.ReadToEnd());
                    #endif
                }//end using sr
                
                return true;
                
            } catch (Exception ex)
            {
                
                throw ex;
                
            }//end try catch
            
            
        }//end bool PostToFeed
        
        
        
        public string GetPhotoAlbums()
        {
            
            throw new NotImplementedException();
            
        }//end string GetPhotoAlbums
        
        
        
        public string GetAllVideos()
        {
            
            throw new NotImplementedException();
            
        }//end string GetAllVideos
        
        
        
        public string GetAlbumPhotos(string albumID)
        {
            
            throw new NotImplementedException();
            
        }//end string GetAlbumPhotos
        
        
        
        public byte[] GetImage(string pictureUrl)
        {
            
            throw new NotImplementedException();
            
        }//end byte[] GetImage
        
        #endregion Public methods
        
    }
}


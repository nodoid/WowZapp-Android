// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Json;
using LOLAccountManagement;
using LOLApp_Common.UrlHelper;

namespace LOLApp_Common
{
	public class LGooglePlusManager : ISocialProviderManager
	{

		#region Constructors

        public LGooglePlusManager(string consumerKey, string consumerSecret, IHttpHelper t)
        {
            this.ConsumerKey = consumerKey;
            this.ConsumerSecret = consumerSecret;
            this.ProviderType = AccountOAuth.OAuthTypes.Google;
            this.HttpHelper = t;
            if (null == ServicePointManager.ServerCertificateValidationCallback)
            {
                ServicePointManager.ServerCertificateValidationCallback = CertValidator.Validator;
            }
        }

		public LGooglePlusManager(string consumerKey, string consumerSecret, string accessToken, string refreshToken, DateTime? accessTokenExpiration, IHttpHelper httpHelper) :
			this(consumerKey, consumerSecret, httpHelper)
		{
			this.AccessToken = accessToken;
			this.RefreshToken = refreshToken;
			this.AccessTokenExpirationTime = accessTokenExpiration;
		}

        /* NOTE TO GJORE - IF YOU EVER SEE ANYTHING WITH "MODIFIED BY ARIAN" WITHIN 100 METERS OF A SOURCE FILE, DELETE IT, BURN IT
         * AND SCRUB YOUR PC IN HOT WATER. THE GUY WAS A FUCKTARD */

		#endregion Cosntructors



		#region Fields

		private string browserAuthUrl;
		private IHttpHelper HttpHelper;
  

		#endregion Fields





		#region ISocialProviderManager implementation

	 
		// Modified by Arian.
		public bool CheckForAccessToken (string url)
		{
			string accessToken;
			if ( null != (accessToken = UrlParse.ParseGetParameterWithRegex(url,"#access_token"))) //; (access_token = CompiledRegexs.GoogleMatchAccessToken.Match(url)).Success)
			{

				string expiresIn;
				if (null != (expiresIn = UrlParse.ParseGetParameterWithRegex(url, "&expires_in")))
				{
				   
					TimeSpan expiration = TimeSpan.FromSeconds(Convert.ToDouble(expiresIn));
					DateTime timeRequested = DateTime.Now;
					DateTime expirationTime = timeRequested.Add(expiration);

					this.AccessToken = accessToken;
					this.AccessTokenExpirationTime = expirationTime;

					return true;
				}
				//url.Substring(url.IndexOf("#access_token=") + 14);
				//	accessToken = accessToken.Substring(0, accessToken.LastIndexOf("&"));

				return false;
			
			} else
			{
			   
				string exchangeCode ;
			  if( null !=(   exchangeCode =  UrlParse.ParseGetParameter(url,"code")))


							// Fetch the access token
							try
							{

								string exchangeParams = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code",
																	 System.Uri.EscapeDataString( exchangeCode),
																	 System.Uri.EscapeDataString( this.ConsumerKey),
																	 System.Uri.EscapeDataString( this.ConsumerSecret),
																	System.Uri.EscapeDataString(  LOLConstants.LinkGooglePlusCallbackUrl));


//								string responseStr = HttpHelper.Post(LOLConstants.LinkGooglePlusRequestTokenUrl, exchangeParams);
					string responseStr = this.Post(LOLConstants.LinkGooglePlusRequestTokenUrl, exchangeParams);
								
								JsonValue responseJson = JsonValue.Parse(responseStr);
								if (responseJson.ContainsKey("access_token"))
								{
									this.AccessToken = (string)responseJson["access_token"];
								}//end if

								if (responseJson.ContainsKey("refresh_token"))
								{
									this.RefreshToken = (string)responseJson["refresh_token"];
								}//end if

								if (responseJson.ContainsKey("expires_in"))
								{
									this.AccessTokenExpirationTime =
										DateTime.Now.Add(TimeSpan.FromSeconds((double)responseJson["expires_in"]));
								}
								
								  

							} catch (Exception ex)
							{
								throw ex;
							} finally
							{
							}//end try catch finally

							return true;
					
				 
				 
				
			}//end if else
		}

	  


		private string Post(string url, string postdata)
		{

			try
			{

				byte[] postDataBuffer = Encoding.ASCII.GetBytes(postdata);
				HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = postDataBuffer.Length;

				using (Stream s = request.GetRequestStream())
				{
					s.Write(postDataBuffer, 0, postDataBuffer.Length);
				}//end using

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
				{
					return sr.ReadToEnd();
				}//end using sr

			} catch (Exception ex)
			{

				throw ex;

			}//end try catch

		}//end string Post




		public bool UserAccepted (string url)
		{
			string error;
			return (error = UrlParse.ParseGetParameter(url, "error")) == null ? true : error == "access_denied";
			/*
			 * if (error != null && error == "access_denied")
				return false;
			return true;
			 * */
			 
			/*
			NameValueCollection nvc = HttpUtility.ParseQueryString(url.Substring(LOLConstants.LinkGooglePlusCallbackUrl.Length + 1));

			foreach (string eachKey in nvc.Keys)
			{
				if (eachKey == "error" && nvc["error"] == "access_denied")
				{
					return false;
				}//end if
				
			}//end foreach
			
			return true;*/
		}





		public string GetBrowserAuthUrl ()
		{
			string url = string.Format("{0}?scope=https://www.googleapis.com/auth/plus.me+" +
				"https://www.googleapis.com/auth/userinfo.profile+" +
				"https://www.googleapis.com/auth/userinfo.email+" +
				"https://www.google.com/m8/feeds" +
				"&response_type=code&client_id={1}&redirect_uri={2}&approval_prompt=force&access_type=offline", 
									   LOLConstants.LinkGooglePlusAuthUrl, 
									   LOLConstants.GooglePlusConsumerKey,
									   LOLConstants.LinkGooglePlusCallbackUrl);

			return url;
		}





		public string ApiKey {
			get {
				throw new System.NotImplementedException ();
			}
		}





		public string AccessToken 
		{
			get;
			private set;
		}//end string AccessToken




		public string AccessTokenSecret 
		{
			get;
			private set;
		}



		public string RefreshToken
		{
			get;
			private set;
		}//end string RefreshToken




		public DateTime? AccessTokenExpirationTime 
		{
			get;
			private set;
		}




		public string BrowserAuthUrl 
		{
			get
			{
				if (string.IsNullOrEmpty(this.browserAuthUrl))
				{
					this.browserAuthUrl = this.GetBrowserAuthUrl();
				}//end if
				return this.browserAuthUrl;
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



		public AccountOAuth.OAuthTypes ProviderType
		{
			get;
			private set;
		}//end AccountOAuth.OAuthTypes ProviderType

		#endregion



		#region Public methods

		public string GetUserInfo()
		{

			string requestString = string.Format("https://www.googleapis.com/oauth2/v1/userinfo?access_token={0}", this.AccessToken);
			//string requestString = string.Format("https://www.googleapis.com/plus/v1/people/me?access_token={0}", this.AccessToken);

			try
			{
//				return HttpHelper.Get(requestString);

				HttpWebRequest request = WebRequest.Create(requestString) as HttpWebRequest;
				request.Method = "GET";
	
				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
				{
					return sr.ReadToEnd();
				}//end using sr

			} catch (Exception ex)
			{

				throw ex;

			}//end try catch

		}//end string GetUserInfo



		public User GetUserInfoObject(AccountOAuth accountOAuth)
		{

			User toReturn = new User();
			string userInfoResponse = this.GetUserInfo();

			JsonValue userInfoJson = JsonValue.Parse(userInfoResponse);
			accountOAuth.OAuthID = (string)userInfoJson["id"];
			accountOAuth.OAuthToken = this.AccessToken;
			accountOAuth.OAuthType = this.ProviderType;
			 
			string firstName = userInfoJson.ContainsKey("given_name") ? (string)userInfoJson["given_name"] : string.Empty;
			string lastName = userInfoJson.ContainsKey("family_name") ? (string)userInfoJson["family_name"] : string.Empty;
			
			toReturn.FirstName = firstName;
			toReturn.LastName = lastName;
			toReturn.EmailAddress = userInfoJson.ContainsKey("email") ? (string)userInfoJson["email"] : string.Empty;
			toReturn.DateOfBirth = userInfoJson.ContainsKey("birthday") ? DateTime.Parse((string)userInfoJson["birthday"]) : new DateTime(1900, 1, 1);

			string profilePicUrl = userInfoJson.ContainsKey("picture") ? (string)userInfoJson["picture"] : string.Empty;

			toReturn.Picture = string.IsNullOrEmpty(profilePicUrl) ? new byte[0] : this.GetUserImage(profilePicUrl);

			return toReturn;

		}//end User GetUserInfoObkect


	  

		/// <summary>
		/// Gets the user friends.
		/// </summary>
		/// <returns>
		/// The user friends.
		/// </returns>
		/// <param name='startIndex'>
		/// Start index. NOTE: On Google, index is 1-based!!!
		/// </param>
		/// <param name='count'>
		/// The number of results to return.
		/// </param>
		public string GetUserFriends(int startIndex, int count)
		{

			if (startIndex < 1)
			{
				throw new ArgumentException("Index on Google is 1-based! Make sure startIndex is >= 1!");
			}//end if

			string requestUrl = 
				string.Format("https://www.google.com/m8/feeds/contacts/default/thin?access_token={0}&start-index={1}&max-results={2}&v=3.0", 
							  this.AccessToken, startIndex, count);

			try
			{
//				return HttpHelper.Get(requestUrl);
				HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
				request.Method = "GET";
	
				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
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

			string requestUrl = 
				string.Format("https://www.google.com/m8/feeds/contacts/default/thin?access_token={0}&max-results=9999&v=3.0",
				              this.AccessToken);

			try
			{
//				return HttpHelper.Get(requestUrl);
				HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
				request.Method = "GET";
	         
				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
				{
					return sr.ReadToEnd();
				}//end using sr
			} catch (Exception ex)
			{
				throw ex;
			}//end try catch

		}//end string GetAllUserFriends



		public byte[] GetUserImage(string profilePictureLink)
		{

			string requestUrl = string.Format("{0}&access_token={1}", profilePictureLink, this.AccessToken);

			try
			{
				HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
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

		}//end byte[] GetContactProfilePicture



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




		/// <summary>
		/// Refreshes the access token for Google+
		/// </summary>
		/// <returns>
		/// Returns true if the access token was successfully refreshed. In that case,
		/// both AccessToken and AccessTokenExpiration time properties contain the new values.
		/// </returns>
		public bool RefreshAccessToken()
		{

			string refreshTokenParams = 
				string.Format("client_id={0}&" +
							  "client_secret={1}&" +
							  "refresh_token={2}&" +
							  "grant_type=refresh_token",
							  this.ConsumerKey,
							  this.ConsumerSecret,
							  this.RefreshToken);

			byte[] paramBuffer = Encoding.ASCII.GetBytes(refreshTokenParams);

			try
			{

				//HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
				HttpWebRequest request = WebRequest.Create(LOLConstants.LinkGooglePlusRequestTokenUrl) as HttpWebRequest;
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = paramBuffer.Length;

				using (Stream s = request.GetRequestStream())
				{
					s.Write(paramBuffer, 0, paramBuffer.Length);
				}//end using s

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
				{
	
					JsonValue jsonObj = JsonValue.Parse(sr.ReadToEnd());
					if (jsonObj.ContainsKey("access_token"))
					{
						this.AccessToken = (string)jsonObj["access_token"];
						this.AccessTokenExpirationTime = DateTime.Now.AddSeconds((double)jsonObj["expires_in"]);
	
						return true;
					}//end if
	
				}//end using sr

			} catch (Exception ex)
			{

				throw ex;

			}//end try catch

			return false;

		}//end string RefreshAccessToken

		#endregion Public methods


		
		public bool PostToFeed(string message, string userID)
		{
			 throw new NotImplementedException();
		 
		}
	}
}


// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using OAuthLib;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.IO;
using LOLAccountManagement;
using System.Json;


namespace LOLApp_Common
{
	public class LTwitterManager : ISocialProviderManager
	{

		#region Constructors

		public LTwitterManager (string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string screenName, string userId)
		{
			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
			this.AccessToken = accessToken;
			this.AccessTokenSecret = accessTokenSecret;
			this.ScreenName = screenName;
			this.UserId = userId;
			this.ProviderType = AccountOAuth.OAuthTypes.Twitter;
			if (null == ServicePointManager.ServerCertificateValidationCallback)
			{
				ServicePointManager.ServerCertificateValidationCallback = CertValidator.Validator;
			}//end if
		}

		#endregion Constructors



		#region Fields

		private string authUrl;
		private OAuthTokenResponse accessTokenResponse;

		#endregion Fields



		#region Properties

		public string ScreenName
		{
			get;
			private set;
		}//end string ScreenName



		public string UserId
		{
			get;
			private set;
		}//end string UserId



		public AccountOAuth.OAuthTypes ProviderType
		{
			get;
			private set;
		}//end OAuthTypes ProviderType


		#endregion Properties





		#region IProviderManager implementation

		public bool CheckForAccessToken (string url)
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



		public bool UserAccepted (string queryString)
		{

			NameValueCollection nvc = HttpUtility.ParseQueryString(queryString);
			string requestToken = nvc["oauth_token"];
			string verifier = nvc["oauth_verifier"];

			this.accessTokenResponse = 
				OAuthUtility.GetAccessToken(this.ConsumerKey, 
				                            this.ConsumerSecret, 
				                            requestToken, string.Empty, 
				                            verifier, LOLConstants.LinkTwitterAccessTokenUrl, 
				                            HTTPVerb.GET, ServiceType.Twitter);

			this.UserId = Convert.ToString(this.accessTokenResponse.UserId);
			this.ScreenName = Convert.ToString(this.accessTokenResponse.ScreenName);
			return true;

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

		}



		public string RefreshToken
		{
			get
			{
				throw new NotImplementedException();
			}//end get

		}//end string RefreshToken



		public string AccessTokenSecret
		{
			get;
			private set;
		}//end string AccessTokenSecret





		public DateTime? AccessTokenExpirationTime 
		{
			get 
			{
				throw new NotImplementedException();
			}
		}


		/// <summary>
		/// Gets the auth URL.
		/// </summary>
		/// <value>
		/// The auth URL, needed for the Twitter app authorization page.
		/// </value>
		public string BrowserAuthUrl 
		{
			get 
			{
				if (string.IsNullOrEmpty(this.authUrl))
				{
					this.authUrl = this.GetBrowserAuthUrl();
				}//end if
				return this.authUrl;
			}
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

			OAuthTokenResponse authTokenResponse = 
				OAuthUtility.GetRequestToken(LOLConstants.TwitterConsumerKey, 
				                             LOLConstants.TwitterConsumerSecret, 
				                             LOLConstants.LinkTwitterCallbackUrl,
				                             LOLConstants.LinkTwitterRequestTokenUrl, HTTPVerb.POST);
			Uri authorizationUri = 
				OAuthUtility.BuildAuthorizationUri(authTokenResponse.Token, LOLConstants.LinkTwitterAuthUrl);

			return authorizationUri.AbsoluteUri;

		}//end string GetAuthUrl



		public string GetUserFriends (int startIndex, int count)
		{
			throw new System.NotImplementedException ("Not implemented for Twitter!");
		}



		public string GetAllUserFriends()
		{
			throw new System.NotImplementedException("Not implemented for Twitter!");
		}//end string GetAllUserFriends

		#endregion



		#region Public methods

		public string GetUserInfo()
		{

			string requestUrl = string.Format("https://api.twitter.com/1/users/show.json?screen_name={0}&include_entities=false", this.ScreenName);

			OAuthTokens authTokens = new OAuthTokens() {

				AccessToken = this.AccessToken,
				AccessTokenSecret = this.AccessTokenSecret,
				ConsumerKey = this.ConsumerKey,
				ConsumerSecret = this.ConsumerSecret

			};

			WebRequestBuilder builder = new WebRequestBuilder(new Uri(requestUrl), OAuthLib.HTTPVerb.GET, authTokens, string.Empty);

			try
			{

				HttpWebResponse response = builder.ExecuteRequest();

				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					return sr.ReadToEnd();
				}//end using st

			} catch (Exception ex)
			{

				throw ex;

			}//end try catch

		}//end string GetUserInfo



		public User GetUserInfoObject(AccountOAuth accountOAuth)
		{

			try
			{

				User toReturn = new User();
				string userInfoResponse = this.GetUserInfo();
	
				JsonValue userInfoJson = JsonValue.Parse(userInfoResponse);
	
				accountOAuth.OAuthID = (string)userInfoJson["id_str"];
				accountOAuth.OAuthToken = this.AccessToken;
				accountOAuth.OAuthType = this.ProviderType;
	
				toReturn.FirstName = userInfoJson.ContainsKey("name") ? (string)userInfoJson["name"] : string.Empty;
				toReturn.LastName = string.Empty; // No firstname or lastname on Twitter, just "name"
				toReturn.DateOfBirth = new DateTime(1900, 1, 1); // No birthday on Twitter
				toReturn.EmailAddress = string.Empty; // No email address on Twitter
				toReturn.Picture = userInfoJson.ContainsKey("profile_image_url_https") ? this.GetUserImage((string)userInfoJson["profile_image_url_https"]) : new byte[0];
	
				return toReturn;

			} catch (Exception ex)
			{

				throw ex;

			}//end try catch

		}//end User GetUserInfoObject



		public byte[] GetUserImage (string criteria)
		{


			try
			{

				HttpWebRequest request = WebRequest.Create(criteria) as HttpWebRequest;

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
		}



		public bool PostToFeed(string message, string userID)
		{
			throw new NotImplementedException("Not yet implemented!");
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


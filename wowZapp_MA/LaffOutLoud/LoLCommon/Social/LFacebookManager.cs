// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.Specialized;
using System.Json;
using System.Web;
using System.Net;
using System.Text;
using LOLAccountManagement;


namespace LOLApp_Common
{
	enum HttpVerb
	{
		GET,
		POST,
		DELETE
	}
	

	/// <summary>
	/// Wrapper around the Facebook Graph API. 
	/// </summary>
	public class LFacebookManager : ISocialProviderManager
	{  

		#region Constructors

		public LFacebookManager (string consumerKey, string consumerSecret)
		{
			
			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
			this.ProviderType = AccountOAuth.OAuthTypes.FaceBook;
			if (null == ServicePointManager.ServerCertificateValidationCallback) {
				ServicePointManager.ServerCertificateValidationCallback = CertValidator.Validator;
			}//end if

		}//end ctor



		public LFacebookManager (string consumerKey, string consumerSecret, string accessToken, DateTime? accessTokenExpiration) :
			this(consumerKey, consumerSecret)
		{
			this.AccessToken = accessToken;
			this.AccessTokenExpirationTime = accessTokenExpiration;

		}//end ctor

		#endregion Constructors





		
		#region Events

		/// <summary>
		/// Occurs when post to wall completed.
		/// </summary>
		public event PostToWallCompletedHandler PostToWallCompleted;
		
		#endregion Events

		


		#region IProviderManager implementation

		public AccountOAuth.OAuthTypes ProviderType {
			get;
			private set;
		}//end OAuthTypes ProviderType




		/// <summary>
		/// Gets the API key.
		/// </summary>
		public string ConsumerKey { 
			get; 
			private set; 
		}



		public string ConsumerSecret {
			get;
			private set;
		}



				
		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>
		/// The access token.
		/// </value>
		public string AccessToken {
			get;
			private set;
		}



		public string AccessTokenSecret {
			get;
			private set;
		}//end string AccessTokenSecret



		public string RefreshToken {
			get {
				throw new NotImplementedException ();
			}//end get

		}//end string RefreshToken
		
		
		
		
		/// <summary>
		/// Gets the access token expiration time.
		/// </summary>
		/// <value>
		/// The access token expiration time.
		/// </value>
		public DateTime? AccessTokenExpirationTime {
			get;
			private set;
		}
		
		
		
		/// <summary>
		/// Gets the auth URL.
		/// </summary>
		/// <value>
		/// The auth URL.
		/// </value>
		public string BrowserAuthUrl {
			
			get {

				return this.GetBrowserAuthUrl ();
				
			}//end get
			
		}//end string AuthUrl






		public bool UserAccepted (string url)
		{

			NameValueCollection nvc = HttpUtility.ParseQueryString (url.Substring (LOLConstants.LinkFacebookLoginSuccessUrl.Length + 1));

			foreach (string eachKey in nvc.Keys) {
				if (eachKey == "error_reason" && nvc ["error_reason"] == "user_denied") {
					return false;
				}//end if
				
			}//end foreach
			
			return true;
		}


		/// <summary>
		/// Checks for and extracts an access token from a url.
		/// </summary>
		/// <returns>
		/// True if the url contains an access token. The access token is then saved in the Facebook.AccessToken property. False otherwise.
		/// </returns>
		/// <param name='url'>
		/// The full url to check.
		/// </param>
		public bool CheckForAccessToken (string url)
		{
			
			if (url.Contains ("#access_token=")) {
			
				string accessToken = url.Substring (url.IndexOf ("#access_token=") + 14);
				accessToken = accessToken.Substring (0, accessToken.LastIndexOf ("&"));
				string expiresIn = url.Substring (url.LastIndexOf ("&expires_in=") + 12);
				TimeSpan expiration = TimeSpan.FromSeconds (Convert.ToDouble (expiresIn));
				DateTime timeRequested = DateTime.Now;
				DateTime expirationTime = timeRequested.Add (expiration);
				
				this.AccessToken = accessToken;
				this.AccessTokenExpirationTime = expirationTime;
				
				return true;
			
			} else {
				
				return false;
				
			}//end if else
				
		}//end void CheckForAccessToken



		public string GetBrowserAuthUrl ()
		{

			return String.Format (LOLConstants.LinkFacebookAuthUrl, this.ConsumerKey);

		}//end string GetBrowserAuthUrl

		#endregion IProviderManager implementation
		
		
		
		#region Public methods

		/// <summary>
		/// Gets the user info raw JSON string.
		/// </summary>
		/// <returns>
		/// The user info.
		/// </returns>
		public string GetUserInfo ()
		{

			string requestUrl = string.Format ("https://graph.facebook.com/me?access_token={0}", this.AccessToken);

			try {

				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";
	
				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {
					return sr.ReadToEnd ();
				}//end using sr

			} catch (Exception ex) {
				throw ex;
			}//end try catch

		}//end string GetUserInfo




		public User GetUserInfoObject (AccountOAuth accountOAuth)
		{

			try {

				User toReturn = new User ();
				string userInfoReponse = this.GetUserInfo ();

				JsonValue userInfoJson = JsonValue.Parse (userInfoReponse);
				accountOAuth.OAuthID = (string)userInfoJson ["id"];
				accountOAuth.OAuthToken = this.AccessToken;
				accountOAuth.OAuthType = this.ProviderType;
				
				/* Facebook doesn't allow us to get birthday for some reason, commenting this out 
                 * 
				DateTime birthDay = new DateTime(1900, 1, 1);
				if (userInfoJson.ContainsKey("birthday"))
				{
					DateTime.TryParse((string)userInfoJson["birthday"], out birthDay);
				}//end if
                 * 
                 */
				toReturn.FirstName = userInfoJson.ContainsKey ("first_name") ? (string)userInfoJson ["first_name"] : string.Empty;
				toReturn.LastName = userInfoJson.ContainsKey ("last_name") ? (string)userInfoJson ["last_name"] : string.Empty;
				toReturn.DateOfBirth = new DateTime (1900, 1, 1);
				toReturn.EmailAddress = userInfoJson.ContainsKey ("email") ? (string)userInfoJson ["email"] : string.Empty;

				toReturn.Picture = new byte[0];

				return toReturn;

			} catch (Exception ex) {

				throw ex;

			}//end try catch

		}//end User GetUserInfoObject




		/// <summary>
		/// Gets the user info for the supplied facebook user id
		/// </summary>
		/// <returns>
		/// The user info response.
		/// </returns>
		/// <param name='id'>
		/// The facebook user id for the user to get information for.
		/// </param>
		public string GetUserInfo (string id)
		{

			string requestUrl = string.Format ("https://graph.facebook.com/{0}?access_token={1}", id, this.AccessToken);

			try {
				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {
					return sr.ReadToEnd ();
				}//end using sr

			} catch (Exception ex) {
				throw ex;
			}//end try catch

		}//end string GetUserInfo



		/// <summary>
		/// Gets the user friends' information in a facebook batch request
		/// </summary>
		/// <returns>
		/// The user friends info batch.
		/// </returns>
		/// <param name='friendIDs'>
		/// A List containing the friends' IDs to return. Only the first 50 items will be considered. 
		/// This is a limit from facebook on batch requests.
		/// </param>
		[Obsolete("Not needed anymore")]
		public string GetUserFriendsInfoBatch (List<string> friendIDs)
		{

			StringBuilder sb = new StringBuilder (string.Format ("access_token={0}&batch=[", this.AccessToken));
			List<string> idList = friendIDs.Count > 50 ? friendIDs.Take (50).ToList () : friendIDs;

			foreach (string eachItem in idList) {

				sb.Append ("{\"method\":\"GET\", \"relative_url\":\"" + eachItem + "\"},");

			}//end foreach

			// Remove the ','
			sb.Remove (sb.Length - 1, 0);
			sb.Append ("]");

			byte[] bodyBuffer = Encoding.ASCII.GetBytes (sb.ToString ());
			string requestUrl = "https://graph.facebook.com";
			try {
				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "POST";
				request.ContentLength = bodyBuffer.Length;

				using (Stream s = request.GetRequestStream()) {
					s.Write (bodyBuffer, 0, bodyBuffer.Length);
				}//end using s

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {
					return sr.ReadToEnd ();
				}//end using sr

			} catch (Exception ex) {

				throw ex;

			}//end try catch


		}//end string GetUserFriendsInfoBatch




		public byte[] GetUserImage (string userId)
		{

			string requestUrl = string.Format ("https://graph.facebook.com/{0}/picture", userId);

			try {
				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";
				request.KeepAlive = true;

				using (MemoryStream ms = new MemoryStream()) {
					byte[] buffer = new byte[4096];
					using (Stream responseStream = request.GetResponse().GetResponseStream()) {
						
						while (true) {
							
							int readCount = responseStream.Read (buffer, 0, buffer.Length);
							if (readCount == 0) {
								break;
							}//end if
							
							ms.Write (buffer, 0, readCount);
							
						}//end while
						
					}//end using responseStream
					
					return ms.ToArray ();
					
				}//end using ms

			} catch (Exception ex) {
				throw ex;
			}//end try catch

		}//end byte[] GetUserImage




		/// <summary>
		/// Gets all the user's friends. The response is to be used with GetUserFriendsInfoBatch
		/// </summary>
		/// <returns>
		/// The user friends.
		/// </returns>
		/// <param name='startIndex'>
		/// Start index.
		/// </param>
		/// <param name='count'>
		/// Count of items to return.
		/// </param>
		public string GetUserFriends (int startIndex, int count)
		{

			//string requestUrl = string.Format("https://graph.facebook.com/me/friends?access_token={0}", this.AccessToken);
			string requestUrl = string.Format ("https://graph.facebook.com/me/friends?offset={0}&limit={1}&access_token={2}&fields=first_name,last_name,birthday",
			                                  startIndex, count, this.AccessToken);

			try {

				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";
	
				string responseJsonStr = string.Empty;
				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {
					responseJsonStr = sr.ReadToEnd ();
				}//end using sr
	
				return responseJsonStr;

			} catch (Exception ex) {

				throw ex;
			}//end try catch

		}//end string GetUserFriends





		/// <summary>
		/// Gets all user's facebook friends.
		/// </summary>
		/// <returns>
		/// Raw JSON response containing all user's friends.
		/// </returns>
		public string GetAllUserFriends ()
		{

			string requestUrl = string.Format ("https://graph.facebook.com/me/friends?access_token={0}&fields=first_name,last_name,birthday",
			                                  this.AccessToken);

			try {

				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";

				string responseJsonStr = string.Empty;
				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {
					responseJsonStr = sr.ReadToEnd ();
				}//end using sr

				return responseJsonStr;

			} catch (Exception ex) {
				throw ex;
			}//end try catch

		}//end string GetAllUserFriends



		/// <summary>
		/// Gets the photo albums of the user.
		/// </summary>
		/// <returns>
		/// A JSON response containing the user's photo albums.
		/// </returns>
		public string GetPhotoAlbums ()
		{

			string requestUrl = string.Format ("https://graph.facebook.com/me/albums?access_token={0}&fields=id,name,count",
			                                  this.AccessToken);

			try {

				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {

					string responseStr = sr.ReadToEnd ();
					return responseStr;

				}//end using sr

			} catch (Exception ex) {

				throw ex;

			}//end try catch

		}//end string GetPhotoAlbums



		/// <summary>
		/// Gets the album photos.
		/// </summary>
		/// <returns>
		/// A JSON response containing photo information for a specific album.
		/// </returns>
		/// <param name='albumID'>
		/// The album ID for which to get the photo information
		/// </param>
		public string GetAlbumPhotos (string albumID)
		{

			string requestUrl = string.Format ("https://graph.facebook.com/{0}/photos?access_token={1}&fields=id,picture,source&offset=0&limit=100",
			                                  albumID, this.AccessToken);

			try {

				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {

					string responseStr = sr.ReadToEnd ();

					return responseStr;

				}//end using sr

			} catch (Exception ex) {
				throw ex;
			}//end try catch

		}//end string GetAlbumPhotos




		public string GetAllVideos ()
		{

			string requestUrl = 
				string.Format ("https://graph.facebook.com/me/videos/uploaded?access_token={0}&fields=id,picture,source&offset=0&limit=100",
				              this.AccessToken);

			try {

				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {
					string responseStr = sr.ReadToEnd ();

					return responseStr;

				}//end using sr

			} catch (Exception ex) {

				throw ex;

			}//end try catch

		}//end string GetAllVideos




		/// <summary>
		/// Gets an image.
		/// </summary>
		/// <returns>
		/// A byte[] buffer containing the image.
		/// </returns>
		/// <param name='pictureUrl'>
		/// The image's url.
		/// </param>
		public byte[] GetImage (string pictureUrl)
		{

			try {

				HttpWebRequest request = WebRequest.Create (pictureUrl) as HttpWebRequest;
				request.Method = "GET";
				request.KeepAlive = true;

				using (MemoryStream ms = new MemoryStream()) {
					byte[] buffer = new byte[4096];
					using (Stream responseStream = request.GetResponse().GetResponseStream()) {
						
						while (true) {
							
							int readCount = responseStream.Read (buffer, 0, buffer.Length);
							if (readCount == 0) {
								break;
							}//end if
							
							ms.Write (buffer, 0, readCount);
							
						}//end while
						
					}//end using responseStream
					
					return ms.ToArray ();
					
				}//end using ms

			} catch (Exception ex) {
				throw ex;
			}//end try catch

		}//end byte[] GetImage





		public bool RefreshAccessToken ()
		{

			try {

				string requestUrl = string.Format ("https://graph.facebook.com/oauth/access_token?" +
					"client_id={0}&" +
					"client_secret={1}&" +
					"grant_type=fb_exchange_token&" +
					"fb_exchange_token={2}",
				                                  this.ConsumerKey,
				                                  this.ConsumerSecret,
				                                  this.AccessToken);

				HttpWebRequest request = WebRequest.Create (requestUrl) as HttpWebRequest;
				request.Method = "GET";
	
				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {
					string responseStr = sr.ReadToEnd ();
	
					NameValueCollection nvc = HttpUtility.ParseQueryString (responseStr);
					this.AccessToken = nvc ["access_token"];
					this.AccessTokenExpirationTime = DateTime.Now.AddSeconds (Convert.ToDouble (nvc ["expires"]));
	
					return true;
	
				}//end using sr

			} catch (Exception ex) {
				throw ex;
			}//end try catch

		}//end bool RefreshAccessToken






		/// <summary>
		/// Posts a message to the specified user's wall.
		/// </summary>
		/// <returns>
		/// True if the wall post was successful.
		/// </returns>
		/// <param name='message'>
		/// The message to send.
		/// </param>
		/// <param name='userID'>
		/// The Facebook user ID of the user's wall to post to.
		/// </param>
		public bool PostToFeed (string message, string userID)
		{

			if (string.IsNullOrEmpty (this.AccessToken)) {
				throw new InvalidOperationException ("The access token property is not set! Try calling CheckForAccessToken first.");
			}//end if

			return this.HttpPostToWall (message, userID, false);

		}//end bool PostToWall
		
		#endregion Public methods

			
		
		
		
		
		#region Private methods
		
				
		private bool HttpPostToWall (string message, string userID, bool doAsync)
		{
			
			try {
				
				HttpWebRequest request = 
					WebRequest.Create (string.Format ("https://graph.facebook.com/{0}/feed?access_token={1}&message={2}", userID, this.AccessToken, message)) as HttpWebRequest;
				request.Method = "POST";
				request.KeepAlive = true;

				if (doAsync) {
					request.BeginGetResponse (this.HttpPostToWallCallback, new RequestState<string> (request, message));
				} else {

					using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream())) {
						string responseStr = sr.ReadToEnd ();
						JsonValue responseJson = JsonValue.Parse (responseStr);
						if (responseJson.ContainsKey ("id")) {
#if(DEBUG)
							System.Diagnostics.Debug.WriteLine ("Post sent, its id is: {0}", (string)responseJson ["id"]);
#endif
							return true;
						} else {
#if(DEBUG)
							System.Diagnostics.Debug.WriteLine ("PostToWall response: {0}", responseStr);
#endif
						}//end if else

					}//end using sr

				}//end if else

				return false;
				
			} catch (Exception ex) {
				
				if (null != this.PostToWallCompleted) {
					this.PostToWallCompleted (this, new PostToWallCompletedEventArgs ("POSTID", message, false, ex));
				}//end if

				if (!doAsync) {
					throw ex;
				}//end try catch
				
				return false;
				
			}//end try catch
			
		}//end bool HttpPostToWall
		
		
		
		
		private void HttpPostToWallCallback (IAsyncResult asRes)
		{
			
			HttpWebRequest request = null;
			HttpWebResponse response = null;
			string responseString = string.Empty;
			string message = string.Empty;
			string postID = string.Empty;
			RequestState<string> requestState = null;
			
			try {
				
				requestState = asRes.AsyncState as RequestState<string>;
				message = requestState.State;
				request = requestState.Request;
				response = request.EndGetResponse (asRes) as HttpWebResponse;
				
				using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
					
					responseString = sr.ReadToEnd ();
					
				}//end using StreamReader;
				
				JsonValue responseJson = JsonValue.Parse (responseString);
				if (responseJson.ContainsKey ("id")) {
					postID = responseJson ["id"];
				}//end if
				
				if (null != this.PostToWallCompleted) {
					
					this.PostToWallCompleted (this, new PostToWallCompletedEventArgs (postID, message, false, null));
					
				}//end if
				
			} catch (WebException ex) {
			
				string errorResponse = string.Empty;
				
				if (ex.Response != null) {
					
					using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream())) {
						errorResponse = sr.ReadToEnd ();
					}//end using sr
					
				}//end if
				
				if (null != this.PostToWallCompleted) {
					this.PostToWallCompleted (this, new PostToWallCompletedEventArgs (string.Empty, message, !string.IsNullOrEmpty (errorResponse), ex));
				}//end if
				
			} finally {
				
				if (null != response) {
					response.Close ();
					response = null;
				}//end if
				
			}//end try catch finally		
			
		}//end void HttpPostToWallCallback
		
		#endregion Private methods

		#region ICertificatePolicy implementation
		public bool CheckValidationResult (ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int certificateProblem)
		{
			return true;
		}
		#endregion
	}
}


namespace OAuthLib
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
#if !SILVERLIGHT
    using System.Web;
#endif

    /// <include file='OAuthUtility.xml' path='OAuthUtility/OAuthUtility/*'/>
    public static class OAuthUtility
    {
        #region Public Methods
        /// <summary>
        /// Gets the request token.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="callbackAddress">The callback address. For PIN-based authentication "oob" should be supplied.</param>
        /// <returns></returns>
        public static OAuthTokenResponse GetRequestToken(string consumerKey, string consumerSecret, string callbackAddress, string requestTokenUrl, HTTPVerb httpVerb)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(callbackAddress))
            {
                throw new ArgumentNullException("callbackAddress", @"You must always provide a callback url when obtaining a request token. For PIN-based authentication, use ""oob"" as the callback url.");
            }

            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri(requestTokenUrl),
                httpVerb,
                new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret },
				"");

            if (!string.IsNullOrEmpty(callbackAddress))
            {
                builder.Parameters.Add("oauth_callback", callbackAddress);
            }

            string responseBody = null;

            try
            {
                HttpWebResponse webResponse = builder.ExecuteRequest();
                Stream responseStream = webResponse.GetResponseStream();
                if (responseStream != null) responseBody = new StreamReader(responseStream).ReadToEnd();
            }
            catch (WebException wex)
            {
                throw new Exception(wex.Message, wex);
            }

            return new OAuthTokenResponse
            {
                Token = ParseQuerystringParameter("oauth_token", responseBody),
                TokenSecret = ParseQuerystringParameter("oauth_token_secret", responseBody),
                VerificationString = ParseQuerystringParameter("oauth_verifier", responseBody)
            };
        }

        /// <summary>
        /// Tries to the parse querystring parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="text">The text.</param>
        /// <returns>The value of the parameter or an empty string.</returns>
        /// <remarks></remarks>
        private static string ParseQuerystringParameter(string parameterName, string text)
        {
            Match expressionMatch = Regex.Match(text, string.Format(@"{0}=(?<value>[^&]+)", parameterName));

            if (!expressionMatch.Success)
            {
                return string.Empty;
            }

            return expressionMatch.Groups["value"].Value;
        }

#if !SILVERLIGHT
        public static OAuthTokenResponse GetRequestToken(string consumerKey, string consumerSecret, string callbackAddress, WebProxy proxy, string requestTokenUrl, HTTPVerb httpVerb)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(callbackAddress))
            {
                throw new ArgumentNullException("callbackAddress", @"You must always provide a callback url when obtaining a request token. For PIN-based authentication, use ""oob"" as the callback url.");
            }

            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri(requestTokenUrl),
                httpVerb,
                new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret },
				"") { Proxy = proxy };

            if (!string.IsNullOrEmpty(callbackAddress))
            {
                builder.Parameters.Add("oauth_callback", callbackAddress);
            }

            string responseBody = null;

            try
            {
                HttpWebResponse webResponse = builder.ExecuteRequest();
                Stream responseStream = webResponse.GetResponseStream();
                if (responseStream != null) responseBody = new StreamReader(responseStream).ReadToEnd();
            }
            catch (WebException wex)
            {
                throw new Exception(wex.Message, wex);
            }

            Match matchedValues = Regex.Match(responseBody,
                                              @"oauth_token=(?<token>[^&]+)|oauth_token_secret=(?<secret>[^&]+)|oauth_verifier=(?<verifier>[^&]+)");

            return new OAuthTokenResponse
            {
                Token = matchedValues.Groups["token"].Value,
                TokenSecret = matchedValues.Groups["secret"].Value,
                VerificationString = matchedValues.Groups["verifier"].Value
            };
        }
#endif
        public static OAuthTokenResponse GetAccessToken(string consumerKey, string consumerSecret, string requestToken, string accessTokenSecret, string verifier, string accessTokenUrl, HTTPVerb httpVerb, ServiceType serviceType)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(requestToken))
            {
                throw new ArgumentNullException("requestToken");
            }

            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri(accessTokenUrl),
                httpVerb,
				//new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret },
				new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret, AccessToken = requestToken, AccessTokenSecret = accessTokenSecret },
				"");

            if (!string.IsNullOrEmpty(verifier))
            {
                builder.Parameters.Add("oauth_verifier", verifier);
            }

            builder.Parameters.Add("oauth_token", requestToken);

            string responseBody;

            try
            {
                HttpWebResponse webResponse = builder.ExecuteRequest();

                responseBody = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException wex)
            {
                throw new Exception(wex.Message, wex);
            }

            OAuthTokenResponse response = new OAuthTokenResponse();
            response.Token = Regex.Match(responseBody, @"oauth_token=([^&]+)").Groups[1].Value;
            response.TokenSecret = Regex.Match(responseBody, @"oauth_token_secret=([^&]+)").Groups[1].Value;
			
			if (serviceType == ServiceType.Twitter)
			{
				response.UserId = long.Parse(Regex.Match(responseBody, @"user_id=([^&]+)").Groups[1].Value, CultureInfo.CurrentCulture);
				response.ScreenName = Regex.Match(responseBody, @"screen_name=([^&]+)").Groups[1].Value;
				
			} else if (serviceType == ServiceType.DropBox)
			{
				
				response.UserId = long.Parse(Regex.Match(responseBody, @"uid=([^&]+)").Groups[1].Value, CultureInfo.CurrentCulture);
				
			}//end if
            return response;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="requestToken">The request token.</param>
        /// <param name="verifier">The pin number or verifier string.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns>
        /// An <see cref="OAuthTokenResponse"/> class containing access token information.
        /// </returns>
        public static OAuthTokenResponse GetAccessToken(string consumerKey, string consumerSecret, string requestToken, string verifier, WebProxy proxy, string accessTokenUrl)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            if (string.IsNullOrEmpty(requestToken))
            {
                throw new ArgumentNullException("requestToken");
            }

            WebRequestBuilder builder = new WebRequestBuilder(
                new Uri(accessTokenUrl),
                HTTPVerb.GET,
				new OAuthTokens { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret },
				"");

            builder.Proxy = proxy;

            if (!string.IsNullOrEmpty(verifier))
            {
                builder.Parameters.Add("oauth_verifier", verifier);
            }

            builder.Parameters.Add("oauth_token", requestToken);

            string responseBody;

            try
            {
                HttpWebResponse webResponse = builder.ExecuteRequest();

                responseBody = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            catch (WebException wex)
            {
                throw new Exception(wex.Message, wex);
            }

            OAuthTokenResponse response = new OAuthTokenResponse();
            response.Token = Regex.Match(responseBody, @"oauth_token=([^&]+)").Groups[1].Value;
            response.TokenSecret = Regex.Match(responseBody, @"oauth_token_secret=([^&]+)").Groups[1].Value;
            response.UserId = long.Parse(Regex.Match(responseBody, @"user_id=([^&]+)").Groups[1].Value, CultureInfo.CurrentCulture);
            response.ScreenName = Regex.Match(responseBody, @"screen_name=([^&]+)").Groups[1].Value;
            return response;
        }
#endif
        #endregion

        /// <summary>
        /// Builds the authorization URI.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <returns>A new <see cref="Uri"/> instance.</returns>
        public static Uri BuildAuthorizationUri(string requestToken, string authUrl)
        {
            return BuildAuthorizationUri(requestToken, false, authUrl);
        }

        /// <summary>
        /// Builds the authorization URI.
        /// </summary>
        /// <param name="requestToken">The request token.</param>
        /// <param name="authenticate">if set to <c>true</c>, the authenticate url will be used. (See: "Sign in with Twitter")</param>
        /// <returns>A new <see cref="Uri"/> instance.</returns>
        public static Uri BuildAuthorizationUri(string requestToken, bool authenticate, string authUrl)
        {
            StringBuilder parameters = new StringBuilder(authUrl);

            if (authenticate)
            {
                parameters.Append("authenticate");
            }
            else
            {
                parameters.Append("authorize");
            }

            parameters.AppendFormat("?oauth_token={0}", requestToken);

            return new Uri(parameters.ToString());
        }

//        #if !LITE && !SILVERLIGHT
//        /// <summary>
//        /// Gets the access token during callback.
//        /// </summary>
//        /// <param name="consumerKey">The consumer key.</param>
//        /// <param name="consumerSecret">The consumer secret.</param>
//        /// <returns>
//        /// Access tokens returned by the Twitter API
//        /// </returns>
//        public static OAuthTokenResponse GetAccessTokenDuringCallback(string consumerKey, string consumerSecret)
//        {
//			
//            HttpContext context = HttpContext.Current;
//            if (context == null || context.Request == null)
//            {
//                throw new ApplicationException("Could not located the HTTP context. GetAccessTokenDuringCallback can only be used in ASP.NET applications.");
//            }
//
//            string requestToken = context.Request.QueryString["oauth_token"];
//            string verifier = context.Request.QueryString["oauth_verifier"];
//
//            if (string.IsNullOrEmpty(requestToken))
//            {
//                throw new ApplicationException("Could not locate the request token.");
//            }
//
//            if (string.IsNullOrEmpty(verifier))
//            {
//                throw new ApplicationException("Could not locate the verifier value.");
//            }
//
//            return GetAccessToken(consumerKey, consumerSecret, requestToken, verifier);
//        }
//
//        /// <summary>
//        /// Adds the OAuth Echo header to the supplied web request.
//        /// </summary>
//        /// <param name="request">The request.</param>
//        /// <param name="tokens">The tokens.</param>
//        public static void AddOAuthEchoHeader(WebRequest request, OAuthTokens tokens)
//        {
//            WebRequestBuilder builder = new WebRequestBuilder(
//                new Uri("https://api.twitter.com/1/account/verify_credentials.json"), 
//                HTTPVerb.POST,
//				tokens,
//				"");
//
//            builder.PrepareRequest();
//
//            request.Headers.Add("X-Verify-Credentials-Authorization", builder.GenerateAuthorizationHeader());
//            request.Headers.Add("X-Auth-Service-Provider", "https://api.twitter.com/1/account/verify_credentials.json");
//        }
//#endif
    }
}

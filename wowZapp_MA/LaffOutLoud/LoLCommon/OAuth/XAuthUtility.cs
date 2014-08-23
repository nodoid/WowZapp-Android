namespace OAuthLib
{
	using System;
    using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Text.RegularExpressions;
	//using Twitterizer.Core;

	/// <summary>
	/// The XAuthUtility class.
	/// </summary>
	public static class XAuthUtility
	{
		/// <summary>
		/// Allows OAuth applications to directly exchange Twitter usernames and passwords for OAuth access tokens and secrets.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns>A <see cref="OAuthTokenResponse"/> instance.</returns>
		public static OAuthTokenResponse GetAccessTokens(string consumerKey, string consumerSecret, string username, string password)
		{
			if (string.IsNullOrEmpty(consumerKey))
			{
				throw new ArgumentNullException("consumerKey");
			}

			if (string.IsNullOrEmpty(consumerSecret))
			{
				throw new ArgumentNullException("consumerSecret");
			}

			if (string.IsNullOrEmpty(username))
			{
				throw new ArgumentNullException("username");
			}

			if (string.IsNullOrEmpty(password))
			{
				throw new ArgumentNullException("password");
			}

			OAuthTokenResponse response = new OAuthTokenResponse();

			try
			{
				WebRequestBuilder builder = new WebRequestBuilder(
					new Uri("https://api.twitter.com/oauth/access_token"),
					HTTPVerb.POST,
					new OAuthTokens() { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret },
					"");

				builder.Parameters.Add("x_auth_username", username);
				builder.Parameters.Add("x_auth_password", password);
				builder.Parameters.Add("x_auth_mode", "client_auth");

				string responseBody = new StreamReader(builder.ExecuteRequest().GetResponseStream()).ReadToEnd();

				response.Token = Regex.Match(responseBody, @"oauth_token=([^&]+)").Groups[1].Value;
				response.TokenSecret = Regex.Match(responseBody, @"oauth_token_secret=([^&]+)").Groups[1].Value;
				if (responseBody.Contains("user_id="))
					response.UserId = long.Parse(Regex.Match(responseBody, @"user_id=([^&]+)").Groups[1].Value, CultureInfo.CurrentCulture);
				response.ScreenName = Regex.Match(responseBody, @"screen_name=([^&]+)").Groups[1].Value;
			}
			catch (WebException wex)
			{
				throw new Exception(wex.Message, wex);
			}

			return response;
		}
	}
}

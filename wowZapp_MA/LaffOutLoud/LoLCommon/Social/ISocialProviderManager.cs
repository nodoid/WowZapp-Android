// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using LOLAccountManagement;


namespace LOLApp_Common
{
	public interface ISocialProviderManager
	{
		string AccessToken { get; }
		string AccessTokenSecret { get; }
		string RefreshToken { get; }
		DateTime? AccessTokenExpirationTime { get; }
		string BrowserAuthUrl { get; }
		bool CheckForAccessToken(string url);
		bool UserAccepted(string url);
		string ConsumerKey { get; }
		string ConsumerSecret { get; }
		AccountOAuth.OAuthTypes ProviderType { get; }
		string GetBrowserAuthUrl();
		string GetUserInfo();
		User GetUserInfoObject(AccountOAuth accountOAuth);
		string GetUserFriends(int startIndex, int count);
		string GetAllUserFriends();
		byte[] GetUserImage(string criteria);
		bool PostToFeed(string message, string userID);
		string GetPhotoAlbums();
		string GetAlbumPhotos(string albumID);
		string GetAllVideos();
		byte[] GetImage(string pictureUrl);

	}
}


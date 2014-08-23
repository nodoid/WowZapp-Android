// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
namespace LOLApp_Common
{
	/// <summary>
	/// Called when the browser has finished loading a URL.
	/// </summary>
	public delegate void LoadedUrlHandler<T>(T browser, BrowserEventArgs args) where T : class;
	
	/// <summary>
	/// Called when the browser has failed to load the URL.
	/// </summary>
	public delegate void LoadUrlFailedHandler<T>(T browser, BrowserEventArgs args) where T : class;
	
	/// <summary>
	/// Called when the browser has received the access token url.
	/// </summary>
	public delegate void AccessTokenReceivedHandler(DateTime occurred, DateTime expires, string accessToken, string accessTokenSecret, string refreshToken);


	/// <summary>
	/// Called when the browser fails to load the auth page of a social network.
	/// </summary>
	public delegate void FailedToLoadAuthPage(object sender, string localizedDescription);
	
	/// <summary>
	/// Upload image completed handler.
	/// </summary>
	public delegate void UploadImageCompletedHandler(object sender, UploadImageCompletedEventArgs args);
	
	/// <summary>
	/// Triggered when the AlbumExists method is called and returns the result.
	/// </summary>
	public delegate void AlbumExistsCompletedHandler(object sender, AlbumExistsEventArgs args);
	
	/// <summary>
	/// Triggered when the CreateAlbum method is called to create an album.
	/// </summary>
	public delegate void CreateAlbumCompletedHandler(object sender, CreateAlbumEventArgs args);
	
	
	
	/// <summary>
	/// Triggered when the PostToWall method is called to create an album.
	/// </summary>
	public delegate void PostToWallCompletedHandler(object sender, PostToWallCompletedEventArgs args);

}


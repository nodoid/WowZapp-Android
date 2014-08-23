// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
namespace LOLApp_Common
{
	public class BrowserEventArgs : EventArgs
	{
		
		public BrowserEventArgs (DateTime timeOccurred, EventArgs platformEventArgs)
		{
			this.DateTimeOccurred = timeOccurred;
			this.PlatformEventArgs = platformEventArgs;
		}
				
		
		
		public DateTime DateTimeOccurred {
			get;
			private set;
		}
		
		
		
		
		public EventArgs PlatformEventArgs {
			get;
			private set;
		}
		
	}//end class BrowserEventArgs
	
	
	
	
	public class UploadImageCompletedEventArgs : EventArgs
	{
		
		public UploadImageCompletedEventArgs(DateTime timeOccurred, string fileName, string photoID, Exception ex)
		{
			
			this.DateTimeOccurred = timeOccurred;
			this.FileName = fileName;
			this.Error = ex;
			
		}
		
		
		
		
		public DateTime DateTimeOccurred
		{
			
			get;
			private set;
			
		}
		
		
		
		
		public string FileName
		{
			
			get;
			private set;
			
		}
		
		
		
		public string PhotoID
		{
			
			get;
			private set;
			
		}
		
		
		
		
		public Exception Error
		{
			
			get;
			private set;
			
		}
		
	}
	
	
	
	public class AlbumExistsEventArgs : EventArgs
	{
		
		public AlbumExistsEventArgs(bool exists, string albumName, string albumID, Exception ex)
		{
			this.Exists = exists;
			this.AlbumName = albumName;
			this.AlbumID = albumID;
			this.Error = ex;
		}
		
		
		
		
		public bool Exists
		{
			get;
			private set;
		}
		
		
		
		
		public string AlbumName
		{
			get;
			private set;
		}
		
		
		
		
		public string AlbumID
		{
			get;
			private set;
		}
		
		
		
		
		public Exception Error
		{
			get;
			private set;
		}
		
	}
	
	
	
	
	public class CreateAlbumEventArgs : EventArgs
	{
		
		public CreateAlbumEventArgs(string newAlbumID, string newAlbumName, string newAlbumDescription, Exception error)
		{
			this.NewAlbumID = newAlbumID;
			this.NewAlbumName = newAlbumName;
			this.NewAlbumDescription = newAlbumDescription;
			this.Error = error;
		}
		
		
		
		
		public string NewAlbumID
		{
			get;
			private set;
		}
		
		
		
		public string NewAlbumName
		{
			get;
			private set;
		}
		
		
		
		
		public string NewAlbumDescription
		{
			get;
			private set;
		}
		
		
		
		
		public Exception Error
		{
			get;
			private set;
		}
		
	}
	
	
	
	
	public class PostToWallCompletedEventArgs : EventArgs
	{
		
		public PostToWallCompletedEventArgs(string postID, string postMessage, bool isAuthException, Exception error)
		{
			
			this.PostID = postID;
			this.PostMessage = postMessage;
			this.Error = error;
			this.IsAuthException = isAuthException;
			
		}//end ctor
		
		
		
		public string PostID
		{
			get;
			private set;
		}//end string PostID
		
		
		
		public string PostMessage
		{
			get;
			private set;
		}//end string PostMessage
		
		
		
		public Exception Error
		{
			get;
			private set;
		}//end Exception Error
		
		
		
		public bool IsAuthException
		{
			get;
			private set;
		}//end bool IsAuthException
		
	}//end class PostToWallCompletedEventArgs
}


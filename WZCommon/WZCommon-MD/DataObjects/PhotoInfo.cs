// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com

namespace WZCommon
{
    public class PhotoInfo
    {

		#region Constructors

        public PhotoInfo(string id, string albumID, string smallUrl, string largeUrl)
        {
            this.ID = id;
            this.AlbumID = albumID;
            this.SmallUrl = smallUrl;
            this.LargeUrl = largeUrl;
        }

		#endregion Constructors




		#region Properties

        public string ID
        {
            get;
            private set;
        }//end string ID



        public string AlbumID
        {
            get;
            set;
        }//end string AlbumID



        public string SmallUrl
        {
            get;
            private set;
        }//end string SmallUrl



        public string LargeUrl
        {
            get;
            private set;
        }//end string LargeUrl



        public byte[] ThumbImage
        {
            get;
            set;
        }//end byte[] ThumbImage

		#endregion Properties



		#region Overrides

        public override string ToString()
        {
            return string.Format("[PhotoInfo: ID={0}, AlbumID={1}, SmallUrl={2}, LargeUrl={3}, ThumbImage={4}]", ID, AlbumID, SmallUrl, LargeUrl, ThumbImage);
        }

		#endregion Overrides
    }
}


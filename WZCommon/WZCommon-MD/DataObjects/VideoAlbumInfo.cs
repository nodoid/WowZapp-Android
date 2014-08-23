using System;

namespace WZCommon
{
    public class VideoAlbumInfo
    {
        public string ID
		{ get; private set; }
        public string Description
		{ get; private set; }
        public string PictureURL
		{ get; private set; }
        public string SourceUrl
		{ get; private set; }
        public DateTime? CreationDate
		{ get; private set; }
        public byte[] VideoThumb
		{ get; set; }
		
        public VideoAlbumInfo(string id, string description, string pictureUrl, string sourceUrl, DateTime? creationDate)
        {
            ID = id;
            Description = description;
            PictureURL = pictureUrl;
            SourceUrl = sourceUrl;
            CreationDate = creationDate;
        }
		
        public override string ToString()
        {
            return string.Format("[VideoAlbumInfo: ID={0}, Description={1}, PictureURL={2}, SourceUrl={3}, CreateDate={4}, VideoThumb={5}]",
			ID, Description, PictureURL, SourceUrl, CreationDate, VideoThumb);
        }
    }
}


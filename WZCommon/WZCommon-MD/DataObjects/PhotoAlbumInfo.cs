// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com

namespace WZCommon
{
    public class PhotoAlbumInfo
    {

		#region Constructors

        public PhotoAlbumInfo(string id, string name, int count)
        {
            this.ID = id;
            this.Name = name;
            this.ItemCount = count;
        }

		#endregion Constructors



		#region Properties

        public string ID
        {
            get;
            private set;
        }//end string ID



        public string Name
        {
            get;
            private set;
        }//end string Name



        public int ItemCount
        {
            get;
            private set;
        }//end int ItemCount

		#endregion Properties



		#region Overrides

        public override string ToString()
        {
            return string.Format("[PhotoAlbumInfo: ID={0}, Name={1}, ItemCount={2}]", ID, Name, ItemCount);
        }

		#endregion Overrides
    }
}


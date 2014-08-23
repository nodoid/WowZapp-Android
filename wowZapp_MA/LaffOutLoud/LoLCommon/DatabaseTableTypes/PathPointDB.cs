using System;
using SQLite;

using PreserveProps = Android.Runtime.PreserveAttribute;

namespace LOLApp_Common
{
	[PreserveProps(AllMembers=true)]
	public class PathPointDB
	{
		
		#region Constructors
		
		public PathPointDB (int drawingInfoDBID, int sortOder, float x, float y)
		{
			this.DrawingInfoDBID = drawingInfoDBID;
			this.SortOrder = sortOder;
			this.X = x;
			this.Y = y;
		}
		
		
		
		public PathPointDB ()
		{
		}//end ctor
		
		#endregion Constructors
		
		
		
		
		
		#region Properties
		
		[PrimaryKey, AutoIncrement]
		public int DBID {
			get;
			private set;
		}//end int DBID
		
		
		
		[Indexed]
		public int DrawingInfoDBID {
			get;
			private set;
		}//end int DrawingInfoDBID
		
		
		
		
		public int SortOrder {
			get;
			private set;
		}//end int SortOrder
		
		
		
		
		public float X {
			get;
			private set;
		}//end float X
		
		
		
		
		public float Y {
			get;
			private set;
		}//end float Y
		
		#endregion Properties
	}
}


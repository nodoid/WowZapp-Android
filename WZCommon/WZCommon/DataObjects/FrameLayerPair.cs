// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;

namespace WZCommon
{
	public struct FrameLayerPair
	{

		#region Constructors

		public FrameLayerPair (int frameID, int layerID) : this()
		{
			this.FrameID = frameID;
			this.LayerID = layerID;
		}

		#endregion Constructors



		#region Properties

		public int FrameID
		{
			get;
			set;
		}//end int FrameID



		public int LayerID
		{
			get;
			set;
		}//end LayerID

		#endregion Properties



		#region Overrides

		public override int GetHashCode ()
		{
			return this.FrameID.GetHashCode() ^ this.LayerID.GetHashCode();
		}



		public override bool Equals (object obj)
		{

			if (obj is FrameLayerPair)
			{
				FrameLayerPair otherObj = (FrameLayerPair)obj;
				return this.FrameID.Equals(otherObj.FrameID) &&
					this.LayerID.Equals(otherObj.LayerID);
			}//end if
			return false;
		}



		public override string ToString ()
		{
			return string.Format ("[FrameLayerPair: FrameID={0}, LayerID={1}]", FrameID, LayerID);
		}

		#endregion Overrides



		#region Operators

		public static bool operator == (FrameLayerPair first, FrameLayerPair second)
		{

			return first.Equals(second);

		}//end static bool operator ==



		public static bool operator != (FrameLayerPair first, FrameLayerPair second)
		{

			return !first.Equals(second);

		}//end static bool operator !=

		#endregion Operators
	}
}


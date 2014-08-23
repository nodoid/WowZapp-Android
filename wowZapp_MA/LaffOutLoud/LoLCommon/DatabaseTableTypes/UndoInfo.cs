using System;
using SQLite;
using wowZapp.Animate;
using PreserveProps = Android.Runtime.PreserveAttribute;


namespace LOLApp_Common
{
	[PreserveProps(AllMembers=true)]
	public class UndoInfo
	{
		public UndoInfo (FrameLayerPair layerAddress, int drawingItemID, UndoAction undoAction, object oldValue, object newValue)
		{		
			this.LayerAddress = layerAddress;
			this.DrawingItemID = drawingItemID;
			this.Action = undoAction;
			this.OldValue = oldValue;
			this.NewValue = newValue;	
		}

		public UndoInfo ()
		{
			
		}//end UndoInfo

		[PrimaryKey, AutoIncrement]
		public int DBID {
			get;
			private set;
		}//end int DBID

		[Ignore]
		public FrameLayerPair LayerAddress {
			get;
			private set;
		}//end FrameLayerPair LayerAddress

		public int SortOrder {
			get;
			set;
		}//end int SortOder

		public int FrameID {
			get {
				return this.LayerAddress.FrameID;
			}
			private set {
				FrameLayerPair layerAddress = this.LayerAddress;
				layerAddress.FrameID = value;
				this.LayerAddress = layerAddress;
			}//end get private set
		}
		
		public int LayerID {
			get {
				return this.LayerAddress.LayerID;
			}
			private set {
				FrameLayerPair layerAddress = this.LayerAddress;
				layerAddress.LayerID = value;
				this.LayerAddress = layerAddress;
			}//end get private set
		}//end int LayerID
		
		public int DrawingItemID {
			get;
			private set;
		}//end int DrawingItemID

		public UndoAction Action {
			get;
			private set;
		}//end UndoAction Action

		[Ignore]
		public object OldValue {
			get;
			private set;
		}//end object OldValue

		[Ignore]
		public object NewValue {
			get;
			private set;
		}//end object NewValue

		public int OldValueDBID {
			get;
			set;
		}//end int OldValueDBID

		public int NewValueDBID {
			get;
			set;
		}//end int NewValueDBID

		public void SetLayerAddress (FrameLayerPair layerAddress)
		{
			this.LayerAddress = layerAddress;
		}//end void SetLayerAddress
	}

	public enum UndoAction
	{
		LayerCreated,
		LayerEdited,
		DrawingItemCreated,
		DrawingItemRotated,
		DrawingItemScaled,
		DrawingItemMoved	
	}//end enum UndoAction
}


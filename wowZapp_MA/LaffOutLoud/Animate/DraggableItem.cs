using Android.Graphics;
using Android.Widget;
using System;
using System.Drawing;
using LOLApp_Common;
using System.Collections.Generic;
using WZCommon;
namespace wowZapp.Animate
{
	public class DraggableItem : IDisposable
	{
		public DraggableItem ()
		{
		}
		
		public ImageView Item {
			get;
			set;
		}//end UIView Item

/*
		public CALayer Layer {
			get;
			set;
		}//end CALayer Layer

		public GridRow Row {
			get;
			set;
		}//end GridRow row
		*/
		
		public List<Pair<FrameLayerPair, RectangleF>> ItemFrames {
			get;
			set;
		}//end List<RectangleF> ItemFrames

		public SizeF OriginOffset {
			get;
			set;
		}//end SizeF OriginOffset

		public void Dispose ()
		{
			this.Dispose (true);
			GC.SuppressFinalize (this);
		}
	
		protected void Dispose (bool disposing)
		{	
			if (disposing) {		
				/*if (null != this.Layer) {
					this.Layer.Dispose ();
					this.Layer = null;
				}//end if
				
				if (null != this.Row) {
					this.Row = null;
				}*/	
			}//end if
		}//end void Dispose

		~DraggableItem ()
		{
			
#if(DEBUG)
			System.Diagnostics.Debug.WriteLine ("DraggableItem finalizer!");
#endif
			this.Dispose (false);
			
		}//end dtor	
	}
}
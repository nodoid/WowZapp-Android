using System;
using System.Collections.Generic;
using System.Drawing;

namespace wowZapp.Animate
{
	public class DraggableRow : IDisposable
	{
		public DraggableRow ()
		{
		}

		/*public GridRow Row {
			get;
			set;
		}//end GridRow Row

		public CALayer Layer {
			get;
			set;
		}//end CALayer Layer
*/
		public int InitialRowIndex {
			get;
			set;
		}//end int InitialRowIndex

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

		~DraggableRow ()
		{
#if(DEBUG)
			System.Diagnostics.Debug.WriteLine ("DraggableRow finalizer!");
#endif
			this.Dispose (false);	
		}//end dtor
		
	}
}


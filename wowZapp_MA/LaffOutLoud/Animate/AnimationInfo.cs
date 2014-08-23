using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LOLApp_Common;

namespace wowZapp.Animate
{
	public class AnimationInfo
	{
		public AnimationInfo (SizeF originalCanvasSize)
		{		
			this.OriginalCanvasSize = originalCanvasSize;	
		}

		public SizeF OriginalCanvasSize {
			get;
			private set;
		}//end SizeF CanvasSize

		public Dictionary<int, FrameInfo> Frames {
			get;
			private set;
		}//end Dictionary<int, FrameInfo> Frames
		
		public void AddFrameInfo (FrameInfo frameInfo)
		{
			this.Frames [frameInfo.ID] = frameInfo;	
		}//end void AddFrameInfo
	}
}


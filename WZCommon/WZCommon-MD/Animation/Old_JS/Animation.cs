using System.Collections.Generic;
using System;

namespace LOLMessageDelivery.Classes.LOLAnimation
{
	[Serializable]
	public partial class Animation
	{
		public Animation ()
		{
			this.ScreenObjects = new List<ScreenObject> ();
			this.Duration = 0;
		}
		
		public Animation (float Duration)
		{
			this.ScreenObjects = new List<ScreenObject> ();
			this.Duration = Duration;
		}
		
		public Animation (float Duration, List<ScreenObject> ScreenObjects)
		{
			this.ScreenObjects = ScreenObjects;
			this.Duration = Duration;
		}
	}
}
using System;
using LOLMessageDelivery.Classes.LOLAnimation;
using LOLMessageDelivery;
using SQLite;

namespace LOLMessageDelivery.Classes.Animation
{
	public partial class Animation
	{
		[Ignore]
		public Guid AnimationID 
		{
			get { return this.AnimationID; }
			set { this.AnimationID = value;}
		}
	}
}


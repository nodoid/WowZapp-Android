using Android.Widget;
using LOLApp_Common;

namespace wowZapp.Animate
{
	public class TransitionDescription
	{
		public TransitionDescription (string title, string description, ImageView icon, TransitionEffectType efType)
		{
			this.Title = title;
			this.Description = description;
			this.Icon = icon;
			this.EffectType = efType;
		}

		public string Title {
			get;
			private set;
		}//end string Title

		public string Description {
			get;
			private set;
		}//end string Description
		
		public ImageView Icon {
			get;
			private set;
		}//end UIImage Icon

		public TransitionEffectType EffectType {
			get;
			private set;
		}//end TransitionEffectType EffectType
	}
}


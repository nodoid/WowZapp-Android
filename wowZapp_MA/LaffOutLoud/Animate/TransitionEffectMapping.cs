using LOLApp_Common;

namespace wowZapp.Animate
{
	public class TransitionEffectMapping
	{
		public TransitionEffectMapping (TransitionEffectType efType, FrameLayerPair ownerLayer)
		{
			this.EffectType = efType;
			this.OwnerLayer = ownerLayer;
		}

		public TransitionEffectType EffectType {
			get;
			private set;
		}//end TransitionEffectType EffectType

		public FrameLayerPair OwnerLayer {
			get;
			private set;
		}//end FrameLayerPair OwnerLayer
	}
}


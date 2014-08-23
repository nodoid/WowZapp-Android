// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using LOLMessageDelivery.Classes.LOLAnimation;

namespace WZCommon
{
	public class TransitionEffectMapping
	{

		#region Constructors

		public TransitionEffectMapping (AnimationTypesTransitionEffectType efType, FrameLayerPair ownerLayer)
		{
			this.EffectType = efType;
			this.OwnerLayer = ownerLayer;
		}

		#endregion Constructors



		#region Properties

		public AnimationTypesTransitionEffectType EffectType
		{
			get;
			private set;
		}//end TransitionEffectType EffectType



		public FrameLayerPair OwnerLayer
		{
			get;
			private set;
		}//end FrameLayerPair OwnerLayer

		#endregion Properties
	}
}


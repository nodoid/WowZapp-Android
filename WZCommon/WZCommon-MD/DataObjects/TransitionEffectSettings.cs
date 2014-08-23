// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using SQLite;
using LOLMessageDelivery.Classes.LOLAnimation;

namespace WZCommon
{
	public class TransitionEffectSettings
	{

		#region Constructors

		public TransitionEffectSettings (int transitionID, FrameLayerPair layerAddress, AnimationTypesTransitionEffectType efTypeFlag, Action<TransitionEffectSettings> wildCardHandler)
		{
			this.TransitionID = transitionID;
			this.LayerAddress = layerAddress;

			if (AnimationUtils.CountTransitionEffects(efTypeFlag) > 1 ||
			    efTypeFlag == AnimationTypesTransitionEffectType.None)
			{
				throw new ArgumentException("Transition effect type argument should only contain 1 flag and should not be TransitionEffectType.None", 
				                            "efTypeFlag");
			}//end if

			this.EffectType = efTypeFlag;
			this.WildcardHandler = wildCardHandler;

			this.Duration = 0.5d;
			this.RotationCount = 1;
			this.Delay = 0d;
		}



		public TransitionEffectSettings()
		{

		}//end ctor

		#endregion Constructors




		#region Properties

		public int TransitionID
		{
			get;
			private set;
		}//end int TransitionID



		[PrimaryKey, AutoIncrement]
		public int DBID
		{
			get;
			private set;
		}//end int DBID



		[Indexed]
		public int TransitionInfoDBID
		{
			get;
			set;
		}//end int TransitionInfoDBID



		#region LayerAddress

		[Ignore]
		public FrameLayerPair LayerAddress
		{
			get;
			private set;
		}//end FrameLayerPair LayerAddress




		public int FrameID
		{
			get
			{
				return this.LayerAddress.FrameID;
			} private set
			{
				FrameLayerPair layerAddress = this.LayerAddress;
				layerAddress.FrameID = value;
				this.LayerAddress = layerAddress;
			}//end get private set

		}//end int FrameID




		public int LayerID
		{
			get
			{
				return this.LayerAddress.LayerID;
			} private set
			{
				FrameLayerPair layerAddress = this.LayerAddress;
				layerAddress.LayerID = value;
				this.LayerAddress = layerAddress;
			}//end get private set
		}//end int LayerID

		#endregion LayerAddress




		public AnimationTypesTransitionEffectType EffectType
		{
			get;
			private set;
		}//end TransitionEffectType EffectType




		[Ignore]
		public Action<TransitionEffectSettings> WildcardHandler
		{
			get;
			private set;
		}//end Action<TransitioknEffectSettings> WildcardHandler



		public double Duration
		{
			get;
			set;
		}//end double Duration



		public int RotationCount
		{
			get;
			set;
		}//end int RotationCount



		public double Delay
		{
			get;
			set;
		}//end double Delay

		#endregion Properties



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[TransitionEffectSettings: TransitionID={0}, LayerAddress={1}, EffectType={2}, WildcardHandler={3}, Duration={4}]", 
			                      TransitionID, LayerAddress, EffectType, WildcardHandler, Duration);
		}

		#endregion Overrides

	}
}


using System;
using SQLite;
using wowZapp.Animate;
using PreserveProps = Android.Runtime.PreserveAttribute;
namespace LOLApp_Common
{
	[PreserveProps(AllMembers=true)]
	public class TransitionEffectSettings
	{
		public TransitionEffectSettings (int transitionID, FrameLayerPair layerAddress, TransitionEffectType efTypeFlag, Action<TransitionEffectSettings> wildCardHandler)
		{
			this.TransitionID = transitionID;
			this.LayerAddress = layerAddress;
			
			if (AnimationUtils.CountTransitionEffects (efTypeFlag) > 1 ||
				efTypeFlag == TransitionEffectType.None) {
				throw new ArgumentException ("Transition effect type argument should only contain 1 flag and should not be TransitionEffectType.None", 
				                            "efTypeFlag");
			}//end if
			
			this.EffectType = efTypeFlag;
			this.WildcardHandler = wildCardHandler;
			
			this.Duration = 0.5d;
			this.RotationCount = 1;
			this.Delay = 0d;
		}

		public TransitionEffectSettings ()
		{
			
		}//end ctor

		public int TransitionID {
			get;
			private set;
		}//end int TransitionID

		[PrimaryKey, AutoIncrement]
		public int DBID {
			get;
			private set;
		}//end int DBID

		[Indexed]
		public int TransitionInfoDBID {
			get;
			set;
		}//end int TransitionInfoDBID

		[Ignore]
		public FrameLayerPair LayerAddress {
			get;
			private set;
		}//end FrameLayerPair LayerAddress

		public int FrameID {
			get {
				return this.LayerAddress.FrameID;
			}
			private set {
				FrameLayerPair layerAddress = this.LayerAddress;
				layerAddress.FrameID = value;
				this.LayerAddress = layerAddress;
			}//end get private set
			
		}//end int FrameID

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

		public TransitionEffectType EffectType {
			get;
			private set;
		}//end TransitionEffectType EffectType

		[Ignore]
		public Action<TransitionEffectSettings> WildcardHandler {
			get;
			private set;
		}//end Action<TransitioknEffectSettings> WildcardHandler

		public double Duration {
			get;
			set;
		}//end double Duration

		public int RotationCount {
			get;
			set;
		}//end int RotationCount

		public double Delay {
			get;
			set;
		}//end double Delay

		public override string ToString ()
		{
			return string.Format ("[TransitionEffectSettings: TransitionID={0}, LayerAddress={1}, EffectType={2}, WildcardHandler={3}, Duration={4}]", TransitionID, 
			LayerAddress, EffectType, WildcardHandler, Duration);
		}
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using wowZapp.Animate;
using PreserveProps = Android.Runtime.PreserveAttribute;
namespace LOLApp_Common
{
	[PreserveProps(AllMembers=true)]
	public class FrameInfo
	{
		public FrameInfo (int id)
		{
			this.ID = id;
			this.Layers = new Dictionary<int, LayerInfo> ();
		}

		public FrameInfo ()
		{
			this.Layers = new Dictionary<int, LayerInfo> ();
		}//end ctor

		public int ID {
			get;
			private set;
		}//end int ID

		[PrimaryKey, AutoIncrement]
		public int DBID {
			get;
			private set;
		}//end int DBID

		[Ignore]
		public Dictionary<int, LayerInfo> Layers {
			get;
			private set;
		}//end Dictionary<int, LayerInfo> Layers

		public void AddLayer (LayerInfo layer)
		{
			
			this.Layers [layer.ID] = layer;
			
		}//end void AddLayerWithTransition

		public bool ContainsLayer (FrameLayerPair layerPair)
		{
			
			return this.ID == layerPair.FrameID &&
				this.Layers.ContainsKey (layerPair.LayerID);
			
		}//end bool ContainsLayer

		public bool ContainsLayer (int layerID)
		{
			
			return this.Layers.ContainsKey (layerID);
			
		}//end bool ContainsLayer

		public void SetLayerActive (int layerID, bool active)
		{
			
			LayerInfo layerInfo = null;
			if (this.Layers.TryGetValue (layerID, out layerInfo)) {
				
				layerInfo.IsCanvasActive = active;
				
			}//end if
			
		}//end void SetLayerActive

		public void DecrementID ()
		{
			
			this.ID--;
			
		}//end void DecreamentID

		public double GetDuration ()
		{
			
			double duration = 0;
			foreach (LayerInfo eachLayerItem in this.Layers.Values) {
				
				if (eachLayerItem.Transitions.Count > 0) {
					
					double maxTransitionDuration = 
						eachLayerItem.Transitions.Values
							.MaxItem<TransitionInfo, double> (s => s.Duration)
							.Duration;
					
					if (maxTransitionDuration > duration) {
						duration = maxTransitionDuration;
					}//end if
					
				}//end if
				
			}//end foreach
			
			return duration;
			
		}//end float GetDuration
	}
}
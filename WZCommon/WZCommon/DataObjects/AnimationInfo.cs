// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using SQLite;
using LOLMessageDelivery.Classes.LOLAnimation;

namespace WZCommon
{

	public class AnimationInfo
	{

		#region Constructors

		public AnimationInfo (SizeF originalCanvasSize, 
		                      Dictionary<int, FrameInfo> frameItems, 
		                      Dictionary<int, AnimationAudioInfo> audioItems)
		{

			this.OriginalCanvasSize = originalCanvasSize;
			this.FrameItems = frameItems;
			this.AudioItems = audioItems;

		}



		public AnimationInfo (SizeF originalCanvasSize) : this(originalCanvasSize, new Dictionary<int, FrameInfo>(), new Dictionary<int, AnimationAudioInfo>())
		{


		}//end AnimationInfo



		public AnimationInfo()
		{

			this.FrameItems = new Dictionary<int, FrameInfo>();
			this.AudioItems = new Dictionary<int, AnimationAudioInfo>();

		}//end AnimationInfo

		#endregion Constructors



		#region Properties

		[PrimaryKey, AutoIncrement]
		public int DBID
		{
			get;
			private set;
		}//end int ID



		[Ignore]
		public Guid MessageID
		{
			get;
			set;
		}//end Guid MessageGuid




		public string MessageGuid
		{
			get
			{
				return this.MessageID.ToString();
			} set
			{
				this.MessageID = new Guid(value);
			}//end get set
		}//end string MessageGuid



		public int? StepNumber
		{
			get;
			set;
		}//end int StepNumber



		public DateTime? CreatedOn
		{
			get;
			set;
		}//end DateTime CreatedOn



		[Ignore]
		public SizeF OriginalCanvasSize 
		{
			get;
			private set;
		}//end SizeF CanvasSize



		public float OriginalCanvasSizeHeight
		{
			get
			{
				return this.OriginalCanvasSize.Height;
			} private set
			{
				SizeF canvasSize = this.OriginalCanvasSize;
				canvasSize.Height = value;
				this.OriginalCanvasSize = canvasSize;
			}//end get private set
		}//end float CanvasSizeHeight



		public float OriginalCanvasSizeWidth
		{
			get
			{
				return this.OriginalCanvasSize.Width;
			} private set
			{
				SizeF canvasSize = this.OriginalCanvasSize;
				canvasSize.Width = value;
				this.OriginalCanvasSize = canvasSize;

			}//end get private set

		}//end float OriginalCanvasSizeWidth



		public bool IsEditing
		{
			get;
			set;
		}//end bool IsEditing



		public bool IsSent
		{
			get;
			set;
		}//end bool IsSent



		[Ignore]
		public Dictionary<int, FrameInfo> FrameItems 
		{
			get;
			private set;
		}//end Dictionary<int, FrameInfo> FrameItems


		[Ignore]
		public Dictionary<int, AnimationAudioInfo> AudioItems
		{
			get;
			private set;
		}//end Dictionary<int, AnimationAudioInfo> AudioItems

		#endregion Properties



		#region Public methods

		public void AddFrameInfo (FrameInfo frameInfo)
		{

			this.FrameItems [frameInfo.ID] = frameInfo;

		}//end void AddFrameInfo




		public void AddAudioInfo(AnimationAudioInfo audioInfo)
		{

			this.AudioItems[audioInfo.ID] = audioInfo;

		}//end void AddAudioInfo



		public double GetAnimationDuration()
		{

			double toReturn = 0d;
			Dictionary<int, double> frameStartTimes = new Dictionary<int, double>();
			foreach (KeyValuePair<int, FrameInfo> eachFrameItem in this.FrameItems)
			{

				frameStartTimes[eachFrameItem.Key] = toReturn;
				toReturn += eachFrameItem.Value.GetDuration();

			}//end foreach

			foreach (KeyValuePair<int, AnimationAudioInfo> eachAudioItem in this.AudioItems)
			{

				double frameStartTime = 0d;
				if (frameStartTimes.TryGetValue(eachAudioItem.Value.FrameID, out frameStartTime))
				{

					double frameAudioTimeframe = frameStartTime + eachAudioItem.Value.Duration;
					if (frameAudioTimeframe > toReturn)
					{

						toReturn += frameAudioTimeframe - toReturn;

					}//end if

				}//end if

			}//end foreach

			return toReturn;

		}//end double GetAnimationDuration

		#endregion Public methods




		#region Static members

		public static AnimationInfo Clone(AnimationInfo animationItem)
		{

			AnimationInfo toReturn = new AnimationInfo(animationItem.OriginalCanvasSize);
		
			foreach (KeyValuePair<int, FrameInfo> eachFrameItem in animationItem.FrameItems)
			{
				FrameInfo frameInfo = new FrameInfo(eachFrameItem.Key);
				foreach (KeyValuePair<int, LayerInfo> eachLayer in eachFrameItem.Value.Layers)
				{
					frameInfo.AddLayer(LayerInfo.Clone(eachLayer.Value, false));
				}//end foreach

				toReturn.AddFrameInfo(frameInfo);

			}//end foreach

			foreach (KeyValuePair<int, AnimationAudioInfo> eachAudioItem in animationItem.AudioItems)
			{

				toReturn.AddAudioInfo(eachAudioItem.Value);

			}//end foreach

			return toReturn;

		}//end static AnimationInfo Clone

		#endregion Static members
	}
}


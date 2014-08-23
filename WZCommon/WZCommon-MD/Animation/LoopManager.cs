// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Linq;
using LOLMessageDelivery.Classes.LOLAnimation;

namespace WZCommon
{
	public class LoopManager<TLoopHandler> 
		where TLoopHandler : ILoopHandler
	{

		#region Constructors
		
		public LoopManager (TLoopHandler loopHandler, AnimationInfo animationItem, SizeF playerSize)
		{

			this.PlayerSize = playerSize;
			this.LoopHandler = loopHandler;
			this.PlayableAnimation = AnimationInfo.Clone(animationItem);

			this.frameStartTimes = new Dictionary<int, double>();
			this.frameCompletedTimes = new Dictionary<int, double>();

			foreach (int eachFrameID in this.PlayableAnimation.FrameItems.Keys)
			{
				this.frameCompletedTimes.Add(eachFrameID, 0d);
			}//end foreach

			// First frame always starts at 0 timestamp
			this.frameStartTimes[1] = 0d;

			foreach (FrameInfo eachFrame in this.PlayableAnimation.FrameItems.Values.OrderBy(s => s.ID).Skip(1))
			{

				double prevFrameDuration = 0d;
				double prevFrameStart = 0d;
				if (this.frameStartTimes.TryGetValue(eachFrame.ID - 1, out prevFrameStart))
				{
					prevFrameDuration = this.PlayableAnimation.FrameItems[eachFrame.ID - 1].GetDuration();
					this.frameStartTimes[eachFrame.ID] = prevFrameDuration + prevFrameStart;
				}//end if

			}//end foreach

			this.Duration = this.PlayableAnimation.GetAnimationDuration();
			this.LoopHandler.PrepareAudioBuffers();

			this.PlayerBox = this.PlayerSize.ScaleAndCenter(this.PlayableAnimation.OriginalCanvasSize, out this.pCanvasScaleFactor);

			if (this.CanvasScaleFactor != 1f)
			{
				foreach (FrameInfo eachFrameInfo in this.PlayableAnimation.FrameItems.Values)
				{

					foreach (LayerInfo eachLayer in eachFrameInfo.Layers.Values)
					{

						AnimationUtils.ApplyScaleTransformToObject(new PointF(0f, 0f),
						                                           this.CanvasScaleFactor,
						                                           this.CanvasScaleFactor,
						                                           eachLayer);

						PointF scaledLocation = eachLayer.GetBoundingBox().Location;
						AnimationUtils.MoveGraphicsObject(new PointF(this.PlayerBox.X + scaledLocation.X,
						                                             this.PlayerBox.Y + scaledLocation.Y),
						                                  scaledLocation, eachLayer);

					}//end foreach

				}//end foreach

			}//end if
			
		}
		
		#endregion Constructors
		
		
		
		
		
		#region Fields
		
		private DateTime startTime;
		private volatile bool keepAlive;
		private double prevTimeStamp;
		private Dictionary<int, double> frameStartTimes;
		private Dictionary<int, double> frameCompletedTimes;
		private float pCanvasScaleFactor;
		
		#endregion Fields
		
		
		
		
		#region Properties
		
		public TLoopHandler LoopHandler
		{
			get;
			private set;
		}//end TLoopHandler LoopHandler



		public int CurrentFrame
		{
			get;
			private set;
		}//end int CurrentFrame




		public double Duration
		{
			get;
			private set;
		}



		public SizeF PlayerSize
		{
			get;
			private set;
		}//end SizeF PlayerSize



		public RectangleF PlayerBox
		{
			get;
			private set;
		}//end RectangleF PlayerBox




		public float CanvasScaleFactor
		{
			get
			{
				return this.pCanvasScaleFactor;
			}//end get
		}//end float CanvasScaleFactor




		public AnimationInfo PlayableAnimation
		{
			get;
			private set;
		}//end AnimationInfo PlayableAnimation
		
		#endregion Properties
		
		
		
		
		#region Private methods
		
		
		private void LoopThread()
		{
			
			this.keepAlive = true;
			this.startTime = DateTime.Now;
			this.prevTimeStamp = 0;
			this.LoopHandler.OnStart();
			
			while (this.keepAlive)
			{
				
				double timeStamp = DateTime.Now.Subtract(this.startTime).TotalMilliseconds;
				double timeStampSec = timeStamp / 1000;

				if (timeStampSec >= this.Duration)
				{
					this.keepAlive = false;
					break;
				} else
				{

					int currentFrame = this.CurrentFrame;
					this.CurrentFrame = this.GetCurrentFrameFromCurrentTimestamp(timeStampSec);

					IEnumerable<AnimationAudioInfo> audioBuffers = null;

					// If we have moved to the next frame,
					// check for audio.
					if (currentFrame != this.CurrentFrame)
					{

						audioBuffers = 
							this.PlayableAnimation.AudioItems
								.Where(s => s.Value.FrameID == this.CurrentFrame)
								.Select(s => s.Value);

					}//end if
					this.LoopHandler.OnUpdate(timeStamp, this.CreateLayersForFrame(timeStampSec), audioBuffers);

				}//end if else

				this.prevTimeStamp = timeStamp;
				
			}//end while
			
			this.LoopHandler.OnStop(DateTime.Now.Subtract(this.startTime).TotalMilliseconds);
			
		}//end void LoopThread





		private List<LayerInfo> CreateLayersForFrame(double currentTimeSec)
		{

			List<LayerInfo> toReturn = new List<LayerInfo>();
			double prevTimestampSec = this.prevTimeStamp / 1000;

			if (this.PlayableAnimation.FrameItems.Count > 1 &&
			    this.CurrentFrame < this.PlayableAnimation.FrameItems.Count)
			{

				FrameInfo currentSourceFrame = this.PlayableAnimation.FrameItems[this.CurrentFrame];
				FrameInfo currentTargetFrame = this.PlayableAnimation.FrameItems[this.CurrentFrame + 1];

				double timeDiff = currentTimeSec - prevTimestampSec;
				this.frameCompletedTimes[this.CurrentFrame] += timeDiff;

				double frameCompletedTime = this.frameCompletedTimes[this.CurrentFrame];
				double frameStartTime = this.frameStartTimes[this.CurrentFrame];

				foreach (LayerInfo eachSourceLayer in currentSourceFrame.Layers.Values)
				{

					LayerInfo eachTargetLayer = null;
					if (currentTargetFrame.Layers.TryGetValue(eachSourceLayer.ID, out eachTargetLayer))
					{

						if (currentTimeSec < (frameStartTime + eachSourceLayer.Transitions[1].Duration))
						{

							TransitionInfo trInfo = eachSourceLayer.Transitions[1];
							RectangleF sourceLayerBox = eachSourceLayer.GetBoundingBox();
							RectangleF targetLayerBox = eachTargetLayer.GetBoundingBox();
							PointF sourceLocation = sourceLayerBox.Location;
							PointF targetLocation = targetLayerBox.Location;

							// NOTE: This is the time factor for the whole FrameInfo
//							float timeFactor = (float)(frameCompletedTime / currentSourceFrame.GetDuration());

//							LayerInfo displayLayer = new LayerInfo(eachSourceLayer.ID, eachSourceLayer.CanvasSize, null);
							LayerInfo displayLayer = LayerInfo.Clone(eachSourceLayer, true);

							foreach (KeyValuePair<AnimationTypesTransitionEffectType, TransitionEffectSettings> eachTrEffect in trInfo.Settings)
							{

								bool isInTimeFrame = currentTimeSec < frameStartTime + eachTrEffect.Value.Duration;

								// TODO: Adjust time factor so that multiple transitions smooth out evenly in both width and height.
								// NOTE: This is the time factor for the current transition's duration
								float timeFactor = isInTimeFrame ? (float)(frameCompletedTime / eachTrEffect.Value.Duration) : 1f;

								switch (eachTrEffect.Key)
								{

								case AnimationTypesTransitionEffectType.Move:

									// Move layer here
									this.MoveLayerInFrame(eachSourceLayer, displayLayer, sourceLocation, targetLocation, timeFactor);

									break;

								case AnimationTypesTransitionEffectType.Scale:

									// Scale layer here
									this.ScaleLayerInFrame(eachSourceLayer, displayLayer, sourceLayerBox, targetLayerBox, timeFactor);

									break;

								case AnimationTypesTransitionEffectType.Rotate:

									// Rotate layer here
									this.RotateLayerInFrame(displayLayer, trInfo.RotationAngle, timeFactor);

									break;

								}//end switch

							}//end foreach

							toReturn.Add(displayLayer);

						} else
						{

							toReturn.Add(eachTargetLayer);

						}//end if else

					}//end if else

				}//end foreach

			}//end if

			if (this.CurrentFrame == this.PlayableAnimation.FrameItems.Count &&
			    currentTimeSec >= this.frameCompletedTimes[this.CurrentFrame])
			{
				foreach (LayerInfo eachLayer in this.PlayableAnimation.FrameItems[this.CurrentFrame].Layers
				         .Values)
				{

					toReturn.Add(eachLayer);

				}//end foreach

			}//end if

			return toReturn;

		}//end List<LayerInfo> CreateLayersForFrame






		private void MoveLayerInFrame(LayerInfo sourceLayer, LayerInfo displayLayer, PointF initialLocation, PointF finalLocation, float timeFactor)
		{

			if (sourceLayer.HasDrawingItems)
			{

				// Move
				SizeF totalOffset = new SizeF(finalLocation.X - initialLocation.X, 
				                              finalLocation.Y - initialLocation.Y);
				
				// Move
				SizeF offsetNow = new SizeF(totalOffset.Width * timeFactor,
				                            totalOffset.Height * timeFactor);

				DrawingInfo drInfo = null;

				foreach (KeyValuePair<int, DrawingInfo> eachDrawingItem in sourceLayer.DrawingItems)
				{

					switch (eachDrawingItem.Value.DrawingType)
					{

					case DrawingLayerType.Drawing:

						this.MovePathInLayer(displayLayer, eachDrawingItem.Value, offsetNow);

						break;

					case DrawingLayerType.Image:

					{

						RectangleF imgFrame = eachDrawingItem.Value.ImageFrame;

						imgFrame.X += offsetNow.Width;
						imgFrame.Y += offsetNow.Height;

						drInfo = new DrawingInfo(eachDrawingItem.Value.DrawingID, eachDrawingItem.Value.ImageBuffer, imgFrame);
						drInfo.RotatedImageBox = imgFrame.Rotate(eachDrawingItem.Value.RotationAngle);
						drInfo.RotationAngle = eachDrawingItem.Value.RotationAngle;

						displayLayer.AddOrReplaceDrawingInfo(drInfo);

					}

						break;

					case DrawingLayerType.Comix:
					case DrawingLayerType.Stamp:
					case DrawingLayerType.Callout:


					{
						RectangleF imgFrame = eachDrawingItem.Value.ImageFrame;

						imgFrame.X += offsetNow.Width;
						imgFrame.Y += offsetNow.Height;

						drInfo = new DrawingInfo(eachDrawingItem.Value.DrawingID, 
						                         eachDrawingItem.Value.ContentPackItemID, 
						                         eachDrawingItem.Value.DrawingType, 
						                         eachDrawingItem.Value.ImageBuffer, imgFrame, 
						                         eachDrawingItem.Value.LineColor);
						drInfo.RotatedImageBox = imgFrame.Rotate(eachDrawingItem.Value.RotationAngle);
						drInfo.RotationAngle = eachDrawingItem.Value.RotationAngle;

						if (eachDrawingItem.Value.DrawingType == DrawingLayerType.Callout)
						{

							RectangleF calloutTextRect = eachDrawingItem.Value.CalloutTextRect;
							calloutTextRect.X += offsetNow.Width;
							calloutTextRect.Y += offsetNow.Height;

							drInfo.CalloutTextRect = calloutTextRect;
							drInfo.CalloutText = eachDrawingItem.Value.CalloutText;

						}//end if
						displayLayer.AddOrReplaceDrawingInfo(drInfo);

					}

						break;

					}//end switch

				}//end foreach

			}//end if

		}//end void MoveLayerInFrame





		private void MovePathInLayer(LayerInfo displayLayer, DrawingInfo sourceDrawingItem, SizeF offset)
		{

			DrawingInfo displayDrawingItem = null;
			
			if (displayLayer.DrawingItems.TryGetValue(sourceDrawingItem.DrawingID, out displayDrawingItem))
			{
				
				for (int i = 0; i < displayDrawingItem.PathPoints.Count; i++)
				{
					
					PointF eachPathPoint = displayDrawingItem.PathPoints[i];
					PointF displayPoint = new PointF(eachPathPoint.X + offset.Width,
					                                 eachPathPoint.Y + offset.Height);

					displayDrawingItem.PathPoints[i] = displayPoint;
					
				}//end foreach
				
				displayLayer.AddOrReplaceDrawingInfo(displayDrawingItem);
				
			} else
			{
				
				List<PointF> displayPoints = new List<PointF>();
				foreach (PointF eachPathPoint in sourceDrawingItem.PathPoints)
				{
					
					PointF displayPoint = new PointF(eachPathPoint.X + offset.Width,
					                                 eachPathPoint.Y + offset.Height);
					
					displayPoints.Add(displayPoint);
					
				}//end for
				
				displayLayer.AddOrReplaceDrawingInfo(new DrawingInfo(sourceDrawingItem.DrawingID, 
				                                                     displayPoints, 
				                                                     sourceDrawingItem.Brush, 
				                                                     sourceDrawingItem.LineColor));
				
			}//end if else

		}//end void MovePathInLayer







		private void ScaleLayerInFrame(LayerInfo sourceLayer, LayerInfo displayLayer, RectangleF initialLayerBox, RectangleF finalLayerBox, float timeFactor)
		{

			// Scale
			float scaleFactorX = finalLayerBox.Width / initialLayerBox.Width;
			float scaleFactorY = finalLayerBox.Height / initialLayerBox.Height;
			
			// Scale
			float scaleXNow;
			float scaleYNow;
			if (scaleFactorX > 1f)
			{
				
				scaleXNow = 1f + ((scaleFactorX * (float)timeFactor) - (float)timeFactor);
				
			} else
			{
				scaleXNow = scaleFactorX + (scaleFactorX * (1f - (float)timeFactor));
			}//end if else
			
			if (scaleFactorY > 1f)
			{
				scaleYNow = 1f + ((scaleFactorY * (float)timeFactor) - (float)timeFactor);
			} else
			{
				scaleYNow = scaleFactorY + (scaleFactorY * (1f - (float)timeFactor));
			}//end if else

			AnimationUtils.ApplyScaleTransformToObject(displayLayer.GetBoundingBox().Location, scaleXNow, scaleYNow, displayLayer);

		}//end void ScaleLayerInFrame




		private void RotateLayerInFrame(LayerInfo displayLayer, double degAngle, float timeFactor)
		{

			// Rotate
			double rotationNow = degAngle * (double)timeFactor;

			AnimationUtils.RotateGraphicsObjectByAngle(rotationNow, displayLayer);

		}//end void RotateLayerInFrame






		private int GetCurrentFrameFromCurrentTimestamp(double currentTimeSec)
		{
			
			if (this.frameStartTimes.Count <= 1)
			{
				return 1;
			} else
			{
				
				for (int i = 0; i < this.frameStartTimes.Count - 1; i++)
				{
					
					double[] rangePair =
						this.frameStartTimes
							.Values
							.Skip(i)
							.Take(2)
							.ToArray();
					
					if (currentTimeSec.IsBetween(rangePair[0], rangePair[1], BoundsEqualityType.EqualsRightOnly))
					{
						return i + 1;
					}//end if
					
				}//end for

				// When we reach this point, all frames have been animated.
				// Only audio remains, if any.
				return this.PlayableAnimation.FrameItems.Count;
				
			}//end if else
			
		}//end int GetCurrentFrameFromCurrentTimestamp
		
		#endregion Private methods
		
		
		
		
		#region Public methods
		
		public void Start()
		{
			
			Thread loopThread = new Thread(new ThreadStart(this.LoopThread));
			loopThread.Name = "LoopThread";
			loopThread.IsBackground = true;
			loopThread.Start();
			
		}//end void Start
		
		
		
		public void Stop()
		{
			
			this.keepAlive = false;
			
		}//end void Stop
		
		#endregion Public methods
	}
}


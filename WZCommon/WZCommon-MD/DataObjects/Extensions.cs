// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using LOLMessageDelivery;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using LOLMessageDelivery.Classes.LOLAnimation;

namespace WZCommon
{
	public static class Extensions
	{

		public static T MaxItem<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
		{
	         T maxItem = default(T);
	         TCompare maxValue = default(TCompare);
	         foreach (var item in collection)
	         {
	             TCompare temp = func(item);
	             if (maxItem == null || temp.CompareTo(maxValue) > 0)
	             {
	                maxValue = temp;
	                maxItem = item;
	            }
	        }
	        return maxItem;
	    }


		//FIXME: If T is a struct, it will just return default(T)
		// In this case, use MinItemEx instead
		public static T MinItem<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
		{

			T minItem = default(T);
			TCompare minValue = default(TCompare);
	        foreach (var item in collection)
	        {
	            TCompare temp = func(item);

	            if (minItem == null || temp.CompareTo(minValue) < 0)
	            {
	               minValue = temp;
	               minItem = item;
	           }
	        }
	        return minItem;
	    }




		public static T MinItemEx<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
		{

			T minItem = collection.First();
			TCompare minValue = func(collection.First());

			foreach (var item in collection)
			{

				TCompare temp = func(item);

				if (minItem == null || temp.CompareTo(minValue) < 0)
				{

					minValue = temp;
					minItem = item;

				}//end if

			}//end foreach

			return minItem;

		}//end static T




		public static RectangleF Rotate(this RectangleF owner, double degAngle)
		{

			PointF center = new PointF(owner.X + (owner.Width / 2f), owner.Y + (owner.Height / 2f));
			
			double radAngle = degAngle * AnimationUtils.DegToRad;
			float angleCos = (float)Math.Cos(radAngle);
			float angleSin = (float)Math.Sin(radAngle);

			List<PointF> rectCorners = new List<PointF>() {

				owner.Location,
				new PointF(owner.Right, owner.Y),
				new PointF(owner.Right, owner.Bottom),
				new PointF(owner.X, owner.Bottom)

			};

			List<PointF> rotatedPoints = new List<PointF>();

			foreach (PointF eachCorner in rectCorners)
			{

				// Translate point
				float tx = eachCorner.X - center.X;
				float ty = eachCorner.Y - center.Y;
				
				// Rotate it
				float rx = (tx * angleCos) - (ty * angleSin);
				float ry = (ty * angleCos) + (tx * angleSin);
				
				// Translate back
				rx += center.X;
				ry += center.Y;
				
				eachCorner.X = rx;
				eachCorner.Y = ry;

				rotatedPoints.Add(eachCorner);
				
			}//end foreach

			return rotatedPoints.GetBoundingBox();

		}//end static RectangleF Rotate





		public static List<PointF> RotatePoints(this IEnumerable<PointF> owner, double degAngle, PointF fixedPoint)
		{

			List<PointF> toReturn = new List<PointF>();

			double radAngle = degAngle * AnimationUtils.DegToRad;
			float angleCos = (float)Math.Cos(radAngle);
			float angleSin = (float)Math.Sin(radAngle);

			foreach (PointF eachPoint in owner)
			{

				// Translate point
				float tx = eachPoint.X - fixedPoint.X;
				float ty = eachPoint.Y - fixedPoint.Y;

				// Rotate it
				float rx = (tx * angleCos) - (ty * angleSin);
				float ry = (ty * angleCos) + (tx * angleSin);

				// Translate back
				rx += fixedPoint.X;
				ry += fixedPoint.Y;

				toReturn.Add(new PointF(rx, ry));

			}//end foreach

			return toReturn;

		}//end List<PointF> RotatePoints




		public static List<PointF> GetCorners(this RectangleF owner)
		{

			List<PointF> toReturn = new List<PointF>();
			toReturn.Add(owner.Location);
			toReturn.Add(new PointF(owner.Right, owner.Y));
			toReturn.Add(new PointF(owner.Right, owner.Bottom));
			toReturn.Add(new PointF(owner.Left, owner.Bottom));

			return toReturn;

		}//end static RectangleF GetCorners





		public static RectangleF GetBoundingBox(this IEnumerable<PointF> owner)
		{

			PointF minXPoint = 
				owner
					.MinItemEx<PointF, float>(s => s.X);
			PointF maxXPoint = 
				owner
					.MaxItem<PointF, float>(s => s.X);
			PointF minYPoint = 
				owner
					.MinItemEx<PointF, float>(s => s.Y);
			PointF maxYPoint = 
				owner
					.MaxItem<PointF, float>(s => s.Y);
			
			return new RectangleF(minXPoint.X,
			                      minYPoint.Y,
			                      maxXPoint.X - minXPoint.X,
			                      maxYPoint.Y - minYPoint.Y);

		}//end static RectangleF GetBoundingBox




		public static RectangleF CenterInRect(this RectangleF owner, SizeF size)
		{

			return new RectangleF(owner.X + ((owner.Width / 2f) - (size.Width / 2f)),
			                      owner.Y + ((owner.Height / 2f) - (size.Height / 2f)),
			                      size.Width,
			                      size.Height);

		}//end static RectangleF CenterInRect




		public static RectangleF ScaleAndCenter(this SizeF owner, SizeF size, out float scale)
		{

			SizeF finalSize = SizeF.Empty;

			scale = owner.Width / size.Width;
			finalSize.Width = owner.Width;

			finalSize.Height = size.Height * scale;

			if (finalSize.Height > owner.Height)
			{

				scale = owner.Height / finalSize.Height;
				finalSize.Height = owner.Height;

				finalSize.Width = finalSize.Width * scale;

			}//end if

			PointF finalLocation = new PointF((owner.Width / 2f) - (finalSize.Width / 2f),
			                                  (owner.Height / 2f) - (finalSize.Height / 2f));


			return new RectangleF(finalLocation, finalSize);

		}//end static RectangleF ScaleAndCenter




		public static bool IsSameAs(this LOLMessageConversation owner, LOLMessageConversation other)
		{

			if (owner.Recipients.Count != other.Recipients.Count)
			{
				return false;
			}//end if

			return owner.Recipients.TrueForAll(s => other.Recipients.Contains(s));

		}//end static bool IsSameAs




		public static LOLMessageConversation Copy(this LOLMessageConversation owner)
		{

			LOLMessageConversation toReturn = new LOLMessageConversation();
			toReturn.LastMessageDate = owner.LastMessageDate;
			toReturn.MessageIDs = new List<Guid>(owner.MessageIDs);
			toReturn.ReadMessageIDs = new List<Guid>(owner.ReadMessageIDs);
			toReturn.Recipients = new List<Guid>(owner.Recipients);

			return toReturn;

		}//end static LOLMessageConversationCopy


		public static Task<TResult> CallAsyncMethod<TResult, TEventArgs>(this LOLConnectClient service,
		                                                                 Action<LOLConnectClient, EventHandler<TEventArgs>, bool> hookAction,
		                                                                 Action<LOLConnectClient> callAction)
			where TEventArgs : AsyncCompletedEventArgs
		{
			
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			
			EventHandler<TEventArgs> callback = null;
			callback = (s, e) => {
				
				try
				{
					
					if (null == e.Error)
					{
						
						Type argsType = typeof(TEventArgs);
						PropertyInfo propInfo = argsType.GetProperty("Result");
						TResult result = (TResult)propInfo.GetValue(e, null);
						tcs.SetResult(result);
						
					} else if (e.Cancelled)
					{
						tcs.SetCanceled();
					} else
					{
						tcs.SetException(e.Error);
					}//end if else
					
				} catch (Exception ex)
				{
					tcs.SetException(ex);
				} finally
				{
					hookAction(service, callback, false);
				}//end try catch
				
			};
			
			hookAction(service, callback, true);
			callAction(service);
			
			return tcs.Task;
			
		}//end static Task<TResult> CallAsyncMethod
		
		
		
		
		
		public static Task<TResult> CallAsyncMethod<TResult, TEventArgs>(LOLMessageClient service,
		                                                                 Action<LOLMessageClient, EventHandler<TEventArgs>, bool> hookAction,
		                                                                 Action<LOLMessageClient> callAction)
			where TEventArgs : AsyncCompletedEventArgs
		{
			
			
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			
			EventHandler<TEventArgs> callback = null;
			callback = (s, e) => {
				
				try
				{
					
					if (null == e.Error)
					{
						
						Type argsType = typeof(TEventArgs);
						PropertyInfo propInfo = argsType.GetProperty("Result");
						TResult result = (TResult)propInfo.GetValue(e, null);
						tcs.SetResult(result);
						
					} else if (e.Cancelled)
					{
						tcs.SetCanceled();
					} else
					{
						tcs.SetException(e.Error);
					}//end if else
					
				} catch (Exception ex)
				{
					tcs.SetException(ex);
				} finally
				{
					hookAction(service, callback, false);
				}//end try catch finally
				
			};
			
			hookAction(service, callback, true);
			callAction(service);
			
			return tcs.Task;
			
		}//end static Task<TResult> CallAsyncMethod






		public static bool IsBetween(this double owner, double left, double right, BoundsEqualityType eqType)
		{

			switch (eqType)
			{

			case BoundsEqualityType.Neither:

				return owner > left && owner < right;

			case BoundsEqualityType.EqualsBoth:

				return owner >= left && owner <= right;

			case BoundsEqualityType.EqualsLeftOnly:

				return owner >= left && owner < right;

			case BoundsEqualityType.EqualsRightOnly:

				return owner > left && owner <= right;

			default:

				throw new InvalidOperationException("Unknown BoundsEqualityType!");

			}//end switch

		}//end static bool IsBetween



		public static double DistanceFrom(this PointF owner, PointF targetPoint)
		{

			double powX = Math.Pow(Convert.ToDouble(owner.X - targetPoint.X), 2);
			double powY = Math.Pow(Convert.ToDouble(owner.Y - targetPoint.Y), 2);

			return Math.Sqrt(powX + powY);

		}//end static double DistanceFrom



		public static PointF GetCenterOfRect(this RectangleF owner)
		{

			return new PointF(owner.X + (owner.Width / 2f),
			                  owner.Y + (owner.Height / 2f));

		}//end static PointF GetCenterOfRect



		public static PointF Scale(this PointF owner, PointF fixedPoint, float scaleX, float scaleY)
		{

			return new PointF(
				scaleX * (owner.X + (-fixedPoint.X)) + fixedPoint.X,
				scaleY * (owner.Y + (-fixedPoint.Y)) + fixedPoint.Y
				);

		}//end static PointF Scale




		public static AnimationStep CreateAnimationStep(this AnimationInfo owner)
		{

			if (null == owner)
			{
				throw new NullReferenceException("Owner object is null!");
			}//end if

			AnimationStep toReturn = new AnimationStep();
			toReturn.OriginalCanvasSize = owner.OriginalCanvasSize;
			toReturn.MessageID = new Guid(owner.MessageGuid);
			toReturn.StepNumber = owner.StepNumber ?? 0;
			toReturn.CreatedOn = owner.CreatedOn ?? DateTime.Now;

			List<AnimationContent> animationContentList = new List<AnimationContent>();
			List<AnimationTransition> animationTransitions = new List<AnimationTransition>();
			List<AnimationAudio> animationAudioList = new List<AnimationAudio>();

			foreach (FrameInfo eachFrameInfo in owner.FrameItems.Values.OrderBy(s => s.ID))
			{

				foreach (LayerInfo eachLayerInfo in eachFrameInfo.Layers.Values.OrderBy(s => s.ID))
				{

					foreach (DrawingInfo eachDrawingInfo in eachLayerInfo.DrawingItems.Values.OrderBy(s => s.DrawingID))
					{

						AnimationContent content = new AnimationContent();
						content.FrameID = eachFrameInfo.ID;
						content.LayerID = eachLayerInfo.ID;
						content.AnimationContentID = eachDrawingInfo.DrawingID;

						if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing)
						{

							content.BrushThickness = eachDrawingInfo.Brush.Thickness;
							content.BrushTypeValue = eachDrawingInfo.Brush.BrushType;

						}//end if

						content.CalloutText = eachDrawingInfo.CalloutText;
						content.CalloutTextRect = eachDrawingInfo.CalloutTextRect;
						content.ColorBuffer = eachDrawingInfo.LineColorBuffer;
						content.ContentPackItemID = eachDrawingInfo.ContentPackItemID;
						content.ImageBuffer = eachDrawingInfo.ImageBuffer;
						content.ImageFrame = eachDrawingInfo.ImageFrame;
						content.LayerType = AnimationUtils.GetLayerTypeForDrawingType(eachDrawingInfo.DrawingType);
						content.PathPoints = eachDrawingInfo.PathPoints;
						content.RotatedImageBox = eachDrawingInfo.RotatedImageBox;
						content.RotationAngle = eachDrawingInfo.RotationAngle;

						animationContentList.Add(content);

					}//end foreach


					foreach (TransitionInfo eachTrInfo in eachLayerInfo.Transitions.Values)
					{

						AnimationTransition transition = new AnimationTransition();
						transition.LayerID = eachLayerInfo.ID;
						transition.FrameID = eachFrameInfo.ID;
						transition.EndLocation = eachTrInfo.EndLocation;
						transition.EndSize = eachTrInfo.EndSize;
						transition.EndSizeFixedPoint = eachTrInfo.EndSizeFixedPoint;
						transition.FadeOpacity = eachTrInfo.FadeOpacity;
						transition.RotationAngle = eachTrInfo.RotationAngle;
						transition.TransitionDuration = eachTrInfo.Duration;

//						Dictionary<AnimationTypesTransitionEffectType, double> efDurations = new Dictionary<AnimationTypesTransitionEffectType, double>();
//						Dictionary<AnimationTypesTransitionEffectType, int> efRotations = new Dictionary<AnimationTypesTransitionEffectType, int>();
//						Dictionary<AnimationTypesTransitionEffectType, double> efDelays = new Dictionary<AnimationTypesTransitionEffectType, double>();

						transition.EffectDelays = new Dictionary<AnimationTypesTransitionEffectType, double>();
						transition.EffectDurations = new Dictionary<AnimationTypesTransitionEffectType, double>();
						transition.EffectRotations = new Dictionary<AnimationTypesTransitionEffectType, int>();

						foreach (TransitionEffectSettings eachTrSetting in eachTrInfo.Settings.Values)
						{

//							efDurations[eachTrSetting.EffectType] = eachTrSetting.Duration;
//							efRotations[eachTrSetting.EffectType] = eachTrSetting.RotationCount;
//							efDelays[eachTrSetting.EffectType] = eachTrSetting.Delay;
							transition.EffectDelays.Add(eachTrSetting.EffectType, eachTrSetting.Delay);
							transition.EffectDurations.Add(eachTrSetting.EffectType, eachTrSetting.Duration);
							transition.EffectRotations.Add(eachTrSetting.EffectType, eachTrSetting.RotationCount);

						}//end foreach

//						transition.EffectDelays = efDelays;
//						transition.EffectDurations = efDurations;
//						transition.EffectRotations = efRotations;

						animationTransitions.Add(transition);

					}//end foreach

				}//end foreach

			}//end foreach


			foreach (AnimationAudioInfo eachAudioItem in owner.AudioItems.Values)
			{

				AnimationAudio animAudio = new AnimationAudio();
				animAudio.ID = eachAudioItem.ID;
				animAudio.AudioBuffer = eachAudioItem.AudioBuffer;
				animAudio.Duration = eachAudioItem.Duration;
				animAudio.FrameID = eachAudioItem.FrameID;

				animationAudioList.Add(animAudio);

			}//end foreach

			toReturn.ContentItems = animationContentList;
			toReturn.AudioItems = animationAudioList;
			toReturn.TransitionItems = animationTransitions;

			return toReturn;

		}//end static AnimationStep CreateAnimationStep




		/// <summary>
		/// Creates an AnimationInfo object from an AnimationStep.
		/// NOTE: All layers in all frames will be identical to the first frame. After this returns, these layers need to be adjusted.
		/// NOTE: ContentPackItems (Stamps, Comix, Callout) graphics are *not* included. 
		/// They have to be managed by the app according to its cache.
		/// </summary>
		/// <returns>The animation info.</returns>
		/// <param name="owner">Owner.</param>
		public static AnimationInfo CreateAnimationInfo(this AnimationStep owner)
		{

			if (null == owner)
			{
				throw new NullReferenceException("Owner object is null!");
			}//end if

			AnimationInfo toReturn = new AnimationInfo(owner.OriginalCanvasSize);

			toReturn.CreatedOn = owner.CreatedOn;
			toReturn.MessageGuid = owner.MessageID.ToString();
			toReturn.StepNumber = owner.StepNumber;

			foreach (AnimationContent eachAnimationContent in owner.ContentItems)
			{

				FrameInfo frameInfo = null;
				LayerInfo layerInfo = null;

				if (!toReturn.FrameItems.TryGetValue(eachAnimationContent.FrameID, out frameInfo))
				{

					frameInfo = new FrameInfo(eachAnimationContent.FrameID);
					toReturn.AddFrameInfo(frameInfo);

				}

				if (!frameInfo.Layers.TryGetValue(eachAnimationContent.LayerID, out layerInfo))
				{

					layerInfo = new LayerInfo(eachAnimationContent.LayerID, owner.OriginalCanvasSize, new Dictionary<int, TransitionInfo>());
					frameInfo.AddLayer(layerInfo);

				}//end if

				DrawingInfo drInfo = null;
				if (eachAnimationContent.LayerType == AnimationTypesAnimationLayerType.Path)
				{

					BrushItem brush = 
						new BrushItem((float)eachAnimationContent.BrushThickness, 
						              eachAnimationContent.BrushTypeValue, 
						              WZColor.ConvertByteArrayToRGBAColor(eachAnimationContent.ColorBuffer), null, null);
					drInfo = new DrawingInfo(eachAnimationContent.AnimationContentID, eachAnimationContent.PathPoints, brush, brush.BrushColor);
					drInfo.RotationAngle = eachAnimationContent.RotationAngle;

				} else if (eachAnimationContent.LayerType == AnimationTypesAnimationLayerType.Image)
				{

					drInfo = new DrawingInfo(eachAnimationContent.AnimationContentID, eachAnimationContent.ImageBuffer, eachAnimationContent.ImageFrame);
					drInfo.ImageFrame = eachAnimationContent.ImageFrame;
					drInfo.RotatedImageBox = eachAnimationContent.RotatedImageBox;
					drInfo.RotationAngle = eachAnimationContent.RotationAngle;

				} else if (eachAnimationContent.LayerType == AnimationTypesAnimationLayerType.Comix ||
				           eachAnimationContent.LayerType == AnimationTypesAnimationLayerType.Callout ||
				           eachAnimationContent.LayerType == AnimationTypesAnimationLayerType.Stamp)
				{

					drInfo = 
						new DrawingInfo(eachAnimationContent.AnimationContentID, 
						                eachAnimationContent.ContentPackItemID, 
						                AnimationUtils.GetDrawingLayerTypeForLayerType(eachAnimationContent.LayerType), 
						                eachAnimationContent.ImageBuffer, eachAnimationContent.ImageFrame, 
						                WZColor.ConvertByteArrayToRGBAColor(eachAnimationContent.ColorBuffer));
					drInfo.ImageFrame = eachAnimationContent.ImageFrame;
					drInfo.RotatedImageBox = eachAnimationContent.RotatedImageBox;
					drInfo.RotationAngle = eachAnimationContent.RotationAngle;

				}//end if else

				layerInfo.DrawingItems.Add(drInfo.DrawingID, drInfo);

			}//end foreach


			foreach (AnimationTransition eachAnimTransition in owner.TransitionItems)
			{

				TransitionInfo trInfo = new TransitionInfo(1);
				trInfo.SetEndLocation(eachAnimTransition.EndLocation);
				trInfo.SetEndScaleValue(eachAnimTransition.EndSize);
				trInfo.SetEndFadeValue(eachAnimTransition.FadeOpacity);
				trInfo.SetRotationValue(eachAnimTransition.RotationAngle);

				FrameLayerPair layerPair = new FrameLayerPair(eachAnimTransition.FrameID, eachAnimTransition.LayerID);

				foreach (KeyValuePair<AnimationTypesTransitionEffectType, double> eachTransitionEffectDuration in eachAnimTransition.EffectDurations)
				{

					TransitionEffectSettings efSetting = null;
					if (!trInfo.Settings.TryGetValue(eachTransitionEffectDuration.Key, out efSetting))
					{

						efSetting = new TransitionEffectSettings(1, layerPair, eachTransitionEffectDuration.Key, null);
						trInfo.Settings.Add(efSetting.EffectType, efSetting);

					}//end if else

					efSetting.Duration = eachTransitionEffectDuration.Value;
					efSetting.Delay = eachAnimTransition.EffectDelays[eachTransitionEffectDuration.Key];
					efSetting.RotationCount = eachAnimTransition.EffectRotations[eachTransitionEffectDuration.Key];

				}//end foreach

				toReturn.FrameItems[layerPair.FrameID].Layers[layerPair.LayerID].Transitions[trInfo.ID] = trInfo;

			}//end foreach


			foreach (AnimationAudio eachAnimationAudio in owner.AudioItems)
			{

				AnimationAudioInfo audioInfo = new AnimationAudioInfo(eachAnimationAudio.ID, eachAnimationAudio.FrameID, eachAnimationAudio.AudioBuffer, eachAnimationAudio.Duration);
				toReturn.AddAudioInfo(audioInfo);

			}//end foreach

			return toReturn;

		}//end static AnimationInfo CreateAnimationInfo

	}

}

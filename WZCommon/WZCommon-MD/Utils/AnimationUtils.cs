// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using LOLAccountManagement;
using LOLCodeLibrary.ErrorsManagement;
using LOLCodeLibrary;
using LOLMessageDelivery.Classes.LOLAnimation;

namespace WZCommon
{
	public class AnimationUtils
	{

		//
		// Angles
		//
		public const double RadToDeg = 180.0 / Math.PI;
		public const double DegToRad = Math.PI / 180.0;


		#region Transformations

		public static void ApplyScaleTransformToObject (PointF fixedPoint, float scaleX, float scaleY, LayerInfo layerItem, 
		                                                ContentPackItem.ItemSize itemSize = ContentPackItem.ItemSize.Small)
		{

			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) 
			{

				switch (eachDrawingInfo.DrawingType)
				{

				case DrawingLayerType.Drawing:

					for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) 
					{

						PointF point = eachDrawingInfo.PathPoints [i];

						point.X = scaleX * (point.X + (-fixedPoint.X)) + fixedPoint.X;
						point.Y = scaleY * (point.Y + (-fixedPoint.Y)) + fixedPoint.Y;

						eachDrawingInfo.PathPoints [i] = point;

					}//end for

					break;

				case DrawingLayerType.Image:
				case DrawingLayerType.Comix:
				case DrawingLayerType.Stamp:
				case DrawingLayerType.Callout:

					RectangleF imgFrame = eachDrawingInfo.ImageFrame;
					imgFrame.Width *= scaleX;
					imgFrame.Height *= scaleY;
					imgFrame.X = scaleX * (imgFrame.X + (-fixedPoint.X)) + fixedPoint.X;
					imgFrame.Y = scaleY * (imgFrame.Y + (-fixedPoint.Y)) + fixedPoint.Y;

					eachDrawingInfo.ImageFrame = imgFrame;
					eachDrawingInfo.RotatedImageBox = eachDrawingInfo.ImageFrame.Rotate(eachDrawingInfo.RotationAngle);

					if (eachDrawingInfo.DrawingType == DrawingLayerType.Callout)
					{

						eachDrawingInfo.CalloutTextRect = GetCalloutTextRect(imgFrame, itemSize);

					}//end if

					break;

				}//end switch

			}//end foreach

		}//end PointF ApplyScaleTrsansformToObject





		public static void MoveGraphicsObject (PointF touchLocation, PointF prevTouchLocation, LayerInfo layerItem)
		{


			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {

				if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing) {

					for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) {

						PointF point = eachDrawingInfo.PathPoints [i];
						float xDiff = touchLocation.X - prevTouchLocation.X;
						float yDiff = touchLocation.Y - prevTouchLocation.Y;

						point.X += xDiff;
						point.Y += yDiff;

						eachDrawingInfo.PathPoints [i] = point;

					}//end for

				} else if (eachDrawingInfo.DrawingType == DrawingLayerType.Image ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Comix ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Stamp ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Callout) 
				{

					RectangleF imgFrame = eachDrawingInfo.ImageFrame;

					float xDiff = touchLocation.X - prevTouchLocation.X;
					float yDiff = touchLocation.Y - prevTouchLocation.Y;

					imgFrame.X += xDiff;
					imgFrame.Y += yDiff;

					eachDrawingInfo.ImageFrame = imgFrame;
					eachDrawingInfo.RotatedImageBox = eachDrawingInfo.ImageFrame.Rotate (eachDrawingInfo.RotationAngle);

					if (eachDrawingInfo.DrawingType == DrawingLayerType.Callout) 
					{

						RectangleF calloutTextRect = eachDrawingInfo.CalloutTextRect;

						calloutTextRect.X += xDiff;
						calloutTextRect.Y += yDiff;

						eachDrawingInfo.CalloutTextRect = calloutTextRect;

					}//end if

				}//end if else if

			}//end foreach

		}//end void MoveGraphicsObject





		public static void ScaleGraphicsObject (PointF touchLocation, 
		                                       PointF prevTouchLocation, 
		                                       LayerInfo layerItem, 
		                                       CanvasControlPoint controlPoint, 
		                                       bool aspectRatioLocked,
		                                       ContentPackItem.ItemSize itemSize = ContentPackItem.ItemSize.Small)
		{

			RectangleF boundingBox = layerItem.GetBoundingBox ();

			bool scaleUpOnly = boundingBox.Width < 10f || boundingBox.Height < 10f;

			float sx = 0;
			float sy = 0;

			PointF fixedPoint = PointF.Empty;

			switch (controlPoint) {

			case CanvasControlPoint.TopLeft:

				// Fixed point is bottom-right
				fixedPoint = new PointF (boundingBox.Right, boundingBox.Bottom);
				sx = boundingBox.Width / (boundingBox.Width + (touchLocation.X - prevTouchLocation.X));
				sy = aspectRatioLocked ? 
					sx :
						boundingBox.Height / (boundingBox.Height + (touchLocation.Y - prevTouchLocation.Y));

				break;

			case CanvasControlPoint.TopRight:

				// Fixed point is bottom-left
				fixedPoint = new PointF (boundingBox.Left, boundingBox.Bottom);

				sx = boundingBox.Width / (boundingBox.Width - (touchLocation.X - prevTouchLocation.X));
				sy = aspectRatioLocked ?
					sx :
						boundingBox.Height / (boundingBox.Height + (touchLocation.Y - prevTouchLocation.Y));

				break;

			case CanvasControlPoint.BottomRight:

				// Fixed point is top-left
				fixedPoint = boundingBox.Location;

				sx = boundingBox.Width / (boundingBox.Width - (touchLocation.X - prevTouchLocation.X));
				sy = aspectRatioLocked ?
					sx :
						boundingBox.Height / (boundingBox.Height - (touchLocation.Y - prevTouchLocation.Y));

				break;

			case CanvasControlPoint.BottomLeft:

				// Fixed point is top-right
				fixedPoint = new PointF (boundingBox.Right, boundingBox.Top);

				sx = boundingBox.Width / (boundingBox.Width + (touchLocation.X - prevTouchLocation.X));
				sy = aspectRatioLocked ?
					sx :
						boundingBox.Height / (boundingBox.Height - (touchLocation.Y - prevTouchLocation.Y));

				break;

			}//end switch

			if (scaleUpOnly &&
				sx < 1) {
				sx = 1;
			}//end if

			if (scaleUpOnly &&
				sy < 1) {
				sy = 1;
			}//end if


			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {

				if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing) {

					for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) {
						
						PointF point = eachDrawingInfo.PathPoints [i];
						point.X = sx * (point.X + (-fixedPoint.X)) + fixedPoint.X;
						point.Y = sy * (point.Y + (-fixedPoint.Y)) + fixedPoint.Y;
						
						eachDrawingInfo.PathPoints [i] = point;
						
					}//end for

				} else if (eachDrawingInfo.DrawingType == DrawingLayerType.Image ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Comix ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Stamp ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Callout) {


					RectangleF imgFrame = eachDrawingInfo.ImageFrame;
					imgFrame.Width *= sx;
					imgFrame.Height *= sy;

					switch (controlPoint) {

					case CanvasControlPoint.BottomLeft:

						imgFrame.X += touchLocation.X - prevTouchLocation.X;

						break;

					case CanvasControlPoint.BottomRight:

						//NOTE: Location is ok, when the only item in the layer is the image.

						break;

					case CanvasControlPoint.TopLeft:

						imgFrame.X += touchLocation.X - prevTouchLocation.X;
						imgFrame.Y += touchLocation.Y - prevTouchLocation.Y;

						break;

					case CanvasControlPoint.TopRight:

						imgFrame.Y += touchLocation.Y - prevTouchLocation.Y;

						break;

					}//end switch

					eachDrawingInfo.ImageFrame = imgFrame;
					eachDrawingInfo.RotatedImageBox = eachDrawingInfo.ImageFrame.Rotate (eachDrawingInfo.RotationAngle);

					if (eachDrawingInfo.DrawingType == DrawingLayerType.Callout) 
					{
						eachDrawingInfo.CalloutTextRect = GetCalloutTextRect (eachDrawingInfo.ImageFrame, itemSize);
					}//end if

				}//end if else
				
			}//end foreach

		}//end void ScaleGraphicsObject






		public static void RotateGraphicsObject (PointF touchLocation, LayerInfo layerItem, ref double prevRadAngle)
		{

			RectangleF boundingRect = layerItem.GetBoundingBox ();
//			PointF center = new PointF (boundingRect.GetMidX (), boundingRect.GetMidY ());
			PointF center = boundingRect.GetCenterOfRect();

			double deltaX = touchLocation.X - center.X;
			double deltaY = touchLocation.Y - center.Y; 

			double radAngle = Math.Atan2 (deltaY, deltaX);
			double angleDiff = radAngle - prevRadAngle;

			float angleCos = (float)Math.Cos (angleDiff);
			float angleSin = (float)Math.Sin (angleDiff);

			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {

				if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing) {

					for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) {

						PointF point = eachDrawingInfo.PathPoints [i];

						// Translate point
						float tx = point.X - center.X;
						float ty = point.Y - center.Y;

						// Rotate it
						float rx = (tx * angleCos) - (ty * angleSin);
						float ry = (ty * angleCos) + (tx * angleSin);

						// Translate back
						rx += center.X;
						ry += center.Y;

						point.X = rx;
						point.Y = ry;

						eachDrawingInfo.PathPoints [i] = point;

					}//end for

				} else if (eachDrawingInfo.DrawingType == DrawingLayerType.Image ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Comix ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Stamp ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Callout) {

					double degAngle = radAngle * RadToDeg;
					RectangleF imgFrame = eachDrawingInfo.ImageFrame;
					eachDrawingInfo.RotatedImageBox = imgFrame.Rotate (degAngle);

				}//end if else if

				eachDrawingInfo.RotationAngle = radAngle * RadToDeg;

			}//end foreach

			prevRadAngle = radAngle;

		}//end void RotateGraphicsObject





		public static void RotateGraphicsObjectByAngle (double degAngle, LayerInfo layerItem)
		{

			RectangleF boundingBox = layerItem.GetBoundingBox ();
//			PointF center = new PointF (boundingBox.GetMidX (), boundingBox.GetMidY ());
			PointF center = boundingBox.GetCenterOfRect();

			double radAngle = degAngle * DegToRad;
			float angleCos = (float)Math.Cos (radAngle);
			float angleSin = (float)Math.Sin (radAngle);

			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {

				if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing) {

					for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) {

						PointF point = eachDrawingInfo.PathPoints [i];

						// Translate point
						float tx = point.X - center.X;
						float ty = point.Y - center.Y;

						// Rotate it
						float rx = (tx * angleCos) - (ty * angleSin);
						float ry = (ty * angleCos) + (tx * angleSin);

						// Translate back
						rx += center.X;
						ry += center.Y;

						point.X = rx;
						point.Y = ry;

						eachDrawingInfo.PathPoints [i] = point;

					}//end for

				} else if (eachDrawingInfo.DrawingType == DrawingLayerType.Image ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Comix ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Stamp) {

					RectangleF imgFrame = eachDrawingInfo.ImageFrame;
					eachDrawingInfo.RotatedImageBox = imgFrame.Rotate (degAngle);
					eachDrawingInfo.RotationAngle = degAngle;

				} else if (eachDrawingInfo.DrawingType == DrawingLayerType.Callout) {

					RectangleF imgFrame = eachDrawingInfo.ImageFrame;
					eachDrawingInfo.RotatedImageBox = imgFrame.Rotate (degAngle);
					eachDrawingInfo.RotationAngle = degAngle;

				}//end if else if

			}//end foreach

		}//end static void RotateGraphicsObjectByAngle



	

		public static RectangleF TranslateRect (RectangleF fromRect, RectangleF rect, SizeF toSize)
		{

			float rectSx = toSize.Width / fromRect.Width;
			float rectSy = toSize.Height / fromRect.Height;

			float x = rect.X * rectSx;
			float y = rect.Y * rectSy;
			float width = rect.Width * rectSx;
			float height = rect.Height * rectSy;

			return new RectangleF (x, y, width, height);

		}//end static RectangleF TranslateRect


		#endregion Transformations



		#region Content










		//NOTE: Ella's numbers
//		1.
//			1024 callouts - vertical-162px; horizontal- 82px; text-box: 800x366px;
//		2.
//			512 callouts - vertical-82px; horizontal- 42px; text-box: 400x182px;
//		3.
//			256 callouts - vertical-42px; horizontal- 22px; text-box: 200x90px;
//		4. 
//			160 callouts - vertical-26px; horizontal- 26px; text-box: 124x56px;

		public static RectangleF GetCalloutTextRectForSize (ContentPackItem.ItemSize itemSize)
		{

			switch (itemSize) {

			case ContentPackItem.ItemSize.Large:

				return new RectangleF (82f, 162f, 800f, 366f);

			case ContentPackItem.ItemSize.Medium:

				return new RectangleF (42f, 82f, 400f, 182f);

			case ContentPackItem.ItemSize.Small:

				return new RectangleF (22f, 42f, 200f, 90f);

			case ContentPackItem.ItemSize.Tiny:

				return new RectangleF (26f, 26f, 124f, 56f);

			default:

				throw new ArgumentException ("Item size must be any of Large, Medium, Small or Tiny!", "itemSize");

			}//end switch

		}//end static RectangleF GetCalloutTextRectForSize





		public static SizeF GetCalloutSizeForPackItemSize (ContentPackItem.ItemSize itemSize)
		{

			switch (itemSize) {

			case ContentPackItem.ItemSize.Large:

				return new SizeF (1024f, 1024f);

			case ContentPackItem.ItemSize.Medium:

				return new SizeF (512f, 512f);

			case ContentPackItem.ItemSize.Small:

				return new SizeF (256f, 256f);

			case ContentPackItem.ItemSize.Tiny:

				return new SizeF (160f, 160f);

			default:

				throw new ArgumentException ("Item size must be any of Large, Medium, Small or Tiny!", "itemSize");

			}//end switch

		}//end static SizeF GetCalloutSizeForPackItemSize




		public static RectangleF GetCalloutTextRect (RectangleF imgFrame, ContentPackItem.ItemSize itemSize)
		{

			RectangleF calloutTextArea = GetCalloutTextRectForSize (itemSize);
			SizeF calloutSize = GetCalloutSizeForPackItemSize (itemSize);
			RectangleF imgRect = imgFrame;
			
			float sx = imgRect.Width / calloutSize.Width;
			float sy = imgRect.Height / calloutSize.Height;
			
			return new RectangleF (imgRect.X + (sx * calloutTextArea.X),
			                      imgRect.Y + (sy * calloutTextArea.Y),
			                      sx * calloutTextArea.Width,
			                      sy * calloutTextArea.Height);

		}//end static RectangleF GetCalloutTextRect






		public static int CountTransitionEffects (AnimationTypesTransitionEffectType val)
		{
			
			int[] values = (int[])Enum.GetValues (typeof(AnimationTypesTransitionEffectType));
			int toReturn = 0;
			
			if (val == AnimationTypesTransitionEffectType.None) {
				toReturn = 1;
			} else {
				
				for (int i = 1; i < values.Length; i++) {
					
					if (val.HasFlag ((AnimationTypesTransitionEffectType)values [i])) {
						toReturn++;
					}//end if
					
				}//end for
				
			}//end if else
			
			return toReturn;
			
		}//end void CountTransitionEffects







		public static RectangleF GetImageFrameForNewImageLayer(SizeF imageSize, RectangleF layerHandlerFrame)
		{
			
			float spacing = 10f;
			
			// Get the image dimension ratio
			float imageRatio = 
				imageSize.Width > imageSize.Height ?
					imageSize.Width / imageSize.Height :
					imageSize.Height / imageSize.Width;
			
			// Get the image view size
			float imageViewSize = 
				layerHandlerFrame.Width > layerHandlerFrame.Height ?
					layerHandlerFrame.Height - (spacing * 8f) :
					layerHandlerFrame.Width - (spacing * 8f);
			
			float displayWidth = 
				imageSize.Width > imageSize.Height ?
					imageViewSize :
					imageViewSize / imageRatio;
			float displayHeight = 
				imageSize.Height > imageSize.Width ?
					imageViewSize :
					imageViewSize / imageRatio;
			
			return new RectangleF (spacing * 4f,
			                      spacing * 4f,
			                      displayWidth,
			                      displayHeight);
			
		}//end static RectangleF GetImageFrameForNewImageLayer


		#endregion Content





		#region Canvas related

		public static void SetOneLayerActive (FrameLayerPair layer, Dictionary<int, FrameInfo> frameItems)
		{

			foreach (FrameInfo eachFrameItem in frameItems.Values) {

				foreach (LayerInfo eachLayerInfo in eachFrameItem.Layers.Values) {

					if (eachFrameItem.ID == layer.FrameID) {
						if (eachLayerInfo.ID == layer.LayerID) {
							eachLayerInfo.IsCanvasActive = true;
						} else {
							eachLayerInfo.IsCanvasActive = false;
						}//end if else
					} else {

						eachLayerInfo.IsCanvasActive = false;

					}//end if else

				}//end foreach

			}//end foreach

		}//end static void SetOneLayerActive





		public static void SetOneLayerActiveInFrame (int layerID, FrameInfo frameItem)
		{

			foreach (LayerInfo eachLayerInfo in frameItem.Layers.Values) {

				if (eachLayerInfo.ID == layerID) {
					eachLayerInfo.IsCanvasActive = true;
				} else {
					eachLayerInfo.IsCanvasActive = false;
				}//end if else

			}//end foreach

		}//end static void SetOneLayerActiveInFrame






		public static void SetAllLayersActive (Dictionary<int, FrameInfo> frameItems, bool active)
		{

			foreach (FrameInfo eachFrameItem in frameItems.Values) {

				foreach (LayerInfo eachLayerItem in eachFrameItem.Layers.Values) {

					eachLayerItem.IsCanvasActive = active;

				}//end foreach

			}//end foreach

		}//end static void SetAllLayersActive

		#endregion Canvas related



		#region Data related

		public static DrawingLayerType GetDrawingTypeForContentPackType (GenericEnumsContentPackType packType)
		{

			switch (packType) {

			case GenericEnumsContentPackType.Callout:

				return DrawingLayerType.Callout;

			case GenericEnumsContentPackType.Comix:

				return DrawingLayerType.Comix;

			case GenericEnumsContentPackType.RubberStamp:

				return DrawingLayerType.Stamp;

			default:

				throw new InvalidOperationException ("There is no equivalent drawing type for this content pack type!");

			}//end switch


		}//end DrawingLayerType GetDrawingTypeForContentPackType





		[Obsolete("Determining transition from final object state is wrong.", true)]
		public static AnimationTypesTransitionEffectType CreateTransitionEffect (LayerInfo guideLayer, TransitionInfo transitionItem)
		{

			AnimationTypesTransitionEffectType toReturn = AnimationTypesTransitionEffectType.None;

			RectangleF layerBox = guideLayer.GetBoundingBox ();
			RectangleF trBox = new RectangleF (transitionItem.EndLocation, transitionItem.EndSize);

			// Check for Move transition
			if (layerBox.Location != transitionItem.EndLocation) {
				toReturn = AnimationTypesTransitionEffectType.Move;
			}//end if

			// Check for Rotation transition
			double guideRotationAngle = guideLayer.DrawingItems.Select (s => s.Value.RotationAngle).Max ();
			if (guideRotationAngle != transitionItem.RotationAngle) 
			{
				if (toReturn == AnimationTypesTransitionEffectType.None) 
				{
					toReturn = AnimationTypesTransitionEffectType.Rotate;
				} else 
				{
					toReturn |= AnimationTypesTransitionEffectType.Rotate;
				}//end if else

			}//end if

			// Check for Scale transition
			if (layerBox.Size != transitionItem.EndSize) 
			{
				if (toReturn == AnimationTypesTransitionEffectType.None) 
				{
					toReturn = AnimationTypesTransitionEffectType.Scale;
				} else 
				{
					toReturn |= AnimationTypesTransitionEffectType.Scale;
				}//end if else

			}//end if

			return toReturn;

		}//end static TransitionEffectType CreateTransitionEffectType






		public static PointF ConvertToPercentagePoint (PointF point, SizeF canvasSize)
		{

			float x = (point.X / canvasSize.Width) * 100;
			float y = (point.Y / canvasSize.Height) * 100;

			return new PointF (x, y);

		}//end PointF ConvertToPercentagePoint




		public static byte[] ConvertRGBAColorToByteArray (float[] components)
		{

			byte[] toReturn = new byte[components.Length];
			for (int i = 0; i < components.Length; i++) {
				toReturn [i] = Convert.ToByte (components [i] * 255f);
			}//end for

			return toReturn;

		}//end static byte[] ConvertRGBAColorToByteArray





		public static AnimationTypesAnimationLayerType GetLayerTypeForDrawingType(DrawingLayerType drType)
		{

			switch (drType)
			{

			case DrawingLayerType.Callout:

				return AnimationTypesAnimationLayerType.Callout;

			case DrawingLayerType.Comix:

				return AnimationTypesAnimationLayerType.Comix;

			case DrawingLayerType.Drawing:

				return AnimationTypesAnimationLayerType.Path;

			case DrawingLayerType.Image:

				return AnimationTypesAnimationLayerType.Image;

			case DrawingLayerType.Stamp:

				return AnimationTypesAnimationLayerType.Stamp;

			default:

				throw new InvalidOperationException("Unknown DrawingLayerType value!");

			}//end switch

		}//end static AnimationLayerType GetLayerTypeForDrawingType



		public static DrawingLayerType GetDrawingLayerTypeForLayerType(AnimationTypesAnimationLayerType layerType)
		{

			switch (layerType)
			{

			case AnimationTypesAnimationLayerType.Callout:

				return DrawingLayerType.Callout;

			case AnimationTypesAnimationLayerType.Comix:

				return DrawingLayerType.Comix;

			case AnimationTypesAnimationLayerType.Image:

				return DrawingLayerType.Image;

			case AnimationTypesAnimationLayerType.Path:

				return DrawingLayerType.Drawing;

			case AnimationTypesAnimationLayerType.Stamp:

				return DrawingLayerType.Stamp;

			default:

				throw new InvalidOperationException("Unknown layer type!");

			}//end switch

		}//end static DrawingLayerType GetDrawingLayerTypeForLayerType



		//TODO: ConvertByteArrayToRGBAColor, make it return the common Color object
//		public static CGColor ConvertByteArrayToRGBAColor (byte[] colorBuffer)
//		{
//
//			float[] components = new float[colorBuffer.Length];
//			for (int i = 0; i < colorBuffer.Length; i++) {
//
//				components [i] = (float)colorBuffer [i] / 255f;
//
//			}//end for
//
//			if (components.Length == 2) {
//
//				return new CGColor (colorBuffer [0],
//				                   colorBuffer [1]);
//
//			} else if (components.Length == 3) {
//
//				return new CGColor (colorBuffer [0],
//				                   colorBuffer [1],
//				                   colorBuffer [2]);
//
//			} else if (components.Length == 4) {
//
//				return new CGColor (components [0],
//				                   components [1],
//				                   components [2],
//				                   components [3]);
//
//			} else {
//				throw new InvalidOperationException ("Cannot create CGColor object.");
//			}//end if else if
//
//		}//end static CGColor ConvertByteArrayToRGBAColor






//		public static ScreenObject CreateScreenObjectFromLayer (LayerInfo layerItem, int frameID)
//		{
//
//			ScreenObject toReturn = null;
//			List<List<PointF>> pathPoints = new List<List<PointF>> ();
//			List<float> lineWidths = new List<float> ();
//			List<string> lineColors = new List<string> ();
//			RectangleF layerBox = layerItem.GetBoundingBox ();
//			byte[] imgBuffer = null;
//
//			if (layerItem.HasDrawingItems) 
//			{
//				switch (layerItem.DrawingItems [1].DrawingType) 
//				{
//
//				case DrawingLayerType.Drawing:
//
//					foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) 
//					{
//
//						pathPoints.Add (ConvertToPercentageList (eachDrawingInfo.PathPoints, layerItem.CanvasSize));
//						lineWidths.Add (eachDrawingInfo.Brush.Thickness);
//						lineColors.Add (GetColorHexStr (eachDrawingInfo.LineColor));
//
//					}//end foreach
//
//					toReturn = new ScreenObject (pathPoints, lineWidths, lineColors, 0f, layerItem.ID);
//					toReturn.SetupNulls();
//					toReturn.InitialPosition = ConvertToPercentagePoint (layerBox.Location, layerItem.CanvasSize);
//					toReturn.Size = ConvertToPercentagePoint (new PointF (layerBox.Size.Width, layerBox.Size.Height), layerItem.CanvasSize);
////					toReturn.Transitions = CreateTransitionObjectsFromLayer(layerItem, frameID);
//
////					toReturn.ObjectType = 1;
//					break;
//
//				case DrawingLayerType.Callout:
//
//#if(DEBUG)
//					Console.WriteLine ("Callouts not supported yet!");
//#endif
//
//					break;
//
//				case DrawingLayerType.Comix:
//				case DrawingLayerType.Image:
//				case DrawingLayerType.Stamp:
//
//					using (NSData imgData = layerItem.DrawingItems[1].Image.AsPNG()) 
//					{
//
//						imgBuffer = imgData.ToArray ();
//
//					}//end using imgData
//
//					PointF initialPosition = ConvertToPercentagePoint (layerBox.Location, layerItem.CanvasSize);
//					PointF size = ConvertToPercentagePoint (new PointF (layerBox.Width, layerBox.Height), layerItem.CanvasSize);
//
//					toReturn = new ScreenObject (initialPosition, size, imgBuffer, 0, layerItem.ID);
//					toReturn.SetupNulls();
////					toReturn.ObjectType = 3;
////					toReturn.Transitions = CreateTransitionObjectsFromLayer(layerItem, frameID);
//
//					break;
//
//				default:
//
//					throw new InvalidOperationException (string.Format ("Not known drawing layer type! {0}", layerItem.DrawingItems [1].DrawingType));
//
//				}//end switch
//
//			}//end if
//
//			return toReturn;
//
//		}//end static ScreenObject 






//		public static List<Transition> CreateTransitionObjectsFromLayer (LayerInfo layerItem, int frameID)
//		{
//
//			List<Transition> toReturn = new List<Transition> ();
//			TransitionInfo trItem = null;
//
//			if (layerItem.Transitions.TryGetValue (1, out trItem)) {
//
//				double startsAt = (frameID - 1) * trItem.Duration;
//
//				RectangleF layerBox = layerItem.GetBoundingBox ();
//				PointF startLocation = AnimationUtils.ConvertToPercentagePoint (layerBox.Location, layerItem.CanvasSize);
//				PointF endLocation = AnimationUtils.ConvertToPercentagePoint (trItem.EndLocation, layerItem.CanvasSize);
//				double degAngle = trItem.RotationAngle;
//
//				foreach (KeyValuePair<TransitionEffectType, TransitionEffectSettings> eachSetting in trItem.Settings) 
//				{
//					
//					if (eachSetting.Key == TransitionEffectType.Move) {
//						
//						Transition playTransition = 
//							new Transition ((float)startsAt, 
//							               (float)eachSetting.Value.Duration, 
//							               TransitionTypes.Move, 
//							               startLocation, 
//							               endLocation);
//						
//						toReturn.Add (playTransition);
//						
//					} else if (eachSetting.Key == TransitionEffectType.Scale) {
//						
//						Transition playTransition = 
//							new Transition ((float)startsAt,
//							               (float)eachSetting.Value.Duration,
//							               TransitionTypes.Scale,
//							               startLocation,
//							               endLocation);
//						
//						toReturn.Add (playTransition);
//						
//					} else if (eachSetting.Key == TransitionEffectType.Rotate) {
//						
//						Transition playTransition = 
//							new Transition ((float)startsAt,
//							               (float)eachSetting.Value.Duration,
//							               (float)trItem.RotationAngle);
//						
//						toReturn.Add (playTransition);
//						
//					} else if (eachSetting.Key == TransitionEffectType.FadeIn ||
//						eachSetting.Key == TransitionEffectType.FadeOut) {
//						
//						Transition playTransition = 
//							new Transition ((float)startsAt,
//							               (float)eachSetting.Value.Duration, 0f, (float)trItem.FadeOpacity);
//						
//						toReturn.Add (playTransition);
//						
//					}//end if else
//					
//				}//end foreach
//
//			}//end if
//
//			return toReturn;
//
//		}//end static Transition CreateTransitionObjectFromLayer
//
//
//
//
//		public static Animation CreateAnimationForSending(Dictionary<int, FrameInfo> frameItems)
//		{
//
//			Animation toReturn = new Animation();
//			toReturn.SetupNulls();
//
//			List<ScreenObject> screenObjects = new List<ScreenObject>();
//			
//			foreach (LayerInfo eachLayerInfo in frameItems[1].Layers.Values.OrderBy(s => s.ID))
//			{
//				
//				ScreenObject screenObject = AnimationUtils.CreateScreenObjectFromLayer(eachLayerInfo, eachLayerInfo.ID);
//				foreach (FrameInfo eachFrameInfo in frameItems.Values.OrderBy(s => s.ID))
//				{
//					
//					LayerInfo layerItem = null;
//					if (eachFrameInfo.Layers.TryGetValue(eachLayerInfo.ID, out layerItem))
//					{
//						
//						List<Transition> transitions = 
//							AnimationUtils.CreateTransitionObjectsFromLayer(layerItem, eachFrameInfo.ID);
//						
//						screenObject.Transitions.AddRange(transitions);
//						
//					}//end if
//					
//				}//end foreach
//				
//				screenObjects.Add(screenObject);
//				
//			}//end foreach
//			
//			toReturn.ScreenObjects = screenObjects;
//
//			return toReturn;
//
//		}//end static Animation CreateAnimationForSending






		public static List<PointF> ConvertToPercentageList (List<PointF> points, SizeF canvasSize)
		{

			List<PointF> toReturn = new List<PointF> ();

			foreach (PointF eachPoint in points) {

				toReturn.Add (ConvertToPercentagePoint (eachPoint, canvasSize));

			}//end foreach

			return toReturn;

		}//end static List<PointF> ConvertPointsToPercentageList




		//TODO: Probably make it accept the common color object.
//		public static string GetColorHexStr (CGColor color)
//		{
//
//			string formatStr = "rgba({0},{1},{2},{3})";
//			string toReturn = string.Empty;
//
//			switch (color.NumberOfComponents) {
//
//			// Grey
//			case 2:
//
//				byte comp = (byte)(color.Components [0] * 255);
//				toReturn = string.Format (formatStr, comp, comp, comp, (byte)color.Components [1] * 255);
//
//				break;
//
//			// RGB
//			case 3:
//
//				toReturn = string.Format (formatStr, 
//				                     (byte)color.Components [0] * 255,
//				                     (byte)color.Components [1] * 255,
//				                     (byte)color.Components [2] * 255,
//				                     "255");
//
//				break;
//
//			// RGBA
//			case 4:
//
//				toReturn = string.Format (formatStr,
//				                     (byte)color.Components [0] * 255,
//				                     (byte)color.Components [1] * 255,
//				                     (byte)color.Components [2] * 255,
//				                     (byte)color.Components [3] * 255);
//
//				break;
//
//			default:
//
//#if(DEBUG)
//				Console.WriteLine ("Color components number not supported.");
//#endif
//				toReturn = string.Format (formatStr, 0, 0, 0, 255);
//
//				break;
//
//
//			}//end switch
//
//			return toReturn;
//
//		}

		#endregion Data related
	}
}


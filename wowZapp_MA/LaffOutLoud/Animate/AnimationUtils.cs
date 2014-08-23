using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using LOLApp_Common;
using LOLAccountManagement;
using LOLMessageDelivery.Classes.LOLAnimation;

using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.Content;

namespace wowZapp.Animate
{
	public class AnimationUtils
	{
		public static void ApplyScaleTransformToObject (System.Drawing.PointF fixedPoint, float scaleX, float scaleY, LayerInfo layerItem)
		{
			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {
				for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) {	
					System.Drawing.PointF point = eachDrawingInfo.PathPoints [i];
					point.X = scaleX * (point.X + (-fixedPoint.X)) + fixedPoint.X;
					point.Y = scaleY * (point.Y + (-fixedPoint.Y)) + fixedPoint.Y;
					
					eachDrawingInfo.PathPoints [i] = point;
				}//end for
			}//end foreach
		}//end PointF ApplyScaleTrsansformToObject

		public static void MoveGraphicsObject (System.Drawing.PointF touchLocation, System.Drawing.PointF prevTouchLocation, LayerInfo layerItem)
		{
			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {	
				if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing) {
					
					for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) {
						
						System.Drawing.PointF point = eachDrawingInfo.PathPoints [i];
						float xDiff = touchLocation.X - prevTouchLocation.X;
						float yDiff = touchLocation.Y - prevTouchLocation.Y;
						
						point.X += xDiff;
						point.Y += yDiff;
						
						eachDrawingInfo.PathPoints [i] = point;
						
					}//end for
					
				} else if (eachDrawingInfo.DrawingType == DrawingLayerType.Image ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Comix ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Stamp ||
					eachDrawingInfo.DrawingType == DrawingLayerType.Callout) {
					RectangleF imgFrame = eachDrawingInfo.ImageFrame;
					
					float xDiff = touchLocation.X - prevTouchLocation.X;
					float yDiff = touchLocation.Y - prevTouchLocation.Y;
					
					imgFrame.X += xDiff;
					imgFrame.Y += yDiff;
					
					eachDrawingInfo.ImageFrame = imgFrame;
					eachDrawingInfo.RotatedImageBox = eachDrawingInfo.ImageFrame.Rotate (eachDrawingInfo.RotationAngle);
					
					if (eachDrawingInfo.DrawingType == DrawingLayerType.Callout) {
						RectangleF calloutTextRect = eachDrawingInfo.CalloutTextRect;
						
						calloutTextRect.X += xDiff;
						calloutTextRect.Y += yDiff;
						
						eachDrawingInfo.CalloutTextRect = calloutTextRect;
					}//end if
				}//end if else if
			}//end foreach	
		}//end void MoveGraphicsObject

		public static void ScaleGraphicsObject (System.Drawing.PointF touchLocation, 
		                                        System.Drawing.PointF prevTouchLocation, 
		                                       LayerInfo layerItem, 
		                                       CanvasControlPoint controlPoint, 
		                                       bool aspectRatioLocked,
		                                       ContentPackItem.ItemSize itemSize = ContentPackItem.ItemSize.Small)
		{
			RectangleF boundingBox = layerItem.GetBoundingBox ();
			
			bool scaleUpOnly = boundingBox.Width < 10f || boundingBox.Height < 10f;
			
			float sx = 0;
			float sy = 0;
			
			System.Drawing.PointF fixedPoint = System.Drawing.PointF.Empty;
			
			switch (controlPoint) {
			case CanvasControlPoint.TopLeft:
				// Fixed point is bottom-right
				fixedPoint = new System.Drawing.PointF (boundingBox.Right, boundingBox.Bottom);
				sx = boundingBox.Width / (boundingBox.Width + (touchLocation.X - prevTouchLocation.X));
				sy = aspectRatioLocked ? sx : boundingBox.Height / (boundingBox.Height + (touchLocation.Y - prevTouchLocation.Y));
				break;
				
			case CanvasControlPoint.TopRight:
				// Fixed point is bottom-left
				fixedPoint = new System.Drawing.PointF (boundingBox.Left, boundingBox.Bottom);
				
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
				fixedPoint = new System.Drawing.PointF (boundingBox.Right, boundingBox.Top);
				
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
						
						System.Drawing.PointF point = eachDrawingInfo.PathPoints [i];
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
					
					if (eachDrawingInfo.DrawingType == DrawingLayerType.Callout) {
						eachDrawingInfo.CalloutTextRect = GetCalloutTextRect (eachDrawingInfo.ImageFrame, itemSize);
					}//end if
				}//end if else
			}//end foreach
		}//end void ScaleGraphicsObject

		public static void RotateGraphicsObject (System.Drawing.PointF touchLocation, LayerInfo layerItem, ref double prevRadAngle)
		{
			RectangleF boundingRect = layerItem.GetBoundingBox ();
			System.Drawing.PointF center = new System.Drawing.PointF (boundingRect.Width / 2, boundingRect.Height / 2);
			
			double deltaX = touchLocation.X - center.X;
			double deltaY = touchLocation.Y - center.Y; 
			
			double radAngle = Math.Atan2 (deltaY, deltaX);
			double angleDiff = radAngle - prevRadAngle;
			
			float angleCos = (float)Math.Cos (angleDiff);
			float angleSin = (float)Math.Sin (angleDiff);
			
			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {
				
				if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing) {
					
					for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) {
						
						System.Drawing.PointF point = eachDrawingInfo.PathPoints [i];
						
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
					
					double degAngle = radAngle * LOLConstants.RadToDeg;
					RectangleF imgFrame = eachDrawingInfo.ImageFrame;
					eachDrawingInfo.RotatedImageBox = imgFrame.Rotate (degAngle);
					
				}//end if else if
				
				eachDrawingInfo.RotationAngle = radAngle * LOLConstants.RadToDeg;
				
			}//end foreach
			
			prevRadAngle = radAngle;
			
		}//end void RotateGraphicsObject

		public static void RotateGraphicsObjectByAngle (double degAngle, LayerInfo layerItem)
		{
			
			RectangleF boundingBox = layerItem.GetBoundingBox ();
			System.Drawing.PointF center = new System.Drawing.PointF (boundingBox.Width / 2, boundingBox.Height / 2);
			
			double radAngle = degAngle * LOLConstants.DegToRad;
			float angleCos = (float)Math.Cos (radAngle);
			float angleSin = (float)Math.Sin (radAngle);
			
			foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {
				
				if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing) {
					
					for (int i = 0; i < eachDrawingInfo.PathPoints.Count; i++) {
						
						System.Drawing.PointF point = eachDrawingInfo.PathPoints [i];
						
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
					
					throw new NotImplementedException ("Implement Callouts in AnimationUtils.RotateGraphicsObjectByAngle!");
					
				}//end if else if
				
			}//end foreach
			
		}//end static void RotateGraphicsObjectByAngle

		
		public static ImageView RotateImage (ImageView image, RectangleF origImageFrame, RectangleF rotatedImageRect, double degAngle, bool disposeOriginalImage)
		{
			ImageView toReturn = image;
			float width = image.Drawable.Bounds.Width () / 2;
			float height = image.Drawable.Bounds.Height () / 2;
			double radAngle = degAngle * LOLConstants.DegToRad;
			Matrix matrix = new Matrix ();
			toReturn.SetScaleType (ImageView.ScaleType.Matrix);   //required
			matrix.PostRotate ((float)degAngle, width, height);
			toReturn.ImageMatrix = matrix;
			
			return toReturn;
			
		}//end static UIImage RotateImage
		
		public static ImageView CropImage (ImageView image, RectangleF cropArea, Context c)
		{
			Bitmap mBitmap = null;
			Bitmap croppedImage = Bitmap.CreateBitmap ((int)cropArea.Width, (int)cropArea.Height,
			                                          Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas (croppedImage);
			Rect dstRect = new Rect (0, 0, (int)cropArea.Width, (int)cropArea.Height);
			Rect croppedArea = new Rect ((int)cropArea.Left, (int)cropArea.Top, (int)cropArea.Right, (int)cropArea.Bottom);
			canvas.DrawBitmap (mBitmap, croppedArea, dstRect, null);
			ImageView v = new ImageView (c);
			v.SetImageBitmap (mBitmap);
			return v;
			
		}//end static UIImage CropImage
		
		
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
		

		public static ImageView GetImageForTransitionEffect (TransitionEffectType efType, Context c)
		{
			ImageView v = new ImageView (c);
			switch (efType) {
			case TransitionEffectType.None:
				
				v.SetImageResource (Resource.Drawable.addtransition);
				break;
			case TransitionEffectType.BarnDoors:
				
				v.SetImageResource (Resource.Drawable.barndoors);
				break;
			case TransitionEffectType.Blinds:
				
				v.SetImageResource (Resource.Drawable.blinds);
				break;
			case TransitionEffectType.Fade:
				
				v.SetImageResource (Resource.Drawable.fade);
				break;
			case TransitionEffectType.FadeThroughBlack:
				
				v.SetImageResource (Resource.Drawable.fadethroughblack);
				break;
			case TransitionEffectType.Flip:
				
				v.SetImageResource (Resource.Drawable.flip);
				break;
			case TransitionEffectType.Fold:
				
				v.SetImageResource (Resource.Drawable.fold);
				break;
			case TransitionEffectType.Glow:
				
				v.SetImageResource (Resource.Drawable.glow);
				break;
			case TransitionEffectType.GradientWipe:
				
				v.SetImageResource (Resource.Drawable.gradientwipe);
				break;
			case TransitionEffectType.RadialWipe:
				
				v.SetImageResource (Resource.Drawable.radialwipe);
				break;
			case TransitionEffectType.RandomBars:
				
				v.SetImageResource (Resource.Drawable.randombars);
				break;
			case TransitionEffectType.RandomDissolve:
				
				v.SetImageResource (Resource.Drawable.randomdissolve);
				break;
			case TransitionEffectType.SlideBottomLeft:
				
				v.SetImageResource (Resource.Drawable.slidebottomleft);
				break;
			case TransitionEffectType.SlideLeft:
				
				v.SetImageResource (Resource.Drawable.slideleft);
				break;
			case TransitionEffectType.SlideRight:
				
				v.SetImageResource (Resource.Drawable.slideright);
				break;
			case TransitionEffectType.SlideTopRight:
				
				v.SetImageResource (Resource.Drawable.slidetopright);
				break;
			case TransitionEffectType.Wheel:
				
				v.SetImageResource (Resource.Drawable.wheel);
				break;
			case TransitionEffectType.Move:
				
				v.SetImageResource (Resource.Drawable.move);
				break;
			case TransitionEffectType.Scale:
				
				v.SetImageResource (Resource.Drawable.scale);
				break;
			case TransitionEffectType.Rotate:
				
				v.SetImageResource (Resource.Drawable.rotate);
				break;
			case TransitionEffectType.FadeIn:
				
				v.SetImageResource (Resource.Drawable.fadein);
				break;
			case TransitionEffectType.FadeOut:
				
				v.SetImageResource (Resource.Drawable.fadeout);
				break;
			default:
				
				if ((efType & (efType - 1)) != 0) {
					return CreateMultipleTransitionImage (efType, c);
				} else {
					return v;
				}
				break;
			}//end switch
			return v;
		}//end static UIImage GetImageForTransitionEffect

		public static ImageView CreateMultipleTransitionImage (TransitionEffectType efType, Context c)
		{
			ImageView toReturn = new ImageView (c);
			ImageView emptyTrImage = new ImageView (c);
			emptyTrImage.SetImageResource (Resource.Drawable.emptytransition);
			int transitionCount = CountTransitionEffects (efType);
			string nsText = transitionCount < 10 ? transitionCount.ToString () : "9+";
			
			Bitmap bitmap = Bitmap.CreateBitmap (emptyTrImage.Width, emptyTrImage.Height, Bitmap.Config.Rgb565);
			Canvas canvas = new Canvas (bitmap);
			Paint paint = new Paint ();
			paint.Color = Android.Graphics.Color.White;
			paint.StrokeWidth = 18f;
			paint.SetXfermode (new PorterDuffXfermode (PorterDuff.Mode.SrcOver)); // Text Overlapping Pattern
			canvas.DrawBitmap (bitmap, 0, 0, paint);
			canvas.DrawText (nsText, 10, 10, paint);

			toReturn.SetImageBitmap (bitmap);
			canvas.Dispose ();
			bitmap.Dispose ();
			
			return toReturn;
		}//end static UIImage CreateMultipleTransitionImage

		public static int CountTransitionEffects (TransitionEffectType val)
		{
			
			int[] values = (int[])Enum.GetValues (typeof(TransitionEffectType));
			int toReturn = 0;
			
			if (val == TransitionEffectType.None) {
				toReturn = 1;
			} else {
				
				for (int i = 1; i < values.Length; i++) {
					
					if (val.HasFlag ((TransitionEffectType)values [i])) {
						toReturn++;
					}//end if
					
				}//end for
				
			}//end if else
			
			return toReturn;
			
		}//end void CountTransitionEffects

		public static float AdjustTextToSize (string text, SizeF box, float edgeInset, Context context)
		{
			
			SizeF constrainSize = new SizeF (box.Width - edgeInset, 9999);
			float toReturn = 0f;
			
			Paint paint = new Paint (PaintFlags.AntiAlias);
			paint.TextSize = 16f;
			paint.SetTypeface (Typeface.Default);
			float height = ImageHelper.convertDpToPixel (paint.MeasureText (text), context);
			
			if (height > box.Height - edgeInset) {
					
				do {
					height -= 0.5f;
					paint.TextSize = height;
					height = ImageHelper.convertDpToPixel (paint.MeasureText (text), context);						
				} while (height > box.Height - edgeInset);
					
				toReturn = paint.TextSize;
					
			} else if (height < box.Height - edgeInset) {
					
				do {
					height += 0.5f;
					paint.TextSize = height;
					height = ImageHelper.convertDpToPixel (paint.MeasureText (text), context);	
						
				} while (height < box.Height - edgeInset);
					
				toReturn = paint.TextSize;
					
			} else {
					
				toReturn = paint.TextSize;
					
			}//end if else
				
			return toReturn;
			
		}//end static SizeF GetTextSize

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

		/// <summary>
		/// Creates and returns the text parameters for the given callout text area.
		/// </summary>
		/// <returns>
		/// A Pair<UIFont, SizeF> containing the font and text size.
		/// </returns>
		/// <param name='textRect'>
		/// The current text area of the callout.
		/// </param>
		/// <param name='text'>
		/// The text for which to calculate and create the parameters.
		/// </param>
		public static float GetTextParamsForCallout (RectangleF textRect, string text, Context context)
		{
			SizeF constrainSize = new SizeF (textRect.Width, 9999);
			Paint paint = new Paint (PaintFlags.AntiAlias);
			paint.TextSize = 18f;
			paint.SetTypeface (Typeface.Default);
			float height = ImageHelper.convertDpToPixel (paint.MeasureText (text), context);

			float minHeight = textRect.Height - 2f;
				
			if (height > minHeight) {
					
				do {
					height -= 1f;
					paint.TextSize = height;
					height = ImageHelper.convertDpToPixel (paint.MeasureText (text), context);
				} while (height > textRect.Height);
					
			} else if (height < minHeight) {
					
				do {
					height += 1f;
					paint.TextSize = height;
					height = ImageHelper.convertDpToPixel (paint.MeasureText (text), context);
				} while (height < minHeight);
					
			}//end if else
			
			return paint.TextSize;
			
		}//end Pair<UIFont, SizeF> GetTextParamsForCallout

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

		public static RectangleF GetImageFrameForNewImageLayer (ImageView image, RectangleF layerHandlerFrame)
		{			
			Rect imageSize = new Rect ();
			float spacing = 10f;
			image.GetDrawingRect (imageSize);
			
			// Get the image dimension ratio
			float imageRatio = 
				imageSize.Width () > imageSize.Height () ?
					imageSize.Width () / imageSize.Height () :
					imageSize.Height () / imageSize.Width ();
			
			// Get the image view size
			float imageViewSize = 
				layerHandlerFrame.Width > layerHandlerFrame.Height ?
					layerHandlerFrame.Height - (spacing * 8f) :
					layerHandlerFrame.Width - (spacing * 8f);
			
			float displayWidth = 
				imageSize.Width () > imageSize.Height () ?
					imageViewSize :
					imageViewSize / imageRatio;
			float displayHeight = 
				imageSize.Height () > imageSize.Width () ?
					imageViewSize :
					imageViewSize / imageRatio;
			
			return new RectangleF (spacing * 4f,
			                      spacing * 4f,
			                      displayWidth,
			                      displayHeight);
			
		}//end static RectangleF GetImageFrameForNewImageLayer

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

        public static DrawingLayerType GetDrawingTypeForContentPackType(LOLCodeLibrary.GenericEnumsContentPackType packType)
		{
			
			switch (packType) {

            case LOLCodeLibrary.GenericEnumsContentPackType.Callout:
				
				return DrawingLayerType.Callout;

            case LOLCodeLibrary.GenericEnumsContentPackType.Comix:
				
			    return DrawingLayerType.Comix;

            case LOLCodeLibrary.GenericEnumsContentPackType.RubberStamp:
				
				return DrawingLayerType.Stamp;
				
			default:
				
				throw new InvalidOperationException ("There is no equivalent drawing type for this content pack type!");
				
			}//end switch
			
			
		}//end DrawingLayerType GetDrawingTypeForContentPackType

		public static TransitionEffectType CreateTransitionEffect (LayerInfo guideLayer, TransitionInfo transitionItem)
		{
			
			TransitionEffectType toReturn = TransitionEffectType.None;
			
			RectangleF layerBox = guideLayer.GetBoundingBox ();
			// Check for Move transition
			if (layerBox.Location != transitionItem.EndLocation) {
				toReturn = TransitionEffectType.Move;
			}//end if
			
			// Check for Rotation transition
			double guideRotationAngle = guideLayer.DrawingItems.Select (s => s.Value.RotationAngle).Max ();
			if (guideRotationAngle != transitionItem.RotationAngle) {
				if (toReturn == TransitionEffectType.None) {
					toReturn = TransitionEffectType.Rotate;
				} else {
					toReturn |= TransitionEffectType.Rotate;
				}//end if else
				
			}//end if
			
			
			// Check for Scale transition
			if (layerBox.Size != transitionItem.EndSize) {
				if (toReturn == TransitionEffectType.None) {
					toReturn = TransitionEffectType.Scale;
				} else {
					toReturn |= TransitionEffectType.Scale;
				}//end if else
				
			}//end if
			
			return toReturn;
			
		}//end static TransitionEffectType CreateTransitionEffectType

		public static TransitionDescription CreateTransitionDescriptionForEffect (TransitionEffectType efType, Context context)
		{
			
			TransitionDescription toReturn = null;
			ImageView iv = new ImageView (context);
			switch (efType) {
				
			case TransitionEffectType.Move:
				iv.SetImageResource (Resource.Drawable.move);
				toReturn = 
					new TransitionDescription (
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsMove),
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsMoveDescription),
						iv, efType);		
				break;
				
			case TransitionEffectType.Scale:
				iv.SetImageResource (Resource.Drawable.scale);
				toReturn = 
					new TransitionDescription (
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsScale),
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsScaleDescription),
						iv, efType);
				break;
				
			case TransitionEffectType.Rotate:
				iv.SetImageResource (Resource.Drawable.rotate);
				toReturn = 
					new TransitionDescription (
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsRotation),
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsRotationDescription),
						iv, efType);
				break;
				
			case TransitionEffectType.FadeIn:
				iv.SetImageResource (Resource.Drawable.fadein);
				toReturn = 
					new TransitionDescription (
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsFadeIn),
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsFadeInDescription),
						iv, efType);
				break;
				
			case TransitionEffectType.FadeOut:
				iv.SetImageResource (Resource.Drawable.fadeout);
				toReturn = 
					new TransitionDescription (
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsFadeOut),
						Application.Context.Resources.GetString (Resource.String.transitionsSettingsFadeOutDescription),
						iv, efType);
				
				break;
				
			default:
				
				throw new InvalidOperationException (string.Format ("Don't know what to do with transition effect: {0}", efType));
				break;
			}//end switch
			
			
			return toReturn;
			
		}//end static TransitionDescription CreateTransitionDescriptionForEffect

		public static System.Drawing.PointF ConvertToPercentagePoint (System.Drawing.PointF point, SizeF canvasSize)
		{
			
			float x = (point.X / canvasSize.Width) * 100;
			float y = (point.Y / canvasSize.Height) * 100;
			
			return new System.Drawing.PointF (x, y);
			
		}//end PointF ConvertToPercentagePoint

		public static byte[] ConvertRGBAColorToByteArray (Android.Graphics.Color components)
		{	
			byte[] toReturn = new byte[3];
			toReturn [0] = components.R;
			toReturn [1] = components.G;
			toReturn [2] = components.B;
			toReturn [3] = components.A;

			return toReturn;
		}//end static byte[] ConvertRGBAColorToByteArray

		public static Android.Graphics.Color ConvertByteArrayToRGBAColor (byte[] colorBuffer)
		{
			byte[] comps = new byte[colorBuffer.Length];
			float[] components = new float[colorBuffer.Length];
			for (int i = 0; i < colorBuffer.Length; i++) {
				
				components [i] = colorBuffer [i] / 255f;
				comps [i] = Convert.ToByte (components [i]);
			}//end for
			
			
			
			if (components.Length == 2) {
				// grey - alpha
				return new Android.Graphics.Color (colorBuffer [0], colorBuffer [0], colorBuffer [0], colorBuffer [1]);
				
			} else if (components.Length == 3) {
				// rgb
				return new Android.Graphics.Color (colorBuffer [0], colorBuffer [1], colorBuffer [2]);
				
			} else if (components.Length == 4) {
				// rgba
				return new Android.Graphics.Color (comps [0], comps [1], comps [2], comps [3]);
				
			} else {
				throw new InvalidOperationException ("Cannot create Color object.");
			}//end if else if
			
		}//end static CGColor ConvertByteArrayToRGBAColor
		
		public static ScreenObject CreateScreenObjectFromLayer (LayerInfo layerItem, int frameID)
		{
			ScreenObject toReturn = null;
			List<List<System.Drawing.PointF>> pathPoints = new List<List<System.Drawing.PointF>> ();
			List<float> lineWidths = new List<float> ();
			List<string> lineColors = new List<string> ();
			RectangleF layerBox = layerItem.GetBoundingBox ();
			byte [] imgBuffer = null;
			if (layerItem.HasDrawingItems) {
				switch (layerItem.DrawingItems [1].DrawingType) {
				case DrawingLayerType.Drawing:
					foreach (DrawingInfo eachDrawingInfo in layerItem.DrawingItems.Values) {
						pathPoints.Add (ConvertToPercentageList (eachDrawingInfo.PathPoints, layerItem.CanvasSize));
						lineWidths.Add (eachDrawingInfo.Brush.Thickness);
						lineColors.Add (GetColorHexStr (eachDrawingInfo.LineColor));
					}
		
					toReturn = new ScreenObject (pathPoints, lineWidths, lineColors, 0f, layerItem.ID);
					toReturn.InitialPosition = ConvertToPercentagePoint (layerBox.Location, layerItem.CanvasSize);
					toReturn.Size = ConvertToPercentagePoint (new System.Drawing.PointF (layerBox.Size.Width, layerBox.Size.Height), layerItem.CanvasSize);
					break;
				case DrawingLayerType.Comix:
				case DrawingLayerType.Image:
				case DrawingLayerType.Stamp:
					/*using (Bitmap imgData = layerItem.DrawingItems[1].Image) {
						using (MemoryStream mem = new MemoryStream ()) {
							imgData.Compress (Bitmap.CompressFormat.Png, 100, mem);
						}
					}*/
					System.Drawing.PointF initialPosition = ConvertToPercentagePoint (layerBox.Location, layerItem.CanvasSize);
					System.Drawing.PointF size = ConvertToPercentagePoint (new System.Drawing.PointF (layerBox.Width, layerBox.Height), layerItem.CanvasSize);
					toReturn = new ScreenObject (initialPosition, size, imgBuffer, 0, layerItem.ID);
					break;
				default:
					throw new InvalidOperationException (string.Format ("Not known drawing layer type {0}", layerItem.DrawingItems [1].DrawingType));
				}
			}
			return toReturn;
		}
		
		public static List<Transition>CreateTransitionObjectsFromLayer (LayerInfo layerItem, int frameID)
		{
			List<Transition> toReturn = new List<Transition> ();
			TransitionInfo trItem = null;
			if (layerItem.Transitions.TryGetValue (1, out trItem)) {
				double startsAt = (frameID - 1) * trItem.Duration;
				RectangleF layerBox = layerItem.GetBoundingBox ();
				System.Drawing.PointF startLocation = AnimationUtils.ConvertToPercentagePoint (layerBox.Location, layerItem.CanvasSize);
				System.Drawing.PointF endLocation = AnimationUtils.ConvertToPercentagePoint (trItem.EndLocation, layerItem.CanvasSize);
				double degAngle = trItem.RotationAngle;
		
				foreach (KeyValuePair<TransitionEffectType, TransitionEffectSettings> eachSetting in trItem.Settings) {
					if (eachSetting.Key == TransitionEffectType.Move) {
						Transition playTransition = new Transition ((float)startsAt, (float)eachSetting.Value.Duration, TransitionTypes.Move, startLocation, endLocation);
						toReturn.Add (playTransition);
					} else
		if (eachSetting.Key == TransitionEffectType.Scale) {
						Transition playTransition = new Transition ((float)startsAt, (float)eachSetting.Value.Duration, TransitionTypes.Scale, startLocation, endLocation);
						toReturn.Add (playTransition);
					} else
		if (eachSetting.Key == TransitionEffectType.Rotate) {
						Transition playTransition = new Transition ((float)startsAt, (float)eachSetting.Value.Duration, (float)trItem.RotationAngle);
						toReturn.Add (playTransition);
					} else
						if (eachSetting.Key == TransitionEffectType.FadeIn || eachSetting.Key == TransitionEffectType.FadeOut) {
						Transition playTransition = new Transition ((float)startsAt, (float)eachSetting.Value.Duration, 0f, (float)trItem.FadeOpacity);
						toReturn.Add (playTransition);
					}
				}
			}
			return toReturn;
		}
		
		public static List<System.Drawing.PointF> ConvertToPercentageList (List<System.Drawing.PointF>points, SizeF canvasSize)
		{
			List<System.Drawing.PointF> toReturn = new List<System.Drawing.PointF> ();
			foreach (System.Drawing.PointF eachPoint in points)
				toReturn.Add (ConvertToPercentagePoint (eachPoint, canvasSize));
			return toReturn;
		}
	
		public static string GetColorHexStr (Android.Graphics.Color color)
		{
			string formatStr = "rgba{0},{1},{2},{3}";
			string toReturn = string.Empty;
			int components = 0;
			/*byte a = color.A * 255;
			byte b = color.B * 255;
			byte r = color.R * 255;
			byte g = color.G * 255;
			
			toReturn = string.Format (formatStr, r, g, b, a);*/
			return toReturn;
		}
	}
}
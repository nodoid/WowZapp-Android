// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

using Android.Graphics;
using Android.Views;
using Android.Content;

using LOLAccountManagement;

namespace wowZapp.Animate
{
	public class DrawingCanvasViewEx : View
	{
		public DrawingCanvasViewEx (RectangleF frame) : base(frame)
		{
			this.Initialize ();
		}
	
		public DrawingCanvasViewEx (IntPtr handle) : base(handle)
		{
			this.Initialize ();
		}//end ctor

		public event EventHandler<DrawingInfoCreatedEventArgs> DrawingItemCreated;

		private bool userDraw;
		private ImageView imgDrawDisplay;
		private ImageLayerView imgLayerView;
		private Path currentPath;
		private Color inactiveColor;
		private Color inactiveImageColor;
		private Color inactiveTextColor;

		public static float[] InactiveColorValues = new float[4] { 230f / 255f, 230f / 255f, 230f / 255f, 1f };
		public static float[] InactiveImageColorValues = new float[4] { 230f / 255f, 230f / 255f, 230f / 255f, 0.4f };
	
		public BrushItem Brush {
			get;
			private set;
		}//end BrushItem Brush

		public DrawingInfo ActiveDrawing {
			get;
			private set;
		}//end DrawingInfo ActiveDrawing

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			
			UITouch touch = touches.AnyObject as UITouch;
			
			if (null != touch) {
				
				if (this.userDraw) {
					int[] location = new int[2];
					this.GetLocationOnScreen (location);
					PointF touchLocation = PointF ((float)location [0], (float)location [1]);
					
					this.ActiveDrawing = new DrawingInfo (-1, new List<PointF> () { touchLocation }, this.Brush, this.Brush.BrushColor);
					this.DrawOnePoint (touchLocation, touchLocation, this.ActiveDrawing);
					
				}//end if
				
			}//end if
			
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			
			UITouch touch = touches.AnyObject as UITouch;
			
			if (null != touch) {
				
				if (this.userDraw) {
					int[] location = new int[2];
					this.GetLocationOnScreen (location);
					PointF touchLocation = PointF ((float)location [0], (float)location [1]);
					this.ActiveDrawing.PathPoints.Add (touchLocation);
					
					this.DrawOnePoint (touch.PreviousLocationInView (this), touchLocation, this.ActiveDrawing);
					
				}//end if
				
			}//end if
			
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			
			if (this.userDraw) {
				
				if (null != this.DrawingItemCreated) {
					this.DrawingItemCreated (this, new DrawingInfoCreatedEventArgs (this.ActiveDrawing));
					this.ActiveDrawing = null;
				}//end if
				
			}//end if
		}
		
		
		
		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
			
		}

		private void Initialize ()
		{
			
			this.Background = Color.White;
			
			this.Brush = new BrushItem (4f, BrushType.Normal, UIColor.Black.CGColor);
			
			/*this.imgDrawDisplay = new ImageView(this);
			imgDrawDisplay,
			this.imgDrawDisplay.ContentMode = UIViewContentMode.ScaleAspectFit;
			this.imgDrawDisplay.BackgroundColor = UIColor.Clear;
			
			this.AddSubview(this.imgDrawDisplay);
			
			this.currentPath = new UIBezierPath();
			this.currentPath.LineCapStyle = CGLineCap.Round;
			this.currentPath.LineJoinStyle = CGLineJoin.Round;*/
			
			currentPath = new Path ();
			
			this.ClearsContextBeforeDrawing = false;
			this.userDraw = true;

			this.inactiveColor = new Color ((byte)InactiveColorValues [0],
			                               (byte)InactiveColorValues [1],
			                               (byte)InactiveColorValues [2],
			                               (byte)InactiveColorValues [3]);

			this.inactiveImageColor = new Color ((byte)InactiveImageColorValues [0],
			                                    (byte)InactiveImageColorValues [1],
			                                    (byte)InactiveImageColorValues [2],
			                                    (byte)InactiveImageColorValues [3]);

			this.inactiveTextColor = Color.LightGray;
			
		}//end void Initialize
		
		
		
		
		
		private void DrawOnePoint (PointF prevPoint, PointF currentPoint, DrawingInfo drawingInfo)
		{
			
			UIGraphics.BeginImageContextWithOptions (this.Bounds.Size, false, UIScreen.MainScreen.Scale);
			
			using (CGContext context = UIGraphics.GetCurrentContext()) {
				
				if (null != this.imgDrawDisplay.Image) {
					this.imgDrawDisplay.Image.Draw (this.Bounds);
				}//end if

				context.InterpolationQuality = CGInterpolationQuality.None;
				context.SetAllowsAntialiasing (false);
				context.SetLineCap (CGLineCap.Round);
				context.SetLineJoin (CGLineJoin.Round);
				
				if (drawingInfo.Brush.BrushType == BrushType.Normal) {
					
					context.SetStrokeColor (drawingInfo.LineColor);
					context.SetLineWidth (drawingInfo.Brush.Thickness);
					
					context.MoveTo (prevPoint.X, prevPoint.Y);
					PointF midPoint = new PointF ((prevPoint.X + currentPoint.X) / 2f,
					                             (prevPoint.Y + currentPoint.Y) / 2f);
					context.AddCurveToPoint (prevPoint.X, prevPoint.Y,
					                        midPoint.X, midPoint.Y,
					                        currentPoint.X, currentPoint.Y);
					
					context.DrawPath (CGPathDrawingMode.Stroke);
					
				} else {

					if (!drawingInfo.Brush.IsSprayBrushActive) {
						drawingInfo.Brush.SetBrushActive (true);
					}//end if
					SizeF brushImageSize = drawingInfo.Brush.BrushImage.Size;
					RectangleF rect = new RectangleF (currentPoint.X - (brushImageSize.Width / 2f),
					                                 currentPoint.Y - (brushImageSize.Height / 2f),
					                                 brushImageSize.Width,
					                                 brushImageSize.Height);
					
					drawingInfo.Brush.BrushImage.Draw (rect.Location);
					
				}//end if else
				
				if (null != this.imgDrawDisplay.Image) {
					this.imgDrawDisplay.Image.Dispose ();
				}//end if
				this.imgDrawDisplay.Image = UIGraphics.GetImageFromCurrentImageContext ();
				
			}//end using context
			UIGraphics.EndImageContext ();
			
		}//end void DrawOnePoint

		
		
		
		
		
		private RectangleF GetBoundingBox (PointF first, PointF second, float spacing)
		{
			
			float fx = first.X;
			float fy = first.Y;
			float sx = second.X;
			float sy = second.Y;
			
			RectangleF rect = RectangleF.Empty;
			
			if (fx > sx) {
				rect.X = sx - spacing;
				rect.Width = (fx - sx) + (spacing * 2f);
			} else {
				rect.X = fx - spacing;
				rect.Width = (sx - fx) + (spacing * 2f);
			}//end if else
			
			if (fy > sy) {
				rect.Y = sy - spacing;
				rect.Height = (fy - sy) + (spacing * 2f);
			} else {
				rect.Y = fy - spacing;
				rect.Height = (sy - fy) + (spacing * 2f);
			}//end if else
			
			return rect;
			
		}//end RectangleF GetBoundingBox


		public void SetBrush (BrushItem brushItem)
		{
			
			this.Brush = brushItem;
			
		}//end void SetBrush
		
		
		
		public void AddImage (ImageView image)
		{
			
			this.userDraw = false;
			
			this.imgLayerView = new ImageLayerView (this.Bounds);
			this.imgLayerView.DeleteTapped += ImgLayerView_DeleteTapped;
			this.imgLayerView.CancelTapped += ImgLayerView_CancelTapped;
			this.imgLayerView.ApplyTapped += ImgLayerView_ApplyTapped;
			
			this.AddSubview (this.imgLayerView);
			
			this.imgLayerView.SetImage (image);
			
		}//end void AddImage





		public void AddImageForContentPack (UIImage image, DrawingLayerType drType, int contentPackItemID)
		{

			this.userDraw = false;

			this.imgLayerView = new ImageLayerView (this.Bounds);
			this.imgLayerView.DeleteTapped += ImgLayerView_DeleteTapped;
			this.imgLayerView.CancelTapped += ImgLayerView_CancelTapped;
			this.imgLayerView.ApplyTapped += ImgLayerView_ApplyTapped;

			this.AddSubview (this.imgLayerView);

			this.imgLayerView.SetImageForContentPackItem (image, drType, contentPackItemID);

		}//end void AddImageForContentPack
		
		
		
		
		private void ImgLayerView_ApplyTapped (object sender, ImageLayerViewEventArgs<UIImage> e)
		{
			this.imgLayerView.ApplyTapped -= ImgLayerView_ApplyTapped;
			this.imgLayerView.CancelTapped -= ImgLayerView_CancelTapped;
			this.imgLayerView.DeleteTapped -= ImgLayerView_DeleteTapped;


			// The new image is added to a new layer
			DrawingInfo drawingInfo = null;

			if (e.DrawingType == DrawingLayerType.Image) {

				drawingInfo = new DrawingInfo (0, e.Image, e.ImageFrame);

			} else if (e.DrawingType == DrawingLayerType.Comix ||
				e.DrawingType == DrawingLayerType.Stamp) {

				drawingInfo = new DrawingInfo (0, e.ContentPackItemID, e.DrawingType, e.Image, e.ImageFrame, null);

			} else if (e.DrawingType == DrawingLayerType.Callout) {

				drawingInfo = new DrawingInfo (0, e.ContentPackItemID, e.DrawingType, e.Image, e.ImageFrame, this.Brush.BrushColor);

				drawingInfo.CalloutTextRect = AnimationUtils.GetCalloutTextRect (drawingInfo.ImageFrame, ContentPackItem.ItemSize.Small);

			}//end if else if

			if (null != this.DrawingItemCreated) {
				this.DrawingItemCreated (this, new DrawingInfoCreatedEventArgs (drawingInfo));
			}//end if
			
			this.imgLayerView.RemoveFromSuperview ();
			
			this.userDraw = true;
		}
		
		
		
		
		private void ImgLayerView_CancelTapped (ImageLayerView obj)
		{
			this.imgLayerView.ApplyTapped -= ImgLayerView_ApplyTapped;
			this.imgLayerView.CancelTapped -= ImgLayerView_CancelTapped;
			this.imgLayerView.DeleteTapped -= ImgLayerView_DeleteTapped;
			
			this.imgLayerView.RemoveFromSuperview ();
			
			this.userDraw = true;
		}
		
		
		
		
		private void ImgLayerView_DeleteTapped (ImageLayerView obj)
		{
			this.imgLayerView.ApplyTapped -= ImgLayerView_ApplyTapped;
			this.imgLayerView.CancelTapped -= ImgLayerView_CancelTapped;
			this.imgLayerView.DeleteTapped -= ImgLayerView_DeleteTapped;
			
			this.imgLayerView.RemoveFromSuperview ();
			
			this.userDraw = true;
		}
		
		
		
		public void SetUserDraw (bool userDraw)
		{
			
			this.userDraw = userDraw;
			
		}//end void SetUserDraw




		public void ShowFrame (FrameInfo frameItem)
		{
			
			UIGraphics.BeginImageContextWithOptions (this.Bounds.Size, false, UIScreen.MainScreen.Scale);
			
			using (CGContext context = UIGraphics.GetCurrentContext()) {
				
				context.SetLineJoin (CGLineJoin.Round);
				context.SetLineCap (CGLineCap.Round);
				context.SetShouldAntialias (true);

				foreach (LayerInfo eachLayerInfo in frameItem.Layers.Values.OrderBy(s => s.ID)) {
					
					if (eachLayerInfo.DrawingItems.Count > 0) {
						
						foreach (KeyValuePair<int, DrawingInfo> eachItem in eachLayerInfo.DrawingItems) {
							
							if (eachItem.Value.DrawingType == DrawingLayerType.Drawing) {
								
								if (eachItem.Value.Brush.BrushType == BrushType.Normal) {

									context.SetStrokeColor (eachLayerInfo.IsCanvasActive ?
									                       eachItem.Value.LineColor :
									                       this.inactiveColor);

									context.SetLineWidth (eachItem.Value.Brush.Thickness);
									
									for (int i = 0; i < eachItem.Value.PathPoints.Count; i++) {
										
										PointF eachPoint = eachItem.Value.PathPoints [i];
										if (i == 0) {
											context.MoveTo (eachPoint.X, eachPoint.Y);
											context.AddLineToPoint (eachPoint.X, eachPoint.Y);
										} else {
											PointF prevPoint = eachItem.Value.PathPoints [i - 1];
											PointF midPoint = new PointF ((prevPoint.X + eachPoint.X) / 2f, (prevPoint.Y + eachPoint.Y) / 2f);
											context.MoveTo (prevPoint.X, prevPoint.Y);
											context.AddCurveToPoint (prevPoint.X, prevPoint.Y, midPoint.X, midPoint.Y, eachPoint.X, eachPoint.Y);
										}//end if else
										
									}//end for
									
									context.DrawPath (CGPathDrawingMode.Stroke);
									
								} else {
									
									SizeF brushImageSize = eachItem.Value.Brush.BrushImage.Size;
									if (eachItem.Value.Brush.IsSprayBrushActive != 
										eachLayerInfo.IsCanvasActive) {
										eachItem.Value.Brush.SetBrushActive (eachLayerInfo.IsCanvasActive);
									}//end if

									for (int i = 0; i < eachItem.Value.PathPoints.Count; i++) {

										PointF eachPoint = eachItem.Value.PathPoints [i];
										RectangleF rect = new RectangleF (eachPoint.X - (brushImageSize.Width / 2f),
										                                 eachPoint.Y - (brushImageSize.Height / 2f),
										                                 brushImageSize.Width,
										                                 brushImageSize.Height);

										eachItem.Value.Brush.BrushImage.Draw (rect.Location);

//										context.SaveState();
//
//										context.SetBlendMode(eachLayerInfo.IsCanvasActive ? 
//										                     CGBlendMode.SourceIn :
//										                     CGBlendMode.SourceAtop);
//										context.SetFillColor(eachLayerInfo.IsCanvasActive ? 
//										                     eachItem.Value.LineColor :
//										                     this.inactiveColor);
//										context.FillRect(rect);
//
//										context.RestoreState();
										
									}//end for
									
								}//end if else
								
							} else if (eachItem.Value.DrawingType == DrawingLayerType.Image ||
								eachItem.Value.DrawingType == DrawingLayerType.Comix ||
								eachItem.Value.DrawingType == DrawingLayerType.Stamp ||
								eachItem.Value.DrawingType == DrawingLayerType.Callout) {

								if (eachItem.Value.RotationAngle == 0) {

									if (!eachLayerInfo.IsCanvasActive) {

										UIImage imgToDraw = eachItem.Value.GetInactiveImage (this.inactiveColor);
										imgToDraw.Draw (eachItem.Value.ImageFrame);
										imgToDraw.Dispose ();

									} else {

										eachItem.Value.Image.Draw (eachItem.Value.ImageFrame);

									}//end if else

									if (eachItem.Value.DrawingType == DrawingLayerType.Callout &&
										!string.IsNullOrEmpty (eachItem.Value.CalloutText)) {

										Pair<UIFont, SizeF> calloutTextParams = 
											AnimationUtils.GetTextParamsForCallout (eachItem.Value.CalloutTextRect, eachItem.Value.CalloutText);

										context.SaveState ();

										context.SetFillColor (eachLayerInfo.IsCanvasActive ? 
										                     eachItem.Value.LineColor : 
										                     this.inactiveTextColor);
										context.SetTextDrawingMode (CGTextDrawingMode.Fill);
										context.SetShouldSmoothFonts (true);
										context.SetAllowsFontSmoothing (true);
										context.SetShouldAntialias (true);

										using (NSString nsText = new NSString(eachItem.Value.CalloutText)) {

											nsText.DrawString (eachItem.Value.CalloutTextRect.CenterInRect (calloutTextParams.ItemB),
											                  calloutTextParams.ItemA,
											                  UILineBreakMode.WordWrap,
											                  UITextAlignment.Center);
										}//end using nsText

										context.RestoreState ();

									}//end if

								} else {

									using (UIImage rotatedImage = 
									       AnimationUtils.RotateImage(eachLayerInfo.IsCanvasActive ? 
									                           eachItem.Value.Image : 
									                           eachItem.Value.GetInactiveImage(this.inactiveColor), 
									                           eachItem.Value.ImageFrame,
									                           eachItem.Value.RotatedImageBox, 
									                           eachItem.Value.RotationAngle, !eachLayerInfo.IsCanvasActive)) {
										rotatedImage.Draw (eachItem.Value.RotatedImageBox);
									}//end using rotatedImage

									if (eachItem.Value.DrawingType == DrawingLayerType.Callout &&
										!string.IsNullOrEmpty (eachItem.Value.CalloutText)) {
										
										Pair<UIFont, SizeF> textParams = 
											AnimationUtils.GetTextParamsForCallout (eachItem.Value.CalloutTextRect, eachItem.Value.CalloutText);

										context.SaveState ();
										
										context.SetFillColor (eachLayerInfo.IsCanvasActive ? 
										                     eachItem.Value.LineColor :
										                     this.inactiveTextColor);
										context.SetTextDrawingMode (CGTextDrawingMode.Fill);
										context.SetShouldSmoothFonts (true);
										context.SetAllowsFontSmoothing (true);
										context.SetShouldAntialias (true);
										
										List<PointF> calloutTextCorners = 
											eachItem.Value.CalloutTextRect.CenterInRect (textParams.ItemB).GetCorners ();
										List<PointF> rotatedCalloutTextCorners = 
											calloutTextCorners.RotatePoints (eachItem.Value.RotationAngle, 
											                                new PointF (eachItem.Value.RotatedImageBox.GetMidX (), eachItem.Value.RotatedImageBox.GetMidY ()));
										
										context.TranslateCTM (rotatedCalloutTextCorners [0].X, rotatedCalloutTextCorners [0].Y);
										context.RotateCTM (((float)eachItem.Value.RotationAngle * (float)LOLConstants.DegToRad));
										context.TranslateCTM (-rotatedCalloutTextCorners [0].X, -rotatedCalloutTextCorners [0].Y);
										
										using (NSString nsText = new NSString(eachItem.Value.CalloutText)) {
											
											nsText.DrawString (new RectangleF (rotatedCalloutTextCorners [0].X, 
											                                 rotatedCalloutTextCorners [0].Y,
											                                 textParams.ItemB.Width,
											                                 textParams.ItemB.Height),
											                  textParams.ItemA,
											                  UILineBreakMode.WordWrap,
											                  UITextAlignment.Center);
											
											
										}//end using nsText
										
										context.RestoreState ();
										
									}//end if

								}//end if else
								
							}//end if else if
							
						}//end foreach
						
					}//end if
					
				}//end foreach

				
				if (null != this.imgDrawDisplay.Image) {
					this.imgDrawDisplay.Image.Dispose ();
					this.imgDrawDisplay.Image = null;
				}//end if
				this.imgDrawDisplay.Image = UIGraphics.GetImageFromCurrentImageContext ();
				
			}//end using context
			
			UIGraphics.EndImageContext ();
			
		}//end void ShowCompleteDrawing

	}
}


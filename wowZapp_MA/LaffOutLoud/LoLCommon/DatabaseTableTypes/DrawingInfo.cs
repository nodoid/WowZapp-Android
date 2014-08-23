using System;
using System.IO;
using System.Collections.Generic;
using SQLite;
using System.Drawing;
using wowZapp.Animate;

using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using PreserveProps = Android.Runtime.PreserveAttribute;

namespace LOLApp_Common
{
	[PreserveProps(AllMembers=true)]
	public class DrawingInfo
	{

		public DrawingInfo (int drawingID, List<System.Drawing.PointF> pathPoints, BrushItem brush, Android.Graphics.Color lineColor)
		{
			this.DrawingID = drawingID;
			this.Brush = brush;
			this.LineColor = lineColor;
			this.PathPoints = pathPoints ?? new List<System.Drawing.PointF> ();
			
			this.DrawingType = DrawingLayerType.Drawing;	
		}

		public DrawingInfo (int drawingID, ImageView image, RectangleF imageFrame)
		{
			
			this.DrawingID = drawingID;
			this.Image = image;
			this.ImageFrame = imageFrame;
			
			this.DrawingType = DrawingLayerType.Image;
			
			this.PathPoints = new List<System.Drawing.PointF> ();
			
		}//end ctor

		/// <summary>
		/// Initializes a new instance of the <see cref="LOLApp_iOS.DrawingInfo"/> class.
		/// </summary>
		/// <param name='drawingID'>
		/// Drawing I.
		/// </param>
		/// <param name='contentPackItemID'>
		/// Content pack item I.
		/// </param>
		/// <param name='drType'>
		/// Drawing layer type. Only for Callouts, Stamps and Comix.
		/// </param>
		public DrawingInfo (int drawingID, int contentPackItemID, DrawingLayerType drType, ImageView image, RectangleF imageFrame, Android.Graphics.Color textColor)
		{
			
			if (drType != DrawingLayerType.Callout &&
				drType != DrawingLayerType.Comix &&
				drType != DrawingLayerType.Stamp) {	
				throw new ArgumentException ("When initializing a DrawingInfo item with this constructor, drType must represent a content pack item!", "drType");	
			}//end if
			
			this.DrawingID = drawingID;
			this.ContentPackItemID = contentPackItemID;
			this.DrawingType = drType;
			this.Image = image;
			this.ImageFrame = imageFrame;
			this.LineColor = textColor;
			
			this.PathPoints = new List<System.Drawing.PointF> ();
			
		}//end ctor

		public DrawingInfo ()
		{
			this.PathPoints = new List<System.Drawing.PointF> ();
		}//end default ctor
		
		[PrimaryKey, AutoIncrement]
		public int DBID {
			get;
			private set;
		}//end int DBID

		public int DrawingID {
			get;
			set;
		}//end int DrawingID

		[Indexed]
		public int LayerDBID {
			get;
			set;
		}//end int LayerDBID

		[Ignore]
		public BrushItem Brush {
			get;
			set;
		}//end BrushItem Brush

		[Indexed]
		public int BrushItemDBID {
			get;
			set;
		}//end int BrushItemID

		[Ignore]
		public Android.Graphics.Color LineColor {
			get;
			private set;
		}//end CGColor LineColor

		public byte[] LineColorBuffer {
			get {
				if (null != this.LineColor) {
					return AnimationUtils.ConvertRGBAColorToByteArray (this.LineColor);
				} else {
					return null;
				}//end if else
				
			}
			private set {
				byte[] buffer = value;
				if (null != buffer) {
					if (buffer.Length > 0) {
						this.LineColor = AnimationUtils.ConvertByteArrayToRGBAColor (buffer);
					} else {
						this.LineColor = Android.Graphics.Color.White;
					}//end if else
					
				} else {
					this.LineColor = Android.Graphics.Color.White;
				}//end if else
				
			}//end private set
			
		}//end byte[] LineColorBuffer

		[Ignore]
		public List<System.Drawing.PointF> PathPoints {
			get;
			private set;
		}//end List<PointF> PathPoints

		public DrawingLayerType DrawingType {
			get;
			private set;
		}//end DrawingLayerType DrawingType

		[Ignore]
		public ImageView Image {
			get;
			private set;
		}//end UIImage Image

		public byte[] ImgBuffer {
			get {
				if (null != this.Image) {
					try {
						using (Bitmap imgData = this.Image.GetDrawingCache(true)) {
							using (MemoryStream mem = new MemoryStream ()) {
								imgData.Compress (Bitmap.CompressFormat.Png, 100, mem);
								return mem.ToArray ();
							}
						}//end using imgData
						
					} catch {
						return new byte[0];
					}//end try catch
					
				} else {
					return new byte[0];
				}//end if else
				
			}
			private set {
				
				byte[] buffer = value;
				if (null != buffer) {
					if (buffer.Length > 0) {
						try {
							using (Bitmap imgData = BitmapFactory.DecodeByteArray(buffer, 0, buffer.Length)) {
								this.Image.SetImageBitmap (imgData);
							}//end using imgData
						} catch {
							this.Image = null;
						}//end try catch
					} else {
						this.Image = null;
					}//end if else
					
				} else {
					this.Image = null;
				}//end if else
				
			}//end get set
			
		}//end byte[] ImgBuffer
		
		[Ignore]
		public RectangleF ImageFrame {
			get;
			set;
		}//end RectangleF ImageFrame

		public float ImageFrameX {
			get {
				return this.ImageFrame.X;
			}
			private set {
				RectangleF imgFrame = this.ImageFrame;
				imgFrame.X = value;
				this.ImageFrame = imgFrame;
			}//end get set
			
		}//end float ImageFrameX

		public float ImageFrameY {
			get {
				return this.ImageFrame.Y;
			}
			private set {
				RectangleF imgFrame = this.ImageFrame;
				imgFrame.Y = value;
				this.ImageFrame = imgFrame;
			}//end get set
			
		}//end float ImageFrameY

		public float ImageFrameWidth {
			get {
				return this.ImageFrame.Width;
			}
			private set {
				RectangleF imgFrame = this.ImageFrame;
				imgFrame.Width = value;
				this.ImageFrame = imgFrame;
			}//end get private set
			
		}//end float ImageFrameWidth

		public float ImageFrameHeight {
			get {
				return this.ImageFrame.Height;
			}
			private set {
				RectangleF imgFrame = this.ImageFrame;
				imgFrame.Height = value;
				this.ImageFrame = imgFrame;
			}//end get private set
			
		}//end float ImageFrameHeight
		
		public double RotationAngle {
			get;
			set;
		}//end double ImageDegAngle

		[Ignore]
		public RectangleF RotatedImageBox {
			get;
			set;
		}//end double RotatedImageBox

		public float RotatedImageBoxX {
			get {
				return this.RotatedImageBox.X;
			}
			private set {
				RectangleF rotatedImgBox = this.RotatedImageBox;
				rotatedImgBox.X = value;
				this.RotatedImageBox = rotatedImgBox;
			}//end get set
			
		}//end float ImageFrameX

		public float RotatedImageBoxY {
			get {
				return this.RotatedImageBox.Y;
			}
			private set {
				RectangleF rotatedImgBox = this.RotatedImageBox;
				rotatedImgBox.Y = value;
				this.RotatedImageBox = rotatedImgBox;
			}//end get set
			
		}//end float ImageFrameY

		public float RotatedImageBoxWidth {
			get {
				return this.RotatedImageBox.Width;
			}
			private set {
				RectangleF rotatedImgBox = this.RotatedImageBox;
				rotatedImgBox.Width = value;
				this.RotatedImageBox = rotatedImgBox;
			}//end get private set
			
		}//end float ImageFrameWidth

		public float RotatedImageBoxHeight {
			get {
				return this.RotatedImageBox.Height;
			}
			private set {
				RectangleF rotatedImgBox = this.RotatedImageBox;
				rotatedImgBox.Height = value;
				this.RotatedImageBox = rotatedImgBox;
			}//end get private set
			
		}//end float ImageFrameHeight

		public int ContentPackItemID {
			get;
			private set;
		}//end int ContentPackItemID

		[MaxLength(200)]
		public string CalloutText {
			get;
			set;
		}//end string CalloutText

		[Ignore]
		public RectangleF CalloutTextRect {
			get;
			set;
		}//end RectangleF CalloutTextRect

		public float CalloutTextRectX {
			get {
				return this.CalloutTextRect.X;
			}
			private set {
				RectangleF calloutRect = this.CalloutTextRect;
				calloutRect.X = value;
				this.CalloutTextRect = calloutRect;
			}//end get private set
		}//end float CalloutTextRectX

		public float CalloutTextRectY {
			get {
				return this.CalloutTextRect.Y;
			}
			private set {
				RectangleF calloutRect = this.CalloutTextRect;
				calloutRect.X = value;
				this.CalloutTextRect = calloutRect;
			}//end get private set
			
		}//end float CalloutTextRectY

		public float CalloutTextRectWidth {
			get {
				return this.CalloutTextRect.Width;
			}
			private set {
				RectangleF calloutRect = this.CalloutTextRect;
				calloutRect.Width = value;
				this.CalloutTextRect = calloutRect;
			}//end get private set
		}//end float CalloutTextRectWidth

		public float CalloutTextRectHeight {
			get {
				return this.CalloutTextRect.Height;
			}
			private set {
				RectangleF calloutRect = this.CalloutTextRect;
				calloutRect.Height = value;
				this.CalloutTextRect = calloutRect;
			}//end get private set
		}//end float CalloutTextRectHeight

		public void SetImage (Bitmap image)
		{
			this.Image.SetImageBitmap (image);
		}//end void SetImage

		public List<PathPointDB> GetPathPointsForDB ()
		{
			
			List<PathPointDB> pathPoints = new List<PathPointDB> ();
			int sortOrder = 0;
			foreach (System.Drawing.PointF eachPoint in this.PathPoints) {
				PathPointDB pathPoint = new PathPointDB (this.DBID, sortOrder++, eachPoint.X, eachPoint.Y);
				pathPoints.Add (pathPoint);
			}//end foreach
			
			return pathPoints;
			
		}//end List<PathPointDB> GetPathPointsForDB

		public void SetPathPointsFromDB (List<PathPointDB> dbPathPoints)
		{
			
			foreach (PathPointDB eachPathPoint in dbPathPoints) {
				this.PathPoints.Add (new System.Drawing.PointF (eachPathPoint.X, eachPathPoint.Y));
			}//end foreach
			
		}//end void SetPathPointsFromDB

		public override string ToString ()
		{
			return string.Format ("[DrawingInfo: DBID={0}, DrawingID={1}, Brush={2}, LineColor={3}, PathPoints={4}, DrawingType={5}, Image={6}, ImgBuffer={7}, ImageFrame={8}, ImageFrameX={9}, ImageFrameY={10}, ImageFrameWidth={11}, ImageFrameHeight={12}, RotationAngle={13}, RotatedImageBox={14}, RotatedImageBoxX={15}, RotatedImageBoxY={16}, RotatedImageBoxWidth={17}, RotatedImageBoxHeight={18}, ContentPackItemID={19}, CalloutText={20}, CalloutTextRect={21}, CalloutTextRectX={22}, CalloutTextRectY={23}, CalloutTextRectWidth={24}, CalloutTextRectHeight={25}]", DBID, DrawingID, Brush, LineColor, PathPoints, DrawingType, Image, ImgBuffer, ImageFrame, ImageFrameX, ImageFrameY, ImageFrameWidth, ImageFrameHeight, RotationAngle, RotatedImageBox, RotatedImageBoxX, RotatedImageBoxY, RotatedImageBoxWidth, RotatedImageBoxHeight, ContentPackItemID, CalloutText, CalloutTextRect, CalloutTextRectX, CalloutTextRectY, CalloutTextRectWidth, CalloutTextRectHeight);
		}
	}
}


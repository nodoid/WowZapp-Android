// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Drawing;
using System.Collections.Generic;
using SQLite;
using System.Linq;

namespace WZCommon
{
	public class DrawingInfo
	{

		#region Constructors

		public DrawingInfo (int drawingID, List<PointF> pathPoints, BrushItem brush, WZColor lineColor)
		{
			this.DrawingID = drawingID;
			this.Brush = brush;
			this.LineColor = lineColor;
			this.PathPoints = pathPoints ?? new List<PointF>();

			this.DrawingType = DrawingLayerType.Drawing;

		}



		public DrawingInfo(int drawingID, byte[] image, RectangleF imageFrame)
		{

			this.DrawingID = drawingID;
			this.ImageBuffer = image;
			this.ImageFrame = imageFrame;

			this.DrawingType = DrawingLayerType.Image;

			this.PathPoints = new List<PointF>();

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
		public DrawingInfo(int drawingID, int contentPackItemID, DrawingLayerType drType, byte[] image, RectangleF imageFrame, WZColor textColor)
		{

			if (drType != DrawingLayerType.Callout &&
			    drType != DrawingLayerType.Comix &&
			    drType != DrawingLayerType.Stamp)
			{
				
				throw new ArgumentException("When initializing a DrawingInfo item with this constructor, drType must represent a content pack item!", "drType");
				
			}//end if

			this.DrawingID = drawingID;
			this.ContentPackItemID = contentPackItemID;
			this.DrawingType = drType;
			this.ImageBuffer = image;
			this.ImageFrame = imageFrame;
			this.LineColor = textColor;

			this.PathPoints = new List<PointF>();

		}//end ctor



		public DrawingInfo()
		{
			this.PathPoints = new List<PointF>();
		}//end default ctor

		#endregion Constructors




		#region Properties

		[PrimaryKey, AutoIncrement]
		public int DBID
		{
			get;
			private set;
		}//end int DBID




		public int DrawingID
		{
			get;
			set;
		}//end int DrawingID



		[Indexed]
		public int LayerDBID
		{
			get;
			set;
		}//end int LayerDBID



		#region BrushItem

		[Ignore]
		public BrushItem Brush
		{
			get;
			set;
		}//end BrushItem Brush



		[Indexed]
		public int BrushItemDBID
		{
			get;
			set;
		}//end int BrushItemID

		#endregion BrushItem



		#region LineColor

		[Ignore]
		public WZColor LineColor
		{
			get;
			private set;
		}//end CGColor LineColor



		public byte[] LineColorBuffer
		{
			get
			{
				if (null != this.LineColor)
				{
//					return AnimationUtils.ConvertRGBAColorToByteArray(this.LineColor.Components);
					return this.LineColor.ToByteArray();

				} else
				{
					return null;
				}//end if else

			} private set
			{

				byte[] buffer = value;
				if (null != buffer)
				{
					if (buffer.Length > 0)
					{
//						this.LineColor = AnimationUtils.ConvertByteArrayToRGBAColor(buffer);
						this.LineColor = WZColor.ConvertByteArrayToRGBAColor(buffer);
					} else
					{
						this.LineColor = null;
					}//end if else

				} else
				{
					this.LineColor = null;
				}//end if else

			}//end private set

		}//end byte[] LineColorBuffer

		#endregion LineColor



		[Ignore]
		public List<PointF> PathPoints
		{
			get;
			private set;
		}//end List<PointF> PathPoints



		public DrawingLayerType DrawingType
		{
			get;
			private set;
		}//end DrawingLayerType DrawingType



		#region Image

		public byte[] ImageBuffer
		{
			get;
			private set;
		}//end UIImage Image




//		public byte[] ImgBuffer
//		{
//			get
//			{
//				if (null != this.Image)
//				{
//					try
//					{
//						using (NSData imgData = this.Image.AsPNG())
//						{
//							return imgData.ToArray();
//						}//end using imgData
//
//					} catch
//					{
//						return new byte[0];
//					}//end try catch
//
//				} else
//				{
//					return new byte[0];
//				}//end if else
//
//			} private set
//			{
//
//				byte[] buffer = value;
//				if (null != buffer)
//				{
//					if (buffer.Length > 0)
//					{
//						try
//						{
//							using (NSData imgData = NSData.FromArray(buffer))
//							{
//								this.Image = UIImage.LoadFromData(imgData);
//							}//end using imgData
//						} catch
//						{
//							this.Image = null;
//						}//end try catch
//					} else
//					{
//						this.Image = null;
//					}//end if else
//
//				} else
//				{
//					this.Image = null;
//				}//end if else
//
//			}//end get set
//
//		}//end byte[] ImgBuffer

		#endregion Image




		#region ImageFrame

		[Ignore]
		public RectangleF ImageFrame
		{
			get;
			set;
		}//end RectangleF ImageFrame




		public float ImageFrameX
		{
			get
			{
				return this.ImageFrame.X;
			} private set
			{
				RectangleF imgFrame = this.ImageFrame;
				imgFrame.X = value;
				this.ImageFrame = imgFrame;
			}//end get set

		}//end float ImageFrameX



		public float ImageFrameY
		{
			get
			{
				return this.ImageFrame.Y;
			} private set
			{
				RectangleF imgFrame = this.ImageFrame;
				imgFrame.Y = value;
				this.ImageFrame = imgFrame;
			}//end get set

		}//end float ImageFrameY




		public float ImageFrameWidth
		{
			get
			{
				return this.ImageFrame.Width;
			} private set
			{
				RectangleF imgFrame = this.ImageFrame;
				imgFrame.Width = value;
				this.ImageFrame = imgFrame;
			}//end get private set

		}//end float ImageFrameWidth



		public float ImageFrameHeight
		{
			get
			{
				return this.ImageFrame.Height;
			} private set
			{
				RectangleF imgFrame = this.ImageFrame;
				imgFrame.Height = value;
				this.ImageFrame = imgFrame;
			}//end get private set

		}//end float ImageFrameHeight

		#endregion ImageFrame



		public double RotationAngle
		{
			get;
			set;
		}//end double ImageDegAngle




		#region RotatedImageBox

		[Ignore]
		public RectangleF RotatedImageBox
		{
			get;
			set;
		}//end double RotatedImageBox



		public float RotatedImageBoxX
		{
			get
			{
				return this.RotatedImageBox.X;
			} private set
			{
				RectangleF rotatedImgBox = this.RotatedImageBox;
				rotatedImgBox.X = value;
				this.RotatedImageBox = rotatedImgBox;
			}//end get set
			
		}//end float ImageFrameX
		
		
		
		public float RotatedImageBoxY
		{
			get
			{
				return this.RotatedImageBox.Y;
			} private set
			{
				RectangleF rotatedImgBox = this.RotatedImageBox;
				rotatedImgBox.Y = value;
				this.RotatedImageBox = rotatedImgBox;
			}//end get set
			
		}//end float ImageFrameY
		
		
		
		public float RotatedImageBoxWidth
		{
			get
			{
				return this.RotatedImageBox.Width;
			} private set
			{
				RectangleF rotatedImgBox = this.RotatedImageBox;
				rotatedImgBox.Width = value;
				this.RotatedImageBox = rotatedImgBox;
			}//end get private set
			
		}//end float ImageFrameWidth
		



		public float RotatedImageBoxHeight
		{
			get
			{
				return this.RotatedImageBox.Height;
			} private set
			{
				RectangleF rotatedImgBox = this.RotatedImageBox;
				rotatedImgBox.Height = value;
				this.RotatedImageBox = rotatedImgBox;
			}//end get private set
			
		}//end float ImageFrameHeight

		#endregion RotatedImageBox



		public int ContentPackItemID
		{
			get;
			private set;
		}//end int ContentPackItemID




		[MaxLength(200)]
		public string CalloutText
		{
			get;
			set;
		}//end string CalloutText




		#region CalloutTextRect

		[Ignore]
		public RectangleF CalloutTextRect
		{
			get;
			set;
		}//end RectangleF CalloutTextRect



		public float CalloutTextRectX
		{
			get
			{
				return this.CalloutTextRect.X;
			} private set
			{
				RectangleF calloutRect = this.CalloutTextRect;
				calloutRect.X = value;
				this.CalloutTextRect = calloutRect;

			}//end get private set
		}//end float CalloutTextRectX




		public float CalloutTextRectY
		{
			get
			{
				return this.CalloutTextRect.Y;
			} private set
			{
				RectangleF calloutRect = this.CalloutTextRect;
				calloutRect.Y = value;
				this.CalloutTextRect = calloutRect;

			}//end get private set

		}//end float CalloutTextRectY




		public float CalloutTextRectWidth
		{
			get
			{
				return this.CalloutTextRect.Width;
			} private set
			{
				RectangleF calloutRect = this.CalloutTextRect;
				calloutRect.Width = value;
				this.CalloutTextRect = calloutRect;
			}//end get private set
		}//end float CalloutTextRectWidth




		public float CalloutTextRectHeight
		{
			get
			{
				return this.CalloutTextRect.Height;
			} private set
			{
				RectangleF calloutRect = this.CalloutTextRect;
				calloutRect.Height = value;
				this.CalloutTextRect = calloutRect;
			}//end get private set
		}//end float CalloutTextRectHeight

		#endregion CalloutTextRect





		#endregion Properties




		#region Public methods

		public void SetImage(byte[] image)
		{

			this.ImageBuffer = image;

		}//end void SetImage




		public List<PathPointDB> GetPathPointsForDB()
		{

			List<PathPointDB> pathPoints = new List<PathPointDB>();
			int sortOrder = 0;
			foreach (PointF eachPoint in this.PathPoints)
			{

				PathPointDB pathPoint = new PathPointDB(this.DBID, sortOrder++, eachPoint.X, eachPoint.Y);
				pathPoints.Add(pathPoint);

			}//end foreach

			return pathPoints;

		}//end List<PathPointDB> GetPathPointsForDB





		public void SetPathPointsFromDB(List<PathPointDB> dbPathPoints)
		{

			foreach (PathPointDB eachPathPoint in dbPathPoints)
			{
				this.PathPoints.Add(new PointF(eachPathPoint.X, eachPathPoint.Y));
			}//end foreach

		}//end void SetPathPointsFromDB

		#endregion Public methods




		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[DrawingInfo: DBID={0}, DrawingID={1}, LayerDBID={2}, Brush={3}, BrushItemDBID={4}, LineColor={5}, LineColorBuffer={6}, PathPoints={7}, DrawingType={8}, Image={9}, ImageFrame={10}, ImageFrameX={11}, ImageFrameY={12}, ImageFrameWidth={13}, ImageFrameHeight={14}, RotationAngle={15}, RotatedImageBox={16}, RotatedImageBoxX={17}, RotatedImageBoxY={18}, RotatedImageBoxWidth={19}, RotatedImageBoxHeight={20}, ContentPackItemID={21}, CalloutText={22}, CalloutTextRect={23}, CalloutTextRectX={24}, CalloutTextRectY={25}, CalloutTextRectWidth={26}, CalloutTextRectHeight={27}]", DBID, DrawingID, LayerDBID, Brush, BrushItemDBID, LineColor, LineColorBuffer, PathPoints, DrawingType, ImageBuffer, ImageFrame, ImageFrameX, ImageFrameY, ImageFrameWidth, ImageFrameHeight, RotationAngle, RotatedImageBox, RotatedImageBoxX, RotatedImageBoxY, RotatedImageBoxWidth, RotatedImageBoxHeight, ContentPackItemID, CalloutText, CalloutTextRect, CalloutTextRectX, CalloutTextRectY, CalloutTextRectWidth, CalloutTextRectHeight);
		}

		#endregion Overrides
	}
}


// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using SQLite;
using LOLMessageDelivery.Classes.LOLAnimation;

namespace WZCommon
{
	public class LayerInfo
	{

		#region Constructors

		public LayerInfo (int id, SizeF canvasSize, Dictionary<int, TransitionInfo> transitions)
		{

			this.ID = id;
			this.CanvasSize = canvasSize;
			this.Transitions = transitions ?? new Dictionary<int, TransitionInfo>();
			this.DrawingItems = new Dictionary<int, DrawingInfo>();

		}



		public LayerInfo()
		{
			this.DrawingItems = new Dictionary<int, DrawingInfo>();
			this.Transitions = new Dictionary<int, TransitionInfo>();
		}//end ctor

		#endregion Constructors




		#region Properties

		public int ID
		{
			get;
			private set;
		}//end int ID



		[PrimaryKey, AutoIncrement]
		public int DBID
		{
			get;
			private set;
		}//end int DBID




		[Indexed]
		public int FrameDBID
		{
			get;
			set;
		}//end int FrameDBID



		[Ignore]
		public Dictionary<int, DrawingInfo> DrawingItems
		{
			get;
			private set;
		}//end Dictionary<int, DrawingInfo> DrawingItems

	
		#region CanvasSize

		[Ignore]
		public SizeF CanvasSize
		{
			get;
			private set;
		}//end SizeF CanvasSize





		public float CanvasSizeWidth
		{
			get
			{
				return this.CanvasSize.Width;
			} private set
			{
				SizeF canvasSize = this.CanvasSize;
				canvasSize.Width = value;
				this.CanvasSize = canvasSize;
			}//end get private set
		}//end float CanvasSizeWidth






		public float CanvasSizeHeight
		{
			get
			{
				return this.CanvasSize.Height;
			} private set
			{
				SizeF canvasSize = this.CanvasSize;
				canvasSize.Height = value;
				this.CanvasSize = canvasSize;
			}//end get private set
		}//end float CanvasSizeHeight

		#endregion CanvasSize



		[Ignore]
		public Dictionary<int, TransitionInfo> Transitions
		{
			get;
			private set;
		}//end List<TransitionInfo> Transitions




		public bool IsCanvasActive
		{
			get;
			set;
		}//end bool IsCanvasActive




		public bool HasDrawingItems
		{
			get
			{
				return this.DrawingItems.Count > 0;
			}//end get

		}//end bool HasDrawingItems

		#endregion Properties



		#region Public methods

		public void AddOrReplaceDrawingInfo(DrawingInfo drInfo)
		{

			this.DrawingItems[drInfo.DrawingID] = drInfo;

		}//end void AddDrawingInfo




		public RectangleF GetBoundingBox()
		{

			List<PointF> allPoints = new List<PointF>();
			foreach (DrawingInfo eachDrawingInfo in this.DrawingItems.Values)
			{

				if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing)
				{

					allPoints.AddRange(eachDrawingInfo.PathPoints);

				} else if (eachDrawingInfo.DrawingType == DrawingLayerType.Image ||
				           eachDrawingInfo.DrawingType == DrawingLayerType.Comix ||
				           eachDrawingInfo.DrawingType == DrawingLayerType.Stamp ||
				           eachDrawingInfo.DrawingType == DrawingLayerType.Callout)
				{

					RectangleF imgFrame = eachDrawingInfo.RotationAngle == 0 ? eachDrawingInfo.ImageFrame : eachDrawingInfo.RotatedImageBox;
					allPoints.Add(imgFrame.Location);
					allPoints.Add(new PointF(imgFrame.Right, imgFrame.Y));
					allPoints.Add(new PointF(imgFrame.Right, imgFrame.Bottom));
					allPoints.Add(new PointF(imgFrame.X, imgFrame.Bottom));

				}//end if else

			}//end foreach

			if (allPoints.Count == 0)
			{
				return RectangleF.Empty;
			} else
			{

				PointF minXPoint = 
					allPoints
						.MinItemEx<PointF, float>(s => s.X);
				PointF maxXPoint = 
					allPoints
						.MaxItem<PointF, float>(s => s.X);
				PointF minYPoint = 
					allPoints
						.MinItemEx<PointF, float>(s => s.Y);
				PointF maxYPoint = 
					allPoints
						.MaxItem<PointF, float>(s => s.Y);

				return new RectangleF(minXPoint.X,
				                      minYPoint.Y,
				                      maxXPoint.X - minXPoint.X,
				                      maxYPoint.Y - minYPoint.Y);

			}//end if else

		}//end RectangleF GetBoundingBox





		public RectangleF GetRotatedImageEnclosingBox()
		{

			DrawingInfo drItem = this.DrawingItems.Values
				.Where(s => s.DrawingType != DrawingLayerType.Drawing)
					.FirstOrDefault();

			if (null != drItem)
			{

				List<PointF> corners = drItem.ImageFrame.GetCorners();
				PointF center = drItem.ImageFrame.GetCenterOfRect();
				List<PointF> rotatedCorners = corners.RotatePoints(drItem.RotationAngle, new PointF(center.X, center.Y));

				float x = 0f;
				float y = 0f;
				float width = 0f;
				float height = 0f;
				if (rotatedCorners[0].X > rotatedCorners[3].X)
				{
					x = rotatedCorners[0].X;
				} else
				{
					x = rotatedCorners[3].X;
				}//end if else


				if (rotatedCorners[0].Y > rotatedCorners[1].Y)
				{
					y = rotatedCorners[0].X;
				} else
				{
					y = rotatedCorners[1].Y;
				}//end if else

				if (rotatedCorners[1].X > rotatedCorners[2].X)
				{
					width = rotatedCorners[2].X - x;
				} else
				{
					width = rotatedCorners[1].X - x;
				}//end if else

				if (rotatedCorners[3].Y > rotatedCorners[2].Y)
				{
					height = rotatedCorners[2].Y - y;
				} else
				{
					height = rotatedCorners[3].Y - y;
				}//end if else

				return new RectangleF(x, y, width, height);

			} else
			{

				throw new InvalidOperationException("Layer is a path drawing!");

			}//end if else

		}//end RectangleF GetRotatedImageEnclosingBox





		public void SetLayerID(int layerID)
		{

			this.ID = layerID;

		}//end void SetLayerID





		public List<PointF> GetAllPointsInPercentage()
		{

			List<PointF> points = new List<PointF>();
			foreach (DrawingInfo eachDrawingInfo in this.DrawingItems.Values)
			{

				foreach (PointF eachPoint in eachDrawingInfo.PathPoints)
				{
					points.Add(AnimationUtils.ConvertToPercentagePoint(eachPoint, this.CanvasSize));
				}//end foreach

			}//end foreach

			return points;

		}//end List<PointF> GetAllPointsInPercentage

		#endregion Public methods



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[LayerInfo: ID={0}, DrawingItems={1}, CanvasSize={2}, Transitions={3}, IsCanvasActive={4}, HasDrawingItems={5}]", ID, DrawingItems, CanvasSize, Transitions, IsCanvasActive, HasDrawingItems);
		}

		#endregion Overrides



		#region Static members

		public static LayerInfo Clone(LayerInfo layerInfo, bool withoutTransitions)
		{

			Dictionary<int, TransitionInfo> transitions = new Dictionary<int, TransitionInfo>();

			if (!withoutTransitions)
			{

				foreach (KeyValuePair<int, TransitionInfo> eachTransition in layerInfo.Transitions)
				{

					TransitionInfo trObj = new TransitionInfo(eachTransition.Value.ID);
					trObj.SetEndFadeValue(eachTransition.Value.FadeOpacity);
					trObj.SetEndLocation(eachTransition.Value.EndLocation);
					trObj.SetEndScaleValue(eachTransition.Value.EndSize);
					trObj.SetRotationValue(eachTransition.Value.RotationAngle);
//					trObj.EffectType = eachTransition.Value.EffectType;
					foreach (KeyValuePair<AnimationTypesTransitionEffectType, TransitionEffectSettings> eachItem in eachTransition.Value.Settings)
					{
						trObj.Settings[eachItem.Key] = eachItem.Value;
					}//end foreach

					transitions.Add(trObj.ID, trObj);

				}//end foreach

			}//end if

			LayerInfo toReturn = new LayerInfo(layerInfo.ID, layerInfo.CanvasSize, transitions);

			foreach (KeyValuePair<int, DrawingInfo> eachDrawingItem in layerInfo.DrawingItems)
			{

				DrawingInfo drawingItem = null;
				if (eachDrawingItem.Value.DrawingType == DrawingLayerType.Drawing)
				{

					drawingItem = 
						new DrawingInfo(eachDrawingItem.Key, 
						                new List<PointF>(eachDrawingItem.Value.PathPoints), 
						                eachDrawingItem.Value.Brush, eachDrawingItem.Value.LineColor);


				} else
				{

					drawingItem = 
						new DrawingInfo(eachDrawingItem.Key, eachDrawingItem.Value.ImageBuffer, eachDrawingItem.Value.ImageFrame);

				}//end if else

				switch (eachDrawingItem.Value.DrawingType)
				{

				case DrawingLayerType.Drawing:

					drawingItem = 
						new DrawingInfo(eachDrawingItem.Key,
						                new List<PointF>(eachDrawingItem.Value.PathPoints),
						                eachDrawingItem.Value.Brush, eachDrawingItem.Value.LineColor);

					break;

				case DrawingLayerType.Image:

					drawingItem = 
						new DrawingInfo(eachDrawingItem.Key, eachDrawingItem.Value.ImageBuffer, eachDrawingItem.Value.ImageFrame);
					drawingItem.RotationAngle = eachDrawingItem.Value.RotationAngle;
					drawingItem.RotatedImageBox = eachDrawingItem.Value.RotatedImageBox;

					break;

				case DrawingLayerType.Callout:
				case DrawingLayerType.Comix:
				case DrawingLayerType.Stamp:

					drawingItem = 
						new DrawingInfo(eachDrawingItem.Key, 
						                eachDrawingItem.Value.ContentPackItemID, 
						                eachDrawingItem.Value.DrawingType, 
						                eachDrawingItem.Value.ImageBuffer, 
						                eachDrawingItem.Value.ImageFrame, 
						                eachDrawingItem.Value.LineColor);
					drawingItem.RotationAngle = eachDrawingItem.Value.RotationAngle;
					drawingItem.RotatedImageBox = eachDrawingItem.Value.RotatedImageBox;
					drawingItem.CalloutText = eachDrawingItem.Value.CalloutText;
					drawingItem.CalloutTextRect = eachDrawingItem.Value.CalloutTextRect;

					break;

				}//end switch

				toReturn.AddOrReplaceDrawingInfo(drawingItem);

			}//end foreach

			return toReturn;

		}//end static LayerInfo Clone

		#endregion Static members
	}
}


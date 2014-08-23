using System;
using System.Collections.Generic;
using System.Drawing;

namespace LOLMessageDelivery.Classes.LOLAnimation
{
	[Serializable]
	public partial class ScreenObject
	{
		// Image and Callout ScreenObject Constructor :
		public ScreenObject (PointF InitialPosition, PointF Size, byte[] ObjectByteData, float AppearsAt, int Layer)
		{
			this.ObjectType = ScreenObjectTypes.Image;
			this.InitialPosition = InitialPosition;
			this.Size = Size;
			this.ObjectByteData = ObjectByteData;
			this.AppearsAt = AppearsAt;
			this.Layer = Layer;
			this.Transitions = new List<Transition> ();
		}
		
		// Freehand Path ScreenObject Constructor : 
		public ScreenObject (List<PointF> DrawnPath, float DrawnPathStroke, string DrawnPathColor, float AppearsAt, int Layer)
		{
			this.ObjectType = ScreenObjectTypes.FreehandPath;
			this.DrawnPath = DrawnPath;
			this.DrawnPathStroke = DrawnPathStroke;
			this.DrawnPathColor = DrawnPathColor;
			this.AppearsAt = AppearsAt;
			this.Layer = Layer;
			this.Transitions = new List<Transition> ();
		}
		
		// Freehand Drawing ScreenObject Constructor : 
		public ScreenObject (List<List<PointF>> DrawnPaths, List<float> DrawnPathStrokes, List<string> DrawnPathColors, float AppearsAt, int Layer)
		{
			this.ObjectType = ScreenObjectTypes.FreehandDrawing;
			this.DrawnPaths = DrawnPaths;
			this.DrawnPathStrokes = DrawnPathStrokes;
			this.DrawnPathColors = DrawnPathColors;
			this.AppearsAt = AppearsAt;
			this.Layer = Layer;
			this.Transitions = new List<Transition> ();
		}
		
		// Brush Path ScreenObject Constructor :
		public ScreenObject (List<PointF> DrawnPath, byte[] DrawnPathBrushByteData, string DrawnPathColor, float AppearsAt, int Layer)
		{
			this.ObjectType = ScreenObjectTypes.BrushPath;
			this.DrawnPath = DrawnPath;
			this.DrawnPathBrushByteData = DrawnPathBrushByteData;
			this.DrawnPathColor = DrawnPathColor;
			this.AppearsAt = AppearsAt;
			this.Layer = Layer;
			this.Transitions = new List<Transition> ();
		}
		
		// Brush Drawing ScreenObject Constructor :
		public ScreenObject (List<List<PointF>> DrawnPaths, List<byte[]> DrawnPathsBrushByteData, List<string> DrawnPathColors, float AppearsAt, int Layer)
		{
			this.ObjectType = ScreenObjectTypes.BrushDrawing;
			this.DrawnPaths = DrawnPaths;
			this.DrawnPathsBrushByteData = DrawnPathsBrushByteData;
			this.DrawnPathColors = DrawnPathColors;
			this.AppearsAt = AppearsAt;
			this.Layer = Layer;
			this.Transitions = new List<Transition> ();
		}
	}
	
	public static class ScreenObjectTypes
	{
		public const int FreehandPath = 1;
		public const int BrushPath = 2;
		public const int Image = 3;
		public const int FreehandDrawing = 4;
		public const int BrushDrawing = 5;
		public const int Callout = 6;
	}
}
using System;
using LOLMessageDelivery;
using LOLMessageDelivery.Classes.LOLAnimation;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using PreserveProps = Android.Runtime.PreserveAttribute;

namespace LOLMessageDelivery.Classes.LOLAnimation
{
	[PreserveProps(AllMembers=true)]
	public partial class Animation
	{
		/*public Animation ()
		{
		}*/
		
		/*[PrimaryKey, AutoIncrement]
		public int ID {
			get;
			private set;
		}
		
		[Ignore]
		public Guid CreatedBy {
			get {
				return this.CreatedBy;
			}
			set {
				this.CreatedBy = value;
			}
		}
		
		[Ignore]
		public DateTime CreatedDate {
			get {
				return this.CreatedDate;
			}
			set {
				this.CreatedDate = value;
			}
		}
		
		[Ignore]
		public float Duration {
			get {
				return this.Duration;
			}
			set {
				this.Duration = value;
			}
		}*/
		
		[Ignore]
		public Guid AnimationID {
			get {
				return this.AnimationID;
			}
			set {
				this.AnimationID = value;
			}
		}
		
		/*[Ignore]
		public List<LOLMessageDelivery.Classes.LOLAnimation.ScreenObject> ScreenObjects {
			get {
				return this.ScreenObjects;
			}
			set {
				this.ScreenObjects = value;
			}
		}*/
	}
	
	/*public partial class ScreenObject
	{
		public ScreenObject ()
		{
		}
	
		public float AppearsAt {
			get {
				return this.AppearsAt;
			}
			set {
				this.AppearsAt = value;
			}
		}
		
		public float DisappearsAt {
			get {
				return this.DisappearsAt;
			}
			set {
				this.DisappearsAt = value;
			}
		}
		
		public int DisplayOrder {
			get {
				return this.DisplayOrder;
			}
			set {
				this.DisplayOrder = value;
			}
		}
		
		public System.Collections.Generic.List<System.Drawing.PointF> DrawnPath {
			get {
				return this.DrawnPath;
			}
			set {
				this.DrawnPath = value;
			}
		}
		
		public byte[] DrawnPathBrushByteData {
			get {
				return this.DrawnPathBrushByteData;
			}
			set {
				this.DrawnPathBrushByteData = value;
			}
		}
		
		public string DrawnPathColor {
			get {
				return this.DrawnPathColor;
			}
			set {
				this.DrawnPathColor = value;
			}
		}
		
		public System.Collections.Generic.List<string> DrawnPathColors {
			get {
				return this.DrawnPathColors;
			}
			set {
				this.DrawnPathColors = value;
			}
		}
		
		public float DrawnPathStroke {
			get {
				return this.DrawnPathStroke;
			}
			set {
				this.DrawnPathStroke = value;
			}
		}
		
		public System.Collections.Generic.List<float> DrawnPathStrokes {
			get {
				return this.DrawnPathStrokes;
			}
			set {
				this.DrawnPathStrokes = value;
			}
		}
		
		public System.Collections.Generic.List<System.Collections.Generic.List<System.Drawing.PointF>> DrawnPaths {
			get {
				return this.DrawnPaths;
			}
			set {
				this.DrawnPaths = value;
			}
		}
		
		public System.Collections.Generic.List<byte[]> DrawnPathsBrushByteData {
			get {
				return this.DrawnPathsBrushByteData;
			}
			set {
				this.DrawnPathsBrushByteData = value;
			}
		}
		
		public System.Guid ID {
			get {
				return this.ID;
			}
			set {
				this.ID = value;
			}
		}
		
		public System.Drawing.PointF InitialPosition {
			get {
				return this.InitialPosition;
			}
			set {
				this.InitialPosition = value;
			}
		}
		
		public float InitialRotation {
			get {
				return this.InitialRotation;
			}
			set {
				this.InitialRotation = value;
			}
		}
		
		public int Layer {
			get {
				return this.Layer;
			}
			set {
				this.Layer = value;
			}
		}
		
		public byte[] ObjectByteData {
			get {
				return this.ObjectByteData;
			}
			set {
				this.ObjectByteData = value;
			}
		}
		
		public int ObjectType {
			get {
				return this.ObjectType;
			}
			set {
				this.ObjectType = value;
			}
		}
		
		public System.Guid OwnerAnimationID {
			get {
				return this.OwnerAnimationID;
			}
			set {
				this.OwnerAnimationID = value;
			}
		}
		
		public System.Drawing.PointF Size {
			get {
				return this.Size;
			}
			set {
				this.Size = value;
			}
		}
		
		public System.Collections.Generic.List<float> TextObjectFontSizes {
			get {
				return this.TextObjectFontSizes;
			}
			set {
				this.TextObjectFontSizes = value;
			}
		}
		
		public System.Collections.Generic.List<System.Drawing.PointF> TextObjectPositions {
			get {
				return this.TextObjectPositions;
			}
			set {
				this.TextObjectPositions = value;
			}
		}
		
		public System.Collections.Generic.List<string> TextObjectsContent {
			get {
				return this.TextObjectsContent;
			}
			set {
				this.TextObjectsContent = value;
			}
		}
		
		public System.Collections.Generic.List<LOLMessageDelivery.Classes.LOLAnimation.Transition> Transitions {
			get {
				return this.Transitions;
			}
			set {
				this.Transitions = value;
			}
		}
	}
	
	public partial class Transition
	{
		public Transition ()
		{
		}
	
		public int DisplayOrder {
			get {
				return this.DisplayOrder;
			}
			set {
				this.DisplayOrder = value;
			}
		}
		
		public float Duration {
			get {
				return this.Duration;
			}
			set {
				this.Duration = value;
			}
		}
		
		public float EndOpacity {
			get {
				return this.EndOpacity;
			}
			set {
				this.EndOpacity = value;
			}
		}
		
		public System.Drawing.PointF EndPosition {
			get {
				return this.EndPosition;
			}
			set {
				this.EndPosition = value;
			}
		}
		
		public System.Drawing.PointF EndSize {
			get {
				return this.EndSize;
			}
			set {
				this.EndSize = value;
			}
		}
		
		public System.Guid ID {
			get {
				return this.ID;
			}
			set {
				this.ID = value;
			}
		}
		
		public System.Guid OwnerScreenObjectID {
			get {
				return this.OwnerScreenObjectID;
			}
			set {
				this.OwnerScreenObjectID = value;
			}
		}
		
		public float RotationsNum {
			get {
				return this.RotationsNum;
			}
			set {
				this.RotationsNum = value;
			}
		}
		
		public float StartOpacity {
			get {
				return this.StartOpacity;
			}
			set {
				this.StartOpacity = value;
			}
		}
		
		public System.Drawing.PointF StartPosition {
			get {
				return this.StartPosition;
			}
			set {
				this.StartPosition = value;
			}
		}
		
		public System.Drawing.PointF StartSize {
			get {
				return this.StartSize;
			}
			set {
				this.StartSize = value;
			}
		}
		
		public float StartsAt {
			get {
				return this.StartsAt;
			}
			set {
				this.StartsAt = value;
			}
		}
		
		public int TransitionType {
			get {
				return this.TransitionType;
			}
			set {
				this.TransitionType = value;
			}
		}
	}*/
}


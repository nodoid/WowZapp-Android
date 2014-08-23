using System;
using System.Drawing;

namespace LOLMessageDelivery.Classes.LOLAnimation
{
	public partial class Transition
	{
		// Move or Scale Transition Constructor :
		public Transition (float StartsAt, float Duration, int transitionType, PointF StartPoint, PointF EndPoint)
		{
			this.StartsAt = StartsAt;
			this.Duration = Duration;
			this.TransitionType = transitionType;
			switch (TransitionType) {
			case TransitionTypes.Move:
				this.StartPosition = StartPoint;
				this.EndPosition = EndPoint;
				break;
				
			case TransitionTypes.Scale:
				this.StartSize = StartPoint;
				this.EndSize = EndPoint;
				break;
			}
		}
		
		// Rotate Transition Constructor :
		public Transition (float StartsAt, float Duration, float RotationsNum)
		{
			this.StartsAt = StartsAt;
			this.Duration = Duration;
			this.TransitionType = TransitionTypes.Rotate;
			this.RotationsNum = RotationsNum;
		}
		
		// FadeIn or FadeOut Transition Constructor :
		public Transition (float StartsAt, float Duration, float StartOpacity, float EndOpacity)
		{
			this.StartsAt = StartsAt;
			this.Duration = Duration;
			this.TransitionType = TransitionTypes.Fade;
			this.StartOpacity = StartOpacity;
			this.EndOpacity = EndOpacity;
		}
	}
	
	public static class TransitionTypes
	{
		public const int Move = 1;
		public const int Scale = 2;
		public const int Rotate = 3;
		public const int Fade = 4;
	}
}

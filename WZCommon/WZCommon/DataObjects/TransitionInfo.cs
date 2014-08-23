// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LOLMessageDelivery.Classes.LOLAnimation;
using SQLite;

namespace WZCommon
{
	public class TransitionInfo
	{

		#region Constructors

		public TransitionInfo (int id)
		{
			this.Settings = new Dictionary<AnimationTypesTransitionEffectType, TransitionEffectSettings> ();
			this.ID = id;

		}



		public TransitionInfo ()
		{
			this.Settings = new Dictionary<AnimationTypesTransitionEffectType, TransitionEffectSettings> ();
		}//end ctor

		#endregion Constructors



		#region Constants

		// Default transition duration, in seconds
		public const double DefaultDuration = 0.5d;
		public const double DefaultMaxDuration = 1d;

		#endregion Constants




		#region Properties

		public int ID {
			get;
			private set;
		}//end int ID




		[PrimaryKey, AutoIncrement]
		public int DBID {
			get;
			private set;
		}//end int DBID




		[Indexed]
		public int LayerDBID {
			get;
			set;
		}//end int LayerDBID





		[Ignore]
		public AnimationTypesTransitionEffectType EffectType {
			get {
				AnimationTypesTransitionEffectType efType = AnimationTypesTransitionEffectType.None;

				foreach (AnimationTypesTransitionEffectType eachEfType in this.Settings.Keys) {
					if (efType == AnimationTypesTransitionEffectType.None) {
						efType = eachEfType;
					} else {
						efType |= eachEfType;
					}//end if else
				}//end foreach

				return efType;
			}//end get

		}//end TransitionEffectType EffectType




		public double FadeOpacity {
			get;
			private set;
		}//end double FadeOpacity





		public double RotationAngle {
			get;
			private set;
		}//end double RotationAngle





		#region EndSize

		[Ignore]
		public SizeF EndSize {
			get;
			private set;
		}//end SizeF EndSize




		public float EndSizeWidth {
			get {
				return this.EndSize.Width;
			}
			private set {
				SizeF endSize = this.EndSize;
				endSize.Width = value;
				this.EndSize = endSize;
			}//end get private set

		}//end float EndSizeWidth




		public float EndSizeHeight {
			get {
				return this.EndSize.Height;
			}
			private set {
				SizeF endSize = this.EndSize;
				endSize.Height = value;
				this.EndSize = endSize;
			}//end get private set

		}//end float EndSizeHeight


		#endregion EndSize





		#region EndSizeFixedPoint

		[Ignore]
		public PointF EndSizeFixedPoint {
			get;
			private set;
		}//end PointF EndSizeFixedPoint




		public float EndSizeFixedPointX {
			get {
				return this.EndSizeFixedPoint.X;
			}
			private set {
				PointF fixedPoint = this.EndSizeFixedPoint;
				fixedPoint.X = value;
				this.EndSizeFixedPoint = fixedPoint;
			}//end get private set

		}//end float EndSizeFixedPointX




		public float EndSizeFixedPointY {
			get {
				return this.EndSizeFixedPoint.Y;
			}
			private set {
				PointF fixedPoint = this.EndSizeFixedPoint;
				fixedPoint.Y = value;
				this.EndSizeFixedPoint = fixedPoint;
			}//end get private set
		}//end float EndSizeFixedPointY

		#endregion EndSizeFixedPoint




		#region EndLocation

		[Ignore]
		public PointF EndLocation {
			get;
			private set;
		}//end PointF EndLocation



		public float EndLocationX {
			get {
				return this.EndLocation.X;
			}
			private set {
				PointF endLocation = this.EndLocation;
				endLocation.X = value;
				this.EndLocation = endLocation;
			}//end get private set

		}//end float EndLocationX



		public float EndLocationY {
			get {
				return this.EndLocation.Y;
			}
			private set {
				PointF endLocation = this.EndLocation;
				endLocation.Y = value;
				this.EndLocation = endLocation;
			}//end get private set
		}//end float EndLocationY

		#endregion EndLocation





		public double Duration 
		{
			get 
			{

				if (this.Settings.Count == 0) 
				{
					return 0d;
				} else {
					return this.Settings.Values.MaxItem<TransitionEffectSettings, double> (s => s.Duration).Duration;
				}//end if else

			}//end get set

		}//end double Duration




		[Ignore]
		public Dictionary<AnimationTypesTransitionEffectType, TransitionEffectSettings> Settings {
			get;
			private set;
		}//end Dictionaru<TransitionEffectType, TransitionEffectSettings> Settings


		#endregion Properties




		#region Public methods

		public void SetEndFadeValue (double fadeOpacity)
		{

			this.FadeOpacity = fadeOpacity;

		}//end void SetEndFadeValue




		public void SetRotationValue (double angle)
		{

			this.RotationAngle = angle;

		}//end void SetRotationValue



		public void SetEndScaleValue (SizeF endSize)
		{

			this.EndSize = endSize;

		}//end void SetEndScaleValue



		public void SetEndLocation (PointF endLocation)
		{

			this.EndLocation = endLocation;

		}//end void SetEndLocation

		#endregion Public methods



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[TransitionInfo: ID={0}, EffectType={1}, FadeOpacity={2}, RotationAngle={3}, EndSize={4}, EndSizeFixedPoint={5}, EndLocation={6}, Duration={7}]", ID, EffectType, FadeOpacity, RotationAngle, EndSize, EndSizeFixedPoint, EndLocation, Duration);
		}

		#endregion Overrides
	}


}


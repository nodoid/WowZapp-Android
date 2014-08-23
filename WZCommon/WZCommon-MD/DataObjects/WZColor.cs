// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;

namespace WZCommon
{
	public class WZColor
	{

		#region Constructors

		public WZColor (byte red, byte green, byte blue, byte alpha)
		{

			this.Red = red;
			this.Green = green;
			this.Blue = blue;
			this.Alpha = alpha;

		}

        public WZColor (byte[] colorArray)
        {
            this.Red = colorArray [0];
            this.Green = colorArray [1];
            this.Blue = colorArray [2];
            this.Alpha = colorArray [3];
        }

		#endregion Constructors



		#region Properties

		public byte Red
		{
			get;
			private set;
		}//end byte Red



		public byte Green
		{
			get;
			private set;
		}//end byte Green



		public byte Blue
		{
			get;
			private set;
		}//end byte Blue



		public byte Alpha
		{
			get;
			private set;
		}//end byte Alpha

		#endregion Properties



		#region Public methods

		public byte[] ToByteArray()
		{

			return new byte[] { this.Red, this.Green, this.Blue, this.Alpha };

		}//end byte[] ToByteArray

		#endregion Public methods





		#region Static members

		public static WZColor ConvertByteArrayToRGBAColor (byte[] colorBuffer)
		{

			if (colorBuffer.Length == 2) 
			{

				return new WZColor (colorBuffer [0],
				                   colorBuffer [1], 0, 0);

			} else if (colorBuffer.Length == 3) 
			{

				return new WZColor (colorBuffer [0],
				                   colorBuffer [1],
				                   colorBuffer [2], 0);

			} else if (colorBuffer.Length == 4) 
			{

				return new WZColor (colorBuffer [0],
				                   colorBuffer [1],
				                   colorBuffer [2],
				                   colorBuffer [3]);

			} else 
			{

				throw new InvalidOperationException ("Cannot create WZColor object.");

			}//end if else if

		}//end static CGColor ConvertByteArrayToRGBAColor

		#endregion Static members





		#region Overrides

		public override int GetHashCode ()
		{
			return this.Red.GetHashCode() ^ this.Green.GetHashCode() ^ this.Blue.GetHashCode() ^ this.Alpha.GetHashCode();
		}



		public override bool Equals (object obj)
		{

			WZColor otherObj = obj as WZColor;
			if (null == (object)otherObj)
			{

				return false;

			} else
			{

				return this.Red.Equals(otherObj.Red) &&
					this.Green.Equals(otherObj.Green) &&
						this.Blue.Equals(otherObj.Blue) &&
						this.Alpha.Equals(otherObj.Alpha);

			}//end if else
		}



		public static bool operator == (WZColor first, WZColor second)
		{

			if (object.ReferenceEquals(first, second))
			{
				return true;
			}//end if

			if ((object)first == null || (object)second == null)
			{
				return false;
			}//end if

			return first.Equals(second);

		}//end static bool operator == 




		public static bool operator != (WZColor first, WZColor second)
		{

			return !(first == second);

		}//end static bool operator !=

		#endregion Overrides


	}
}


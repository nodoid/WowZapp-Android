// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;

namespace WZCommon
{
	public struct PixelRGB
	{

		#region Constructors

		public PixelRGB(byte red, byte green, byte blue, byte alpha) : this()
		{

			this.Red = red;
			this.Green = green;
			this.Blue = blue;
			this.Alpha = alpha;

		}//end ctor

		#endregion Constructors




		#region Properties

		public byte Red
		{
			get;
			set;
		}//end byte Red



		public byte Green
		{
			get;
			set;
		}//end byte Green



		public byte Blue
		{
			get;
			set;
		}//end byte Blue



		public byte Alpha
		{
			get;
			set;
		}//end byte Alpha

		#endregion Properties



	}
}


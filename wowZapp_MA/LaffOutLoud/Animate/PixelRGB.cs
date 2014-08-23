namespace wowZapp.Animate
{
	public struct PixelRGB
	{
		public PixelRGB (byte red, byte green, byte blue, byte alpha) : this()
		{
			this.Red = red;
			this.Green = green;
			this.Blue = blue;
			this.Alpha = alpha;	
		}//end ctor
		
		public byte Red {
			get;
			set;
		}//end byte Red
	
		public byte Green {
			get;
			set;
		}//end byte Green
	
		public byte Blue {
			get;
			set;
		}//end byte Blue
	
		public byte Alpha {
			get;
			set;
		}//end byte Alpha	
	}
}


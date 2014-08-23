using System;
using System.Collections.Generic;
using System.Drawing;
using SQLite;
using wowZapp.Animate;

using Android.Content;
using Android.Widget;
using Android.Graphics;
using Android.App;
using Android.OS;

using PreserveProps = Android.Runtime.PreserveAttribute;

namespace LOLApp_Common
{
	[PreserveProps(AllMembers=true)]
	public class BrushItem
	{
		private Context context;
		
		public BrushItem (float thickness, BrushType brushType, Android.Graphics.Color brushColor, Context c)
		{
			this.Thickness = thickness;
			this.BrushType = brushType;
			this.BrushColor = brushColor;
			context = c;
			
			if (this.BrushType == BrushType.Spray) {
				
				this.IsSprayBrushActive = true;
				
			}//end if
			
			/*this.inactiveColor = new Android.Graphics.Color (DrawingCanvasViewEx.InactiveColorValues [0],
			                                 DrawingCanvasViewEx.InactiveColorValues [1],
			                                 DrawingCanvasViewEx.InactiveColorValues [2],
			                                 DrawingCanvasViewEx.InactiveColorValues [3]);*/
			
		}

		public BrushItem ()
		{
			
		}//end BrushItem

		private Android.Graphics.Color inactiveColor;
		private ImageView pBrushImage;
		
		[PrimaryKey, AutoIncrement]
		public int DBID {
			get;
			private set;
		}//end int DBID

		public float Thickness {
			get;
			private set;
		}//end float Thickness

		public BrushType BrushType {
			get;
			private set;
		}//end BrushType BrushType

		[Ignore]
		public ImageView BrushImage {
			get {
				if (null == this.pBrushImage) {
					using (ImageView originalImage = this.GetBrushImageForThickness()) {
						this.pBrushImage = this.GetBrushImageWithColor (originalImage, this.BrushColor);
						
					}//end using
					
				}//end if
				
				return this.pBrushImage;
				
			}//end get
			
		}//end UIImage BrushImage

		[Ignore]
		public Android.Graphics.Color BrushColor {
			get;
			private set;
		}//end CGColor BrushColor

		public byte[] BrushColorBuffer {
			get {
				if (null != this.BrushColor) {
					return AnimationUtils.ConvertRGBAColorToByteArray (this.BrushColor);
				} else {
					return null;
				}//end if else
				
			}
			private set {
				
				byte[] buffer = value;
				if (null != buffer) {
					this.BrushColor = AnimationUtils.ConvertByteArrayToRGBAColor (buffer);
				} else {
					this.BrushColor = Android.Graphics.Color.White;
				}//end if else
				
			}//end get private set
			
		}//end byte[] BrushColorBuffer

		public bool IsSprayBrushActive {
			get;
			private set;
		}//end bool IsSprayBrushActive
		
		private ImageView GetBrushImageForThickness ()
		{
			ImageView brush = new ImageView (context);
			if (this.Thickness == 4) {
				brush.SetImageResource (wowZapp.Resource.Drawable.SoftBrush1);
			} else if (this.Thickness == 8) {
				brush.SetImageResource (wowZapp.Resource.Drawable.SoftBrush2);
			} else if (this.Thickness == 12) {
				brush.SetImageResource (wowZapp.Resource.Drawable.SoftBrush3);
			} else if (this.Thickness == 16) {
				brush.SetImageResource (wowZapp.Resource.Drawable.SoftBrush4);
			} else if (this.Thickness == 20) {
				brush.SetImageResource (wowZapp.Resource.Drawable.SoftBrush5);
			} else if (this.Thickness == 24) {
				brush.SetImageResource (wowZapp.Resource.Drawable.SoftBrush6);
			} else {
				brush.SetImageResource (wowZapp.Resource.Drawable.SoftBrush1);
			}//end if else
			return brush;
		}//end UIImage GetBrushImageForThickness

		private ImageView GetBrushImageWithColor (ImageView brushImage, Android.Graphics.Color color)
		{
			ImageView toReturn = new ImageView (context);
			using (Bitmap sourceBitmap = brushImage.DrawingCache) {
				if (sourceBitmap != null) {
					float R = (float)color.R;
					float G = (float)color.G;
					float B = (float)color.B;

					float[] colorTransform =
				{       
					R / 255f  ,0         ,0       ,0,  0,                 // R color
					0,          G / 255f   ,   0      ,0  ,0                      // G color
					,0    ,0 ,          B / 255f, 0, 0                        // B color
					,0    ,0             ,0          ,1f  ,0f
				};                  
				
					ColorMatrix colorMatrix = new ColorMatrix ();
					colorMatrix.SetSaturation (0f); //Remove Colour 
				
					colorMatrix.Set (colorTransform); 
					ColorMatrixColorFilter colorFilter = new ColorMatrixColorFilter (colorMatrix);
					Paint paint = new Paint ();
					paint.SetColorFilter (colorFilter);   
				
					Bitmap mutableBitmap = sourceBitmap.Copy (Bitmap.Config.Argb8888, true);
					toReturn.SetImageBitmap (mutableBitmap);
					Canvas canvas = new Canvas (mutableBitmap);
					canvas.DrawBitmap (mutableBitmap, 0, 0, paint);      
				}
			}
			return toReturn;
		}
		
		public void ChangeBrushImageColor (Android.Graphics.Color toColor)
		{
			
			this.BrushColor = toColor;
			if (this.BrushType == BrushType.Spray) {
				this.pBrushImage = this.GetBrushImageWithColor (this.BrushImage, toColor);
			}//end if
			
		}//end void ChangeBrushImageColor

		public void SetBrushActive (bool active)
		{
			
			if (this.BrushType == BrushType.Spray) {
				
				this.IsSprayBrushActive = active;
				if (null != this.BrushImage) {
					this.BrushImage.Dispose ();
					this.pBrushImage = null;
				}//end if
				using (ImageView originalBrush = this.GetBrushImageForThickness()) {
					
					this.pBrushImage = this.GetBrushImageWithColor (originalBrush, active ? this.BrushColor : this.inactiveColor);
					
				}//end using
				
			} else {
				
				throw new InvalidOperationException ("Cannot activate/deactivate the brush for Normal brush type.");
				
			}//end if else
			
		}//end void SetBrushActive
		
		public override int GetHashCode ()
		{
			return this.Thickness.GetHashCode () ^ this.BrushType.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			BrushItem other = obj as BrushItem;
			if (null == (object)other) {
				return false;
			} else {
				return this.Thickness.Equals (other.Thickness) &&
					this.BrushType.Equals (other.BrushType);	
			}//end if else
		}

		public override string ToString ()
		{
			return string.Format ("[BrushItem: Thickness={0}, BrushType={1}, BrushImage={2}]", Thickness, BrushType, BrushImage);
		}

		public static bool operator == (BrushItem first, BrushItem second)
		{
			if (object.ReferenceEquals (first, second)) {
				return true;
			}//end if
			
			if ((object)first == null || (object)second == null) {
				return false;
			}//end if
			
			return first.Equals (second);
		}//end static bool operator

		public static bool operator != (BrushItem first, BrushItem second)
		{
			return !(first == second);	
		}//end static bool operator !=
	}
}


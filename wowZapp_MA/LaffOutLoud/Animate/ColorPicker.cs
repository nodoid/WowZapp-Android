using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace wowZapp.Animate
{
	public delegate void ColorChangedEventHandler (object sender,ColorChangedEventArgs e);
	
	public class ColorChangedEventArgs : EventArgs
	{
		public Color Color { get; set; }
	}

	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class ColorPickerDialog : Android.App.Dialog
	{
		public event ColorChangedEventHandler ColorChanged;
		private static Color initialColor;
		
		public ColorPickerDialog (Context context, Color initColor): base(context)
		{
			initialColor = initColor;
		}
		
		protected ColorPickerDialog (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			initialColor = Color.IndianRed;
		}
		
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			
			var cPickerView = new ColorPickerView (Context, initialColor);
			cPickerView.ColorChanged += delegate(object sender, ColorChangedEventArgs args) {
				if (ColorChanged != null)
					ColorChanged (this, args);
				Dismiss ();
			};
			SetContentView (cPickerView);
			SetTitle (Application.Context.Resources.GetString (Resource.String.colorPickerTitle));
		}
	}
	
	public class ColorPickerView : Android.Views.View
	{
		private Paint myPaint, myCenterPaint, myOuterPaint;
		private int[] colors, bwcolors;
		public event ColorChangedEventHandler ColorChanged;
		private bool trackingCentre, highlightCentre;
		
		public ColorPickerView (Context c, int color) : base (c)
		{
			colors = new[] {
				new Color (255, 0, 0, 255).ToArgb (), new Color (255, 0, 255, 255).ToArgb (), new Color (0, 0, 255, 255).ToArgb (),
				new Color (0, 255, 255, 255).ToArgb (), new Color (0, 255, 0, 255).ToArgb (), new Color (255, 255, 0, 255).ToArgb (),
				new Color (255, 0, 0, 255).ToArgb ()
			};
				
			bwcolors = new [] {
				new Color (255, 255, 255, 255).ToArgb (),
				new Color (127, 127, 127, 255).ToArgb (),
				new Color (0, 0, 0, 255).ToArgb ()
			};
					
			Shader s = new SweepGradient (0, 0, colors, null);
			myPaint = new Paint (PaintFlags.AntiAlias);
			myPaint.SetShader (s);
			myPaint.SetStyle (Paint.Style.Stroke);
			myPaint.StrokeWidth = 100;
			
			myCenterPaint = new Paint (PaintFlags.AntiAlias);
			myCenterPaint.Color = colorFromInt (color);
			myCenterPaint.StrokeWidth = 20;
			
			Shader t = new SweepGradient (0, 0, Color.Black, Color.White);
			myOuterPaint = new Paint (PaintFlags.AntiAlias);
			myOuterPaint.StrokeWidth = 25;
			myOuterPaint.SetShader (t);
			myOuterPaint.SetStyle (Paint.Style.Stroke);
			myOuterPaint.Color = Color.Gray;
		}
	
		protected override void OnDraw (Canvas canvas)
		{
			base.OnDraw (canvas);
			float r = 100 - myPaint.StrokeWidth * 0.5f;
			float outr = 120;
			canvas.Translate (144f, 140f);
			canvas.DrawOval (new RectF (-r, -r, r, r), myPaint);
			canvas.DrawOval (new RectF (-outr, -outr, outr, outr), myOuterPaint);
			canvas.DrawRect (new RectF (-50f, 140f, 100f, 160f), myCenterPaint);
				
			if (trackingCentre) {
				Color c = myCenterPaint.Color;
				myCenterPaint.SetStyle (Paint.Style.Stroke);
				myCenterPaint.Alpha = (highlightCentre == true ? 0xff : 0x80);
				Rect rect = new Rect (-50, 140, 100, 160);
				myCenterPaint.SetStyle (Paint.Style.Fill);
				myCenterPaint.Color = c;
				canvas.DrawRect (rect, myCenterPaint);
			}
		}
			
		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure (widthMeasureSpec, heightMeasureSpec);
			SetMeasuredDimension (150 * 2, 160 * 2);
		}
	
		private Color colorFromInt (int color)
		{
			return Color.Argb (Color.GetAlphaComponent (color), Color.GetRedComponent (color), Color.GetGreenComponent (color), Color.GetBlueComponent (color));
		}
	
		private int floatToByte (float x)
		{
			return Convert.ToInt32 (Math.Round (x));
		}
			
		private int pinToByte (int n)
		{
			if (n < 0)
				n = 0;
			else
				if (n > 255)
				n = 255;
			return n;
		}
			
		private int ave (int s, int d, float p)
		{
			return Convert.ToInt32 (s + Math.Round (p * (d - s)));
		}
			
		private Color interpColor (IList<int> colours, float unit)
		{
			if (unit <= 0)
				return colorFromInt (colours [0]);
			if (unit >= 1)
				return colorFromInt (colours [colours.Count - 1]);
			float p = unit * (colours.Count - 1);
			var i = (int)p;
			p -= i;
			
			int c0 = colours [i];
			int c1 = colours [i + 1];
			int a = ave (Color.GetAlphaComponent (c0), Color.GetAlphaComponent (c1), p);
			int r = ave (Color.GetRedComponent (c0), Color.GetRedComponent (c1), p);
			int g = ave (Color.GetGreenComponent (c0), Color.GetGreenComponent (c1), p);
			int b = ave (Color.GetBlueComponent (c0), Color.GetBlueComponent (c1), p);
			return Color.Argb (a, r, g, b);
		}
			
		public override bool OnTouchEvent (MotionEvent e)
		{
			float x = e.GetX () - 100;
			float y = e.GetY () - 100;
			bool inCenter = Math.Sqrt (x * x + y * y) <= 32;
			
			switch (e.Action) {
			case MotionEventActions.Down:
				trackingCentre = inCenter;
				if (inCenter) {
					highlightCentre = true;
					Invalidate ();
					break;
				}
				break;
			case MotionEventActions.Move:
				if (trackingCentre) {
					if (highlightCentre != inCenter) {
						highlightCentre = inCenter;
						Invalidate ();
					} 
				} else {
					#if DEBUG
					System.Diagnostics.Debug.WriteLine ("x = {0}, y = {1}", x, y);
					#endif
					float angle = (float)Math.Atan2 (y, x);
					float unit = angle / (2 * (float)Math.PI);
					if (unit < 0)
						unit += 1;
					/*if (x > 150 || x < -150 || y > 95 || y < -95) {
						System.Diagnostics.Debug.WriteLine ("Inner x = {0}, y = {1}", x, y);
						ColorUtils.HSV hsv, hsv2;
						hsv = ColorUtils.ColorToHSV (myCenterPaint.Color);
						hsv2 = ColorUtils.ColorToHSV (myOuterPaint.Color);
						System.Diagnostics.Debug.WriteLine ("hsv.H = {0}, hsv2.H = {1}", hsv.H, hsv2.H);
						System.Diagnostics.Debug.WriteLine ("hsv.S = {0}, hsv2.S = {1}", hsv.S, hsv2.S);
						System.Diagnostics.Debug.WriteLine ("hsv.V = {0}, hsv2.V = {1}", hsv.V, hsv2.V);
						hsv.V *= hsv2.V; // value component
						if (hsv.V > 1)
							hsv.V = 1;
						if (hsv.V < 0)
							hsv.V = 0;
						if (hsv.H > 1)
							hsv.H = 1;
						if (hsv.H < 0)
							hsv.H = 0;
						if (hsv.S > 1)
							hsv.S = 1;
						if (hsv.S < 0)
							hsv.S = 0;
						Color c = ColorUtils.HSVToColor (hsv);
						myOuterPaint.Color = interpColor (bwcolors, unit);
						myCenterPaint.Color = c;
					} else*/
					myCenterPaint.Color = interpColor (colors, unit);
					Invalidate ();
				}
				break;
			case MotionEventActions.Up:
				if (trackingCentre) {
					if (inCenter) {
						if (null != ColorChanged)
							ColorChanged (this, new ColorChangedEventArgs { Color = myCenterPaint.Color });
					}
					trackingCentre = false;
					Invalidate ();
					GC.Collect ();
				}
				break;
			}
			return true;
		}
	}
}
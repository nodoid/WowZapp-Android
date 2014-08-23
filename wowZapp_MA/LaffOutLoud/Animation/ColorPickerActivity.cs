using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace wowZapp.Animations
{	
    public delegate void ColorChangedEventHandler(object sender,ColorChangedEventArgs e);
	
    public class ColorChangedEventArgs : EventArgs
    {
        public Color myColor
        { 
            get
            {
                return colorUtil.color;
            }
            set
            {
                colorUtil.color = value;
            } 
        }
    }
		
    public static class colorUtil
    {
        public static Color color
		{ get; set; }
        public static Color hsvColor
		{ get; set; }
        public static byte a
		{ get { return 255; } }
		
        public static byte r
        { 
            get;
            set;
        }
        public static byte g
		{ get; set; }
			
        public static byte b
		{ get; set; }
    }
	
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class ColorPickerDialog : Activity
    {
        public event ColorChangedEventHandler ColorChanged;
        private static Color initialColor;
        ColorPickerView cPickerView;
		
        ImageView seethrough, bright;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.colorpicker);
            string colorIn = base.Intent.GetStringExtra("color");
			
            if (string.IsNullOrEmpty(colorIn))
                colorUtil.r = colorUtil.g = colorUtil.b = 255;
            else
            {
                convertHexToColor(colorIn);
            }
			
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewLoginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
			
            Header.headertext = Application.Context.Resources.GetString(Resource.String.colorPickerTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
            Button btnDone = FindViewById<Button>(Resource.Id.btnDone);
            btnDone.Click += delegate
            {
                string hexColor = convertColorToHex();
                Intent i = new Android.Content.Intent();
                i.PutExtra("color", hexColor);
                SetResult(Result.Ok, i);
                Finish();
            };
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            SeekBar brightness = FindViewById<SeekBar>(Resource.Id.seekBright);
            bright = FindViewById<ImageView>(Resource.Id.imgBright);
            Context context = btnDone.Context;
			
            colorUtil.color = new Color(colorUtil.r, colorUtil.g, colorUtil.b, colorUtil.a);
            brightness.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) => changeBrightness(sender, e);
			
            btnBack.Click += delegate
            {
                Intent i = new Intent();
                SetResult(Result.Canceled, i);
                Finish();
            };
			
            bright.SetBackgroundColor(new Color(colorUtil.color));
			
            LinearLayout colorLayout = FindViewById<LinearLayout>(Resource.Id.colorLayout);
            cPickerView = new ColorPickerView(colorLayout.Context, initialColor);
            cPickerView.ColorChanged += delegate(object sender, ColorChangedEventArgs args)
            {
                if (ColorChanged != null)
                    ColorChanged(this, args);
                colorUtil.r = colorUtil.color.R;
                colorUtil.g = colorUtil.color.G;
                colorUtil.b = colorUtil.color.B;
                setColor();
            };
            colorLayout.AddView(cPickerView);
        }
		
        private void convertHexToColor(string colorIn)
        {
            string r = colorIn.Substring(0, 2);
            string g = colorIn.Substring(2, 2);
            string b = colorIn.Substring(4);
            colorUtil.r = Convert.ToByte(r, 16);
            colorUtil.g = Convert.ToByte(g, 16);
            colorUtil.b = Convert.ToByte(b, 16);
        }
		
        private string convertColorToHex()
        {
            string toReturn = colorUtil.r.ToString("X") + colorUtil.g.ToString("X") + colorUtil.b.ToString("X");
            return toReturn;
        }
		
        private void setColor()
        {
            RunOnUiThread(() => bright.SetBackgroundColor(Color.Argb((int)colorUtil.a, (int)colorUtil.r, (int)colorUtil.g, (int)colorUtil.b)));
            cPickerView.Invalidate();
        }
		
        private void changeBrightness(object s, SeekBar.ProgressChangedEventArgs e)
        {
            int[] bwcolors = new [] {
				new Color(255, 255, 255, 255).ToArgb(),
				new Color(127, 127, 127, 255).ToArgb(),
				new Color(0, 0, 0, 255).ToArgb()
			};
            ColorUtils.HSV hsv, hsv2;
            hsv = ColorUtils.ColorToHSV(colorUtil.color);
            Color bw = new Color((byte)e.Progress, (byte)e.Progress, (byte)e.Progress, colorUtil.a);
            hsv2 = ColorUtils.ColorToHSV(bw);
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("hsv.H = {0}, hsv2.H = {1}", hsv.H, hsv2.H);
            System.Diagnostics.Debug.WriteLine("hsv.S = {0}, hsv2.S = {1}", hsv.S, hsv2.S);
            System.Diagnostics.Debug.WriteLine("hsv.V = {0}, hsv2.V = {1}", hsv.V, hsv2.V);
            #endif
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
            Color c = ColorUtils.HSVToColor(hsv);
            cPickerView.myCenterPaint.Color = c;
            cPickerView.ColorChanged += delegate
            {
                ColorChanged(this, new ColorChangedEventArgs { myColor = c });
                colorUtil.r = c.R;
                colorUtil.b = c.B;
                colorUtil.g = c.G;
                setColor();
            };
            cPickerView.Invalidate();
        }
		
        private Color interpColor(IList<int> colours, float unit)
        {
            if (unit <= 0)
                return colorFromInt(colours [0]);
            if (unit >= 1)
                return colorFromInt(colours [colours.Count - 1]);
            float p = unit * (colours.Count - 1);
            var i = (int)p;
            p -= i;
			
            int c0 = colours [i];
            int c1 = colours [i + 1];
            int a = ave(Color.GetAlphaComponent(c0), Color.GetAlphaComponent(c1), p);
            int r = ave(Color.GetRedComponent(c0), Color.GetRedComponent(c1), p);
            int g = ave(Color.GetGreenComponent(c0), Color.GetGreenComponent(c1), p);
            int b = ave(Color.GetBlueComponent(c0), Color.GetBlueComponent(c1), p);
            return Color.Argb(a, r, g, b);
        }
		
        private Color colorFromInt(int color)
        {
            return Color.Argb(Color.GetAlphaComponent(color), Color.GetRedComponent(color), Color.GetGreenComponent(color), Color.GetBlueComponent(color));
        }
		
        private int ave(int s, int d, float p)
        {
            return Convert.ToInt32(s + Math.Round(p * (d - s)));
        }
    }
	
    public class ColorPickerView : Android.Views.View
    {
        public Paint myPaint, myCenterPaint, myOuterPaint;
        private int[] colors, bwcolors;
        public event ColorChangedEventHandler ColorChanged;
        private bool trackingCentre, highlightCentre;
        private int width, height;
        private float strokeWidth;
        public ColorPickerView(Context c, int color) : base (c)
        {
            colors = new[] {
				new Color(255, 0, 0, 255).ToArgb(), new Color(255, 0, 255, 255).ToArgb(), new Color(0, 0, 255, 255).ToArgb(),
				new Color(0, 255, 255, 255).ToArgb(), new Color(0, 255, 0, 255).ToArgb(), new Color(255, 255, 0, 255).ToArgb(),
				new Color(255, 0, 0, 255).ToArgb()
			};
			
            bwcolors = new [] {
				new Color(255, 255, 255, 255).ToArgb(),
				new Color(127, 127, 127, 255).ToArgb(),
				new Color(0, 0, 0, 255).ToArgb()
			};
			
            strokeWidth = 100;
			
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
            {
                strokeWidth = ImageHelper.convertDpToPixel((25 * (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth) / 100, c);
                width = height = Convert.ToInt32(strokeWidth * 2) - 20;
            } else
            {
                width = 180;
                height = 190;
            }
			
            Shader s = new SweepGradient(0, 0, colors, null);
            myPaint = new Paint(PaintFlags.AntiAlias);
            myPaint.SetShader(s);
            myPaint.SetStyle(Paint.Style.Stroke);
            myPaint.StrokeWidth = strokeWidth;
			
            myCenterPaint = new Paint(PaintFlags.AntiAlias);
            myCenterPaint.Color = colorFromInt(color);
            myCenterPaint.StrokeWidth = 0;
        }
		
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            float r = strokeWidth - myPaint.StrokeWidth * 0.5f;
            canvas.Translate((float)wowZapp.LaffOutOut.Singleton.ScreenXWidth / 1.5f, (float)wowZapp.LaffOutOut.Singleton.ScreenYHeight / 2.5f);
            canvas.DrawOval(new RectF(-r, -r, r, r), myPaint);
            canvas.DrawRect(new RectF(0f, 0f, 0f, 0f), myCenterPaint);
			
            if (trackingCentre)
            {
                Color c = myCenterPaint.Color;
                myCenterPaint.SetStyle(Paint.Style.Stroke);
                myCenterPaint.Alpha = (highlightCentre == true ? 0xff : 0x80);
                Rect rect = new Rect(0, 0, 0, 0);
                myCenterPaint.SetStyle(Paint.Style.Fill);
                myCenterPaint.Color = c;
                canvas.DrawRect(rect, myCenterPaint);
            }
        }
		
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            SetMeasuredDimension(width * 2, height * 2);
        }
		
        private Color colorFromInt(int color)
        {
            return Color.Argb(Color.GetAlphaComponent(color), Color.GetRedComponent(color), Color.GetGreenComponent(color), Color.GetBlueComponent(color));
        }
		
        private int floatToByte(float x)
        {
            return Convert.ToInt32(Math.Round(x));
        }
		
        private int pinToByte(int n)
        {
            if (n < 0)
                n = 0;
            else
				if (n > 255)
                n = 255;
            return n;
        }
		
        private int ave(int s, int d, float p)
        {
            return Convert.ToInt32(s + Math.Round(p * (d - s)));
        }
		
        private Color interpColor(IList<int> colours, float unit)
        {
            if (unit <= 0)
                return colorFromInt(colours [0]);
            if (unit >= 1)
                return colorFromInt(colours [colours.Count - 1]);
            float p = unit * (colours.Count - 1);
            var i = (int)p;
            p -= i;
			
            int c0 = colours [i];
            int c1 = colours [i + 1];
            int a = ave(Color.GetAlphaComponent(c0), Color.GetAlphaComponent(c1), p);
            int r = ave(Color.GetRedComponent(c0), Color.GetRedComponent(c1), p);
            int g = ave(Color.GetGreenComponent(c0), Color.GetGreenComponent(c1), p);
            int b = ave(Color.GetBlueComponent(c0), Color.GetBlueComponent(c1), p);
            return Color.Argb(a, r, g, b);
        }
		
        public override bool OnTouchEvent(MotionEvent e)
        {
            float x = e.GetX() - strokeWidth;
            float y = e.GetY() - strokeWidth;
            bool inCenter = Math.Sqrt(x * x + y * y) <= 32;
			
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    trackingCentre = inCenter;
                    if (inCenter)
                    {
                        highlightCentre = true;
                        Invalidate();
                        break;
                    }
                    break;
                case MotionEventActions.Move:
                    if (trackingCentre)
                    {
                        if (highlightCentre != inCenter)
                        {
                            highlightCentre = inCenter;
                            Invalidate();
                        } 
                    } else
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("x = {0}, y = {1}", x, y);
#endif
                        float angle = (float)Math.Atan2(y, x);
                        float unit = angle / (2 * (float)Math.PI);
                        if (unit < 0)
                            unit += 1;
                        myCenterPaint.Color = interpColor(colors, unit);
                        ColorChanged(this, new ColorChangedEventArgs { myColor = myCenterPaint.Color });
                        Invalidate();
                    }
                    break;
                case MotionEventActions.Up:
                    if (trackingCentre)
                    {
                        if (inCenter)
                        {
                            if (null != ColorChanged)
                                ColorChanged(this, new ColorChangedEventArgs { myColor = myCenterPaint.Color });
                        }
                        trackingCentre = false;
                        Invalidate();
                        GC.Collect();
                    }
                    break;
            }
            return true;
        }
    }
}
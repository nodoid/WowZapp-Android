using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace LaffOutLoud
{
	public class VerticalSeekBar : SeekBar
	{
		public VerticalSeekBar (Context context) : base(context)
		{
		}

		public VerticalSeekBar (Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
		}

		public VerticalSeekBar (Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		protected override void OnSizeChanged (int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged (w, h, oldw, oldh);
		}

		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure (heightMeasureSpec, widthMeasureSpec);
			SetMeasuredDimension (MeasuredWidth, MeasuredHeight);
		}

		protected void onDraw (Canvas c)
		{
			c.Rotate (-90);
			c.Translate (-Height, 0);
			base.OnDraw (c);
		}

		public override bool OnTouchEvent (MotionEvent e)
		{

			if (!Enabled) 
				return false;

			switch (e.Action) {
			case Android.Views.MotionEventActions.Down:
			case Android.Views.MotionEventActions.Move:
			case Android.Views.MotionEventActions.Up:
				Progress = (Max - (int)(Max * e.GetY () / Height));
				OnSizeChanged (Width, Height, 0, 0);
				break;

			case Android.Views.MotionEventActions.Cancel:
				break;
			}

			return true;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using LOLMessageDelivery.Classes.LOLAnimation;
using LOLApp_Common;

using WZCommon;

namespace wowZapp.Animations
{
    public class CreateAnimationDrawer : View
    {
        private Bitmap myBitmap;
        private Canvas myCanvas;
        private Path myPath;
        private Paint myPaint;
        private Paint myBoundsPaint;
        private RectF myBoundsRect;
        private bool addOnly;
        private float myX, myY;
        private static float tolerance = 4f;
        private int status;
        private string DrawerStateInternal = "brush_drawing";

        public event ColorChangedEventHandler ColorChanged;
		
        public CreateAnimationDrawer(Context c, BrushItem brush, Canvas canvas, Bitmap myBmp, bool tooAdd = false, int cell = 1, string DrawerState = "brush_selection", Path pathToUse = null)
            : base(c)
        {
            myBitmap = myBmp;
            myCanvas = canvas;
            DrawerStateInternal = DrawerState;
            addOnly = tooAdd;
            status = 0;
            myPath = new Path();
            myPaint = new Paint(PaintFlags.Dither);
            myPaint.AntiAlias = true;
            myPaint.Dither = true;
            myPaint.SetStyle(Paint.Style.Stroke);
            myPaint.StrokeJoin = Paint.Join.Round;
            myPaint.StrokeWidth = brush.Thickness;
            myPaint.StrokeCap = Paint.Cap.Round;
            myPaint.SetARGB(colorUtil.a, colorUtil.r, colorUtil.g, colorUtil.b);
			
            if (brush.BrushType == AnimationTypesBrushType.Spray)
                myPaint.SetShadowLayer(brush.Thickness, 0, 0, ImageHelper.convWZColorToColor(brush.BrushColor));

            if (DrawerState == "brush_selection")
            {
                if (pathToUse != null)
                {
                    myBoundsPaint = new Paint();
                    myBoundsPaint = new Paint(PaintFlags.Dither);
                    myBoundsPaint.AntiAlias = true;
                    myBoundsPaint.Dither = true;
                    myBoundsPaint.SetStyle(Paint.Style.Stroke);
                    myBoundsPaint.StrokeJoin = Paint.Join.Round;
                    myBoundsPaint.StrokeWidth = 10f;
                    myBoundsPaint.StrokeCap = Paint.Cap.Round;
                    myBoundsPaint.SetARGB(255, 0, 0, 0);
                    myBoundsPaint.SetPathEffect(new DashPathEffect(new float[]
                    {
                        10f,
                        20f
                    }, 0));

                    myPath = pathToUse;
                    AnimationUtil.theCanvas.DrawPath(myPath, myPaint);
                    AnimationUtil.theCanvas.DrawPath(myPath, myPaint);

                    myBoundsRect = new RectF();
                    myPath.ComputeBounds(myBoundsRect, true);
                    AnimationUtil.theCanvas.DrawRect(myBoundsRect, myBoundsPaint);
                }
            }
        }
	
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
        }
		
        protected override void OnDraw(Canvas canvas)
        {
            /*	if (addOnly) {
				if (AnimationUtil.imgAttr [AnimationUtil.currentImage].attr != null)
					AnimationUtil.theCanvas.DrawBitmap (myBitmap, AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.X, 
					                                    AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.Y, null);
				else
					AnimationUtil.theCanvas.DrawBitmap (myBitmap, 0, 0, null);
			} else {
				AnimationUtil.theCanvas.DrawColor (Color.White);
				AnimationUtil.theCanvas.DrawBitmap (myBitmap, 0, 0, myPaint);
				AnimationUtil.theCanvas.DrawPath (myPath, myPaint);
			}*/

            myCanvas.DrawColor(Color.White);
            int t = 0;
            foreach (Bitmap bmp in AnimationUtil.imagesForCanvas)
            {
                if (t == 0)
                    myCanvas.DrawBitmap(bmp, 0, 0, myPaint);
                else
                    myCanvas.DrawBitmap(bmp, AnimationUtil.imgAttr [t].attr.X, 
                                                        AnimationUtil.imgAttr [t].attr.Y, null);
                t++;
            }
            AnimationUtil.theCanvas.DrawPath(myPath, myPaint);

            if (DrawerStateInternal == "brush_selection")
            {
                if (myPath != null && myBoundsPaint != null)
                {
                    myBoundsRect = new RectF();
                    myPath.ComputeBounds(myBoundsRect, true);
                    if (myBoundsPaint != null)
                        canvas.DrawRect(myBoundsRect, myBoundsPaint);
                }
            }

            Invalidate();
        }
	
        private void touchStart(float x, float y)
        {
            myPath.Reset();
            myPath.MoveTo(x, y);
            myX = x;
            myY = y;
        }
	
        private void touchMove(float x, float y)
        {
            float dx = Math.Abs(x - myX);
            float dy = Math.Abs(y - myY);
            if (dx >= tolerance || dy >= tolerance)
            {
                myPath.QuadTo(myX, myY, (x + myX) / 2, (y + myY) / 2);
                myX = x;
                myY = y;
            }
        }
	
        private void touchUp()
        {
            if (AnimationUtil.imagePos == null)
                AnimationUtil.imagePos = new System.Drawing.PointF(myX, myY);
            else
            {
                AnimationUtil.imagePos.X = myX;
                AnimationUtil.imagePos.Y = myY;
            }
			
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].attr == null)
                AnimationUtil.imgAttr [AnimationUtil.currentImage].attr = new System.Drawing.PointF();
            AnimationUtil.imgAttr [AnimationUtil.currentImage].attr = AnimationUtil.imagePos;
			
            myPath.LineTo(myX, myY);
            myCanvas.DrawPath(myPath, myPaint);
            myCanvas.DrawPath(myPath, myPaint);
            myPath.Reset();

            if (DrawerStateInternal == "brush_selection")
            {
                if (myPath != null)
                {
                    myBoundsRect = new RectF();
                    myPath.ComputeBounds(myBoundsRect, true);
                    if (myBoundsPaint != null)
                        AnimationUtil.theCanvas.DrawRect(myBoundsRect, myBoundsPaint);
                }
            }
        }
	
        public override bool OnTouchEvent(MotionEvent ev)
        {
            float x = ev.GetX();
            float y = ev.GetY();
			
            float myXPos = AnimationUtil.imagePos.X;
            float myYPos = AnimationUtil.imagePos.Y;
            float lowX = myXPos - (AnimationUtil.boundBox.X / 2) > 0 ? myXPos - AnimationUtil.boundBox.X / 2 : 0;
            float highX = lowX != 0 ? myXPos + (AnimationUtil.boundBox.X / 2) : AnimationUtil.boundBox.X;
            float lowY = myYPos - (AnimationUtil.boundBox.Y / 2) > 0 ? myYPos - (AnimationUtil.boundBox.Y / 2) : 0;
            float highY = lowY != 0 ? myYPos + (AnimationUtil.boundBox.Y / 2) : AnimationUtil.boundBox.Y;
            status++;
            if (status == 3)
                status = 0;
            if ((x < lowX) || (x > highX) || (y < lowY) || (y > myYPos + highY) || status == 0)
            {
#if DEBUG
                Console.WriteLine("not close to touching the icon or status = 0");
                Console.WriteLine("x = {0}, y = {0}", x, y);
                Console.WriteLine("lowX = {0}, highX = {1}, lowY = {2}, highY = {3}", lowX, highX, lowY, highY);
                Console.WriteLine("myXPos = {0}, myYPos = {1}", myXPos, myYPos);
#endif
                //return false;
            }
#if DEBUG
            Console.WriteLine("drawing a box");
#endif
            Paint paint = new Paint();
            paint.Color = Color.Blue;
            myCanvas.DrawColor(status == 1 ? Color.Red : Color.Gainsboro);
            myCanvas.DrawRect(myXPos - 10, myYPos - 10, myXPos + AnimationUtil.boundBox.X + 10, 
			                   myYPos + AnimationUtil.boundBox.Y + 10, paint);
			
            switch (ev.Action)
            {
                case MotionEventActions.Down:
                    touchStart(x, y);
                    Invalidate();
                    break;
                case MotionEventActions.Move:
                    touchMove(x, y);
                    Invalidate();
                    break;
                case MotionEventActions.Up:
                    touchUp();
                    Invalidate();
                    break;
            }
            return true;
        }

        public Path GetFinalPath()
        {
            return myPath;
        }
    }
}
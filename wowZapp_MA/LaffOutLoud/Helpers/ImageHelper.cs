using System;
using System.IO;
using System.Collections.Generic;
using Android.Graphics;
using System.Drawing;
using Android.Content.Res;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using Android.App;

using WZCommon;

namespace wowZapp
{
    public class ImageHelper
    {
        private static Size GetImageSizeFromArray(byte[] imgBuffer)
        {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;

            BitmapFactory.DecodeByteArray(imgBuffer, 0, imgBuffer.Length, options);

            return new Size(options.OutWidth, options.OutHeight);
        }

        public static int CalculateSampleSizePower2(Size originalSize, int reqWidth, int reqHeight)
        {
            int height = originalSize.Height;
            int width = originalSize.Width;
            int IMAGE_MAX_SIZE = reqWidth >= reqHeight ? reqWidth : reqHeight;

            int inSampleSize = 1;

            if (height > IMAGE_MAX_SIZE || width > IMAGE_MAX_SIZE)
            {
                inSampleSize = (int)Math.Pow(2, (int)Math.Round(Math.Log(IMAGE_MAX_SIZE /
                    (double)Math.Max(height, width)) / Math.Log(0.5)));
            }

            return inSampleSize;
        }

        private static int CalculateSampleSize(Size originalSize, int reqWidth, int reqHeight)
        {
            int sampleSize = 1;

            if (originalSize.Height > reqHeight || originalSize.Width > reqWidth) 
                sampleSize = Convert.ToInt32(originalSize.Width > originalSize.Height ? 
                    (double)originalSize.Height / (double)reqHeight : (double)originalSize.Width / (double)reqWidth);

            return sampleSize;

        }//end static int CalculateSampleSize

        public static Bitmap CreateUserProfileImageForDisplay(byte[] userImage, int width, int height, Resources res)
        {
            if (userImage.Length > 0 && userImage.Length != 2)
            {
                Size imgSize = GetImageSizeFromArray(userImage);

                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InSampleSize = CalculateSampleSizePower2(imgSize, width, height);
                Bitmap scaledUserImage = BitmapFactory.DecodeByteArray(userImage, 0, userImage.Length, options);

                int scaledWidth = scaledUserImage.Width;
                int scaledHeight = scaledUserImage.Height;

                Bitmap resultImage = Bitmap.CreateScaledBitmap(BitmapFactory.DecodeResource(res, Resource.Drawable.dummy), scaledWidth, scaledHeight, true);
                using (Canvas canvas = new Canvas (resultImage))
                {
                    using (Paint paint = new Paint (PaintFlags.AntiAlias))
                    {
                        paint.Dither = false;
                        paint.FilterBitmap = true;
                        paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.DstIn));
                        canvas.DrawBitmap(scaledUserImage, 0, 0, null);
                        scaledUserImage.Recycle();
								
                        using (Bitmap maskImage = Bitmap.CreateScaledBitmap (BitmapFactory.DecodeResource (res, Resource.Drawable.emptybackground), scaledWidth, scaledHeight, true))
                        {
                            canvas.DrawBitmap(maskImage, 0, 0, paint);
                            maskImage.Recycle();
                        }
                    }
                }
                return resultImage;
            } else
            {
                return null;
            }

        }//end static Bitmap CreateUserProfileImageFordisplay

        public static void fontSizeInfo(Context context)
        {
            int imageSizeX = 0;
            using (BitmapFactory.Options opt = new BitmapFactory.Options())
            {
                opt.InJustDecodeBounds = true;
                BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.headerlogo, opt);
                imageSizeX = opt.OutWidth + (int)convertDpToPixel(12f, context);
            }
            float sizeLeft = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth - (float)imageSizeX;
            if (sizeLeft < 0)
                return;
            Paint paint = new Paint(PaintFlags.AntiAlias);
            paint.TextSize = convertDpToPixel((float)Header.fontsize, context);
            paint.SetTypeface(Typeface.DefaultBold);
            float width = paint.MeasureText(Header.headertext);
#if DEBUG
            System.Diagnostics.Debug.WriteLine("width = {0}, sizeLeft = {1}, textSize = {2}", width, sizeLeft, paint.TextSize);
#endif
            while (width >= sizeLeft)
            {
                Header.fontsize -= convertDpToPixel(1f, context);
                paint.TextSize = convertDpToPixel((float)Header.fontsize, context);
                width = paint.MeasureText(Header.headertext);
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("new width = {0}", width);
                #endif
            }

#if DEBUG
            Console.WriteLine("Header.fontsize = {0}", Header.fontsize);
#endif
            /*if (sizeLeft < width) {
				width -= convertDpToPixel (.5f, context);
				paint.TextSize = width;
				width = convertDpToPixel (paint.MeasureText (Header.headertext), context);
				#if DEBUG
				System.Diagnostics.Debug.WriteLine ("new width = {0}", convertDpToPixel (width, context));
				#endif
			}*/
        }

        public static void setupTopPanel(ImageView buttons, TextView text, RelativeLayout layout, Context context)
        {
            layout.RemoveAllViewsInLayout();
            using (LinearLayout overall = new LinearLayout (context))
            {
                overall.Orientation = Android.Widget.Orientation.Horizontal;
                overall.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent))
                {
                    lp.SetMargins((int)convertDpToPixel(6f, context), 0, (int)convertDpToPixel(6f, context), 0);
                    buttons.LayoutParameters = lp;
                }
                using (LinearLayout l = new LinearLayout (context))
                {
                    l.LayoutParameters = new LinearLayout.LayoutParams((int)convertDpToPixel(60f, context), LinearLayout.LayoutParams.MatchParent);
                    l.AddView(buttons);
                    overall.AddView(l);
                }
                using (LinearLayout m = new LinearLayout (context))
                {
                    float x = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth - convertDpToPixel(120f, context);
                    m.LayoutParameters = new LinearLayout.LayoutParams((int)x, LinearLayout.LayoutParams.MatchParent);
                    m.SetGravity(GravityFlags.CenterHorizontal);
                    m.AddView(text);
                    overall.AddView(m);
                }
                layout.AddView(overall);
            }
        }

        public static void setupButtonsPosition<T>(T[] buttons, LinearLayout layout, Context context) where T : View
        {
            layout.RemoveAllViewsInLayout();
            float eachSection = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth / buttons.Length;
            foreach (T btn in buttons)
            {
                int tag = (int)btn.Tag;
                using (LinearLayout l = new LinearLayout (context))
                {
                    l.LayoutParameters = new LinearLayout.LayoutParams((int)eachSection, LinearLayout.LayoutParams.MatchParent);
                    if (tag == 0)
                        l.SetGravity(GravityFlags.Left);
                    if (tag == buttons.Length - 1)
                        l.SetGravity(GravityFlags.Right);
                    if (tag != buttons.Length - 1 && tag != 0)
                        l.SetGravity(GravityFlags.Center);

                    l.AddView(btn);
                    layout.AddView(l);
                }
            }
        }
		
        public static void setupButtonsPosition<T>(T[] buttons, Button btn, LinearLayout layout, Context context) where T : View
        {
            layout.RemoveAllViewsInLayout();
            float midPoint = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth / 2;
            float leftBtn = midPoint - (convertDpToPixel(120f, context) / 2);
            float midBtn = (midPoint - leftBtn) + midPoint;
            float rightBtn = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth - convertDpToPixel(60f, context);
            using (LinearLayout l = new LinearLayout(context))
            {
                l.LayoutParameters = new LinearLayout.LayoutParams((int)leftBtn, LinearLayout.LayoutParams.MatchParent);
                l.SetGravity(GravityFlags.Left);
                l.AddView(buttons [0]);
                layout.AddView(l);
            }
            using (LinearLayout l = new LinearLayout (context))
            {
                l.LayoutParameters = new LinearLayout.LayoutParams((int)midBtn, LinearLayout.LayoutParams.MatchParent);
                l.SetGravity(GravityFlags.Left);
                l.AddView(btn);
                //layout.AddView (l);
            }
            using (LinearLayout l = new LinearLayout(context))
            {
                l.LayoutParameters = new LinearLayout.LayoutParams((int)rightBtn, LinearLayout.LayoutParams.MatchParent);
                l.SetGravity(GravityFlags.Right);
                l.AddView(buttons [1]);
                layout.AddView(l);
            }
        }
		
        public static void setupButtonsPosition(ImageButton iButton, Button button, LinearLayout layout, Context context, bool isCenter = false)
        {
            layout.RemoveAllViewsInLayout();
            float eachSection = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth / 2;
            float left = eachSection - (convertDpToPixel(100f, context) / 2);
            float right = (eachSection - left) + eachSection;
            using (LinearLayout l = new LinearLayout (context))
            {
                l.LayoutParameters = new LinearLayout.LayoutParams((int)left, LinearLayout.LayoutParams.MatchParent);
                l.SetGravity(GravityFlags.Left);
                l.AddView(iButton);
                layout.AddView(l);
            }
            using (LinearLayout l = new LinearLayout (context))
            {
                l.LayoutParameters = new LinearLayout.LayoutParams((int)right, LinearLayout.LayoutParams.MatchParent);
                l.SetGravity(GravityFlags.Left);
                l.AddView(button);
                layout.AddView(l);
            }	
        }
		
        public static void setupButtonsPosition(ImageView iButton, Button button, LinearLayout layout, Context context, bool isCenter = false)
        {
            layout.RemoveAllViewsInLayout();
            float eachSection = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth / 2;
            float left = eachSection - (convertDpToPixel(100f, context) / 2);
            float right = (eachSection - left) + eachSection;
            using (LinearLayout l = new LinearLayout (context))
            {
                l.LayoutParameters = new LinearLayout.LayoutParams((int)left, LinearLayout.LayoutParams.MatchParent);
                l.SetGravity(GravityFlags.Left);
                l.AddView(iButton);
                layout.AddView(l);
            }
            using (LinearLayout l = new LinearLayout (context))
            {
                l.LayoutParameters = new LinearLayout.LayoutParams((int)right, LinearLayout.LayoutParams.MatchParent);
                l.SetGravity(GravityFlags.Left);
                l.AddView(button);
                layout.AddView(l);
            }	
        }

        public static void resizeWidget(Button[] buttons, Context c, GravityFlags gravity)
        {
            int[] newSize = new int[2];
            float[] tSize = new float[2];
            int m = 0;
            ViewGroup.MarginLayoutParams btnParams;
            string text = "";
            float fontSize = 0f;
            foreach (Button btn in buttons)
            {
                btnParams = (ViewGroup.MarginLayoutParams)btn.LayoutParameters;
                newSize = getNewSizes(btn, c);
                text = btn.Text;
                fontSize = btn.TextSize;
				
                using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams (newSize [0], newSize [1]))
                {
                    layParams.SetMargins((int)convertDpToPixel(btnParams.LeftMargin, c), (int)convertDpToPixel(btnParams.TopMargin, c), 
										(int)convertDpToPixel(btnParams.RightMargin, c), (int)convertDpToPixel(btnParams.BottomMargin, c));
                    btn.LayoutParameters = layParams;
                }
                btn.Gravity = gravity;
                tSize [m] = resizeFont(text, fontSize, btn.Width, c);
                m++;
            }
            int mr = tSize [0] > tSize [1] ? 0 : 1;

            buttons [0].SetTextSize(Android.Util.ComplexUnitType.Dip, convertPixelToDp(tSize [mr], c));
            buttons [1].SetTextSize(Android.Util.ComplexUnitType.Dip, convertPixelToDp(tSize [mr], c));
        }
		
		

        public static void resizeWidget <T>(T[] buttons, Context c) where T : View
        {
            int[] newSize = new int[2];
			
            ViewGroup.MarginLayoutParams btnParams;
            foreach (T iv in buttons)
            {
                btnParams = (ViewGroup.MarginLayoutParams)iv.LayoutParameters;
                newSize = getNewSizes(iv, c);
				
                using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams (newSize [0], newSize [1]))
                {
                    layParams.SetMargins((int)convertDpToPixel(btnParams.LeftMargin, c), (int)convertDpToPixel(btnParams.TopMargin, c), 
				                      (int)convertDpToPixel(btnParams.RightMargin, c), (int)convertDpToPixel(btnParams.BottomMargin, c));
                    iv.LayoutParameters = layParams;
                }
            }
        }
		
        public static void resizeWidget<T>(T[] buttons, LinearLayout[] layout, Context c) where T : View
        {
            int[] newButtonSize = new int[2];
            newButtonSize = getNewSizes(buttons [0], c);
            int buttonsPerRow = buttons.Length / layout.Length;
			
        }
		
        public static void resizeWidget<T>(T iv, Context c) where T : View
        {
            if (iv == null)
                return;
            int[] newSize = new int[2];
			
            ViewGroup.MarginLayoutParams btnParams;
            btnParams = (ViewGroup.MarginLayoutParams)iv.LayoutParameters;
            newSize = getNewSizes(iv, c);
				
            using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams (newSize [0], newSize [1]))
            {
                layParams.SetMargins((int)convertDpToPixel(btnParams.LeftMargin, c), (int)convertDpToPixel(btnParams.TopMargin, c), 
				                      (int)convertDpToPixel(btnParams.RightMargin, c), (int)convertDpToPixel(btnParams.BottomMargin, c));
                iv.LayoutParameters = layParams;
            }
        }
		
        public static void resizeLayout(LinearLayout[] layout, Context c)
        {
            int[] newSize = new int[2];
			
            foreach (LinearLayout ll in layout)
            {
                newSize = getNewSizes(ll, c);
                using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams (newSize [0], newSize [1]))
                {
                    ll.LayoutParameters = layParams;
                }
            }
        }
		
        public static void resizeLayout(LinearLayout layout, Context c)
        {
            int[] newSize = new int[2];
            using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams (newSize [0], newSize [1]))
            {
                layout.LayoutParameters = layParams;
            }
        }
		
        public static Bitmap rotateImage(bool left = false)
        {
            Bitmap toReturn = null;
            using (Bitmap bmp = BitmapFactory.DecodeFile(AndroidData.imageFileName))
            {
                if (bmp != null)
                {
                    using (Matrix matrix = new Matrix())
                    {
                        matrix.PostRotate(!left ? 90 : -90);
                        toReturn = Bitmap.CreateBitmap(bmp, 0, 0, 
				                              bmp.Width, bmp.Height, 
				                              matrix, true);
				                              
                        using (MemoryStream stream = new MemoryStream())
                        {
                            toReturn.Compress(Bitmap.CompressFormat.Png, 0, stream);
                            byte[] bitmapData = stream.ToArray();
                            File.WriteAllBytes(AndroidData.imageFileName, bitmapData);
                        }
                    }
                }
            }
            return toReturn;
        }
		
        public static Bitmap rotateImage(Context c, int res)
        {
            Bitmap toReturn = null;
            using (Bitmap bmp = BitmapFactory.DecodeResource(c.Resources, res))
            {
                if (bmp != null)
                {
                    using (Matrix matrix = new Matrix())
                    {
                        matrix.PostRotate(Animations.AnimationUtil.imgAttr [Animations.AnimationUtil.currentImage].fRotation);
                        toReturn = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, matrix, true);
                    }
                }
            }
            return toReturn;
        }
		
        public static Bitmap rotateImage(string file)
        {
            Bitmap toReturn = null;
            using (Bitmap bmp = BitmapFactory.DecodeFile(file))
            {
                if (bmp != null)
                {
                    using (Matrix matrix = new Matrix())
                    {
                        matrix.PostRotate(Animations.AnimationUtil.imgAttr [Animations.AnimationUtil.currentImage].fRotation);
                        toReturn = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, matrix, true);
                    }
                }
            }
            return toReturn;
        }

        private static float resizeFont(string text, float size, float btnWidth, Context context)
        {
            Paint paint = new Paint(PaintFlags.AntiAlias);
            paint.TextSize = size;
            paint.SetTypeface(Typeface.DefaultBold);
            float width = convertDpToPixel(paint.MeasureText(text), context);
            float pxWidth = convertDpToPixel(btnWidth, context);
            while (width <= pxWidth)
            {
                size += .5f;
                paint.TextSize = size;
                width = convertDpToPixel(paint.MeasureText(text), context);
            }
			
            return size;
        }

        private static int[] getNewSizes<T>(T f, Context c) where T : View
        {
            float width = f.Width == 0 ? (float)f.LayoutParameters.Width : convertDpToPixel((float)f.Width, c);
            float height = f.Height == 0 ? (float)f.LayoutParameters.Height : convertDpToPixel((float)f.Height, c);
            float xwidth = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth;
            float yheight = (float)wowZapp.LaffOutOut.Singleton.ScreenYHeight;
            int [] size = new int[2];
            size [0] = (int)((width / 480) * xwidth);
            size [1] = (int)((height / 800) * yheight);		
            return size;		
        }
		
        public static float[] getNewSizes(float[] sizes, Context c)
        {
            float xSize = convertDpToPixel(sizes [0], c);
            float ySize = convertDpToPixel(sizes [1], c);
            float xwidth = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth;
            float yheight = (float)wowZapp.LaffOutOut.Singleton.ScreenYHeight;
            float [] finalSize = new float[2];
            finalSize [0] = convertPixelToDp(((xSize / 480) * xwidth), c);
            finalSize [1] = convertPixelToDp(((ySize / 800) * yheight), c);
            return finalSize;
        }
		
        public static void setBackground(LinearLayout linLay, Context c)
        {
            switch (AndroidData.Skin)
            {
                case 0:
                    linLay.SetBackgroundResource(Resource.Drawable.background);
                    break;
            }
        }

        public static byte[] convColToByteArray(Android.Graphics.Color color)
        {
            byte [] toReturn = new byte[4];
            toReturn [0] = color.R;
            toReturn [1] = color.G;
            toReturn [2] = color.B;
            toReturn [3] = color.A;
            return toReturn;
        }
        
        public static Android.Graphics.Color convWZColorToColor(WZColor wzcolor)
        {
            Android.Graphics.Color toReturn;
            toReturn.R = wzcolor.Red;
            toReturn.G = wzcolor.Green;
            toReturn.B = wzcolor.Blue;
            toReturn.A = wzcolor.Alpha;
            return toReturn;
        }

        public static float getNewFontSize(float size, Context c)
        {
            float fSize = convertDpToPixel(size, c);
            float calc = fSize + (fSize * .1f);
            return convertPixelToDp(calc, c);
        }

        public static float convertDpToPixel(float dp, Context context)
        {
            Android.Util.DisplayMetrics metrics = context.Resources.DisplayMetrics;
            return dp * ((float)metrics.DensityDpi / 160f);
        }
		
        public static float convertPixelToDp(float px, Context context)
        {
            Android.Util.DisplayMetrics metrics = context.Resources.DisplayMetrics;
            return (px * 160f) / (float)metrics.DensityDpi;
        }
    }
}


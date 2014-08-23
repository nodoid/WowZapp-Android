using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace wowZapp.Animations
{
    public partial class CreateAnimationActivity : Activity
    {
        private void displayPhoto(Canvas canvas, string filename)
        {
            using (Bitmap bmp = BitmapFactory.DecodeFile(filename))
            {
                float xSize = (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth / 4f;
                float ySize = (float)wowZapp.LaffOutOut.Singleton.ScreenYHeight / 4f;
                int xScale = bmp.Width > (int)xSize ? (int)xSize : bmp.Width;
                int yScale = bmp.Height > (int)ySize ? (int)ySize : bmp.Height;
                using (Bitmap scaled = Bitmap.CreateScaledBitmap(bmp, xScale, yScale, false))
                {
                    AnimationUtil.imagesForCanvas.Add(scaled);
                    //AnimationUtil.theCanvas.DrawBitmap (scaled, 200, 200, null);
                    relLay.AddView(new CreateAnimationDrawer(context, currentBrush, canvas, bmp, true));
                }
            }
        }
    }
}

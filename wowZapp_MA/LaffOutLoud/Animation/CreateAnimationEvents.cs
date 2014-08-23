using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Graphics;

namespace wowZapp.Animations
{			
    public partial class CreateAnimationActivity : Activity
    {
        private void addImageView(object s, EventArgs e)
        {
            //Bitmap bmp = BitmapFactory.DecodeResource (context.Resources, Resource.Drawable.cup);
            Bitmap bmp = BitmapFactory.DecodeFile(AnimationUtil.contentFilename);
            AnimationUtil.boundBox = new System.Drawing.Point(bmp.Width, bmp.Height);
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].bbox == null)
                AnimationUtil.imgAttr [AnimationUtil.currentImage].bbox = new System.Drawing.Point();
            AnimationUtil.imgAttr [AnimationUtil.currentImage].bbox = AnimationUtil.boundBox;
            AnimationUtil.imgAttr [AnimationUtil.currentImage].sImage = AnimationUtil.contentFilename;
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].attr != null)
            {
                if (AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.X != 0 || AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.Y != 0) 
                    AnimationUtil.theCanvas.DrawBitmap(bmp, AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.X, AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.Y, null);
                else	
                    AnimationUtil.theCanvas.DrawBitmap(bmp, 0, 0, null);
            } else
                AnimationUtil.theCanvas.DrawBitmap(bmp, 0, 0, null);
            relLay.AddView(new CreateAnimationDrawer(context, currentBrush, canvas, bmp, true));
        }
		
        private void rotateMinus(object s, EventArgs e)
        {
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].iImage == 0)
                return;
            AnimationUtil.imgAttr [AnimationUtil.currentImage].fRotation -= 5f;
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].fRotation < -355f)
                AnimationUtil.imgAttr [AnimationUtil.currentImage].fRotation = 0f;
            Bitmap bmp = null;
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].fRotation != 0f)
                bmp = ImageHelper.rotateImage(AnimationUtil.imgAttr [AnimationUtil.currentImage].sImage);
            else
                bmp = BitmapFactory.DecodeFile(AnimationUtil.imgAttr [AnimationUtil.currentImage].sImage);
            AnimationUtil.imgAttr [AnimationUtil.currentImage].bbox = new System.Drawing.Point(bmp.Width, bmp.Height);
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].attr != null)
            {
                if (AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.X != 0 || AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.Y != 0) 
                    AnimationUtil.theCanvas.DrawBitmap(bmp, AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.X, AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.Y, null);
                else	
                    AnimationUtil.theCanvas.DrawBitmap(bmp, 0, 0, null);
            } else
                AnimationUtil.theCanvas.DrawBitmap(bmp, 0, 0, null);
            relLay.AddView(new CreateAnimationDrawer(context, currentBrush, canvas, bmp, true));
        }
		
        private void rotatePlus(object s, EventArgs e)
        {
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].iImage == 0)
                return;
            AnimationUtil.imgAttr [AnimationUtil.currentImage].fRotation += 5f;
            Bitmap bmp = null;
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].fRotation > 355f)
                AnimationUtil.imgAttr [AnimationUtil.currentImage].fRotation = 0f;
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].fRotation != 0f)
                bmp = ImageHelper.rotateImage(AnimationUtil.imgAttr [AnimationUtil.currentImage].sImage);
            else
                bmp = BitmapFactory.DecodeFile(AnimationUtil.imgAttr [AnimationUtil.currentImage].sImage);
            AnimationUtil.imgAttr [AnimationUtil.currentImage].bbox = new System.Drawing.Point(bmp.Width, bmp.Height);
            if (AnimationUtil.imgAttr [AnimationUtil.currentImage].attr != null)
            {
                if (AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.X != 0 || AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.Y != 0) 
                    AnimationUtil.theCanvas.DrawBitmap(bmp, AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.X, AnimationUtil.imgAttr [AnimationUtil.currentImage].attr.Y, null);
                else	
                    AnimationUtil.theCanvas.DrawBitmap(bmp, 0, 0, null);
            } else
                AnimationUtil.theCanvas.DrawBitmap(bmp, 0, 0, null);
            relLay.AddView(new CreateAnimationDrawer(context, currentBrush, canvas, bmp, true));
        }
		
        private void placeBitmap(object s, EventArgs e)
        {
            AnimationUtil.theImage.Add(AnimationUtil.currentImage, AnimationUtil.imgAttr);
            AnimationUtil.currentImage++;
            AnimationUtil.imgAttr.Add(new ImageAttr());
        }
    }
}


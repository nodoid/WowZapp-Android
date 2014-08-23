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
using Android.Media;
using Android.Graphics;

namespace wowZapp
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
    public class EditVideo : Activity, ISurfaceHolderCallback
    {
        private MediaRecorder mediaRecorder;
        private MediaPlayer mediaPlayer;
        private ISurfaceHolder holder;
        private SurfaceView surface;
        private string filename;
        private Context context;
        private int start = 0, end = 1, fps, timeUsed, camID;
        private Dialog LightboxDialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EditVideo);
            filename = base.Intent.GetStringExtra("filename");
            fps = base.Intent.GetIntExtra("fps", 12);
            timeUsed = base.Intent.GetIntExtra("duration", 20);

#if DEBUG
            Console.WriteLine("filename = {0}, fps = {1}, duration = {2}s", filename, fps, timeUsed);
#endif

            HorizontalScrollView hsvImages = FindViewById<HorizontalScrollView>(Resource.Id.hsvImages);
            HorizontalScrollView callouts = FindViewById<HorizontalScrollView>(Resource.Id.hsvCallouts);
            SurfaceView surface = FindViewById<SurfaceView>(Resource.Id.surfaceView1);
            context = callouts.Context;

            ImageButton returnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            returnBack.Tag = 0;
            ImageButton returnHome = FindViewById<ImageButton>(Resource.Id.btnHome);
            returnHome.Tag = 1;
            
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            
            ImageButton[] btn = new ImageButton[2];
            btn [0] = returnBack;
            btn [1] = returnHome;
            
            ImageHelper.setupButtonsPosition(btn, bottom, context);

            createImages(hsvImages);

            returnBack.Click += delegate
            {
                Finish();
            };
            
            returnHome.Click += delegate
            {
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.AddFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };

            mediaPlayer = new MediaPlayer();

            holder = surface.Holder;
            holder.AddCallback(this);
            holder.SetType(Android.Views.SurfaceType.PushBuffers);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            mediaPlayer.SetDisplay(holder);
            mediaPlayer.SetDataSource(filename);
            mediaPlayer.Prepared += new EventHandler(mediaPlayer_Prepared);
            mediaPlayer.PrepareAsync();
        }
        
        private void mediaPlayer_Prepared(object sender, EventArgs e)
        {
            mediaPlayer.Start();
        }
        
        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            mediaPlayer.Release();
        }
        
        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format fmt, int j, int k)
        {
        }

        private void createImages(HorizontalScrollView view)
        {
            MediaMetadataRetriever data = new MediaMetadataRetriever();
            data.SetDataSource(filename);
            int videoLength = Convert.ToInt32(data.ExtractMetadata(MetadataKey.Duration));
#if DEBUG
            Console.WriteLine("Duration = {0}, videoLength = {1}", data.ExtractMetadata(MetadataKey.Duration), videoLength);
#endif
            LinearLayout linLay = new LinearLayout(context);
            linLay.Orientation = Android.Widget.Orientation.Horizontal;
            linLay.LayoutParameters = new ViewGroup.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);

            ShowLightboxDialog(Application.Resources.GetString(Resource.String.videoEditGenerateThumbs));
            ImageView imageView = new ImageView(context);
            imageView.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(55f, context),
                                                                    (int)ImageHelper.convertDpToPixel(55f, context));

#if DEBUG
            for (int n = 0; n < videoLength; ++n)
            {
                using (Bitmap bmp = data.GetFrameAtTime((long)n * 1000, (int)Option.ClosestSync))
                {
                    if (bmp != null)
                        Console.WriteLine("frame {0} contains an image", n);
                }
            }
#endif

            for (int n = 10; n < /*videoLength*/timeUsed * fps; ++n)
            {
                using (Bitmap bmp = data.GetFrameAtTime((long)n, (int)Option.ClosestSync))
                {
                    if (bmp != null)
                    {
                        using (Bitmap smBmp = Bitmap.CreateScaledBitmap(bmp, (int)ImageHelper.convertDpToPixel(55f, context),
                                                                    (int)ImageHelper.convertDpToPixel(55f, context), true))
                        {
                            int m = new int();
                            m = n;
                            imageView.Tag = m;
                            imageView.Click += (object sender, EventArgs e) => frameClicked(sender, e);
                            imageView.SetImageBitmap(smBmp);
                        }
                    } else
                        imageView.SetBackgroundColor(Color.AliceBlue);
                }
                RunOnUiThread(() => linLay.AddView(imageView));
            }

            DismissLightboxDialog();  
            data.Release();
        }

        private void frameClicked(object sv, EventArgs e)
        {
            ImageView tmp = (ImageView)sv;
            int s = (int)tmp.Tag;
            if (start != s)
                start = s;
            if (s > start)
                end = s;
            if (s < start)
                GeneralUtils.Alert(context, Resource.String.videoErrorTitle, Resource.String.videoEditErrorWrongWay);
        }

        private void ShowLightboxDialog(string message)
        {
            LightboxDialog = new Dialog(this, Resource.Style.lightbox_dialog);
            LightboxDialog.SetContentView(Resource.Layout.LightboxDialog);
            ((TextView)LightboxDialog.FindViewById(Resource.Id.dialogText)).Text = message;
            LightboxDialog.Show();
        }
        
        private void DismissLightboxDialog()
        {
            if (LightboxDialog != null)
                LightboxDialog.Dismiss();
            
            LightboxDialog = null;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}


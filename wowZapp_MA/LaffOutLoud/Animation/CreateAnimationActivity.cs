using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using Android.Provider;
using Android.Graphics.Drawables;

using LOLAccountManagement;
using LOLApp_Common;

using LOLMessageDelivery.Classes.LOLAnimation;

using WZCommon;

using wowZapp.Animate;

namespace wowZapp.Animations
{
    public static class AnimationUtil
    {
        /*public static Animation animation
        {
            get;
            set;
        }*/
        public static bool result
		{ get; set; }
        public static string contentFilename
		{ get; set; }	
        public static Dictionary<int, List<ImageAttr>> theImage
		{ get; set; }
        public static System.Drawing.PointF imagePos
        {
            get;
            set;
        }
        public static System.Drawing.Point boundBox
        {
            get;
            set;
        }
        public static int currentImage
		{ get; set; }
		
        public static List<ImageAttr> imgAttr
		{ get; set; }
        public static Canvas theCanvas
        {
            get;
            set;
        }

        public static List<Bitmap> imagesForCanvas
        { get; set; }
    }
	
    public class ImageAttr
    {
        public System.Drawing.PointF attr;
        public System.Drawing.Point bbox;
        public int iImage;
        public string sImage;
        public float fRotation;
        public bool isSelected;
		
        public ImageAttr()
        {
            attr = new System.Drawing.PointF();
            bbox = new System.Drawing.Point();
            sImage = string.Empty;
            fRotation = 0f;
            iImage = 0;
            isSelected = false;
        }
    }
	
    [Activity (ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
    public partial class CreateAnimationActivity : Activity, SlidingDrawer.IOnDrawerCloseListener, SlidingDrawer.IOnDrawerOpenListener
    {
        private SlidingDrawer drawer;
        private bool drawerState;
        private Context context;
        private TextView[] percents;
        private ImageView[] colours;
        private int r, g, b, o, currentCell, currentLayer;
        private Button sliderToggle;
        private const int TEXT = 0, COMIX = 2, STAMP = 4;
        private const int CAMERA = 10, AUDIO = 11, AUDIOPACK = 12, CAMERA_RESOURCE = 13;
        private const int COLOR = 20;
        private const int FACEBOOK = 100;
        private Dialog ModalPreviewDialog;
        private ImageView modalPicture;
        private DBManager dbm;
        private AnimationUndoManager<UndoInfo> undoManager;
        private float myX, myY;
        private static float tolerance = 8f;
		
        private Bitmap myBitmap;

        private BrushItem currentBrush;
        private WZColor currentColor;
        private WZColor inactiveColor;

        public Dictionary<int, FrameInfo> FrameItems
		{ get; private set; }
        public Dictionary<int, AnimationAudioInfo> AudioItems
		{ get; private set; }
        public string AudioPath
        {
            get{ return System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "anim_voice.3gp");}
        }
        private FrameLayerPair pCurrentLayer;
        public FrameLayerPair CurrentLayer
        {
            get{ return pCurrentLayer;}
            private set{ pCurrentLayer = value;}
        }

        public event ColorChangedEventHandler ColorChanged;
        private RelativeLayout relLay;
        private byte[] brushImage;
        private Canvas canvas;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			
            SetContentView(Resource.Layout.CreateAnimation);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewLoginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            Header.headertext = Application.Context.Resources.GetString(Resource.String.animationTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
			
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            ImageButton imgUndo = FindViewById<ImageButton>(Resource.Id.imgUndo);
            ImageButton imgStamp = FindViewById<ImageButton>(Resource.Id.imgStamp);
            ImageButton imgPaint = FindViewById<ImageButton>(Resource.Id.imgPaint);
            ImageButton imgColour = FindViewById<ImageButton>(Resource.Id.imgColour);
            ImageButton imgText = FindViewById<ImageButton>(Resource.Id.imgText);
            ImageButton imgComix = FindViewById<ImageButton>(Resource.Id.imgComix);
            ImageButton imgSound = FindViewById<ImageButton>(Resource.Id.imgSound);
            ImageButton imgPicture = FindViewById<ImageButton>(Resource.Id.imgPicture);
            ImageButton imgConfig = FindViewById<ImageButton>(Resource.Id.imgConfig);
            ImageButton imgPlayAnimation = FindViewById<ImageButton>(Resource.Id.imgPlayAnimation);
            ImageButton btnHome = FindViewById<ImageButton>(Resource.Id.btnHome);
            drawer = FindViewById<SlidingDrawer>(Resource.Id.slidingDrawer1);
            sliderToggle = FindViewById<Button>(Resource.Id.handle);
            context = header.Context;
			
            brushImage = null;

            relLay = FindViewById<RelativeLayout>(Resource.Id.relativeLayout2);
            relLay.LayoutParameters = new LinearLayout.LayoutParams((int)wowZapp.LaffOutOut.Singleton.ScreenXWidth,
				(int)wowZapp.LaffOutOut.Singleton.ScreenXWidth); 

            if (AnimationUtil.imagesForCanvas == null)
                AnimationUtil.imagesForCanvas = new List<Bitmap>();

            if (currentColor == null)
            {

                currentColor = new WZColor(ImageHelper.convColToByteArray(Color.Black));
                inactiveColor = new WZColor(ImageHelper.convColToByteArray(Color.Gray));
                colorUtil.color = ImageHelper.convWZColorToColor(currentColor);
            }
				
            if (currentBrush == null)
            {
                using (Bitmap myBrush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.HardBrush1))
                {
                    MemoryStream stream = new MemoryStream();
                    myBrush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                    brushImage = stream.ToArray();
                    currentBrush = new BrushItem(4f, AnimationTypesBrushType.Normal, currentColor, inactiveColor, brushImage);
                }
            }
			
            myBitmap = Bitmap.CreateBitmap((int)wowZapp.LaffOutOut.Singleton.ScreenXWidth, 
			                                (int)wowZapp.LaffOutOut.Singleton.ScreenYHeight / 2 /*- (int)ImageHelper.convertDpToPixel (320f, context)*/, Bitmap.Config.Argb8888);
            AnimationUtil.imagesForCanvas.Add(myBitmap);
            canvas = new Canvas(myBitmap);
			
            if (AnimationUtil.theImage == null)
                AnimationUtil.theImage = new Dictionary<int, List<ImageAttr>>();
            if (AnimationUtil.imgAttr == null)
                AnimationUtil.imgAttr = new List<ImageAttr>();
            AnimationUtil.imgAttr.Add(new ImageAttr());
            AnimationUtil.currentImage = 0;
			
            relLay.AddView(new CreateAnimationDrawer(context, currentBrush, canvas, myBitmap));

            if (AudioItems == null || AudioItems.Count == 0)
                AudioItems = new Dictionary<int, AnimationAudioInfo>();
				
            CurrentLayer = new FrameLayerPair(1, 1);
			
            dbm = LaffOutOut.Singleton.dbm;
			
            drawerState = false;
			
            btnBack.Click += delegate
            {
                SaveAnimation();
                Finish();
            };
			
            btnHome.Click += delegate
            {
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.SetFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };
			
            imgUndo.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
            imgStamp.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
            imgPaint.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
            imgColour.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
            imgText.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
            imgComix.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
            imgSound.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
            imgPicture.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
            imgConfig.Click += (object sender, EventArgs e) => {
                openDrawer(sender, e);};
				
            imgPlayAnimation.Click += delegate
            {
                Intent m = new Intent(this, typeof(PlayAnimation));
                StartActivity(m);
            };
            modalPicture = new ImageView(context);
            percents = new TextView[4];
            colours = new ImageView[2];
        }

        public void OnDrawerOpened()
        {
            drawer.AnimateOpen();
            sliderToggle.SetBackgroundResource(Resource.Drawable.scrolldown);
            drawerState = !drawerState;
        }
		
        public void OnDrawerClosed()
        {
            drawer.AnimateClose();
            sliderToggle.SetBackgroundResource(Resource.Drawable.scrollup);
            drawerState = !drawerState;
        }
		
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            string filename = string.Empty;
            switch (requestCode)
            {
                case CAMERA:
                    if (resultCode == Result.Ok)
                    {
                        filename = data.GetStringExtra("filename");
                        if (!string.IsNullOrEmpty(filename))
                        {
                            displayPhoto(AnimationUtil.theCanvas, filename);
                        }
                    }
                    break;
                case CAMERA_RESOURCE:
                    if (resultCode == Result.Ok)
                    {
                        filename = getRealPathFromUri(data.Data);
                        if (!string.IsNullOrEmpty(filename))
                        {
                            displayPhoto(AnimationUtil.theCanvas, filename);
                        }
                    }
                    break;
                case COMIX:
                case STAMP:
                case TEXT:
                    if (AnimationUtil.result)
                    {
                        filename = AnimationUtil.contentFilename;
                        AnimationUtil.result = false;
                        if (!string.IsNullOrEmpty(filename))
                        {
                            Bitmap newBitmap = BitmapFactory.DecodeFile(AnimationUtil.contentFilename);
                            //canvas.DrawBitmap(newBitmap, 100, 100, null);
                            AnimationUtil.imagesForCanvas.Add(newBitmap);
                            AnimationUtil.currentImage++;
                            AnimationUtil.imgAttr [AnimationUtil.currentImage].attr = new System.Drawing.PointF(100, 100);
                            relLay.AddView(new CreateAnimationDrawer(context, currentBrush, canvas, newBitmap, true));
                        }
                    }
                    break;
                case COLOR:
                    if (resultCode == Result.Ok)
                    {
                        currentColor = new WZColor(ImageHelper.convColToByteArray(colorUtil.color));
                        currentBrush = new BrushItem(currentBrush.Thickness, currentBrush.BrushType, currentColor, inactiveColor, brushImage);
                    }
                    break;
            }
        }
		
        private void openDrawer(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            if (!drawerState)
                RunOnUiThread(() => drawer.Close());
				
            drawerState = true;
            LinearLayout content = FindViewById<LinearLayout>(Resource.Id.content);
            LinearLayout v = new LinearLayout(context);
            v.RemoveAllViews();
            content.RemoveAllViews();
            RunOnUiThread(() => sliderToggle.SetBackgroundResource(Resource.Drawable.scrolldown));
            switch (button.Id)
            {
                case Resource.Id.imgColour:	
                    Intent r = new Intent(this, typeof(ColorPickerDialog));
                    StartActivityForResult(r, COLOR);
                    break;
                case Resource.Id.imgConfig:
                    v = createConfigPicker();
                    RunOnUiThread(delegate
                    {
                        content.AddView(v);
                        drawer.Open();
                    });
				
                    drawerState = false;
                    break;
                case Resource.Id.imgPaint:
                    v = createPaintBrushes();
                    RunOnUiThread(delegate
                    {
                        content.AddView(v);
                        drawer.Open();
                    });
				
                    drawerState = false;
                    break;
                case Resource.Id.imgStamp:
                case Resource.Id.imgText:
                case Resource.Id.imgComix:
                    Intent i = new Intent(this, typeof(Messages.ContentPackActivity));
                    if (button.Id == Resource.Id.imgStamp)
                    {
                        i.PutExtra("pack", 4);
                        i.PutExtra("animated", true);
                        i.PutExtra("fromanimated", true);
                        StartActivityForResult(i, STAMP);
                    } else
                    {
                        if (button.Id == Resource.Id.imgText)
                        {
                            i.PutExtra("pack", 0);
                            i.PutExtra("animated", true);
                            i.PutExtra("fromanimated", true);
                            StartActivityForResult(i, TEXT);
                        } else
                        {
                            i.PutExtra("pack", 2);
                            i.PutExtra("animated", true);
                            i.PutExtra("fromanimated", true);
                            StartActivityForResult(i, COMIX);
                        }
                    }
                    break;
                case Resource.Id.imgSound:
                case Resource.Id.imgPicture:
                    ShowModalPreviewDialog(button.Id == Resource.Id.imgPicture ? 0 : 1);
                    break;
            }
        }

        private void transition_Selected(object s, EventArgs e)
        {
            ImageView image = (ImageView)s;
            int tag = (int)image.Tag;
            switch (tag)
            {
                case 1:
				//tranDict.Add (currentCell, AnimationTransitions.FadeIn);
                    break;
                case 2:
				//tranDict.Add (currentCell, AnimationTransitions.FadeOut);
                    break;
                case 3:
				//tranDict.Add (currentCell, AnimationTransitions.Rotate);
                    break;
                case 4:
				//tranDict.Add (currentCell, AnimationTransitions.Move);
                    break;
            }
            //transitions.Add (tranDict);
            drawer.Close();
        }
		
        private void brush_Clicked(object s, EventArgs e)
        {
            ImageView image = (ImageView)s;
            int tag = (int)image.Tag;
            Bitmap brush = null;
            byte[] myArray = null;
            switch (tag)
            {
                case 1:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.HardBrush1))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(4f, AnimationTypesBrushType.Normal, currentColor, inactiveColor, myArray);
                    break;
                case 2:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.HardBrush2))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(6f, AnimationTypesBrushType.Normal, currentColor, inactiveColor, myArray);
                    break;
                case 3:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.HardBrush3))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(8f, AnimationTypesBrushType.Normal, currentColor, inactiveColor, myArray);
                    break;
                case 4:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.HardBrush4))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(10f, AnimationTypesBrushType.Normal, currentColor, inactiveColor, myArray);
                    break;
                case 5:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.HardBrush5))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(12f, AnimationTypesBrushType.Normal, currentColor, inactiveColor, myArray);
                    break;
                case 6:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.HardBrush6))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(14f, AnimationTypesBrushType.Normal, currentColor, inactiveColor, myArray);
                    break;
                case 10:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.SoftBrush1))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(4f, AnimationTypesBrushType.Spray, currentColor, inactiveColor, myArray);
                    break;
                case 11:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.SoftBrush2))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(6f, AnimationTypesBrushType.Spray, currentColor, inactiveColor, myArray);
                    break;
                case 12:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.SoftBrush3))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(8f, AnimationTypesBrushType.Spray, currentColor, inactiveColor, myArray);
                    break;
                case 13:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.SoftBrush4))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(10f, AnimationTypesBrushType.Spray, currentColor, inactiveColor, myArray);
                    break;
                case 14:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.SoftBrush5))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(12f, AnimationTypesBrushType.Spray, currentColor, inactiveColor, myArray);
                    break;
                case 15:
                    using (brush = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.SoftBrush6))
                    {
                        MemoryStream stream = new MemoryStream();
                        brush.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        myArray = stream.ToArray();
                    }
                    currentBrush = new BrushItem(14f, AnimationTypesBrushType.Spray, currentColor, inactiveColor, myArray);
                    break;
            }
            brushImage = myArray;
            drawer.Close();
        }
		
        private void ShowModalPreviewDialog(int choice)
        {
            ModalPreviewDialog = new Dialog(this, Resource.Style.lightbox_dialog);
            ModalPreviewDialog.SetContentView(choice == 0 ? Resource.Layout.ModalPreviewDialogButtons : Resource.Layout.ModalPreviewDialogAudio);
            bool[] test = new bool[2];
            test = GeneralUtils.CamRec();
            if (choice == 0)
            {
                if (test [0] == false || !test [1] == false)
                    ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnTop)).Enabled = false;
            }
				
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnTop)).Click += delegate
            {
                Intent a = null;
                if (choice == 0)
                    a = new Intent(this, typeof(CameraVideo.CameraTakePictureActivity));
                else
                    a = new Intent(this, typeof(Messages.ComposeAudioMessageActivity));
                RunOnUiThread(() => DismissModalPreviewDialog());
                StartActivityForResult(a, choice == 0 ? CAMERA : AUDIO);
            };
				
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnMiddle)).Click += delegate
            {
                if (choice == 0)
                {
                    RunOnUiThread(() => DismissModalPreviewDialog());
                    var imageIntent = new Intent();
                    imageIntent.SetType("image/*");
                    imageIntent.SetAction(Intent.ActionGetContent);
                    StartActivityForResult(Intent.CreateChooser(imageIntent, "Choose Image"), CAMERA_RESOURCE);	
                }
            };
			
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnBottom)).Click += delegate
            {
                if (choice == 0)
                {
                    RunOnUiThread(() => DismissModalPreviewDialog());
                    onlineChoice();
                } else
                {
                    RunOnUiThread(() => DismissModalPreviewDialog());
                    Intent i = new Intent(this, typeof(Messages.ContentPackActivity));
                    i.PutExtra("pack", 5);
                    StartActivityForResult(i, AUDIOPACK);
                }
            };

            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnCancel)).Click += delegate
            {
                DismissModalPreviewDialog();
            };
			
            ModalPreviewDialog.Show();
        }
		
        private void onlineChoice()
        {
            RunOnUiThread(delegate
            {
                ModalPreviewDialog = new Dialog(this, Resource.Style.lightbox_dialog);
                ModalPreviewDialog.SetContentView(Resource.Layout.ModalOnlineSelector);
                ((ImageView)ModalPreviewDialog.FindViewById(Resource.Id.imgItemPic)).Click += delegate
                {
                    RunOnUiThread(() => DismissModalPreviewDialog());
                    Intent i = new Intent(this, typeof(Photoalbums.GetPhotoAlbumActivity));
                    i.PutExtra("media", false);
                    i.PutExtra("network", 3);
                    StartActivityForResult(i, FACEBOOK);
                };
			
                ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnCancel)).Click += delegate
                {
                    RunOnUiThread(() => DismissModalPreviewDialog());
                    ShowModalPreviewDialog(0);
                };
                ModalPreviewDialog.Show();
            });
        }
		
        public void DismissModalPreviewDialog()
        {
            if (ModalPreviewDialog != null)
                ModalPreviewDialog.Dismiss();
			
            ModalPreviewDialog = null;
        }
		
        private string getRealPathFromUri(Android.Net.Uri contentUri)
        {
            string[] proj = { MediaStore.Images.ImageColumns.Data };
            ICursor cursor = this.ManagedQuery(contentUri, proj, null, null, null);
            int column_index = cursor.GetColumnIndexOrThrow(MediaStore.Images.ImageColumns.Data);
            cursor.MoveToFirst();
            return cursor.GetString(column_index);
        }
    }
}


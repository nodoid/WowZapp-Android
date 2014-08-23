using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LOLAccountManagement;
using LOLApp_Common;
using LOLMessageDelivery;
using Android.Webkit;
using Android.Graphics;
using Android.Media;

using WZCommon;

namespace wowZapp.Messages
{
    public static class ContentPackItemsUtil
    {
        public static int contentPackItemID
		{ get; set; }
        public static byte[] content
		{ get; set; }
    }

    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class ContentPackItemActivity : Activity
    {
        private Context context;
        private DBManager dbm;
        private LOLCodeLibrary.GenericEnumsContentPackType packType;
        private int contentPackID, currentStep;
        private string ContentPath, iconFile;
        private List<ContentPackItemDB> packItems;
        private LinearLayout listContentPack;
        private Dialog LightboxDialog;
        private Dialog ModalPreviewDialog;
        private byte[] currentPreviewItemData;
        private Dictionary<string, ContentPackItem> contentPackItems;
        private bool isAnimation, isSet, isPreview, fromAnimation;
        private LinearLayout[] layouts;
        private ContentPackItem[] result;
        private WebView previewWebImage;
        private ImageView previewImage;
        private float size;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ContentPackItem);
            fromAnimation = base.Intent.GetBooleanExtra("fromanimation", false);
            listContentPack = FindViewById<LinearLayout>(Resource.Id.listContentPack);
            TextView txtContentPackHeader = FindViewById<TextView>(Resource.Id.txtContentPackHeader);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, txtContentPackHeader, relLayout, txtContentPackHeader.Context);
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            isAnimation = base.Intent.GetBooleanExtra("animated", false); // false = not from animation
            Header.headertext = base.Intent.GetStringExtra("ContentPackTitle").ToUpper();
            currentStep = base.Intent.GetIntExtra("CurrentStep", 1);
            context = listContentPack.Context;

            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(txtContentPackHeader.Context);
            txtContentPackHeader.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            txtContentPackHeader.Text = Header.headertext;
            isSet = false;
            contentPackID = base.Intent.GetIntExtra("packid", -1);
            iconFile = "";
            packType = ContentPackActivity.packType;
            ContentPath = wowZapp.LaffOutOut.Singleton.ContentDirectory;
            dbm = wowZapp.LaffOutOut.Singleton.dbm;
            packItems = new List<ContentPackItemDB>();
            contentPackItems = new Dictionary<string, ContentPackItem>();
            btnBack.Click += delegate
            {
                sendBack(true);
            };
            Button btnDone = FindViewById<Button>(Resource.Id.btnDone);
            btnDone.Click += delegate
            {
                sendBack(!isSet);
            };
            isPreview = false;
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            ImageHelper.setupButtonsPosition(btnBack, btnDone, bottom, context);
            layouts = new LinearLayout[4];
            size = 100f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                size *= wowZapp.LaffOutOut.Singleton.bigger;
            getContentPackItems();
        }

        private void sendBack(bool ok = false)
        {
            if (!fromAnimation)
                ComposeGenericResults.success = !ok;
            else
            {
                Animations.AnimationUtil.result = !string.IsNullOrEmpty(iconFile) ? true : false;
                Animations.AnimationUtil.contentFilename = iconFile;
            }
            Finish();
        }
		
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (!hasFocus && isPreview && wowZapp.LaffOutOut.Singleton.resizeFonts)
            {
                RunOnUiThread(delegate
                {
                    ImageHelper.resizeLayout(layouts, context);
                    if (previewImage != null)
                        ImageHelper.resizeWidget(previewImage, context);
                    else
                        ImageHelper.resizeWidget(previewWebImage, context);
                });
            }
        }

        private bool CheckContentPackItemIconExist(ContentPackItemDB contentPackItem)
        {
            return contentPackItem != null ? File.Exists(System.IO.Path.Combine(ContentPath, contentPackItem.ContentPackItemIconFile)) : false;
        }

        private bool CheckContentPackItemDataExist(ContentPackItemDB contentPackItem)
        {
            return contentPackItem != null ? File.Exists(System.IO.Path.Combine(ContentPath, contentPackItem.ContentPackDataFile)) : false;
        }

        private byte[] GetBufferFromPropertyFile(string filename)
        {
            string dataFile = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, filename);
            if (!File.Exists(dataFile))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("File {0} doesn't exist", dataFile);
#endif
                return new byte[0];
            }
            bool rv = false;
            byte[] dataBuffer = null;
            RunOnUiThread(delegate
            {
                dataBuffer = File.ReadAllBytes(dataFile);
                rv = dataBuffer == null ? true : false;
            });
            return rv == true ? new byte[0] : dataBuffer;
        }

        private List<ContentPackItemDB> GetAllLocalContentPackItems(bool iconOnly)
        {
            List<ContentPackItemDB> allContentPackItms = dbm.GetAllContentPackItems(contentPackID);
            List<ContentPackItemDB> toReturn = new List<ContentPackItemDB>();

            foreach (ContentPackItemDB eachContentPackItem in allContentPackItms)
            {
                if (iconOnly)
                {
                    if (this.CheckContentPackItemIconExist(eachContentPackItem))
                    {
                        eachContentPackItem.ContentPackItemIcon = GetBufferFromPropertyFile(eachContentPackItem.ContentPackItemIconFile);

                        if (!string.IsNullOrEmpty(eachContentPackItem.ContentPackDataFile) && this.CheckContentPackItemDataExist(eachContentPackItem))
                            eachContentPackItem.ContentPackData = GetBufferFromPropertyFile(eachContentPackItem.ContentPackDataFile);

                        toReturn.Add(eachContentPackItem);
                    }
                } else
                {
                    if (this.CheckContentPackItemIconExist(eachContentPackItem) && this.CheckContentPackItemDataExist(eachContentPackItem))
                    {
                        eachContentPackItem.ContentPackItemIcon = GetBufferFromPropertyFile(eachContentPackItem.ContentPackItemIconFile);
                        eachContentPackItem.ContentPackData = GetBufferFromPropertyFile(eachContentPackItem.ContentPackDataFile);
                        toReturn.Add(eachContentPackItem);
                    }
                }
            }
            return toReturn;
        }

        private void getContentPackItems()
        {
            RunOnUiThread(() => ShowLightboxDialog(Application.Resources.GetString(Resource.String.contentGrabbingYourPack)));
            LOLConnectClient connect = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            contentPackItems = GetAllLocalContentPackItems(true).ToDictionary(s => Convert.ToString(s.ContentPackItemID), s => ContentPackItemDB.ConvertFromContentPackItemDB(s));

            if (contentPackItems.Count != 0)
                DisplayContentPacks(contentPackItems);
            List<int> excludeContentPackItemsIDs = new List<int>();
            excludeContentPackItemsIDs = contentPackItems.Values.Select(s => s.ContentPackItemID).ToList();

            connect.ContentPackGetPackItemsLightCompleted += new EventHandler<ContentPackGetPackItemsLightCompletedEventArgs>(connect_ContentPackGetPackItemsLightCompleted);
            connect.ContentPackGetPackItemsLightAsync(contentPackID, ContentPackItem.ItemSize.Tiny, excludeContentPackItemsIDs, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
        }

        private void DisplayContentPacks(Dictionary<string, ContentPackItem> contentPackItem)
        {
            isPreview = false;
            RunOnUiThread(delegate
            {
                List<ContentPackItemDB> contentPackItemList = new List<ContentPackItemDB>();

                LinearLayout layout = new LinearLayout(context);
                layout.Orientation = Android.Widget.Orientation.Horizontal;
                layout.SetGravity(GravityFlags.Center);
                layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, 0, (int)ImageHelper.convertDpToPixel(10f, context));
                layout.SetGravity(GravityFlags.CenterHorizontal);

                int counter = 0;
                result = contentPackItem.Values.ToArray();
                foreach (ContentPackItem eachItem in contentPackItem.Values)
                {
                    counter++;
                    
                    ImageView contentpackpic = new ImageView(context);
                    contentpackpic.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(size, context), (int)ImageHelper.convertDpToPixel(size, context));
                    contentpackpic.SetPadding(0, 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);

                    using (MemoryStream stream = new MemoryStream (eachItem.ContentPackItemIcon))
                    {
                        using (Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromStream (stream, "Profile"))
                        {
                            contentpackpic.SetImageDrawable(draw);
                            int itemId = new int();
                            itemId = counter - 1;
                            contentpackpic.Click += delegate
                            {
                                ShowModalPreviewDialog(itemId);
                            };

                            layout.AddView(contentpackpic);
                        }
                    }

                    ContentPackItemDB contentPackItemDB = ContentPackItemDB.ConvertFromContentPackItem(eachItem);
                    SaveContentPackItem(contentPackItemDB);
                    contentPackItemList.Add(contentPackItemDB);

                    if (counter == contentPackItems.Count || layout.ChildCount == 3)
                    {
                        RunOnUiThread(() => listContentPack.AddView(layout));
                        layout = new LinearLayout(context);
                        layout.Orientation = Android.Widget.Orientation.Horizontal;
                        layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, 0, (int)ImageHelper.convertDpToPixel(10f, context));
                        layout.SetGravity(GravityFlags.CenterHorizontal);
                    }

                }

                dbm.InsertOrUpdateContentPackItems(contentPackItemList);

                DismissLightboxDialog();
            });
        }
		
        private void displayImage(byte[] image, ImageView contactPic)
        {
            if (image.Length > 0)
            {
                using (Android.Graphics.Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)size, (int)size, this.Resources))
                {
                    RunOnUiThread(delegate
                    {
                        contactPic.SetImageBitmap(myBitmap);
                    });
                }
            }
        }
		
        private void connect_ContentPackGetPackItemsLightCompleted(object s, ContentPackGetPackItemsLightCompletedEventArgs e)
        {
            isPreview = false;
            LOLConnectClient service = (LOLConnectClient)s;
            service.ContentPackGetPackItemsLightCompleted -= connect_ContentPackGetPackItemsLightCompleted;
            if (e.Error == null)
            {
                RunOnUiThread(delegate
                {
                    if (result == null)
                        result = e.Result.contentPackItems.ToArray();
                    if (contentPackItems.Count == result.Length)
                        return;
                    List<ContentPackItemDB> contentPackItemList = new List<ContentPackItemDB>();

                    LinearLayout layout = new LinearLayout(context);
                    layout.Orientation = Android.Widget.Orientation.Horizontal;
                    layout.SetGravity(GravityFlags.Center);
                    layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, 0, (int)ImageHelper.convertDpToPixel(10f, context));
                    layout.SetGravity(GravityFlags.CenterHorizontal);

                    int counter = 0;

                    for (int z = 0; z < result.Length; z++)
                    {
                        counter++;

                        if (result [z].Errors.Count > 0)
                        {
                            #if DEBUG
                            System.Diagnostics.Debug.WriteLine(StringUtils.CreateErrorMessageFromGeneralErrors(result [z].Errors));
                            #endif
                        } else
                        {
                            ImageView contentpackpic = new ImageView(context);
							
                            contentpackpic.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(size, context), (int)ImageHelper.convertDpToPixel(size, context));
                            contentpackpic.SetPadding(0, 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);
							
                            using (MemoryStream stream = new MemoryStream (result [z].ContentPackItemIcon))
                            {
                                using (Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromStream (stream, "Profile"))
                                {
                                    contentpackpic.SetImageDrawable(draw);
                                    int itemId = new int();
                                    itemId = z;
                                    contentpackpic.Click += delegate
                                    {
                                        ShowModalPreviewDialog(itemId);
                                    };

                                    layout.AddView(contentpackpic);
                                }
                            }
                            ContentPackItemDB contentPackItemDB = ContentPackItemDB.ConvertFromContentPackItem(result [z]);
                            SaveContentPackItem(contentPackItemDB);
                            contentPackItemList.Add(contentPackItemDB);

                            if (counter == result.Length || layout.ChildCount == 3)
                            {
                                RunOnUiThread(() => listContentPack.AddView(layout));
                                layout = new LinearLayout(context);
                                layout.Orientation = Android.Widget.Orientation.Horizontal;
                                layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, 0, (int)ImageHelper.convertDpToPixel(10f, context));
                                layout.SetGravity(GravityFlags.CenterHorizontal);
                            }
                        }
                    }
                    dbm.InsertOrUpdateContentPackItems(contentPackItemList);

                    DismissLightboxDialog();
                });
            } else
            {
                string m = packType + Application.Context.Resources.GetString(Resource.String.errorDownloadPackItems) + e.Error.Message + ")";
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorContentPackFail), m);
                });
            }
        }

        private void playAudio(ProgressBar pb, LOLCodeLibrary.GenericEnumsContentPackType pack)
        {
            string temppath = "";
            if (packType == LOLCodeLibrary.GenericEnumsContentPackType.SoundFX) 
                temppath = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "tempaudio-soundFX.wav");
            else
                temppath = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "tempaudio-comicon.wav");
            if (!File.Exists(temppath))
                return;
				
            AudioPlayer csfx = new AudioPlayer(context);
            int duration = csfx.findDuration(temppath);
            pb.Max = duration / 2;

            new Thread(new ThreadStart(() => {
                csfx.playFromFile(temppath);
                for (int i = 0; i <= duration / 2; i++)
                {
                    this.RunOnUiThread(() => {
                        pb.Progress = i;
                    });
                    Thread.Sleep(1);
                }
            })).Start();
        }
			
		
        private void ShowModalPreviewDialog(int itemId)
        {
            isPreview = true;
            ProgressBar pb = null;
            ImageButton ib = null;
            ModalPreviewDialog = new Dialog(this, Resource.Style.lightbox_dialog);
            if (packType == LOLCodeLibrary.GenericEnumsContentPackType.Emoticon)
            {
                ModalPreviewDialog.SetContentView(Resource.Layout.ModalPreviewGIFDialog);
                if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                {
                    layouts [0] = ((LinearLayout)ModalPreviewDialog.FindViewById(Resource.Id.linearLayout1));
                    layouts [1] = ((LinearLayout)ModalPreviewDialog.FindViewById(Resource.Id.linearLayout2));
                    layouts [3] = ((LinearLayout)ModalPreviewDialog.FindViewById(Resource.Id.linearLayout3));
                    layouts [2] = ((LinearLayout)ModalPreviewDialog.FindViewById(Resource.Id.linearLayout4));
                    previewWebImage = ((WebView)ModalPreviewDialog.FindViewById(Resource.Id.webPreview));
                }
                ((WebView)ModalPreviewDialog.FindViewById(Resource.Id.webPreview)).VerticalScrollBarEnabled = false;
                ((WebView)ModalPreviewDialog.FindViewById(Resource.Id.webPreview)).HorizontalScrollBarEnabled = false;
                ((WebView)ModalPreviewDialog.FindViewById(Resource.Id.webPreview)).LoadDataWithBaseURL(null,
                "<body style=\"margin:0;padding:0;background:#8e8f8f;\"><div style=\"margin:10px;\">Loading...</div></body>", "text/html", "UTF-8", null);
                LOLConnectClient connect = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                connect.ContentPackItemGetDataCompleted += new EventHandler<ContentPackItemGetDataCompletedEventArgs>(connect_ContentPackItemGetDataCompleted);
                connect.ContentPackItemGetDataAsync(result [itemId].ContentPackItemID, ContentPackItem.ItemSize.Small, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
            } else
            {
                ModalPreviewDialog.SetContentView(Resource.Layout.ModalPreviewDialog);
                pb = ((ProgressBar)ModalPreviewDialog.FindViewById(Resource.Id.timerBar));
                ib = ((ImageButton)ModalPreviewDialog.FindViewById(Resource.Id.playButton));
                if (packType != LOLCodeLibrary.GenericEnumsContentPackType.Comicon && packType != LOLCodeLibrary.GenericEnumsContentPackType.SoundFX)
                    pb.Visibility = ib.Visibility = ViewStates.Invisible;
                else
                    ib.Click += delegate
                    {
                        playAudio(pb, packType);
                    };
				
                if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                {
                    layouts [0] = ((LinearLayout)ModalPreviewDialog.FindViewById(Resource.Id.linearLayout1));
                    layouts [1] = ((LinearLayout)ModalPreviewDialog.FindViewById(Resource.Id.linearLayout2));
                    layouts [3] = ((LinearLayout)ModalPreviewDialog.FindViewById(Resource.Id.linearLayout3));
                    layouts [2] = ((LinearLayout)ModalPreviewDialog.FindViewById(Resource.Id.linearLayout4));
                    previewImage = ((ImageView)ModalPreviewDialog.FindViewById(Resource.Id.imgItemPic));
                }
                using (MemoryStream stream = new MemoryStream (result [itemId].ContentPackItemIcon))
                {
                    using (Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromStream (stream, "Profile"))
                    {
                        ((ImageView)ModalPreviewDialog.FindViewById(Resource.Id.imgItemPic)).SetImageDrawable(draw);
                    }
                }
            }

            ((TextView)ModalPreviewDialog.FindViewById(Resource.Id.txtItemTitle)).Text = result [itemId].ContentItemTitle;
            
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnAdd)).Click += delegate
            {
                if (!isAnimation)
                {
                    MessageStep msgStep = new MessageStep();
                    switch (packType)
                    {
                        case LOLCodeLibrary.GenericEnumsContentPackType.Comicon:
                            msgStep.StepType = MessageStep.StepTypes.Comicon;
                            break;
                        case LOLCodeLibrary.GenericEnumsContentPackType.Comix:
                            msgStep.StepType = MessageStep.StepTypes.Comix;
                            break;
                        case LOLCodeLibrary.GenericEnumsContentPackType.Emoticon:
                            msgStep.StepType = MessageStep.StepTypes.Emoticon;
                            break;
                        case LOLCodeLibrary.GenericEnumsContentPackType.SoundFX:
                            msgStep.StepType = MessageStep.StepTypes.SoundFX;
                            break;
                    }
                    ComposeMessageMainUtil.contentPackID [currentStep] = msgStep.ContentPackItemID = ContentPackItemsUtil.contentPackItemID = result [itemId].ContentPackItemID;

                    if (currentStep > ComposeMessageMainUtil.msgSteps.Count)
                    {
                        msgStep.StepNumber = ComposeMessageMainUtil.msgSteps.Count + 1;
                        ComposeMessageMainUtil.msgSteps.Add(msgStep);


                    } else
                    {
                        msgStep.StepNumber = currentStep;
                        ComposeMessageMainUtil.msgSteps [currentStep - 1] = msgStep;
                    }

                    ContentPackItemsUtil.content = result [itemId].ContentPackItemIcon;
                    if (currentStep == 1)
                    {
                        DismissModalPreviewDialog();
                        isSet = true;
                        sendBack();
                    } else
                    {
                        DismissModalPreviewDialog();
                        isSet = true;
                        isPreview = false;
                        sendBack();
                    }
                } else
                {
                    DismissModalPreviewDialog();
                    isSet = true;
                    sendBack();
                }
            };
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnCancel)).Click += delegate
            {
                DismissModalPreviewDialog();
            };

            ModalPreviewDialog.Show();

            if (packType == LOLCodeLibrary.GenericEnumsContentPackType.Comicon ||
                packType == LOLCodeLibrary.GenericEnumsContentPackType.SoundFX)
            {
                LOLConnectClient connect = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                connect.ContentPackItemGetDataCompleted += new EventHandler<ContentPackItemGetDataCompletedEventArgs>(connect_ContentPackItemGetDataCompleted);
                connect.ContentPackItemGetDataAsync(result [itemId].ContentPackItemID, ContentPackItem.ItemSize.Small, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
            }
        }

        void connect_ContentPackItemGetDataCompleted(object sender, ContentPackItemGetDataCompletedEventArgs e)
        {
            LOLConnectClient connect = (LOLConnectClient)sender;
            connect.ContentPackItemGetDataCompleted -= connect_ContentPackItemGetDataCompleted;
            if (packType == LOLCodeLibrary.GenericEnumsContentPackType.Emoticon)
            {
                string base64String = System.Convert.ToBase64String(e.Result.ItemData, 0, e.Result.ItemData.Length);
                ((WebView)ModalPreviewDialog.FindViewById(Resource.Id.webPreview)).LoadDataWithBaseURL(null,
                    "<body style=\"margin:0;padding:0;background:#8e8f8f;\"><img src=\"data:image/gif;base64," + base64String
                    + "\" style=\"width:100%;height:100%;margin:0;padding:0;border:none;\" /></body>", "text/html", "UTF-8", null);
            }

            if (packType == LOLCodeLibrary.GenericEnumsContentPackType.SoundFX)
            {
                string temppath = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "tempaudio-soundFX.wav");
                if (File.Exists(temppath))
                    File.Delete(temppath);

                byte[] audio = e.Result.ItemData; 
                System.IO.File.WriteAllBytes(temppath, audio);
				
            }

            if (packType == LOLCodeLibrary.GenericEnumsContentPackType.Comicon)
            {
                string temppath = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "tempaudio-comicon.wav");
                if (File.Exists(temppath))
                    File.Delete(temppath);

                byte[] audio = e.Result.ItemData;
                System.IO.File.WriteAllBytes(temppath, audio);
            }
        }

        public void DismissModalPreviewDialog()
        {
            if (ModalPreviewDialog != null)
                ModalPreviewDialog.Dismiss();
            isPreview = false;
            ModalPreviewDialog = null;
        }

        private void SaveContentPackItem(ContentPackItemDB pack)
        {
            pack.ContentPackDataFile = StringUtils.ConstructContentPackItemDataFile(pack.ContentPackItemID);
            pack.ContentPackItemIconFile = StringUtils.ConstructContentPackItemIconFilename(pack.ContentPackItemID);
            SaveContentPackItemBuffers(pack);
        }

        private void SaveContentPackItemBuffers(ContentPackItemDB contentPackItem)
        {
            if (contentPackItem.ContentPackItemIcon != null && contentPackItem.ContentPackItemIcon.Length > 0)
            {
                RunOnUiThread(delegate
                {
                    iconFile = System.IO.Path.Combine(ContentPath, contentPackItem.ContentPackItemIconFile);

                    if (File.Exists(iconFile))
                        File.Delete(iconFile);

                    try
                    {
                        File.WriteAllBytes(iconFile, contentPackItem.ContentPackItemIcon);
                    } catch (IOException ex)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Error saving content pack item icons - message {0}", ex.Message);
#endif
                    }
                });
            }

            if (contentPackItem.ContentPackData != null && contentPackItem.ContentPackData.Length > 0)
            {
                RunOnUiThread(delegate
                {
                    string dataFile = System.IO.Path.Combine(this.ContentPath, contentPackItem.ContentPackDataFile);

                    if (File.Exists(dataFile))
                        File.Delete(dataFile);

                    try
                    {
                        File.WriteAllBytes(dataFile, contentPackItem.ContentPackData);
                    } catch (IOException e)
                    {
#if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("Error saving content pack item data file! Message: " + e.Message);
#endif
                    }
                });
            }//end if
        }

        public void ShowLightboxDialog(string message)
        {
            isPreview = false;
            LightboxDialog = new Dialog(this, Resource.Style.lightbox_dialog);
            LightboxDialog.SetContentView(Resource.Layout.LightboxDialog);
            ((TextView)LightboxDialog.FindViewById(Resource.Id.dialogText)).Text = message;
            LightboxDialog.Show();
        }

        public void DismissLightboxDialog()
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
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;

using WZCommon;

using LOLApp_Common;
using LOLAccountManagement;
using Android.Widget;
using Android.Views;
using Android.Util;

namespace wowZapp.Messages
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class ContentPackActivity : Activity
    {
        private DBManager dbm;
        private Context context;
        private int packID;
        private bool cPackIsLoaded, isAnimation, fromAnimation;
        private LinearLayout listContentPack;
        private Dialog LightboxDialog;
        private string contentPath;
        private Dictionary<string, ContentPack> contentPacks;
        private float size;

        public static LOLCodeLibrary.GenericEnumsContentPackType packType
        { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ContentPack);
            isAnimation = base.Intent.GetBooleanExtra("animated", false);
            fromAnimation = base.Intent.GetBooleanExtra("fromanimated", false);
            listContentPack = FindViewById<LinearLayout>(Resource.Id.listContentPack);
            TextView txtContentPackHeader = FindViewById<TextView>(Resource.Id.txtContentPackHeader);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, txtContentPackHeader, relLayout, txtContentPackHeader.Context);
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);

            context = listContentPack.Context;

            Header.fontsize = 36f;
            
            int pack = base.Intent.GetIntExtra("pack", -1);
			
            string text = "";
            switch (pack)
            {
                case 0:
                    packType = LOLCodeLibrary.GenericEnumsContentPackType.Callout;
                    text = Application.Context.GetString(Resource.String.contentCallout);
                    break;
                case 1:
                    packType = LOLCodeLibrary.GenericEnumsContentPackType.Comicon;
                    text = Application.Context.GetString(Resource.String.contentComicon);
                    break;
                case 2:
                    packType = LOLCodeLibrary.GenericEnumsContentPackType.Comix;
                    text = Application.Context.GetString(Resource.String.contentComix);
                    break;
                case 3:
                    packType = LOLCodeLibrary.GenericEnumsContentPackType.Emoticon;
                    text = Application.Context.GetString(Resource.String.contentEmoticon);
                    break;
                case 4:
                    packType = LOLCodeLibrary.GenericEnumsContentPackType.RubberStamp;
                    text = Application.Context.GetString(Resource.String.contentRubber);
                    break;
                case 5:
                    packType = LOLCodeLibrary.GenericEnumsContentPackType.SoundFX;
                    text = Application.Context.GetString(Resource.String.contentSoundFX
                    );
                    break;
            }
            Header.headertext = text;
            ImageHelper.fontSizeInfo(txtContentPackHeader.Context);
            txtContentPackHeader.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            txtContentPackHeader.Text = Header.headertext;
            dbm = wowZapp.LaffOutOut.Singleton.dbm;
            cPackIsLoaded = false;
            contentPath = wowZapp.LaffOutOut.Singleton.ContentDirectory;
            packID = -1;
            contentPacks = new Dictionary<string, ContentPack>();
            btnBack.Click += delegate
            {
                Finish();
            };
            size = 100f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                size *= wowZapp.LaffOutOut.Singleton.bigger;
            getContentPack();
        }

        private void finishactivity()
        {
            Intent resultData = new Intent();
            resultData.PutExtra("packid", packID);
            resultData.PutExtra("isloaded", cPackIsLoaded);
            SetResult(Result.Ok, resultData);
            Finish();
        }

        private List<ContentPackDB> GetAllLocalContentPacks(LOLCodeLibrary.GenericEnumsContentPackType contentPackType)
        {
            List<ContentPackDB> allContentPacks = dbm.GetAllContentPacks(packType);
            List<ContentPackDB> toReturn = new List<ContentPackDB>();
            foreach (ContentPackDB eachContentPack in allContentPacks)
            {
                if (CheckContentPackDataExists(eachContentPack))
                {
                    eachContentPack.ContentPackIcon = GetBufferFromPropertyFile(eachContentPack.ContentPackIconFile);
                    eachContentPack.ContentPackAd = GetBufferFromPropertyFile(eachContentPack.ContentPackAdFile);
                    toReturn.Add(eachContentPack);
                }
            }
            return toReturn;
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


        private bool CheckContentPackDataExists(ContentPackDB contentPack)
        {
            if (contentPack != null)
                return File.Exists(Path.Combine(contentPath, contentPack.ContentPackIconFile)) &&
                    File.Exists(Path.Combine(contentPath, contentPack.ContentPackAdFile));
            else
                return false;
        }

        private void getContentPack()
        {
            RunOnUiThread(() => ShowLightboxDialog(Application.Resources.GetString(Resource.String.contentGrabbingYourPack)));
            LOLConnectClient connect = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            contentPacks = GetAllLocalContentPacks(packType).ToDictionary(s => Convert.ToString(s.ContentPackID),
                s => ContentPackDB.ConvertFromContentPackDB(s));
            
            if (contentPacks.Count != 0)
                DisplayContentPack(contentPacks);

            List<int> excludeContentPacks = new List<int>();
            excludeContentPacks = contentPacks.Values.Select(s => s.ContentPackID).ToList();

            connect.ContentPacksGetByTypeAndAccountIDCompleted += new EventHandler<ContentPacksGetByTypeAndAccountIDCompletedEventArgs>(connect_ContentPacksGetByTypeCompleted);
            connect.ContentPacksGetByTypeAndAccountIDAsync(AndroidData.CurrentUser.AccountID, packType, excludeContentPacks, new Guid(AndroidData.ServiceAuthToken));
        }

        private void DisplayContentPack(Dictionary<string, ContentPack> contentPacks)
        {
            RunOnUiThread(delegate
            {
                List<ContentPackDB> contentPackList = new List<ContentPackDB>();

                LinearLayout layout = new LinearLayout(context);
                layout.Orientation = Android.Widget.Orientation.Horizontal;
                layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), (int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context));
                layout.SetGravity(GravityFlags.CenterHorizontal);

                int counter = 0;
                foreach (ContentPack eachItem in contentPacks.Values)
                {
                    counter++;

                    if (eachItem.Errors.Count > 0)
                    {
#if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("Error: {0}", StringUtils.CreateErrorMessageFromGeneralErrors(eachItem.Errors));
#endif
                    } else
                    {
                        ImageView contentpackpic = new ImageView(context);
						
                        contentpackpic.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(size, context), (int)ImageHelper.convertDpToPixel(size, context));
                        contentpackpic.SetPadding(0, 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);

                        using (MemoryStream stream = new MemoryStream (eachItem.ContentPackIcon))
                        {
                            using (Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromStream (stream, "Profile"))
                            {
                                contentpackpic.SetImageDrawable(draw);
                                int cpackId = new int();
                                cpackId = eachItem.ContentPackID;
                                string cpackTitle = new string(new char[1]);
                                cpackTitle = eachItem.ContentPackTitle;

                                contentpackpic.Click += delegate
                                {
                                    StartContentPackItemActivity(cpackId, cpackTitle);
                                };

                                layout.AddView(contentpackpic);
                            }
                        }

                        ContentPackDB contentPackDB = ContentPackDB.ConvertFromContentPack(eachItem);
                        SaveContentPack(contentPackDB);
                        contentPackList.Add(contentPackDB);
                        packID = contentPackDB.ID;

                        if (counter == contentPacks.Count || layout.ChildCount == 3)
                        {
                            listContentPack.AddView(layout);
                            layout = new LinearLayout(context);
                            layout.Orientation = Android.Widget.Orientation.Horizontal;
                            layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, 0, (int)ImageHelper.convertDpToPixel(10f, context));
                            layout.SetGravity(GravityFlags.CenterHorizontal);
                        }
                    }
                }
            });
            DismissLightboxDialog();
        }

        private void connect_ContentPacksGetByTypeCompleted(object s, ContentPacksGetByTypeAndAccountIDCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)s;
            service.ContentPacksGetByTypeAndAccountIDCompleted -= connect_ContentPacksGetByTypeCompleted;
            if (e.Error == null)
            {
			
                RunOnUiThread(delegate
                {
                    ContentPack[] result = e.Result.ToArray();
                    List<ContentPackDB> contentPackList = new List<ContentPackDB>();

                    LinearLayout layout = new LinearLayout(context);
                    layout.Orientation = Android.Widget.Orientation.Horizontal;
                    layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), (int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context));
                    layout.SetGravity(GravityFlags.CenterHorizontal);

                    int counter = 0;

                    foreach (ContentPack eachItem in result)
                    {
                        counter++;

                        if (eachItem.Errors.Count > 0)
                        {
#if(DEBUG)
                            System.Diagnostics.Debug.WriteLine("Error: {0}", StringUtils.CreateErrorMessageFromGeneralErrors(eachItem.Errors));
#endif
                        } else
                        {
                            ImageView contentpackpic = new ImageView(context);

                            contentpackpic.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(size, context), (int)ImageHelper.convertDpToPixel(size, context));
                            contentpackpic.SetPadding(0, 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);

                            using (MemoryStream stream = new MemoryStream (eachItem.ContentPackIcon))
                            {
                                using (Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromStream (stream, "Profile"))
                                {
                                    contentpackpic.SetImageDrawable(draw);
                                    int cpackId = new int();
                                    cpackId = eachItem.ContentPackID;
                                    string cpackTitle = new string(new char[1]);
                                    cpackTitle = eachItem.ContentPackTitle;

                                    contentpackpic.Click += delegate
                                    {
                                        StartContentPackItemActivity(cpackId, cpackTitle);
                                    };

                                    layout.AddView(contentpackpic);
                                }
                            }

                            ContentPackDB contentPackDB = ContentPackDB.ConvertFromContentPack(eachItem);
                            SaveContentPack(contentPackDB);
                            contentPackList.Add(contentPackDB);
                            packID = contentPackDB.ID;

                            if (counter == result.Length || layout.ChildCount == 3)
                            {
                                listContentPack.AddView(layout);
                                layout = new LinearLayout(context);
                                layout.Orientation = Android.Widget.Orientation.Horizontal;
                                layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, 0, (int)ImageHelper.convertDpToPixel(10f, context));
                                layout.SetGravity(GravityFlags.CenterHorizontal);
                            }
                            SaveContentPack(contentPackDB);
                        }
                    }
                    DismissLightboxDialog();
                    dbm.InsertOrUpdateContentPacks(contentPackList);
                    cPackIsLoaded = true;
                });
            } else
            {
                string m = packType + Application.Context.Resources.GetString(Resource.String.errorDownloadFailGeneric);
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.Resources.GetString(Resource.String.errorContentPackFail), m);
                });
            }
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
		
        private void SaveContentPack(ContentPackDB pack)
        {
            pack.ContentPackAdFile = StringUtils.ConstructContentPackAdFilename(pack.ContentPackID);
            pack.ContentPackIconFile = StringUtils.ConstructContentPackIconFilename(pack.ContentPackID);
            SaveContentPackBuffers(pack);
        }

        private const int NPACK = 254, APACK = 255;
        private void StartContentPackItemActivity(int cpackId, string cpackTitle)
        {
            Intent i = new Intent(context, typeof(ContentPackItemActivity));
            i.PutExtra("packid", cpackId);
            i.PutExtra("animated", isAnimation);
            i.PutExtra("fromanimation", fromAnimation);
            i.PutExtra("CurrentStep", base.Intent.GetIntExtra("CurrentStep", 1));
            i.PutExtra("ContentPackTitle", cpackTitle);
            i.SetFlags(ActivityFlags.SingleTop);
            if (fromAnimation)
                StartActivityForResult(i, APACK);
            else
                StartActivityForResult(i, NPACK);
            Finish();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == APACK)
            {
                Intent m = new Intent();
                m.PutExtra("filename", data.GetStringExtra("filename"));
                SetResult(resultCode, m);
                Finish();
            }
        }

        private void SaveContentPackBuffers(ContentPackDB contentPack)
        {
            if (contentPack.ContentPackIcon != null && contentPack.ContentPackIcon.Length > 0)
            {
                RunOnUiThread(delegate
                {
                    string iconFile = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, contentPack.ContentPackIconFile);
                    if (File.Exists(iconFile))
                        File.Delete(iconFile);
                    try
                    {
                        File.WriteAllBytes(iconFile, contentPack.ContentPackIcon);
                    } catch (IOException e)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Unable to save iconfile - Message = {0}", e.Message);
#endif
                    }
                });
            }

            if (null != contentPack.ContentPackAd && contentPack.ContentPackAd.Length > 0)
            {
                RunOnUiThread(delegate
                {
                    string adFile = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, contentPack.ContentPackAdFile);

                    if (File.Exists(adFile))
                        File.Delete(adFile);
                    try
                    {
                        File.WriteAllBytes(adFile, contentPack.ContentPackAd);
                    } catch (IOException ex)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Unable to save advert pack - Message = {0}", ex.Message);
#endif
                    }
                });
            }
        }

        public void ShowLightboxDialog(string message)
        {
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using LOLMessageDelivery;
using LOLAccountManagement;
using LOLApp_Common;

using WZCommon;

namespace wowZapp.Messages
{
    public static class ComposeGenericResults
    {
        public static bool success
		{ get; set; }
    }

    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
    public class ComposeGenericMessage : Activity
    {
        private const int COMIX = 1, COMICON = 2, AUDIO = 3, VIDEO = 4, SFX = 5, EMOTICON = 6, ANIMATION = 7, TEXT = 8;
        private Button btnAdd, btnSend;
        private Context context;
        private int option, currentStep, pack;
        private MessageManager mmg;
        private ImageButton btnCreate;
        private LinearLayout layout;
        private ImageView images;
        private WebView webImages;
        private float[] newSizes;
        private ProgressBar pb;
        private ImageButton ib;
		
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            option = base.Intent.GetIntExtra("MessageType", 0);
            currentStep = base.Intent.GetIntExtra("CurrentStep", 1);
            pack = base.Intent.GetIntExtra("pack", -1);
            string head = string.Empty;
            switch (option)
            {
                case 1:
                    SetContentView(Resource.Layout.ComposeComixMessage);
                    head = Application.Context.Resources.GetString(Resource.String.titleComix);
                    break;
                case 2:
                    SetContentView(Resource.Layout.ComposeComiconMessage);
                    head = Application.Context.Resources.GetString(Resource.String.titleComicon);
                    break;
                case 3:
                    SetContentView(Resource.Layout.ComposeAudioMessage);
                    head = Application.Context.Resources.GetString(Resource.String.titleAudio);
                    break;
                case 4:
                    SetContentView(Resource.Layout.ComposeVideoMessage);
                    head = Application.Context.Resources.GetString(Resource.String.titleVideo);
                    break;
                case 5:
                    SetContentView(Resource.Layout.ComposeSFXMessage);
                    head = Application.Context.Resources.GetString(Resource.String.titleSFX);
                    break;
                case 6:
                    SetContentView(Resource.Layout.ComposeEmoticonMessage);
                    head = Application.Context.Resources.GetString(Resource.String.titleEmoticon);
                    break;
                case 7:
                    SetContentView(Resource.Layout.ComposeAnimationMessage);
                    head = Application.Context.Resources.GetString(Resource.String.titleAnimation);
                    break;
            /*case 8:
                    SetContentView(Resource.Layout.ComposeTextMessage);
                    head = Application.Context.Resources.GetString(Resource.String.titleText);
                    break;*/
            }
			
            mmg = wowZapp.LaffOutOut.Singleton.mmg;
			
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewloginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            layout = FindViewById<LinearLayout>(Resource.Id.imageLayout);
			
            ib = FindViewById<ImageButton>(Resource.Id.playButton);
            pb = FindViewById<ProgressBar>(Resource.Id.timerBar);
			
            if (option == 2 || option == 5)
                ib.Visibility = pb.Visibility = ViewStates.Invisible;
			
            context = relLayout.Context;
            Header.headertext = head;
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
			
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            btnBack.Tag = 0;
            ImageButton btnHome = FindViewById<ImageButton>(Resource.Id.btnHome);
            btnHome.Tag = 1;
			
            btnBack.Click += delegate
            {
                Finish();
            };
			
            btnHome.Click += delegate
            {
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.AddFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };
            btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
            btnSend = FindViewById<Button>(Resource.Id.btnSend);
            btnAdd.Visibility = ViewStates.Invisible;
            btnSend.Visibility = ViewStates.Invisible;
            newSizes = new float[2];
            newSizes [0] = newSizes [1] = 200f;
			
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
            { 
                newSizes [0] *= wowZapp.LaffOutOut.Singleton.bigger;
                newSizes [1] = newSizes [0];
            }
			
            images = null;
            webImages = null;
			
            if (ComposeMessageMainUtil.contentPackID == null)
                ComposeMessageMainUtil.contentPackID = new int[6];
			
            if (btnAdd.Visibility == ViewStates.Invisible)
            {
                RunOnUiThread(delegate
                {
                    ImageView preview = new ImageView(context);
                    preview.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
                    preview.SetScaleType(ImageView.ScaleType.FitXy);
                    switch (option)
                    {
                        case 1:
                            preview.SetBackgroundResource(Resource.Drawable.comix);
                            break;
                        case 2:
                            preview.SetBackgroundResource(Resource.Drawable.comicon);
                            break;
                        case 3:
                            preview.SetBackgroundResource(Resource.Drawable.audiofile);
                            break;
                        case 4:
                            preview.SetBackgroundResource(Resource.Drawable.video);
                            break;
                        case 5:
                            preview.SetBackgroundResource(Resource.Drawable.audiofile);
                            break;
                        case 6:
                            preview.SetBackgroundResource(Resource.Drawable.emoticon);
                            break;
                        case 7:
                            preview.SetBackgroundResource(Resource.Drawable.drawicon);
                            break;
                    }
                    preview.Click += (object sender, EventArgs e) => createNewMessage(sender, e);
                    if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                    {
                        images = preview;
                    }
                    layout.AddView(preview);
                });
            }
			
            ImageButton[] buttons = new ImageButton[2];
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            buttons [0] = btnBack;
            buttons [1] = btnHome;
            ImageHelper.setupButtonsPosition(buttons, bottom, context);
            cpcutil.genericIsDone = false;
			
            btnSend.Click += (object sender, EventArgs e) => SendMessage(sender, e);
            btnAdd.Click += (object sender, EventArgs e) => AddToMessages(sender, e);
        }
		
        private void AddToMessages(object s, EventArgs e)
        {
            MessageStep msgStep = new MessageStep();
            switch (option)
            {
                case 1:
                    msgStep.StepType = MessageStep.StepTypes.Comix;
                    break;
                case 2:
                    msgStep.StepType = MessageStep.StepTypes.Comicon;
                    break;
                case 3:
                    msgStep.StepType = MessageStep.StepTypes.Voice;
                    break;
                case 4:
                    msgStep.StepType = MessageStep.StepTypes.Video;
                    break;
                case 5:
                    msgStep.StepType = MessageStep.StepTypes.SoundFX;
                    break;
                case 6:
                    msgStep.StepType = MessageStep.StepTypes.Emoticon;
                    break;
                case 7:
                    msgStep.StepType = MessageStep.StepTypes.Animation;
                    break;
            }
			
            if (currentStep > ComposeMessageMainUtil.msgSteps.Count)
            {
                msgStep.StepNumber = ComposeMessageMainUtil.msgSteps.Count + 1;
                ComposeMessageMainUtil.msgSteps.Add(msgStep);
            } else
            {
                msgStep.StepNumber = currentStep;
                ComposeMessageMainUtil.msgSteps [currentStep - 1] = msgStep;
            }
			
            if (currentStep == 1)
            {
                StartActivity(typeof(ComposeMessageMainActivity));
                Finish();
            } else
            {
                Finish();
            }
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            //RunOnUiThread (delegate {
            if (hasFocus == true)
            {
                if (images != null || webImages != null)
                {
                    if (btnAdd.Visibility == ViewStates.Visible)
                    {
                        if (cpcutil.genericIsDone == false)
                        {
                            cpcutil.genericIsDone = true;
                            if (images != null)
                                RunOnUiThread(() => ImageHelper.resizeWidget(images, context));
                            else
                                RunOnUiThread(() => ImageHelper.resizeWidget(webImages, context));
                        }
                    }
                }
            }
            //});	
        }
		
        private void SendAudioMessage()
        {
            string filename2 = Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, "voice_msg_{0}.3gp");
            string path = string.Format(filename2, currentStep);
		
            MessageStepDB messageStep = new MessageStepDB();
            messageStep.StepNumber = 1;
            messageStep.StepType = MessageStep.StepTypes.Voice;
			
            MessageDB message = new MessageDB();
            message.FromAccountID = AndroidData.CurrentUser.AccountID;
            message.MessageStepDBList = new List<MessageStepDB>() { messageStep };
			
            List<Guid> recipientGuids = new List<Guid>();
            recipientGuids = Contacts.SelectContactsUtil.selectedContacts.Select(s => s.ContactAccountID).ToList();
			
            ContentInfo contentInfo = new ContentInfo(MessageDB.ConvertFromMessageDB(message), recipientGuids);
			
            try
            {
                byte[] audioFile = File.ReadAllBytes(path);
                contentInfo.VoiceRecordings.Add(messageStep.StepNumber, audioFile);
#if DEBUG
                System.Diagnostics.Debug.WriteLine("audioFile.Length = {0}", audioFile.Length);
#endif
            } catch (IOException e)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Exception thrown (audioFile read) : {0} - {1}", e.Message, e.StackTrace);
#endif
                return;
            }
			
#if DEBUG
            File.Delete(Android.OS.Environment.ExternalStorageDirectory + "/wz/audio.3gp");
            File.Copy(path, Android.OS.Environment.ExternalStorageDirectory + "/wz/audio.3gp", true);
#endif
			
            //RunOnUiThread (delegate {
            mmg.QueueMessage(contentInfo, false, context);
				
            if (null != wowZapp.LaffOutOut.Singleton.OnGlobalReceived)
                wowZapp.LaffOutOut.Singleton.OnGlobalReceived(this, new MessageStepCreatedEventArgs(messageStep, null, path, null, null));
            if (MessageReceivedUtil.FromMessages)
                MessageReceivedUtil.FromMessagesDone = true;
            Finish();
            //});
        }
		
        private void playAudio(ProgressBar pb)
        {
            string temppath = "";
            if (option == 5) 
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
		
        private void SendContentPackMessage()
        {
            MessageStepDB messageStep = new MessageStepDB();
            messageStep.StepNumber = 1;
            switch (option)
            {
                case 1:
                    messageStep.StepType = MessageStep.StepTypes.Comix;
                    break;
                case 2:
                    messageStep.StepType = MessageStep.StepTypes.Comicon;
                    break;
                case 5:
                    messageStep.StepType = MessageStep.StepTypes.SoundFX;
                    break;
                case 6:
                    messageStep.StepType = MessageStep.StepTypes.Emoticon;
                    break;
                case 7:
                    messageStep.StepType = MessageStep.StepTypes.Animation;
                    break;
            }
            if (option != 7)
                messageStep.ContentPackItemID = ContentPackItemsUtil.contentPackItemID;
            //else
            //    messageStep.AnimationData = Animations.AnimationUtil.animation;
			
            MessageDB message = new MessageDB();
            message.FromAccountID = AndroidData.CurrentUser.AccountID;
            message.MessageStepDBList = new List<MessageStepDB>() { messageStep };
			
            List<Guid> recipientGuids = new List<Guid>();
            recipientGuids = Contacts.SelectContactsUtil.selectedContacts.Select(s => s.ContactAccountID).ToList();
			
            ContentInfo contentInfo = new ContentInfo(MessageDB.ConvertFromMessageDB(message), recipientGuids);
            //RunOnUiThread (delegate {
            mmg.QueueMessage(contentInfo, false, context);
				
            if (null != wowZapp.LaffOutOut.Singleton.OnGlobalReceived)
                wowZapp.LaffOutOut.Singleton.OnGlobalReceived(this, new MessageStepCreatedEventArgs(messageStep, null, null, null, null));
            if (MessageReceivedUtil.FromMessages)
                MessageReceivedUtil.FromMessagesDone = true;
            Finish();
            //});
        }
		
        private void SendMessage(object s, EventArgs e)
        {
            switch (option)
            {
                case 1:
                case 2:
                case 5:
                case 6:
                case 4:
                case 7:
                    SendContentPackMessage();
                    break;
                case 3:
                    SendAudioMessage();
                    break;
            }
        }
		
        private void createNewMessage(object s, EventArgs e)
        {
            Intent i = null;
            switch (option)
            {
                case 1:
                    i = new Intent(this, typeof(ContentPackActivity));
                    i.PutExtra("pack", pack);
                    i.PutExtra("CurrentStep", currentStep);
                    StartActivityForResult(i, COMIX);
                    break;
                case 2:
                    i = new Intent(this, typeof(ContentPackActivity));
                    i.PutExtra("pack", pack);
                    i.PutExtra("CurrentStep", currentStep);
                    StartActivityForResult(i, COMICON);
                    break;
                case 3:
                    i = new Intent(this, typeof(ComposeAudioMessageActivity));
                    i.PutExtra("CurrentStep", currentStep);
                    StartActivityForResult(i, AUDIO);
                    break;
                case 4: 
#if DEBUG
                    i = new Intent(this, typeof(VideoMessage.VideoMessageActivity));
                    i.PutExtra("CurrentStep", currentStep);
                    StartActivityForResult(i, VIDEO);
#endif
                    break;
                case 5:
                    i = new Intent(this, typeof(ContentPackActivity));
                    i.PutExtra("CurrentStep", currentStep);
                    i.PutExtra("pack", pack);
                    StartActivityForResult(i, SFX);
                    break;
                case 6:
                    i = new Intent(this, typeof(ContentPackActivity));
                    i.PutExtra("CurrentStep", currentStep);
                    i.PutExtra("pack", 3);
                    StartActivityForResult(i, EMOTICON);
                    break;
                case 7:
#if DEBUG
                    i = new Intent(this, typeof(Animations.CreateAnimationActivity));
                    i.PutExtra("CurrentStep", currentStep);
                    StartActivityForResult(i, ANIMATION);
#endif
                    break;
            }
        }
		
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (ComposeGenericResults.success)
            {
                if (option != 7)
                    ComposeMessageMainUtil.contentPackID [currentStep - 1] = ContentPackItemsUtil.contentPackItemID;
					
                RunOnUiThread(() => layout.RemoveViewAt(1));
                if (requestCode == COMIX || requestCode == COMICON || requestCode == EMOTICON || requestCode == SFX)
                {
                    displayNewIcon();
                } else
                    displayNormalIcon(requestCode);
            }
        }
		
        private void displayNormalIcon(int code)
        {
            ImageView preview = new ImageView(context);
            newSizes [0] = newSizes [1] = 200f;
            preview.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
            preview.SetScaleType(ImageView.ScaleType.FitXy);
            preview.Click += (object sender, EventArgs e) => createNewMessage(sender, e);
            switch (code)
            {
                case 3:
                    preview.SetBackgroundResource(Resource.Drawable.audiofile);
                    break;
                case 4:
                    preview.SetBackgroundResource(Resource.Drawable.video);
                    break;
			//case 5:
			//	preview.SetBackgroundResource (Resource.Drawable.soundfxmsg);
                    break;
                case 7:
                    preview.SetBackgroundResource(Resource.Drawable.drawicon);
                    break;
            }
            images = preview;
            layout.AddView(preview);
            RunOnUiThread(delegate
            {
                btnAdd.Visibility = ViewStates.Visible;
                btnSend.Visibility = ViewStates.Visible;
            });
        }
		
        private void displayNewIcon()
        {
            float[] newSizes = new float[2];
            newSizes [0] = newSizes [1] = 100f;
            if (option == 2 || option == 5)
                ib.Visibility = pb.Visibility = ViewStates.Visible;
				
            if (option == 6)
            {
                string base64String = System.Convert.ToBase64String(ContentPackItemsUtil.content, 0, ContentPackItemsUtil.content.Length);
                RunOnUiThread(delegate
                {
                    if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                    {
                        newSizes [0] *= wowZapp.LaffOutOut.Singleton.bigger;
                        newSizes [1] = newSizes [0];
                    }
                    WebView wv = new WebView(context);
                    string url = "<img src=\"data:image/gif;base64," + base64String + "\" width=\"" + ((int)newSizes [0]).ToString() + "\" height=\"" + ((int)newSizes [1]).ToString() + "\" />";
                    wv.LoadDataWithBaseURL(null, url, "text/html", "UTF-8", null);
                    wv.VerticalScrollBarEnabled = false;
                    wv.HorizontalScrollBarEnabled = false;
                    wv.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
                    wv.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    webImages = wv;
                    layout.AddView(wv);
                    btnAdd.Visibility = ViewStates.Visible;
                    btnSend.Visibility = ViewStates.Visible;
                });
            } else
            {
                if (option == 2 || option == 5)
                {
                    pb.Visibility = ib.Visibility = ViewStates.Visible;
                    ib.Click += delegate
                    {
                        playAudio(pb);
                    };				
                }
                RunOnUiThread(delegate
                {
                    MemoryStream stream = new MemoryStream(ContentPackItemsUtil.content);
                    Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromStream(stream, "Profile");
                    ImageView preview = new ImageView(context);
                    preview.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(newSizes [0], context), (int)ImageHelper.convertDpToPixel(newSizes [1], context));
                    preview.SetScaleType(ImageView.ScaleType.FitXy);
                    preview.SetBackgroundDrawable(draw);
                    images = preview;
                    layout.AddView(preview);
                    btnAdd.Visibility = ViewStates.Visible;
                    btnSend.Visibility = ViewStates.Visible;
                });
            }
        }
		
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}


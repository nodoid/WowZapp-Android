using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using LOLApp_Common;
using LOLAccountManagement;
using LOLMessageDelivery;

using WZCommon;

namespace wowZapp.Messages
{
    public static class ComposeMessageMainUtil
    {
        public static LOLMessageDelivery.Message message
        { get; set; }
        public static List<MessageStep> msgSteps
        { get; set; }
        public static PollingStep[] pollSteps
        { get; set; }
        public static byte[][] VoiceData
        { get; set; }
        public static MessageDB messageDB
        { get; set; }
        public static int[] currentPosition
        { get; set; }
        public static int[] contentPackID
        { get; set; }
        public static bool returnTo
        { get; set; }
    }

    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public class ComposeMessageMainActivity : Activity, View.IOnTouchListener, View.IOnLongClickListener
    {
        private ImageView[] imgSteps;
        private bool[] imgSets;
        private ImageView btnPlay, imgProfilePic;
        private Button btnSend;
        private ImageButton btnBack, btnHome;
        private TextView txtFullname, txtMessage;
        private Context context;
        private DBManager dbm;
        private MessageManager mmg;
        private Dialog LightboxDialog;
        private Dialog ModalPreviewDialog;
        private int co, steps, thumbImageWidth, thumbImageHeight;
        private bool readOnly, hasMoved, regenerate;
        private Dictionary<int, ContentPackItem> contentPack;
        private object s;
        private EventArgs e;

        private const int DUMDUM = 0, FROM_MESSAGES = 99;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ComposeMessageMain);
            readOnly = base.Intent.GetBooleanExtra("readonly", false);

            dbm = wowZapp.LaffOutOut.Singleton.dbm;
            mmg = wowZapp.LaffOutOut.Singleton.mmg;
            Guid profPic = Guid.Empty;
            UserDB user = null;
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewLoginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            context = header.Context;

            Header.headertext = Application.Context.Resources.GetString(Resource.String.messageCreateMessageTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            imgProfilePic = FindViewById<ImageView>(Resource.Id.imgProfilePic);
            imgProfilePic.SetBackgroundResource(Resource.Drawable.emptybackground);
            hasMoved = regenerate = false;

            this.thumbImageWidth = (int)ImageHelper.convertDpToPixel(80f, context);
            this.thumbImageHeight = (int)ImageHelper.convertDpToPixel(100f, context);

            btnSend = FindViewById<Button>(Resource.Id.btnSend);
            btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            btnBack.Tag = 0;
            btnHome = FindViewById<ImageButton>(Resource.Id.btnHome);
            btnHome.Tag = 1;
            btnPlay = FindViewById<ImageView>(Resource.Id.imgPlay);
            btnPlay.Click += new EventHandler(btnPlay_Click);
            txtFullname = FindViewById<TextView>(Resource.Id.txtAuthorName);
            txtMessage = FindViewById<TextView>(Resource.Id.txtMainMessage);
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
			
            ImageButton[] buttons = new ImageButton[2];
            buttons [0] = btnBack;
            buttons [1] = btnHome;
            ImageHelper.setupButtonsPosition(buttons, bottom, context);
			
            if (ComposeMessageMainUtil.contentPackID == null)
                ComposeMessageMainUtil.contentPackID = new int[7];
			
            for (int i = 0; i < 6; ++i)
                ComposeMessageMainUtil.contentPackID [i] = -1;
			
            txtMessage.Click += delegate
            {
                int n = 0, t = -1;
                for (n = 0; n < ComposeMessageMainUtil.currentPosition.Length; ++n)
                {
                    if (ComposeMessageMainUtil.currentPosition [n] == 0)
                    {
                        t = n;
                        break;
                    }
                }
                if (n == ComposeMessageMainUtil.currentPosition.Length)
                {
                    for (n = 0; n < ComposeMessageMainUtil.currentPosition.Length; ++n)
                    {
                        if (ComposeMessageMainUtil.msgSteps [n].StepType == MessageStep.StepTypes.Text)
                        {
                            if (ComposeMessageMainUtil.msgSteps [n].MessageText == txtMessage.Text)
                            {
                                t = n;
                                break;
                            }
                        }
                    }
                    if (t == -1)
                        RunOnUiThread(() => Toast.MakeText(context, Application.Context.GetString(Resource.String.errorNoSpaceForText), ToastLength.Short).Show());
                } else
                {
                    RunOnUiThread(delegate
                    {
                        Intent it = new Intent(this, typeof(ComposeTextMessageActivity));
                        it.PutExtra("text", txtMessage.Text != Application.Context.Resources.GetString(Resource.String.composeTextMessagelblMessage) ? 
						             ComposeMessageMainUtil.msgSteps [t].MessageText : string.Empty);
                        it.PutExtra("CurrentStep", base.Intent.GetIntExtra("CurrentStep", 1));
                        StartActivity(it);
                    });
                }
            };
			
            imgSteps = new ImageView[6];
            imgSets = new bool[6];
            imgSteps [0] = FindViewById<ImageView>(Resource.Id.imgStep1);
            imgSteps [0].Tag = 0;
            imgSteps [1] = FindViewById<ImageView>(Resource.Id.imgStep2);
            imgSteps [1].Tag = 1;
            imgSteps [2] = FindViewById<ImageView>(Resource.Id.imgStep3);
            imgSteps [2].Tag = 2;
            imgSteps [3] = FindViewById<ImageView>(Resource.Id.imgStep4);
            imgSteps [3].Tag = 3;
            imgSteps [4] = FindViewById<ImageView>(Resource.Id.imgStep5);
            imgSteps [4].Tag = 4;
            imgSteps [5] = FindViewById<ImageView>(Resource.Id.imgStep6);
            imgSteps [5].Tag = 5;
            for (int z = 0; z < 6; ++z)
                imgSets [z] = true;
            bool multi = false;

            if (ComposeMessageMainUtil.messageDB != null)
            {
                if (ComposeMessageMainUtil.messageDB.MessageRecipientDBList != null)
                {
                    int n = 0;
                    for (int j = 0; j < ComposeMessageMainUtil.currentPosition.Length; ++j)
                        ComposeMessageMainUtil.currentPosition [j] = 0;
                    regenerateIcons();
                    foreach (MessageStepDB mesg in ComposeMessageMainUtil.messageDB.MessageStepDBList)
                    {
                        #if DEBUG
                        Console.WriteLine("MessageStepDB item = {0}", mesg);
                        #endif
                        MessageStep mSt = MessageStepDB.ConvertFromMessageStepDB(mesg);
                        ComposeMessageMainUtil.msgSteps.Add(mSt);
                        ComposeMessageMainUtil.currentPosition [n] = 1;
                    }
                }
            }

            if (readOnly)
            {
                user = dbm.GetUserWithAccountID(ComposeMessageMainUtil.messageDB.FromAccountID.ToString());
                if (ComposeMessageMainUtil.messageDB.MessageRecipientDBList.Count > 1)
                    multi = true;
                if (user.Picture.Length == 0)
                    profPic = user.AccountID;
                else 
                    loadProfilePicture(user, imgProfilePic);
            } else
            {
                user = UserDB.ConvertFromUser(AndroidData.CurrentUser);
                if (AndroidData.CurrentUser.Picture.Length == 0)
                    profPic = AndroidData.CurrentUser.AccountID;
                else
                    loadProfilePicture(user, imgProfilePic);
            }
			
            imgProfilePic.Tag = new Java.Lang.String("profilepic_" + user.AccountID);
            string name = user.FirstName + " " + user.LastName;
            if (multi == true)
            {
                if (ComposeMessageMainUtil.messageDB == null)
                    name += " and " + (Contacts.SelectContactsUtil.selectedContacts.Count - 1).ToString() +
                        (Contacts.SelectContactsUtil.selectedContacts.Count - 1 > 1 ? " others" : "other");
                else
                    name += " and " + (ComposeMessageMainUtil.messageDB.MessageRecipientDBList.Count - 1).ToString() +
                        (ComposeMessageMainUtil.messageDB.MessageRecipientDBList.Count - 1 > 1 ? " others" : "other");
            }
            float fontSize = txtFullname.TextSize;
            txtFullname.SetTextSize(Android.Util.ComplexUnitType.Dip, ImageHelper.getNewFontSize(fontSize, context));
            txtFullname.Text = name;
            fontSize = txtMessage.TextSize;
            txtMessage.SetTextSize(Android.Util.ComplexUnitType.Dip, ImageHelper.getNewFontSize(fontSize, context));
            if (readOnly == false)
            {
                for (int n = 0; n < 6; ++n)
                {
					
                    if (imgSets [n])
                    {	
                        int step = new int();
                        step = n;
                        imgSteps [step].Click -= (object s, EventArgs e) => {
                            imgClickEvent(s, e, 9, step);};
                        imgSteps [step].Click += (object s, EventArgs e) => {
                            imgClickEvent(s, e, 9, step);};						
                    }
                    imgSteps [n].SetOnLongClickListener(this);
                }
				
                ReloadMessageSteps();
            } else
            {
                this.SetButtonImagesForMessageSteps();
                header.Text = Application.Context.Resources.GetString(Resource.String.messageReadMessage);
                btnSend.Visibility = ViewStates.Invisible;
                ImageView a1 = FindViewById<ImageView>(Resource.Id.arrow1);
                ImageView a2 = FindViewById<ImageView>(Resource.Id.arrow2);
                ImageView a3 = FindViewById<ImageView>(Resource.Id.arrow3);
                ImageView a4 = FindViewById<ImageView>(Resource.Id.arrow4);
                a1.Visibility = a2.Visibility = a3.Visibility = a4.Visibility = ViewStates.Invisible;
            }

            co = 0;

            btnBack.Click += delegate
            {
                for (int i = 0; i < 6; ++i)
                    ComposeMessageMainUtil.currentPosition [i] = 0;
                for (int i = 0; i < 7; ++i)
                    ComposeMessageMainUtil.contentPackID [i] = -1;
                ComposeMessageMainUtil.message = null;
                ComposeMessageMainUtil.messageDB = null;
                ComposeMessageMainUtil.msgSteps = null;
                ComposeMessageMainUtil.pollSteps = null;
                Finish();
            };
            btnHome.Click += delegate
            {
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.AddFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };

            btnSend.Click += delegate
            {
                SendMessage();
                Finish();
            };

            if (profPic != Guid.Empty)
            {
                Guid finder = profPic != Guid.Empty ? profPic : AndroidData.CurrentUser.AccountID;
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
                service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, finder, new Guid(AndroidData.ServiceAuthToken));
            }
        }

        private void loadProfilePicture(UserDB user, ImageView image)
        {
            RunOnUiThread(delegate
            {

                using (Bitmap img = ImageHelper.CreateUserProfileImageForDisplay(user.Picture, this.thumbImageWidth, this.thumbImageHeight, this.Resources))
                {
                    image.SetImageBitmap(img);
                }//end using
            });
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == FROM_MESSAGES)
                Finish();
				
            if (resultCode == Result.Ok)
            {
                int lastZero = 0;
                for (int n = 0; n < ComposeMessageMainUtil.currentPosition.Length; ++n)
                {
                    if (ComposeMessageMainUtil.currentPosition [n] == 0)
                    {
                        lastZero = n;
                        break;
                    }
                }
                if (lastZero == 0)
                    lastZero = ComposeMessageMainUtil.currentPosition.Length;
				
                ComposeMessageMainUtil.currentPosition [lastZero - 1] = 0;
            }
        }

        private void ReloadMessageSteps()
        {
            if (ComposeMessageMainUtil.msgSteps != null)
            {
                int i = 0, opt = 0, step = 0;
                for (int n = 0; n < ComposeMessageMainUtil.currentPosition.Length; ++n)
                {
                    if (ComposeMessageMainUtil.currentPosition [n] == 0)
                    {
                        step = n;
                        break;
                    }
                }
                if (step == 0)
                    step = ComposeMessageMainUtil.currentPosition.Length;
                Intent it = null;
                for (i = 0; i < step; i++)
                {
                    if (ComposeMessageMainUtil.msgSteps.Count == 0)
                        break;
                    //step = i;
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.Text)
                    {
                        opt = 0;
                        imgSteps [i].SetImageResource(Resource.Drawable.textmsg);
                        if (txtMessage.Text == Application.Resources.GetString(Resource.String.composeTextMessagelblMessage))
                            txtMessage.Text = ComposeMessageMainUtil.msgSteps [i].MessageText;
                        imgSteps [i].Click -= (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSteps [i].Click += (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSets [i] = false;
                    }
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.Comicon)
                    {
                        opt = 1;
                        imgSteps [i].SetImageResource(Resource.Drawable.comicon);
                        imgSteps [i].Click -= (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSteps [i].Click += (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSets [i] = false;
                    }
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.Comix)
                    {
                        opt = 2;
                        imgSteps [i].SetImageResource(Resource.Drawable.comix);
                        imgSteps [i].Click -= (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSteps [i].Click += (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSets [i] = false;
                    }
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.Emoticon)
                    {
                        opt = 3;
                        imgSteps [i].SetImageResource(Resource.Drawable.emoticon);
                        imgSteps [i].Click -= (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSteps [i].Click += (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSets [i] = false;
                    }
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.Polling)
                    {
                        opt = 4;
                        imgSteps [i].SetImageResource(Resource.Drawable.polls);
                        imgSteps [i].Click -= (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSteps [i].Click += (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSets [i] = false;
                    }
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.SoundFX)
                    {
                        opt = 5;
                        imgSteps [i].SetImageResource(Resource.Drawable.audiofile);
                        imgSteps [i].Click -= (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSteps [i].Click += (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSets [i] = false;
                    }
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.Video)
                        imgSteps [i].SetImageResource(Resource.Drawable.camera);
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.Animation)
                        imgSteps [i].SetImageResource(Resource.Drawable.drawicon);
                    if (ComposeMessageMainUtil.msgSteps [i].StepType == MessageStep.StepTypes.Voice)
                    {
                        opt = 8;
                        imgSteps [i].SetImageResource(Resource.Drawable.microphone);
                        imgSteps [i].Click -= (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSteps [i].Click += (object s, EventArgs e) => {
                            imgClickEvent(s, e, opt, step);};
                        imgSets [i] = false;
                    }
                }
                if (regenerate)
                {
                    regenerateIcons();
                    regenerate = false;
                }
            }
        }
		
        private void regenerateIcons()
        {
            if (ComposeMessageMainUtil.msgSteps != null)
            {
                for (int i = ComposeMessageMainUtil.msgSteps.Count; i < 6; ++i)
                {
                    imgSteps [i].SetImageResource(Resource.Drawable.emptybackground);
                    imgSteps [i].Click -= (object s, EventArgs e) => {
                        imgClickEvent(s, e, 9, i);};
                    imgSteps [i].Click += (object s, EventArgs e) => {
                        imgClickEvent(s, e, 9, i);};
                    imgSets [i] = true;
                }
            }
        }

        private void imgClickEvent(object s, EventArgs e, int opt, int step)
        {
            Intent it = null;
            ComposeMessageMainUtil.currentPosition [step] = 1;
            switch (opt)
            {
                case 0: 
                    RunOnUiThread(delegate
                    {
                        it = new Intent(this, typeof(ComposeTextMessageActivity));
                        it.PutExtra("text", txtMessage.Text != Application.Context.Resources.GetString(Resource.String.composeTextMessagelblMessage) ? 
						ComposeMessageMainUtil.msgSteps [step].MessageText : string.Empty);
                        it.PutExtra("CurrentStep", step + 1);
                        StartActivityForResult(it, DUMDUM);
                    });
                    break;
                case 1:
                    RunOnUiThread(delegate
                    {
                        it = new Intent(this, typeof(ContentPackActivity));
                        it.PutExtra("pack", 1);
                        it.PutExtra("CurrentStep", step + 1);
                        StartActivityForResult(it, DUMDUM);
                    });
                    break;
                case 2:
                    RunOnUiThread(delegate
                    {
                        it = new Intent(this, typeof(ContentPackActivity));
                        it.PutExtra("pack", 2);
                        it.PutExtra("CurrentStep", step + 1);
                        StartActivityForResult(it, DUMDUM);
                    });
                    break;
                case 3:
                    RunOnUiThread(delegate
                    {
                        it = new Intent(this, typeof(ContentPackActivity));
                        it.PutExtra("pack", 3);
                        it.PutExtra("CurrentStep", step + 1);
                        StartActivityForResult(it, DUMDUM);
                    });
                    break;
                case 4:
                    RunOnUiThread(delegate
                    {
                        it = new Intent(this, typeof(ComposePollChoiceActivity));
                        it.PutExtra("CurrentStep", step + 1);
                        StartActivityForResult(it, DUMDUM);
                    });
                    break;
                case 5:
                    RunOnUiThread(delegate
                    {
                        it = new Intent(this, typeof(ContentPackActivity));
                        it.PutExtra("pack", 5);
                        it.PutExtra("CurrentStep", step + 1);
                        StartActivityForResult(it, DUMDUM);
                    });
                    break;
                case 6: // video message
                    break;
                case 7: // animation
                    break;
                case 8:
                    RunOnUiThread(delegate
                    {
                        it = new Intent(this, typeof(ComposeAudioMessageActivity));
                        it.PutExtra("pack", 5);
                        it.PutExtra("CurrentStep", step + 1);
                        StartActivityForResult(it, DUMDUM);
                    });
                    break;
                case 9:
                    RunOnUiThread(delegate
                    {
                        it = new Intent(context, typeof(ComposeMessageChooseContent));
                        it.PutExtra("CurrentStep", step + 1);
                        StartActivityForResult(it, DUMDUM);
                    });
                    break;
            }
        }

        private void SetButtonImagesForMessageSteps()
        {
            int msgStepsCount = ComposeMessageMainUtil.messageDB.MessageStepDBList.Count;
            int stepper = 0, opt = 0;
            for (int i = 0; i < msgStepsCount; i++)
            {
                bool hasStep = i < msgStepsCount;
                if (hasStep)
                {
                    stepper = i;
                    MessageStep step = ComposeMessageMainUtil.messageDB.MessageStepDBList [i];
                    switch (step.StepType)
                    {
                        case MessageStep.StepTypes.Text:
                            opt = 0;
                            this.imgSteps [stepper].SetImageResource(Resource.Drawable.textmsg);
                            if (readOnly)
                                txtMessage.Text = ComposeMessageMainUtil.msgSteps [stepper].MessageText != string.Empty ? ComposeMessageMainUtil.msgSteps [stepper].MessageText : "No text found";
                            imgSteps [stepper].Click -= (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSteps [stepper].Click += (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSets [stepper] = false;
                            break;
                        case MessageStep.StepTypes.Comicon:
                            opt = 1;
                            this.imgSteps [stepper].SetImageResource(Resource.Drawable.comicon);
                            imgSteps [stepper].Click -= (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSteps [stepper].Click += (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSets [stepper] = false;
                            break;
                        case MessageStep.StepTypes.Comix:
                            opt = 2;
                            this.imgSteps [stepper].SetImageResource(Resource.Drawable.comix);
                            imgSteps [stepper].Click -= (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSteps [stepper].Click += (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSets [stepper] = false;
                            break;
                        case MessageStep.StepTypes.Emoticon:
                            opt = 3;
                            this.imgSteps [stepper].SetImageResource(Resource.Drawable.emoticon);
                            imgSteps [stepper].Click -= (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSteps [stepper].Click += (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSets [stepper] = false;
                            break;
                        case MessageStep.StepTypes.Polling:
                            opt = 4;
                            this.imgSteps [stepper].SetImageResource(Resource.Drawable.polls);
                            imgSteps [stepper].Click -= (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSteps [stepper].Click += (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSets [stepper] = false;
                            break;
                        case MessageStep.StepTypes.SoundFX:
                            opt = 5;
                            this.imgSteps [stepper].SetImageResource(Resource.Drawable.audiofile);
                            imgSteps [stepper].Click -= (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSteps [stepper].Click += (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSets [stepper] = false;
                            break;
                        case MessageStep.StepTypes.Video:
                            this.imgSteps [stepper].SetImageResource(Resource.Drawable.camera);
                            break;
                        case MessageStep.StepTypes.Animation:
                            imgSteps [stepper].SetImageResource(Resource.Drawable.drawicon);
                            break;
                        case MessageStep.StepTypes.Voice:
                            opt = 8;
                            this.imgSteps [stepper].SetImageResource(Resource.Drawable.microphone);
                            imgSteps [stepper].Click -= (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSteps [stepper].Click += (object s, EventArgs e) => {
                                imgClickEvent(s, e, opt, stepper);};
                            imgSets [stepper] = false;
                            break;
                    }//end switch
                } else
                    this.imgSteps [stepper].Visibility = ViewStates.Invisible;
            }//end for
        }//end void SetButtonImagesForMessageSteps

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus == true)
            {
                ReloadMessageSteps();
            }
        }

        public bool OnLongClick(View v)
        {
            v.SetOnTouchListener(this);
            return true;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            ImageView view = (ImageView)v;
            ImageView dupView = view;
            int stepNumber = (int)view.Tag;
            if (stepNumber >= ComposeMessageMainUtil.msgSteps.Count)
                return false;

            int[] position = new int[2];
            view.GetLocationOnScreen(position);
            int x = 0, y = 0, w = 0, h = 0;
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    x = (int)e.GetX();
                    y = (int)e.GetY();
                    w = view.Width;
                    h = view.Height;
                    view.SetImageResource(Resource.Drawable.smalltrashbin);
                    break;
                case MotionEventActions.Move:
                    var left = (int)(e.RawX - x);
                    var right = (int)(left + view.Width);
                    var top = (int)(e.RawY - y);
                    var bottom = (int)(top + view.Height);
                    view.Layout(left, top, right, bottom);
                    hasMoved = true;
                    break;
                case MotionEventActions.Up:
                    if (hasMoved == false)
                    {
                        view = dupView;
                        break;
                    } else
                    {
                        view.Layout(x, y, x + w, y + h);
                        view.SetImageResource(Resource.Drawable.emptybackground);
                        hasMoved = false;
                        int msgStepsCount = ComposeMessageMainUtil.msgSteps.Count;
                        if (stepNumber == msgStepsCount)
                        {
                            ComposeMessageMainUtil.msgSteps.RemoveAt(stepNumber);
                            break;
                        } else
                        {
                            ComposeMessageMainUtil.currentPosition [stepNumber] = 0;
                            ComposeMessageMainUtil.contentPackID [stepNumber] = 0;
                            ComposeMessageMainUtil.msgSteps.RemoveAt(stepNumber);
                            regenerate = true;
                            ReloadMessageSteps();
                        }
                    }
                    break;
            }
            return true;
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

        private void btnPlay_Click(object s, EventArgs e)
        {
            if (readOnly == true)
            {
                MessageReceived player = new MessageReceived(ComposeMessageMainUtil.messageDB, context, true);
                return;
            }
            int i = 0;
            MessageDB tmpMessage = new MessageDB();
            tmpMessage.FromAccountID = AndroidData.CurrentUser.AccountID;
            tmpMessage.MessageStepDBList = new List<MessageStepDB>();
            tmpMessage.MessageRecipientDBList = new List<MessageRecipientDB>();
            for (i = 0; i < ComposeMessageMainUtil.msgSteps.Count; ++i)
            {
                tmpMessage.MessageStepDBList.Add(MessageStepDB.ConvertFromMessageStep(ComposeMessageMainUtil.msgSteps [i]));
                tmpMessage.MessageStepDBList [i].ContentPackItemID = ComposeMessageMainUtil.contentPackID [i];
            }
            for (i = 0; i < Contacts.SelectContactsUtil.selectedContacts.Count; ++i)
            {
                tmpMessage.MessageRecipientDBList.Add(new MessageRecipientDB());
                tmpMessage.MessageRecipientDBList [i].AccountGuid = Contacts.SelectContactsUtil.selectedContacts [i].ContactGuid;
            }

            MessageReceived play = new MessageReceived(tmpMessage, context);

            return;
        }
  
        private void SendMessage()
        {
            List<Guid> recipientIDs = new List<Guid>();
            foreach (ContactDB contact in Contacts.SelectContactsUtil.selectedContacts)
                recipientIDs.Add(contact.ContactAccountID);
			
            ComposeMessageMainUtil.message.MessageSteps = ComposeMessageMainUtil.msgSteps;
            List<int> t = new List<int>();
            for (int l = 0; l < 6; ++l)
                if (ComposeMessageMainUtil.contentPackID [l] != -1)
                    t.Add(ComposeMessageMainUtil.contentPackID [l]);
            for (int m = 0; m < t.Count; ++m)
            {
                ComposeMessageMainUtil.message.MessageSteps [m].ContentPackItemID = t [m];
            }
			
            ContentInfo contentInfo = new ContentInfo(ComposeMessageMainUtil.message, recipientIDs);
            if (ComposeMessageMainUtil.pollSteps != null)
            {
                foreach (PollingStep pollStep in ComposeMessageMainUtil.pollSteps)
                {
                    if (pollStep.PollingAnswer1 != null || pollStep.PollingData1 != null)
                    {
                        contentInfo.PollingSteps.Add(pollStep.StepNumber, pollStep);
                    }
                }
            }
			
            for (int m = 0; m < 6; ++m)
                ComposeMessageMainUtil.contentPackID [m] = ComposeMessageMainUtil.currentPosition [m] = 0;
			
            mmg.QueueMessage(contentInfo, true, context);
        }

        private void Service_UserGetImageDataCompleted(object sender, UserGetImageDataCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;

            if (e.Result.Errors.Count == 0 && e.Result.ImageData.Length > 0)
            {
                RunOnUiThread(delegate
                {
                    using (Bitmap userImage = ImageHelper.CreateUserProfileImageForDisplay(e.Result.ImageData, this.thumbImageWidth, this.thumbImageHeight, this.Resources))
                    {
                        ImageView pic = (ImageView)imgProfilePic.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID));
                        if (pic != null)
                            pic.SetImageBitmap(userImage);
                    }

                    service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;

                    dbm.UpdateUserImage(e.Result.AccountID.ToString(), e.Result.ImageData);
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
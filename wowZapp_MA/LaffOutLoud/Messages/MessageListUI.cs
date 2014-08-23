using System.IO;
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
using Android.Graphics;

using LOLApp_Common;
using WZCommon;

namespace wowZapp.Messages
{
    public partial class MessageList
    {
        private Dialog contactsDialog;
        private View myView;
        private void CreateMessagesUI()
        {
            List<UserDB> participants = new List<UserDB>();
            bool moreThanOne = false, isCurrentMe = false;
			
            if (MessageConversations.clearView)
            {
                listWrapper.RemoveAllViewsInLayout();
                MessageConversations.clearView = false;
            }
			
            if (Contacts.SelectContactsUtil.selectedContacts.Count != 0)
                Contacts.SelectContactsUtil.selectedContacts.Clear();
			
            if (MessageConversations.currentConversationMessages [0].MessageRecipientDBList.Count > 1)
            {
                moreThanOne = true;
                for (int m = 0; m < MessageConversations.currentConversationMessages[0].MessageRecipientDBList.Count; ++m)
                {
                    if (MessageConversations.currentConversationMessages [0].MessageRecipientDBList [m].AccountGuid != AndroidData.CurrentUser.AccountID.ToString())
                    {
                        UserDB userDetails = dbm.GetUserWithAccountID(MessageConversations.currentConversationMessages [0].MessageRecipientDBList [m].AccountGuid);
                        participants.Add(userDetails);
                        ContactDB contact = new ContactDB();
                        contact.ContactUser = new LOLAccountManagement.User();
                        contact.ContactUser.AccountID = userDetails.AccountID;
                        Contacts.SelectContactsUtil.selectedContacts.Add(contact);
                    } else
                    {
                        UserDB userDetails = UserDB.ConvertFromUser(AndroidData.CurrentUser);
                        participants.Add(userDetails);
                    }
                }
            } 
					
            if (moreThanOne)
            {
                string toReturn = "";
                List<UserDB> sortedList = new List<UserDB>();
                sortedList = participants.OrderBy(s => s.LastName).OrderBy(s => s.FirstName).ToList();
                foreach (UserDB eachItem in sortedList)
                    toReturn += string.Format("{0} {1}, ", eachItem.FirstName, eachItem.LastName);
                int last = toReturn.LastIndexOf(", ");
                toReturn = toReturn.Remove(last);
						
                using (LinearLayout btnlayout = new LinearLayout (context))
                {
                    btnlayout.Orientation = Android.Widget.Orientation.Vertical;
                    btnlayout.SetGravity(GravityFlags.Center);
                    btnlayout.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
                    btnlayout.SetPadding((int)ImageHelper.convertDpToPixel(5f, context), 0, (int)ImageHelper.convertDpToPixel(5f, context), (int)ImageHelper.convertDpToPixel(10f, context));
								
                    using (TextView name = new TextView(context))
                    {
                        name.Text = toReturn;
                        name.SetTextSize(Android.Util.ComplexUnitType.Dip, 18f);
                        name.SetTextColor(Color.Black);
                        RunOnUiThread(() => btnlayout.AddView(name));
                    }
								
                    using (Button showAll = new Button (context))
                    {
                        showAll.Gravity = GravityFlags.CenterVertical;
                        showAll.Text = Application.Context.Resources.GetString(Resource.String.messageShowAllInConversation);
                        showAll.Click += (object sender, EventArgs e) => {
                            showParticipants(sender, e, participants); };
                        showAll.SetWidth((int)ImageHelper.convertDpToPixel(180f, context));
                        showAll.SetHeight((int)ImageHelper.convertDpToPixel(30f, context));
                        showAll.SetBackgroundResource(Resource.Drawable.button);
                        RunOnUiThread(() => btnlayout.AddView(showAll));
                    }
                    RunOnUiThread(() => listWrapper.AddView(btnlayout));
                }			
            }
            myView = null;                                                         
            LayoutInflater factory = LayoutInflater.From(this);
			
            foreach (MessageDB message in MessageConversations.currentConversationMessages)
            {
                if (message != null)
                {
                    if (!moreThanOne)
                        myView = factory.Inflate(Resource.Layout.lstConversation, null);
                    else
                        myView = factory.Inflate(Resource.Layout.lstConversationMulti, null);
			
                    isCurrentMe = message.FromAccountID != AndroidData.CurrentUser.AccountID ? false : true;
                    LinearLayout shell = new LinearLayout(context);
                
                    shell.Orientation = Orientation.Horizontal;
                    shell.SetGravity(GravityFlags.CenterVertical);
                    shell.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
                    shell.SetPadding(0, 0, 0, (int)ImageHelper.convertDpToPixel(5f, context));
                    RunOnUiThread(() => shell.AddView(myView));
				
                    UserDB whoAmI = new UserDB();
                    whoAmI = message.FromAccountID != AndroidData.CurrentUser.AccountID ? dbm.GetUserWithAccountID(message.FromAccountGuid) : UserDB.ConvertFromUser(AndroidData.CurrentUser);

                    ImageView userImage = shell.FindViewById<ImageView>(Resource.Id.imgProfile1);
                    if (Contacts.ContactsUtil.contactFilenames.Contains(message.FromAccountGuid))
                    {
                        using (Bitmap bm = BitmapFactory.DecodeFile (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, message.FromAccountGuid)))
                        {
                            using (MemoryStream ms = new MemoryStream ())
                            {
                                bm.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                                byte[] image = ms.ToArray();
                                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)imageSize[0], (int)imageSize[1], this.Resources))
                                {
                                    RunOnUiThread(delegate
                                    {
                                        userImage.SetImageBitmap(myBitmap);
                                    });
                                }
                            }
                        }
                    } else
                    {
                        userImage.LayoutParameters = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel(imageSize [0], context), (int)ImageHelper.convertDpToPixel(imageSize [1], context));
                        userImage.SetScaleType(ImageView.ScaleType.FitXy);
                        userImage.SetImageResource(Resource.Drawable.defaultuserimage);
                    }
				
				
                    int left = (int)wowZapp.LaffOutOut.Singleton.ScreenXWidth - (int)ImageHelper.convertDpToPixel(imageSize [0] + 20f, context);
                    int leftOver = (int)wowZapp.LaffOutOut.Singleton.ScreenXWidth - (int)ImageHelper.convertDpToPixel(imageSize [0] + 40f, context);
				
                    TextView whoIsIt = shell.FindViewById<TextView>(Resource.Id.textNames);
                    whoIsIt.Gravity = !isCurrentMe ? GravityFlags.Left : GravityFlags.Right;
                    whoIsIt.Text = whoAmI.FirstName + " " + whoAmI.LastName;
				
                    for (int m = 0; m < message.MessageStepDBList.Count; ++m)
                    {
                        if (message.MessageStepDBList [m].StepType == LOLMessageDelivery.MessageStep.StepTypes.Text)
                        {
                            TextView messageText = shell.FindViewById<TextView>(Resource.Id.textMessageBubble);
						
                            messageText.SetBackgroundResource(message.FromAccountID != AndroidData.CurrentUser.AccountID ? Resource.Drawable.bubblesolidleft : 
						                                   Resource.Drawable.bubblesolidright);
                            int lr = (int)ImageHelper.convertDpToPixel(20f, context);                   
                            int tb = lr / 2;
                            messageText.SetPadding(lr, tb, lr, tb);
                            messageText.SetTextColor(Color.White);
                            messageText.SetTextSize(Android.Util.ComplexUnitType.Dip, 14f);
                            string messager = message.MessageStepDBList [m].MessageText;
                            if (messager.Length > 40)
                                messager = messager.Substring(0, 40) + "...";
                            messageText.Text = messager;
                            messageText.ContentDescription = message.MessageGuid;
                            break;
                        }
                    }
				
                    TextView messageDate = shell.FindViewById<TextView>(Resource.Id.textDateSent);
                    messageDate.SetTextColor(Color.Black);
                    messageDate.SetTextSize(Android.Util.ComplexUnitType.Dip, 10f);
                    messageDate.Gravity = message.FromAccountID != AndroidData.CurrentUser.AccountID ? GravityFlags.Left : GravityFlags.Right;
                    messageDate.Text = message.MessageSent.ToShortTimeString() + ", " + message.MessageSent.ToShortDateString();
				
                    TextView txtMessageUnread = shell.FindViewById<TextView>(Resource.Id.txtMessageUnread);
                    txtMessageUnread.Visibility = ViewStates.Gone;

                    LinearLayout messageItems = shell.FindViewById<LinearLayout>(Resource.Id.linearLayout7);
                    messageItems = createMessageBar(messageItems, message, leftOver);
				
                    RunOnUiThread(delegate
                    {
                        listWrapper.AddView(shell);
                        shell = null;
                    });
                }
            }
            if (progress != null)
                RunOnUiThread(() => progress.Dismiss());
        }
		
        private LinearLayout createMessageBar(LinearLayout mBar, MessageDB message, int leftOver)
        {
            RunOnUiThread(delegate
            {
                LinearLayout icons = new LinearLayout(context);
                ImageView random = null;
                icons.Orientation = Orientation.Horizontal;
                icons.SetGravity(GravityFlags.Left);
                icons.SetVerticalGravity(GravityFlags.CenterVertical);
                icons.SetMinimumHeight(30);
                int topPos = 0;
                if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                    topPos = 0;
                icons.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);
                if (message.MessageStepDBList.Count == 0)
                {
                    using (random = new ImageView (context))
                    {
                        using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                        {
                            lp.SetMargins(0, 0, leftOver - (int)ImageHelper.convertDpToPixel(30f, context), 0);
                            random.LayoutParameters = lp;
                        }
                        random.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(30f, context), (int)ImageHelper.convertDpToPixel(30f, context));
                        random.SetBackgroundResource(Resource.Drawable.playblack);
                        random.ContentDescription = message.MessageGuid;
                        random.Click += delegate
                        {
                            Messages.MessageReceived m = new Messages.MessageReceived(message, context);
                        };
                        icons.AddView(random);
                    }
                } else
                {
                    int end = message.MessageStepDBList.Count > 3 ? 3 : message.MessageStepDBList.Count;
                    int iconSize = (int)ImageHelper.convertDpToPixel(34f, context);
                    int toEnd = leftOver - (2 * iconSize) - (end * iconSize);
                    for (int i = 0; i < end; ++i)
                    {
                        switch (message.MessageStepDBList [i].StepType)
                        {
                            case LOLMessageDelivery.MessageStep.StepTypes.Text:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.textmsg);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                            case LOLMessageDelivery.MessageStep.StepTypes.Animation:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.drawicon);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                            case LOLMessageDelivery.MessageStep.StepTypes.Comicon:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.comicon);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                            case LOLMessageDelivery.MessageStep.StepTypes.Comix:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.comix);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                            case LOLMessageDelivery.MessageStep.StepTypes.Emoticon:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.emoticon);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                            case LOLMessageDelivery.MessageStep.StepTypes.Polling:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.polls);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                            case LOLMessageDelivery.MessageStep.StepTypes.SoundFX:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.audiofile);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                            case LOLMessageDelivery.MessageStep.StepTypes.Video:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.camera);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                            case LOLMessageDelivery.MessageStep.StepTypes.Voice:
                                using (random = new ImageView (context))
                                {
                                    using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                                    {
                                        lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                        random.LayoutParameters = lp;
                                    }
                                    random.SetBackgroundResource(Resource.Drawable.microphone);
                                    random.ContentDescription = message.MessageID.ToString();
                                    random.Click += new EventHandler(imgMessage_Click);
                                    icons.AddView(random);
                                }
                                break;
                        }
                    }
                    using (random = new ImageView (context))
                    {
                        using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                        {
                            lp.SetMargins(toEnd, 0, 0, 0);
                            random.LayoutParameters = lp;
                        }
                        random.SetBackgroundResource(Resource.Drawable.playblack);
                        random.ContentDescription = message.MessageID.ToString();
                        random.Click += new EventHandler(random_Click);
                        icons.AddView(random);
                    }
                }
                mBar.AddView(icons);
            });
            return mBar;
        }
		
        private void imgMessage_Click(object s, EventArgs e)
        {
            ImageView senderObj = (ImageView)s;
            string conID = senderObj.ContentDescription;
            playOut(conID);
        }
		
        private void random_Click(object s, EventArgs e)
        {
            ImageView senderObj = (ImageView)s;
            string conID = senderObj.ContentDescription;
            MessageDB message = new MessageDB();
            message = dbm.GetMessage(conID);
            MessageReceived player = new MessageReceived(message, context);
        }
		
        private void playOut(string message)
        {
            MessageDB mess = new MessageDB();
            mess = dbm.GetMessage(message);
            Messages.ComposeMessageMainUtil.messageDB = mess;
            Intent i = new Intent(this, typeof(Messages.ComposeMessageMainActivity));
            i.PutExtra("readonly", true);
            StartActivity(i);
        }
		
        private void showParticipants(object s, EventArgs e, List<UserDB> contacts)
        {
            contactsDialog = new Dialog(this, Resource.Style.lightbox_dialog);
            contactsDialog.SetContentView(Resource.Layout.ModalContactsConversation);
            LinearLayout mainLayout = ((LinearLayout)contactsDialog.FindViewById(Resource.Id.contactMainLayout));
            Context localContext = mainLayout.Context;
            List<Guid> profilePicsToBeGrabbed = new List<Guid>();
			
            for (int n = 0; n < contacts.Count; n++)
            {
                LinearLayout layout = new LinearLayout(context);
                layout.Orientation = Android.Widget.Orientation.Horizontal;
                layout.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
                layout.SetPadding((int)ImageHelper.convertDpToPixel(20f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), (int)ImageHelper.convertDpToPixel(10f, context));
					
                ImageView profilepic = new ImageView(context);
                profilepic.LayoutParameters = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel(60f, context), (int)ImageHelper.convertDpToPixel(60f, context));
                profilepic.Tag = new Java.Lang.String("profilepic_" + contacts [n].AccountID);
                if (contacts [n].Picture.Length == 0)
                {
                    profilePicsToBeGrabbed.Add(contacts [n].AccountID);
                } else
                {
                    if (contacts [n].Picture.Length > 0)
                        LoadUserImage(contacts [n], profilepic);
                    else
                        RunOnUiThread(() => profilepic.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.defaultuserimage)));
                }
                RunOnUiThread(() => layout.AddView(profilepic));
					
                TextView text = new TextView(context);
                text.LayoutParameters = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel(235f, context), (int)ImageHelper.convertDpToPixel(40f, context));
                text.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);
                text.Gravity = GravityFlags.CenterVertical;
                text.TextSize = 16f;
                text.SetTextColor(Android.Graphics.Color.White);
                if (contacts [n].FirstName != "" || contacts [n].LastName != "")
                {
                    text.Text = contacts [n].FirstName + " " + contacts [n].LastName;
                } else
                {
                    text.Text = contacts [n].EmailAddress;
                }
                RunOnUiThread(delegate
                {
                    layout.AddView(text);
                    mainLayout.AddView(layout);
                });
            }
            ((Button)contactsDialog.FindViewById(Resource.Id.btnCancel)).Click += delegate
            {
                DismissModalPreviewDialog();
            };
			
            if (profilePicsToBeGrabbed.Count > 0)
            {
                cpUI = 0;
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
                service.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [0], new Guid(AndroidData.ServiceAuthToken));
            }
			
            contactsDialog.Show();
        }
		
        private void LoadUserImage(UserDB user, ImageView profilePic)
        {
            if (user.Picture.Length == 0)
                RunOnUiThread(() => profilePic.SetImageResource(Resource.Drawable.defaultuserimage));
            else
                using (Bitmap img = ImageHelper.CreateUserProfileImageForDisplay(user.Picture, (int)imageSize[0], (int)imageSize[0], this.Resources))
                {
                    RunOnUiThread(() => profilePic.SetImageBitmap(img));
                }//end using
        }
		
        public void DismissModalPreviewDialog()
        {
            if (contactsDialog != null)
                contactsDialog.Dismiss();
			
            contactsDialog = null;
        }
    }
}
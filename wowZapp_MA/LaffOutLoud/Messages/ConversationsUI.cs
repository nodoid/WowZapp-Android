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
using LOLMessageDelivery;
using LOLAccountManagement;

using WZCommon;

namespace wowZapp.Messages
{
    public partial class Conversations
    {
        private Dialog ModalNewContact;
        private View myView;
        private void CreateUI()
        {
            if (MessageConversations.clearView)
            {
                RunOnUiThread(() => listWrapper.RemoveAllViewsInLayout());
                MessageConversations.clearView = false;
            }
            int c = 0;
            List<int> unreadMessages = new List<int>();
            int numberInConversation = 0;
            foreach (LOLMessageConversation conversation in MessageConversations.conversationsList)
            {
                unreadMessages.Add(conversation.MessageIDs.Count - conversation.ReadMessageIDs.Count);
                int t = 0;
            }
            if (unknownUsers == null)
                unknownUsers = new List<Guid>();
            else
                unknownUsers.Clear();
				
            myView = null;                                                         
            LayoutInflater factory = LayoutInflater.From(this);
            List<Guid> unknownMessages = new List<Guid>();
            foreach (MessageDB latestMessage in MessageConversations.initialMessages)
            {
                UserDB whoFrom = new UserDB();
                whoFrom = dbm.GetUserWithAccountID(MessageConversations.conversationsList [c].Recipients [0].ToString());
                if (latestMessage.MessageRecipientDBList.Count != 0)
                {
				
                    if (whoFrom == null && latestMessage.MessageRecipientDBList [0].AccountGuid == AndroidData.CurrentUser.AccountID.ToString())
                        whoFrom = UserDB.ConvertFromUser(AndroidData.CurrentUser);
                    if (whoFrom != null)
                    {
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("c = {0}, Recipient[0] Guid = {1}, whoFrom name = {2} {3}", c, MessageConversations.conversationsList [c].Recipients [0], 
					                  whoFrom.FirstName, whoFrom.LastName);
                        #endif
                        List<UserDB> users = new List<UserDB>();
                        numberInConversation = latestMessage.MessageRecipientDBList.Count;
                        for (int i = 0; i < (numberInConversation > 3 ? 3 : numberInConversation); ++i)
                        {
                            if (latestMessage.MessageRecipientDBList [i] != null)
                            {
                                UserDB current = dbm.GetUserWithAccountID(latestMessage.MessageRecipientDBList [i].AccountGuid.ToString());
                                if (current == null && latestMessage.MessageRecipientDBList [i].AccountGuid != AndroidData.CurrentUser.AccountID.ToString())
                                    unknownUsers.Add(new Guid(latestMessage.MessageRecipientDBList [i].AccountGuid));
                                else
                                {
                                    //users.Add (current);
                                    if (current == null && latestMessage.MessageRecipientDBList [i].AccountGuid == AndroidData.CurrentUser.AccountID.ToString())
                                        users.Add(UserDB.ConvertFromUser(AndroidData.CurrentUser));
                                    else
                                        users.Add(current);
                                }
                            }
                        }

                        UserDB sender = new UserDB();
                        sender = dbm.GetUserWithAccountID(latestMessage.FromAccountGuid);
                        if (sender == null)
                            sender = UserDB.ConvertFromUser(AndroidData.CurrentUser);

                        if (numberInConversation == 1)
                            myView = factory.Inflate(Resource.Layout.lstConversation, null);
                        else
                            myView = factory.Inflate(Resource.Layout.lstConversationMulti, null);
							
                        int leftOver = (int)wowZapp.LaffOutOut.Singleton.ScreenXWidth - (int)ImageHelper.convertDpToPixel(imageSize [0] + 30f, context);
                        LinearLayout shell = new LinearLayout(context);
						
                        shell.Orientation = Orientation.Horizontal;
                        shell.SetGravity(GravityFlags.CenterVertical);
                        shell.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
                        shell.SetPadding(0, 0, 0, (int)ImageHelper.convertDpToPixel(5f, context));	
						
                        RunOnUiThread(() => shell.AddView(myView));
                        if (users.Count != 0)
                            generateUserImage(users, shell);
						
                        TextView whoAmI = shell.FindViewById<TextView>(Resource.Id.textNames);
                        string myName = sender.FirstName + " " + sender.LastName;
                        if (numberInConversation != 1)
                        {
                            myName += string.Format(" and {0} other{1}", numberInConversation == 2 ? "1" : 
							                         (numberInConversation - 1).ToString(), numberInConversation == 2 ? "." : "s.");
                        }
                        whoAmI.Text = myName;
                        whoAmI.SetTextSize(Android.Util.ComplexUnitType.Dip, 12f);

                        TextView txtMessage = shell.FindViewById<TextView>(Resource.Id.textMessageBubble);        
                        if (latestMessage.MessageStepDBList.Count == 1 && latestMessage.MessageStepDBList [0].StepType == MessageStep.StepTypes.Text)
                        {
                            txtMessage = messageTextBox(txtMessage, latestMessage, 0, leftOver);
                        } else
                        {
                            for (int n = 0; n < latestMessage.MessageStepDBList.Count; ++n)
                            {
                                if (latestMessage.MessageStepDBList [n].StepType == MessageStep.StepTypes.Text)
                                {
                                    txtMessage = messageTextBox(txtMessage, latestMessage, n, leftOver);
                                    break;
                                }
                            }
                        }
					
                        txtMessage.ContentDescription = latestMessage.MessageID.ToString();
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("ContentDesctription = {0}, ID = {1}", txtMessage.ContentDescription, latestMessage.ID);
                        #endif
                        txtMessage.Click += textMessage_Click;
				
                        TextView txtDateSent = shell.FindViewById<TextView>(Resource.Id.textDateSent);
                        txtDateSent.Text = latestMessage.MessageSent.ToShortDateString();

                        TextView txtMessageUnread = shell.FindViewById<TextView>(Resource.Id.txtMessageUnread);
                        txtMessageUnread.SetTextSize(Android.Util.ComplexUnitType.Dip, 10f);
                        if (unreadMessages [c] == 0)
                            txtMessageUnread.Text = context.Resources.GetString(Resource.String.conversationUINoUnreadMessages);
                        else
                            txtMessageUnread.Text = string.Format("({0}) {1}{2}", unreadMessages [c], 
                                                                   context.Resources.GetString(Resource.String.conversationUIUnreadMessages), 
                                                                   unreadMessages [c] == 1 ? "." : "s.");
						
                        LinearLayout messageItems = shell.FindViewById<LinearLayout>(Resource.Id.linearLayout7);
                        if (latestMessage.MessageStepDBList.Count > 0)
                        {
                            createMessageBar(messageItems, latestMessage, leftOver);
                            messageItems.ContentDescription = latestMessage.MessageGuid;
                            messageItems.Click += messageBar_Click;
                        }
											
                        RunOnUiThread(delegate
                        {
                            listWrapper.AddView(shell);
                            shell = null;
                        });
						
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("done a loop");
                        #endif

                    }
                }
                if (c < MessageConversations.initialMessages.Count - 2)
                    c++;
					
            }
            //});

            if (unknownUsers.Count != 0)
            {
                Contacts.AddUnknownUser auu = new Contacts.AddUnknownUser(unknownUsers, context);
            }
            if (progress != null)
                RunOnUiThread(() => progress.Dismiss());
        }
		
        private TextView messageTextBox(TextView txtMessage, MessageDB message, int position, int leftOver)
        {
            if (position < message.MessageStepDBList.Count)
            {
                if (string.IsNullOrEmpty(message.MessageStepDBList [position].MessageText))
                    txtMessage.Text = "";
                else
                    txtMessage.Text = message.MessageStepDBList [position].MessageText;
            }
            int nolines = (int)(ImageHelper.convertDpToPixel((txtMessage.Text.Length / 27f) * 12f, context));
            txtMessage.SetHeight(nolines);
            txtMessage.SetTextColor(Android.Graphics.Color.Black);
            return txtMessage;
        }
		
        private LinearLayout createMessageBar(LinearLayout mBar, MessageDB message, int leftOver)
        {
            LinearLayout icons = new LinearLayout(context);
            icons.Orientation = Orientation.Horizontal;
            icons.SetGravity(GravityFlags.Left);
            icons.SetVerticalGravity(GravityFlags.CenterVertical);
            icons.SetMinimumHeight(30);

            icons.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);
            if (message.MessageStepDBList.Count == 0)
            {
                ImageView random1 = new ImageView(context);
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                {
                    lp.SetMargins(0, 0, leftOver - (int)ImageHelper.convertDpToPixel(30f, context), 0);
                    random1.LayoutParameters = lp;
                }
                random1.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(30f, context), (int)ImageHelper.convertDpToPixel(30f, context));
                random1.SetBackgroundResource(Resource.Drawable.playblack);
                random1.ContentDescription = message.MessageGuid;
                random1.Click += delegate
                {
                    Messages.MessageReceived m = new Messages.MessageReceived(message, context);
                };
                RunOnUiThread(() => icons.AddView(random1));
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
                            ImageView random2 = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                random2.LayoutParameters = lp;
                            }
                            random2.SetBackgroundResource(Resource.Drawable.textmsg);
                            random2.ContentDescription = message.MessageID.ToString();
                            random2.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(random2));
                            break;
                        case LOLMessageDelivery.MessageStep.StepTypes.Animation:
                            ImageView random3 = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                random3.LayoutParameters = lp;
                            }
                            random3.SetBackgroundResource(Resource.Drawable.drawicon);
                            random3.ContentDescription = message.MessageID.ToString();
                            random3.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(random3));
                            break;
                        case LOLMessageDelivery.MessageStep.StepTypes.Comicon:
                            ImageView random4 = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                random4.LayoutParameters = lp;
                            }
                            random4.SetBackgroundResource(Resource.Drawable.comicon);
                            random4.ContentDescription = message.MessageID.ToString();
                            random4.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(random4));
                            break;
                        case LOLMessageDelivery.MessageStep.StepTypes.Comix:
                            ImageView random5 = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                random5.LayoutParameters = lp;
                            }
                            random5.SetBackgroundResource(Resource.Drawable.comix);
                            random5.ContentDescription = message.MessageID.ToString();
                            random5.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(random5));
                            break;
                        case LOLMessageDelivery.MessageStep.StepTypes.Emoticon:
                            ImageView random6 = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                random6.LayoutParameters = lp;
                            }
                            random6.SetBackgroundResource(Resource.Drawable.emoticon);
                            random6.ContentDescription = message.MessageID.ToString();
                            random6.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(random6));
                            break;
                        case LOLMessageDelivery.MessageStep.StepTypes.Polling:
                            ImageView random7 = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                random7.LayoutParameters = lp;
                            }
                            random7.SetBackgroundResource(Resource.Drawable.polls);
                            random7.ContentDescription = message.MessageID.ToString();
                            random7.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(random7));	
                            break;
                        case LOLMessageDelivery.MessageStep.StepTypes.SoundFX:
                            ImageView random8 = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                random8.LayoutParameters = lp;
                            }
                            random8.SetBackgroundResource(Resource.Drawable.audiofile);
                            random8.ContentDescription = message.MessageID.ToString();
                            random8.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(random8));
                            break;
                        case LOLMessageDelivery.MessageStep.StepTypes.Video:
                            ImageView random9 = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                random9.LayoutParameters = lp;
                            }
                            random9.SetBackgroundResource(Resource.Drawable.camera);
                            random9.ContentDescription = message.MessageID.ToString();
                            random9.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(random9));
                            break;
                        case LOLMessageDelivery.MessageStep.StepTypes.Voice:
                            ImageView randomA = new ImageView(context);
                            using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                            {
                                lp.SetMargins(0, 0, (int)ImageHelper.convertDpToPixel(1f, context), 0);
                                randomA.LayoutParameters = lp;
                            }
                            randomA.SetBackgroundResource(Resource.Drawable.microphone);
                            randomA.ContentDescription = message.MessageID.ToString();
                            randomA.Click += new EventHandler(imgMessage_Click);
                            RunOnUiThread(() => icons.AddView(randomA));
                            break;
                    }
                }
                ImageView randomp = new ImageView(context);
                using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context)))
                {
                    lp.SetMargins(toEnd, 0, 0, 0);
                    randomp.LayoutParameters = lp;
                }
                randomp.SetBackgroundResource(Resource.Drawable.playblack);
                randomp.ContentDescription = message.MessageID.ToString();
                randomp.Click += new EventHandler(random_Click);
                RunOnUiThread(() => icons.AddView(randomp));
            }
            RunOnUiThread(() => mBar.AddView(icons));
            return mBar;
        }
				
        private void generateUserImage(List<UserDB>users, LinearLayout shell)
        {			
            Bitmap blank = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.defaultuserimage);
            Bitmap smallBlank = null;
            byte[] img = null;
            ImageView prof1, prof2, prof3, prof4;
            prof1 = shell.FindViewById<ImageView>(Resource.Id.imgProfile1);
            prof2 = prof3 = prof4 = null;
            if (users.Count > 1)
            {
                smallBlank = Bitmap.CreateScaledBitmap(blank, (int)imageSize [0] / 2, (int)imageSize [0] / 2, false);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                smallBlank.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                img = ms.ToArray();
                prof2 = shell.FindViewById<ImageView>(Resource.Id.imgProfile2);
                prof3 = shell.FindViewById<ImageView>(Resource.Id.imgProfile3);
                prof4 = shell.FindViewById<ImageView>(Resource.Id.imgProfile4);
            }
            float imageXY = imageSize [0] / 2;
			
            List<UserDB> newUsers = users.Where(s => s.AccountID != AndroidData.CurrentUser.AccountID && s != null).ToList();
			
            if (newUsers.Count == 0 || (newUsers.Count == 1 && newUsers [0].Picture.Length == 0))
                return;
		
            List<byte[]> userImages = new List<byte[]>();
            for (int n = 0; n < newUsers.Count; ++n)
            {
                if (newUsers [n].Picture.Length > 0)
                {
                    using (Bitmap sm = BitmapFactory.DecodeByteArray(newUsers[n].Picture, 0, newUsers[n].Picture.Length))
                    {
                        using (Bitmap smScale = Bitmap.CreateScaledBitmap(sm, (int)imageXY, (int)imageXY, false))
                        {
                            using (System.IO.MemoryStream mm = new System.IO.MemoryStream())
                            {
                                smScale.Compress(Bitmap.CompressFormat.Jpeg, 80, mm);
                                userImages.Add(mm.ToArray());
                            }
                        }
                    }
                } else
                {
                    if (Contacts.ContactsUtil.contactFilenames.Contains(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, newUsers [n].AccountGuid)))
                    {
                        using (Bitmap small = BitmapFactory.DecodeFile (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, newUsers [n].AccountGuid)))
                        {
                            using (Bitmap smallUser = Bitmap.CreateScaledBitmap (small, (int)imageXY, (int)imageXY, false))
                            {
                                using (System.IO.MemoryStream m = new System.IO.MemoryStream ())
                                {
                                    smallUser.Compress(Bitmap.CompressFormat.Jpeg, 80, m);
                                    userImages.Add(m.ToArray());
                                }
                            }
                        }
                    } else
                        userImages.Add(img);
                }
            }

            Bitmap [] bitmaps = new Bitmap[userImages.Count];
		
            for (int n = 0; n < userImages.Count; ++n)
            {
                bitmaps [n] = ImageHelper.CreateUserProfileImageForDisplay(userImages [n], (int)imageXY, (int)imageXY, this.Resources);
                if (bitmaps [n] == null)
                    bitmaps [n] = blank;
            }
		
            if (userImages.Count == 1 && users.Count == 1)
            {
                prof1.SetImageBitmap(bitmaps [0]);
            }
		
            int diff = users.Count - userImages.Count;
            switch (userImages.Count)
            {
                case 2:
                    prof1.SetImageBitmap(bitmaps [0]);
                    prof2.SetImageBitmap(bitmaps [1]);
                    break;
                case 3:
                    prof1.SetImageBitmap(bitmaps [0]);
                    prof2.SetImageBitmap(bitmaps [1]);
                    prof3.SetImageBitmap(bitmaps [2]);
                    break;
                case 4:
                    prof1.SetImageBitmap(bitmaps [0]);
                    prof2.SetImageBitmap(bitmaps [1]);
                    prof3.SetImageBitmap(bitmaps [2]);
                    prof3.SetImageBitmap(bitmaps [3]);
                    break;
            }
        }
		
        private void displayImage(byte[] image, ImageView contactPic)
        {
            if (image.Length > 0)
            {
                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)imageSize[0], (int)imageSize[1], this.Resources))
                {
                    RunOnUiThread(delegate
                    {
                        contactPic.SetImageBitmap(myBitmap);
                    });
                }
            }
        }
    }
}
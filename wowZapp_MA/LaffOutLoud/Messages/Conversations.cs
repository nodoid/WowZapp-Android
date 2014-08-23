using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LOLMessageDelivery;
using LOLApp_Common;
using LOLAccountManagement;

using WZCommon;

namespace wowZapp.Messages
{
    public static class MessageConversations
    {
        public static List<LOLMessageConversation> conversationsList
		{ get; set; }
        public static List<MessageDB> storedMessages
		{ get; set; }
        public static List<MessageDB> currentConversationMessages 
		{ get; set; }
        public static LOLMessageConversation currentConversation
		{ get; set; }
        public static List<MessageDB> initialMessages
		{ get; set; }
        public static string firstUserName
		{ get; set; }
        public static bool clearView
		{ get; set; }
    }		
	
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public partial class Conversations : Activity
    {
        private Context context;
        private LinearLayout listWrapper;
        private ScrollView scroller;
        private ProgressDialog progress;
        private float[] imageSize;
        private DBManager dbm;
        private int newUsersToAdd, counter, guidToGrab;
        private List<User> newUsers;
        private List<Guid> unknownUsers;
        private List<ContactDB> contacts;
        private ImageView modalImage;
        private bool createIt;
        private AutoResetEvent signal;
        private int waitThreadCount;
        private volatile bool isRefreshing;
		
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (MessageConversations.conversationsList == null)
                MessageConversations.conversationsList = new List<LOLMessageConversation>();
            else
                MessageConversations.conversationsList.Clear();
				
            dbm = wowZapp.LaffOutOut.Singleton.dbm;
			
            SetContentView(Resource.Layout.MessageLists);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            Header.headertext = Application.Context.Resources.GetString(Resource.String.messageListHeaderViewTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
            listWrapper = FindViewById<LinearLayout>(Resource.Id.linearListWrapper);
            context = listWrapper.Context;
            //ViewGroup Parent = listWrapper;
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            btnBack.Tag = 0;
            ImageButton btnHome = FindViewById<ImageButton>(Resource.Id.btnHome);
            btnHome.Tag = 1;
            ImageButton btnAdd = FindViewById<ImageButton>(Resource.Id.btnAdd);
            btnAdd.Tag = 2;
            Messages.MessageReceivedUtil.FromMessagesDone = false;
            Messages.MessageReceivedUtil.FromMessages = true;
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            ImageButton[] buttons = new ImageButton[3];
            buttons [0] = btnBack;
            buttons [1] = btnHome;
            buttons [2] = btnAdd;
            ImageHelper.setupButtonsPosition(buttons, bottom, context);
            isRefreshing = true;
            scroller = FindViewById<ScrollView>(Resource.Id.scrollViewContainer);
            btnBack.Click += delegate
            {
                wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedConversationMessages;
                if (MessageReceivedUtil.FromMessagesDone)
                {
                    MessageReceivedUtil.FromMessagesDone = false;
                    Intent i = new Intent(this, typeof(Main.HomeActivity));
                    i.SetFlags(ActivityFlags.ClearTop);
                    StartActivity(i);
                } else
                    Finish();
            };
            signal = new AutoResetEvent(false);
            createIt = false;

            if (contacts == null)
                contacts = new List<ContactDB>();
            else
                contacts.Clear();

            btnHome.Click += delegate
            {
                wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedConversationMessages;
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.SetFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };
			
            btnAdd.Click += delegate
            {
                wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedConversationMessages;
                Intent i = new Intent(this, typeof(Contacts.SelectContactsActivity));
                StartActivity(i);
            };
				
            newUsers = new List<User>();
            RunOnUiThread(delegate
            {
                progress = ProgressDialog.Show(context, Application.Context.Resources.GetString(Resource.String.messageRefreshingMessages),
				                                Application.Context.Resources.GetString(Resource.String.commonOneSec));
            });
            ThreadPool.QueueUserWorkItem(delegate
            {
                getConversations();
            });
            wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedConversationMessages;
		
            float size = 56f;
            imageSize = new float[2];
            imageSize [0] = imageSize [1] = size;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                imageSize = ImageHelper.getNewSizes(imageSize, context);
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("imagesize = {0}", imageSize [0]);
            #endif
            LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted += MessageManager_MessageSendConfirm;
        }	
		
        private void AppDelegate_ReceivedConversationMessages(object sender, IncomingMessageEventArgs e)
        {
            if (MessageConversations.conversationsList == null)
                MessageConversations.conversationsList = new List<LOLMessageConversation>();
            else
                MessageConversations.conversationsList.Clear();
            MessageConversations.clearView = true;
            getConversations();
        }

        private void getConversations()
        {
            LOLMessageClient client = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            client.MessageGetConversationListCompleted += Message_ConversationsListComplete;
            client.MessageGetConversationListAsync(AndroidData.CurrentUser.AccountID, /*AndroidData.LastConvChecked*/new DateTime(1901, 1, 1), DateTime.Now, 
			                                        new Guid(AndroidData.ServiceAuthToken));
        }
		
        private void Message_ConversationsListComplete(object s, MessageGetConversationListCompletedEventArgs e)
        {
            LOLMessageClient client = (LOLMessageClient)s;
            client.MessageGetConversationListCompleted -= Message_ConversationsListComplete;
            if (e.Error == null)
            {
                if (MessageConversations.conversationsList.Count > 0)
                    MessageConversations.conversationsList.Clear();
                MessageConversations.conversationsList = e.Result;
                AndroidData.LastConvChecked = DateTime.Now;
                if (MessageConversations.initialMessages == null)
                    MessageConversations.initialMessages = new List<MessageDB>();
                else
                    MessageConversations.initialMessages.Clear();
					
                List<Guid> messagesToGet = new List<Guid>();
                for (int i = 0; i < MessageConversations.conversationsList.Count; ++i)
                {
                    MessageDB message = new MessageDB();
                    message = dbm.GetMessage(MessageConversations.conversationsList [i].MessageIDs [0].ToString());
                    if (message == null)
                    {
                        messagesToGet.Add(MessageConversations.conversationsList [i].MessageIDs [0]);
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("MessageID (being sucked) = {0}", MessageConversations.conversationsList [i].MessageIDs [0]);
                        #endif
                    } else
                    {
                        MessageConversations.initialMessages.Add(message);
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("MessageID (already stored) = {0}", message.MessageGuid);
                        #endif
                    }
                }
                if (messagesToGet.Count != 0)
                {
                    client.MessageGetByListCompleted += Message_GetByListCompleted;
                    client.MessageGetByListAsync(AndroidData.CurrentUser.AccountID, messagesToGet, AndroidData.CurrentUser.AccountID,  
                                                  new Guid(AndroidData.ServiceAuthToken));
                } else
                {
                    List<Guid> users = new List<Guid>();
                    for (int i = 0; i < MessageConversations.initialMessages.Count; ++i)
                    {
                        foreach (MessageRecipientDB guid in MessageConversations.initialMessages[i].MessageRecipientDBList)
                        {
                            UserDB user = new UserDB();
                            user = dbm.GetUserWithAccountID(guid.AccountGuid);
                            if (user == null && (guid.AccountGuid != AndroidData.CurrentUser.AccountID.ToString()))
                                users.Add(new Guid(guid.AccountGuid));
                        }
                    }
                    if (users.Count == 0)
                    {
                        RunOnUiThread(delegate
                        {
                            if (progress != null)
                                progress.Dismiss();
                            CreateUI();
                        });
					
                    } else
                    {
                        createIt = true;
                        Contacts.AddUnknownUser auu = new Contacts.AddUnknownUser(users, context);
                    }
                }
            } else
            {
                int threadCount = waitThreadCount;
                for (int i = 0; i < threadCount; i++)
                {
                    signal.Set();
                }//end for
                isRefreshing = false;
			
                RunOnUiThread(() => GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.errorConversationGrabTitle), 
				                         Application.Context.GetString(Resource.String.errorConversationGrabMessage)));
            }
        }
		
        public void Message_GetByListCompleted(object s, MessageGetByListCompletedEventArgs e)
        {
            LOLMessageClient client = (LOLMessageClient)s;
            client.MessageGetByListCompleted -= Message_GetByListCompleted;
            if (e.Error == null)
            {
                MessageDB msgDB = new MessageDB();
                foreach (LOLMessageDelivery.Message message in e.Result)
                {
                    msgDB = MessageDB.ConvertFromMessage(message);
                    MessageConversations.initialMessages.Add(msgDB);
                }
					
                List<MessageDB> sortedList = new List<MessageDB>();
                sortedList = MessageConversations.initialMessages.OrderBy(t => t.MessageSent).ToList();
                MessageConversations.initialMessages = sortedList;
                dbm.InsertOrUpdateMessages(sortedList);
                List<Guid> users = new List<Guid>();
                for (int i = 0; i < MessageConversations.initialMessages.Count; ++i)
                {
                    foreach (MessageRecipientDB guid in MessageConversations.initialMessages[i].MessageRecipientDBList)
                    {
                        UserDB user = new UserDB();
                        user = dbm.GetUserWithAccountID(guid.AccountGuid);
                        if (user == null && (guid.AccountGuid != AndroidData.CurrentUser.AccountID.ToString()))
                            users.Add(new Guid(guid.AccountGuid));
                    }
                }
                if (users.Count == 0)
                {
                    RunOnUiThread(delegate
                    {
                        if (progress != null)
                            progress.Dismiss();
                        CreateUI();
                    });
                } else
                {
                    createIt = true;
                    Contacts.AddUnknownUser auu = new Contacts.AddUnknownUser(users, context);
                    //createNewUsers (users);
                }
            }
        }
		
        private void textMessage_Click(object s, EventArgs e)
        {
            TextView senderObj = (TextView)s;
            string conID = senderObj.ContentDescription;
            Guid conGUID = new Guid(conID);
            if (MessageConversations.currentConversationMessages == null)
                MessageConversations.currentConversationMessages = new List<MessageDB>();
            else
                MessageConversations.currentConversationMessages.Clear();
            // find the conversation
			
            LOLMessageClient client = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            client.MessageGetConversationMessageCompleted += Message_GetConversationMessages;
            client.MessageGetConversationMessageAsync(AndroidData.CurrentUser.AccountID, conGUID, new DateTime(1901, 1, 1), DateTime.Now, new Guid(AndroidData.ServiceAuthToken));
        }
		
        private void Message_GetConversationMessages(object s, MessageGetConversationMessageCompletedEventArgs e)
        {
            LOLMessageClient client = (LOLMessageClient)s;
            if (e.Error == null)
            {
                if (MessageConversations.currentConversationMessages.Count != 0)
                    MessageConversations.currentConversationMessages.Clear();
					
                LOLMessageConversation conversation = e.Result;
                List<Guid> notFound = new List<Guid>();
                foreach (Guid guid in conversation.MessageIDs)
                {
                    MessageDB message = new MessageDB();
                    message = dbm.GetMessage(guid.ToString());
                    if (message == null)
                        notFound.Add(guid);
                    else
                        MessageConversations.currentConversationMessages.Add(message);
                }
                if (notFound.Count != 0)
                {
                    client.MessageGetByListCompleted += MessageListComplete;
                    client.MessageGetByListAsync(AndroidData.CurrentUser.AccountID, notFound, AndroidData.CurrentUser.AccountID, 
                                                  new Guid(AndroidData.ServiceAuthToken));
                } else
                {
                    UserDB user = new UserDB();
                    if (MessageConversations.currentConversationMessages [0].FromAccountGuid == AndroidData.CurrentUser.AccountID.ToString())
                        user = UserDB.ConvertFromUser(AndroidData.CurrentUser);
                    else
                        user = dbm.GetUserWithAccountID(MessageConversations.currentConversationMessages [0].FromAccountGuid);

                    if (user != null)
                    {
                        MessageConversations.firstUserName = user.FirstName + " " + user.LastName;
                        List<MessageDB> sortedList = new List<MessageDB>();
                        sortedList = MessageConversations.currentConversationMessages.OrderBy(t => t.MessageSent).ToList();
                        MessageConversations.currentConversationMessages = sortedList;
                        if (sortedList.Count > 0)
                        {
                            LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted -= MessageManager_MessageSendConfirm;
                            Intent g = new Intent(this, typeof(MessageList));
                            StartActivity(g);
                        }
                    }
                }
            } 
            #if DEBUG
            /*else
				System.Diagnostics.Debug.WriteLine ("Unable to get the conversation messages");*/
            #endif
        }
		
        private void MessageListComplete(object s, MessageGetByListCompletedEventArgs e)
        {
            LOLMessageClient client = (LOLMessageClient)s;
            client.MessageGetByListCompleted -= MessageListUpdatedComplete;
            if (e.Error == null)
            {
                foreach (LOLMessageDelivery.Message message in e.Result)
                {
                    MessageDB me = new MessageDB();
                    me = MessageDB.ConvertFromMessage(message);
                    MessageConversations.currentConversationMessages.Add(me);
                }
                dbm.InsertOrUpdateMessages(MessageConversations.currentConversationMessages);
			
                UserDB user = dbm.GetUserWithAccountID(MessageConversations.currentConversationMessages [0].FromAccountGuid);
                if (user == null && MessageConversations.currentConversationMessages [0].FromAccountGuid == AndroidData.CurrentUser.AccountID.ToString())
                {
                    user = UserDB.ConvertFromUser(AndroidData.CurrentUser);
                } else
                {
                    if (MessageConversations.currentConversationMessages [0].MessageRecipientDBList.Count > 0)
                        user = dbm.GetUserWithAccountID(MessageConversations.currentConversationMessages [0].MessageRecipientDBList [0].AccountGuid);
                }
                if (user != null)
                {	
                    MessageConversations.firstUserName = user.FirstName + " " + user.LastName;
                    List<MessageDB> sortedList = new List<MessageDB>();
                    sortedList = MessageConversations.currentConversationMessages.OrderBy(t => t.MessageSent).ToList();
                    MessageConversations.currentConversationMessages = sortedList;
                    if (sortedList.Count > 0)
                    {
                        LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted -= MessageManager_MessageSendConfirm;
                        Intent i = new Intent(this, typeof(MessageList));
                        StartActivity(i);
                    }
                }
            }
        }
		
        private void imgMessage_Click(object s, EventArgs e)
        {
            ImageView senderObj = (ImageView)s;
            string conID = senderObj.ContentDescription;
            playOut(conID);
        }
		
        private void messageBar_Click(object s, EventArgs e)
        {
            LinearLayout senderObj = (LinearLayout)s;
            string conID = senderObj.ContentDescription;
            MessageDB message = new MessageDB();
            message = dbm.GetMessage(conID);
            MessageReceived player = new MessageReceived(message, context);
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
		
        private void getNewMessages(List<Guid> newMessages)
        {
            LOLMessageClient client = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
            client.MessageGetByListCompleted += MessageListUpdatedComplete;
            client.MessageGetByListAsync(AndroidData.CurrentUser.AccountID, newMessages, AndroidData.CurrentUser.AccountID, 
                                          new Guid(AndroidData.ServiceAuthToken));
        }
		
        private void MessageListUpdatedComplete(object s, MessageGetByListCompletedEventArgs e)
        {
            LOLMessageClient client = (LOLMessageClient)s;
            client.MessageGetByListCompleted -= MessageListUpdatedComplete;
            if (e.Result == null)
            {
                List<MessageDB> newMessages = new List<MessageDB>();
                foreach (LOLMessageDelivery.Message message in e.Result)
                {
                    newMessages.Add(MessageDB.ConvertFromMessage(message));
                }
                dbm.InsertOrUpdateMessages(newMessages);
            }
        }
		
        private void Service_MessageMarkReadCompleted(object sender, MessageMarkReadCompletedEventArgs e)
        {
            LOLMessageClient service = (LOLMessageClient)sender;
            if (e.Error == null)
            {
                Queue<MessageDB> msgQ = (Queue<MessageDB>)e.UserState;
                MessageDB messageDB = msgQ.Dequeue();
                if (e.Result.ErrorNumber == "0" || string.IsNullOrEmpty(e.Result.ErrorNumber))
                {
                    dbm.MarkMessageRead(messageDB.MessageGuid, AndroidData.CurrentUser.AccountID.ToString());
                    if (msgQ.Count > 0)
                        service.MessageMarkReadAsync(msgQ.Peek().MessageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, new Guid(AndroidData.ServiceAuthToken), msgQ);
                    else
                    {
                        service.MessageMarkReadCompleted -= Service_MessageMarkReadCompleted;
                        MessageConversations.clearView = true;
                        getConversations();
                    }
                }
            } else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Exception when marking messages as read {0} {1}", e.Error.Message, e.Error.StackTrace);
#endif
            }
        }


        private void MessageManager_MessageSendConfirm(object sender, MessageSendConfirmEventArgs e)
        {
            MessageDB message = e.Message;
            if (message.MessageConfirmed)
            {
                MessageConversations.clearView = true;
                Thread m = new Thread(new ThreadStart(pause));
                m.Start();
            }
        }

        private void pause()
        {
            RunOnUiThread(() => progress = ProgressDialog.Show(context, Application.Context.Resources.GetString(Resource.String.messageRefreshingMessages), 
			                               Application.Context.Resources.GetString(Resource.String.messageViewingPleaseWait)));
            Thread.Sleep(2000);
            getConversations();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedConversationMessages;
            Messages.MessageReceivedUtil.FromMessagesDone = false;
            LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted += MessageManager_MessageSendConfirm;
            if (MessageConversations.clearView)
                getConversations();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
            LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted -= MessageManager_MessageSendConfirm;
        }
    }
}


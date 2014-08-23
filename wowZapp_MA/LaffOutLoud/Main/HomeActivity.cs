using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Graphics;
using Android.Views;
using LOLApp_Common;
using LOLMessageDelivery;
using LOLAccountManagement;

using WZCommon;

namespace wowZapp.Main
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
    public partial class HomeActivity : Activity
    {
        private const int CONTACTS = 0, MESSAGE_LIST = 1, LOGOUT = 2;
        private const int TEXT_MESSAGE = 10, DRAW_MESSAGE = 11, COMIX_MESSAGE = 12, 
            VOICE_MESSAGE = 13, COMICON_MESSAGE = 14, POLL_MESSAGE = 15, 
            VIDEO_MESSAGE = 16, SFX_MESSAGE = 17, EMOTICON_MESSAGE = 18;
        
        private Context context;
        private DBManager dbm;
        TextView txtMessage, txtName;
        ImageView contactPic;
        LinearLayout messageBar;
        ViewGroup parent;
        HorizontalScrollView hsv;
        private int picSize, posn, newX;
        private System.Timers.Timer scroller;
        private Dialog ModalPreviewDialog;
        private int markAsRead, current, unknownContact;
        private List<MessageDB> message;
        private List<Guid> unknownContacts;
        private Messages.MessageInfo messageItem;
        private bool killed;

		
        public override void OnBackPressed()
        {
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.HomeScreenscrollview);
            checkForImages();
            messageBar = FindViewById<LinearLayout>(Resource.Id.linmessageBlock);
            parent = messageBar;
            MessageItems = new Dictionary<Guid, Messages.MessageInfo>();
            context = messageBar.Context;
			
            if (unknownContacts == null)
                unknownContacts = new List<Guid>();
            else
                unknownContacts.Clear();
			
            if (Messages.ComposeMessageMainUtil.currentPosition == null)
            {
                Messages.ComposeMessageMainUtil.currentPosition = new int[6];
                for (int n = 0; n < 6; ++n)
                    Messages.ComposeMessageMainUtil.currentPosition [n] = 0;
            } else
                for (int n = 0; n < 6; ++n)
                    Messages.ComposeMessageMainUtil.currentPosition [n] = 0;

            if (Messages.ComposeMessageMainUtil.msgSteps != null)
                Messages.ComposeMessageMainUtil.msgSteps.Clear();
				
            if (Messages.MessageConversations.storedMessages == null)
                Messages.MessageConversations.storedMessages = new List<MessageDB>();
				
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewloginHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, context);
			
            Header.headertext = Application.Context.Resources.GetString(Resource.String.mainTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
            killed = false;
            newX = (int)wowZapp.LaffOutOut.Singleton.ScreenXWidth;
            //string filename = base.Intent.GetStringExtra ("playname");
            //string toplay = filename == "" ? String.Empty : filename;
            bool[] checkCameraRecord = new bool[3];
            AndroidData.IsAppActive = true;
            wowZapp.LaffOutOut.Singleton.ScreenXWidth = WindowManager.DefaultDisplay.Width;
            if (AndroidData.GeoLocationEnabled)
                wowZapp.LaffOutOut.Singleton.EnableLocationTimer();

            ImageButton btnAddressBook = FindViewById<ImageButton>(Resource.Id.btnAddressBook);
            btnAddressBook.Tag = 0;
            btnAddressBook.Click += delegate
            {
                StartActivityForResult(typeof(Contacts.ManageContactsActivity), CONTACTS);
            };
            ImageButton btnMessageList = FindViewById<ImageButton>(Resource.Id.btnMessageList);
            btnMessageList.Click += delegate
            {
                wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
                if (scroller != null)
                {
                    scroller.Stop();
                }
                Intent i = new Intent(this, typeof(Messages.Conversations));
                i.PutExtra("list", true);
                StartActivityForResult(i, MESSAGE_LIST); 
            };
            btnMessageList.Tag = 1;
            ImageButton btnLogout = FindViewById<ImageButton>(Resource.Id.btnLogout);
            btnLogout.Click += new EventHandler(btnLogout_Click);
            btnLogout.Tag = 2;

            if (Contacts.SelectContactsUtil.selectedContacts == null)
                Contacts.SelectContactsUtil.selectedContacts = new List<ContactDB>();

            ImageButton[] buttons = new ImageButton[3];
            buttons [0] = btnAddressBook;
            buttons [1] = btnMessageList;
            buttons [2] = btnLogout;
            Messages.MessageReceivedUtil.FromMessages = false;
            ImageButton startTextMessage = FindViewById<ImageButton>(Resource.Id.btnTextMessage);
            startTextMessage.Click += delegate
            {
                if (scroller != null)
                {
                    scroller.Stop();
                }
                Contacts.SelectContactsUtil.messageType = TEXT_MESSAGE;
                StartActivity(typeof(Contacts.SelectContactsActivity));
            };

            ImageButton startDrawMessage = FindViewById<ImageButton>(Resource.Id.btnDrawMessage);
            #if DEBUG
            startDrawMessage.Click += delegate
            {
                if (scroller != null)
                {
                    scroller.Stop();
                }
                Contacts.SelectContactsUtil.messageType = DRAW_MESSAGE;
                StartActivity(typeof(Contacts.SelectContactsActivity));
            };
            #endif

            ImageButton startComixMessage = FindViewById<ImageButton>(Resource.Id.btnComixMessage);
            startComixMessage.Click += delegate
            {
                if (scroller != null)
                {
                    scroller.Stop();
                }
                Contacts.SelectContactsUtil.messageType = COMIX_MESSAGE;
                StartActivity(typeof(Contacts.SelectContactsActivity));
            };

            ImageButton startComiconMessage = FindViewById<ImageButton>(Resource.Id.btnComiconMessage);
            startComiconMessage.Click += delegate
            {
                if (scroller != null)
                {
                    scroller.Stop();
                }
                Contacts.SelectContactsUtil.messageType = COMICON_MESSAGE;
                StartActivity(typeof(Contacts.SelectContactsActivity));
            };

            ImageButton startPollMessage = FindViewById<ImageButton>(Resource.Id.btnPollMessage);
            startPollMessage.Click += delegate
            {
                if (scroller != null)
                {
                    scroller.Stop();
                }
                Contacts.SelectContactsUtil.messageType = POLL_MESSAGE;
                StartActivity(typeof(Contacts.SelectContactsActivity));
            };

            ImageButton startSFXMessage = FindViewById<ImageButton>(Resource.Id.btnSFXMessage);
            startSFXMessage.Click += delegate
            {
                if (scroller != null)
                {
                    scroller.Stop();
                }
                Contacts.SelectContactsUtil.messageType = SFX_MESSAGE;
                StartActivity(typeof(Contacts.SelectContactsActivity));
            };

            ImageButton startEmoticonMessage = FindViewById<ImageButton>(Resource.Id.btnEmotMessage);
            startEmoticonMessage.Click += delegate
            {
                if (scroller != null)
                {
                    scroller.Stop();
                }
                Contacts.SelectContactsUtil.messageType = EMOTICON_MESSAGE;
                StartActivity(typeof(Contacts.SelectContactsActivity));
            };

            ImageButton startVideoMessage = FindViewById<ImageButton>(Resource.Id.btnVideoMessage);
            ImageButton startVoiceMessage = FindViewById<ImageButton>(Resource.Id.btnVoiceMessage);
			
            hsv = FindViewById<HorizontalScrollView>(Resource.Id.horizontalScrollView1);
            hsv.SmoothScrollingEnabled = true;
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            ImageHelper.setupButtonsPosition(buttons, bottom, context);

            dbm = wowZapp.LaffOutOut.Singleton.dbm;
            checkCameraRecord = GeneralUtils.CamRecLong();

            if (checkCameraRecord [1] == false)
                startVoiceMessage.SetBackgroundResource(Resource.Drawable.nomicrophone2);
            else
                startVoiceMessage.Click += delegate
                {
                    if (scroller != null)
                    {
                        scroller.Stop();
                    }
                    Contacts.SelectContactsUtil.messageType = VOICE_MESSAGE;
                    StartActivity(typeof(Contacts.SelectContactsActivity));
                };

            if (checkCameraRecord [0] == false || checkCameraRecord [2] == false)
                startVideoMessage.SetBackgroundResource(Resource.Drawable.novideo2);
#if DEBUG
			else
                startVideoMessage.Click += delegate
                {
                    if (scroller != null)
                    {
                        scroller.Stop();
                    }
                    Contacts.SelectContactsUtil.messageType = VIDEO_MESSAGE;
                    StartActivity(typeof(Contacts.SelectContactsActivity));
                };
#endif

            float picXSize = 100f;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                picXSize = (25 * (float)wowZapp.LaffOutOut.Singleton.ScreenXWidth) / 100;
            picSize = (int)ImageHelper.convertDpToPixel(picXSize, context);

            if (Messages.ComposeMessageMainUtil.msgSteps == null)
                Messages.ComposeMessageMainUtil.msgSteps = new List<LOLMessageDelivery.MessageStep>();
            if (Messages.ComposeMessageMainUtil.messageDB == null)
                Messages.ComposeMessageMainUtil.messageDB = new MessageDB();
            wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;
            //RunOnUiThread (delegate {
            wowZapp.LaffOutOut.Singleton.EnableMessageTimer();
            wowZapp.LaffOutOut.Singleton.CheckForUnsentMessages(context);
            getcontacts();
            //});
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (scroller != null)
                scroller.Start();
        }
		
        private void checkForImages()
        {
            string path = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, AndroidData.CurrentUser.AccountID.ToString());
            if (Contacts.ContactsUtil.contactFilenames == null)
                Contacts.ContactsUtil.contactFilenames = new List<string>();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AndroidData.HasImages = false;
            } else
            {
                if (Directory.GetFiles(path).Count() != 0)
                {
					
                    AndroidData.HasImages = true;
                    Contacts.ContactsUtil.contactFilenames = Directory.GetFiles(path).ToList();
                } else 
                    AndroidData.HasImages = false;
            }
            wowZapp.LaffOutOut.Singleton.ImageDirectory = path;
        }

        public Dictionary<Guid, Messages.MessageInfo> MessageItems
        {
            get;
            private set;
        }

        List<MessageDB> messageListAll = new List<MessageDB>();
		

        private void scroller_Elapsed(object s, System.Timers.ElapsedEventArgs e)
        {
            if (posn < messageListAll.Count - 2)
            {
                if (scroller.Interval == 0)
                    scroller.Interval = 2000;
                posn++;
                newX += (int)wowZapp.LaffOutOut.Singleton.ScreenXWidth;
                if (posn < 0)
                    posn = 0;
                UserDB tmp = dbm.GetUserWithAccountID(messageListAll [posn].FromAccountID.ToString());
                if (tmp != null && messageListAll [posn].MessageStepDBList.Count > 0)
                {
                    string messager = shortenBubble(messageListAll [posn]);
                    generateMessageBarAndAnimate(messager, messageListAll [posn], tmp, true);
                }
            } else
            {
                posn = -1;
                scroller.Interval = 0;
            }
        }

        private void AppDelegate_ReceivedMessages(object sender, IncomingMessageEventArgs e)
        {
            List <Messages.MessageInfo> messageItems = new List<Messages.MessageInfo>();
            Guid me = AndroidData.CurrentUser.AccountID;

            //RunOnUiThread (delegate {
            foreach (LOLMessageDelivery.Message eachMessage in e.Messages)
            {
                MessageDB msgDB = MessageDB.ConvertFromMessage(eachMessage);
                UserDB user = msgDB.FromAccountID == me ? UserDB.ConvertFromUser(AndroidData.CurrentUser) :
						dbm.GetUserWithAccountID(msgDB.FromAccountGuid);
						
                if (user == null)
                {
                    unknownContacts.Add(msgDB.FromAccountID);
                    //Contacts.AddUnknownUser uku = new Contacts.AddUnknownUser (unknowns, context);
                }
					
                Messages.MessageInfo msgInfo = new Messages.MessageInfo(msgDB, user);

                if (msgInfo != null)
                {
                    messageItems.Add(msgInfo);
                }
            }

            if (messageItems.Count > 0)
            {
                if (message == null)
                    message = new List<MessageDB>();
                else
                    message.Clear();
                foreach (Messages.MessageInfo eachMessageInfo in messageItems)
                {
                    this.MessageItems [eachMessageInfo.Message.MessageID] = eachMessageInfo;
                    message.Add(eachMessageInfo.Message);
                }

                dbm.InsertOrUpdateMessages(message);
                messageItem = messageItems [0];
                markAsRead = message.Count;
                current = 0;
                LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                service.MessageMarkReadCompleted += Service_MessageMarkReadCompleted;
                service.MessageMarkReadAsync(message [current].MessageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, new Guid(AndroidData.ServiceAuthToken), 
				                              message [current].MessageID);
            }
            //});
        }
		
        private void Service_MessageMarkReadCompleted(object sender, MessageMarkReadCompletedEventArgs e)
        {
            LOLMessageClient service = (LOLMessageClient)sender;
			
            if (null == e.Error)
            {
				
                if (e.Result.ErrorNumber == "0" || string.IsNullOrEmpty(e.Result.ErrorNumber))
                {
                    Guid messageID = (Guid)e.UserState;
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Marked message as read!");
#endif
                    dbm.MarkMessageRead(messageID.ToString(), AndroidData.CurrentUser.AccountID.ToString());
                    current++;
                    if (current < markAsRead)
                    {
                        service.MessageMarkReadAsync(message [current].MessageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, new Guid(AndroidData.ServiceAuthToken), 
						                              message [current].MessageID);
                    } else
                    {
                        service.MessageMarkReadCompleted -= Service_MessageMarkReadCompleted;
                        if (unknownContacts.Count != 0)
                        {
                            getNewContacts(unknownContacts);
                        } else
						if (message [0].MessageRecipientDBList != null)
                        {
                            MessageDB mess = new MessageDB();
                            mess = messageItem.Message;
                            generateMessageBarAndAnimate(shortenBubble(mess), message [0], messageItem.MessageUser);
                        }
                    }
                }//end if
            } else
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception in message mark as read! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
            }//end if else
        }

        private void textMessage_Click(object s, EventArgs e)
        {
            TextView senderObj = (TextView)s;
            Messages.MessageInfo msgInfo = null;
            Messages.ComposeMessageMainUtil.messageDB = null;
            if (this.MessageItems.TryGetValue(new Guid(senderObj.ContentDescription), out msgInfo))
            {
                Messages.ComposeMessageMainUtil.messageDB = msgInfo.Message;
                Intent i = new Intent(this, typeof(Messages.ComposeMessageMainActivity));
                i.PutExtra("readonly", true);
                StartActivity(i);
            }
        }

        private void imgMessage_Click(object s, EventArgs e)
        {
            ImageView senderObj = (ImageView)s;
            Messages.MessageInfo msgInfo = null;
            Messages.ComposeMessageMainUtil.messageDB = null;

            if (this.MessageItems.TryGetValue(new Guid(senderObj.ContentDescription), out msgInfo))
            {
                Messages.ComposeMessageMainUtil.messageDB = msgInfo.Message;
                Intent i = new Intent(this, typeof(Messages.ComposeMessageMainActivity));
                i.PutExtra("readonly", true);
                StartActivity(i);
            }
        }

        private void random_Click(object s, EventArgs e)
        {
            ImageView senderObj = (ImageView)s;
            Messages.MessageInfo msgInfo = null;
            Messages.ComposeMessageMainUtil.messageDB = null;
            if (this.MessageItems.TryGetValue(new Guid(senderObj.ContentDescription), out msgInfo))
            {
                Messages.ComposeMessageMainUtil.messageDB = msgInfo.Message;
                Intent i = new Intent(this, typeof(Messages.ComposeMessageMainActivity));
                i.PutExtra("readonly", true);
                StartActivity(i);
            }
        }

        private void Service_UserGetImageDataCompleted(object sender, UserGetImageDataCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;

            if (e.Result.Errors.Count == 0 && (e.Result.ImageData.Length > 0 && e.Result.ImageData.Length != 2))
            {
                using (Bitmap userImage = ImageHelper.CreateUserProfileImageForDisplay(e.Result.ImageData, picSize, picSize, this.Resources))
                {
                    ImageView pic = (ImageView)messageBar.FindViewWithTag(new Java.Lang.String("profilepic_" + e.Result.AccountID));
                    if (pic != null)
                    {
                        RunOnUiThread(() => pic.SetImageBitmap(userImage));
                        File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, e.Result.AccountID.ToString()), e.Result.ImageData);
                        Contacts.ContactsUtil.contactFilenames.Add(e.Result.AccountID.ToString());
                    } else
                        RunOnUiThread(() => pic.SetImageResource(Resource.Drawable.defaultuserimage));
                }

                service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
                dbm.UpdateUserImage(e.Result.AccountID.ToString(), e.Result.ImageData);
            }
        }

        private void GetMessages()
        {
            if (!killed)
            {
                if (Messages.MessageConversations.storedMessages.Count != 0)
                    Messages.MessageConversations.storedMessages.Clear();
                List<MessageDB> message = new List<MessageDB>();
                message = dbm.GetAllMessagesForOwner(AndroidData.CurrentUser.AccountID.ToString());
                foreach (MessageDB m in message)
                    Messages.MessageConversations.storedMessages.Add(m);
                if (Messages.MessageConversations.storedMessages.Count == 0)
                {
                    List<Guid> excludeMessageGuids = new List<Guid>();
                    /*excludeMessageGuids = this.MessageItems.Values.Where (s => s.Message.MessageStepDBList.Count (t => t.StepID == default(Guid)) == 0)
                    .Select (s => s.Message.MessageID).ToList ();*/
                    excludeMessageGuids = Messages.MessageConversations.storedMessages.Where(s => s.MessageStepDBList.Count(t => t.StepID == default(Guid)) == 0)
						.Select(s => s.MessageID).ToList();
                    LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                    service.MessageGetNewCompleted += Service_MessageGetNewCompleted;
                    service.MessageGetNewAsync(AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID,
                                                     excludeMessageGuids,
                                                     new Guid(AndroidData.ServiceAuthToken));
                } else
                {
                    List<MessageDB> messages = new List<MessageDB>();
                    List<UserDB> users = new List<UserDB>();
                    foreach (MessageDB eachMessageDB in message)
                    {
                        if (eachMessageDB != null)
                        {
                            UserDB contactUser = null;
                            Messages.MessageInfo msgInfoItem = null;

                            if (eachMessageDB.FromAccountID != AndroidData.CurrentUser.AccountID)
                            {
                                foreach (ContactDB contact in Contacts.ContactsUtil.contacts)
                                {
                                    if (contact.ContactAccountID == eachMessageDB.FromAccountID)
                                    {
                                        contactUser = UserDB.ConvertFromUser(contact.ContactUser);
                                        break;
                                    } else
                                        contactUser = dbm.GetUserWithAccountID(eachMessageDB.FromAccountGuid);
                                }

                                msgInfoItem = new Messages.MessageInfo(eachMessageDB, contactUser);
                            } else
                            {
                                contactUser = UserDB.ConvertFromUser(AndroidData.CurrentUser);
                                msgInfoItem = new Messages.MessageInfo(eachMessageDB, contactUser);

                                if (!eachMessageDB.MessageConfirmed)
                                {
                                    ContentInfo contentInfo = dbm.GetContentInfoByMessageDBID(eachMessageDB.ID);
                                    msgInfoItem.ContentInfo = contentInfo;
                                }
                            }
                            messages.Add(eachMessageDB);
                            users.Add(contactUser);
                            this.MessageItems [eachMessageDB.MessageID] = msgInfoItem;
                        }
                    }
                }
            }
        }

        private void Service_MessageGetNewCompleted(object sender, MessageGetNewCompletedEventArgs e)
        {
            LOLMessageClient service = (LOLMessageClient)sender;
            service.MessageGetNewCompleted -= Service_MessageGetNewCompleted;

            if (null == e.Error)
            {
                LOLMessageDelivery.Message[] result = e.Result.ToArray();
                List<MessageDB> msgList = new List<MessageDB>();
                UserDB contactUser = null;
                foreach (LOLMessageDelivery.Message eachMessage in result)
                {
                    if (eachMessage.Errors.Count > 0)
                    {
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("Error retrieving message: {0}", StringUtils.CreateErrorMessageFromMessageGeneralErrors(eachMessage.Errors));
                        #endif
                    } else
                    {
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("**Message id received: {0}", eachMessage.MessageID);
                        #endif

                        MessageDB msgDB = MessageDB.ConvertFromMessage(eachMessage);
                        msgList.Add(msgDB);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Message in conversation received! {0}", msgDB);
                        #endif

                        contactUser = msgDB.FromAccountID == AndroidData.CurrentUser.AccountID ? UserDB.ConvertFromUser(AndroidData.CurrentUser) :
                                                                                   dbm.GetUserWithAccountID(msgDB.FromAccountGuid);

                        this.MessageItems [eachMessage.MessageID] = new Messages.MessageInfo(msgDB, contactUser);
                    }//end if else
                }//end foreach

                if (msgList.Count > 0)
                    dbm.InsertOrUpdateMessages(msgList);
                if (!killed)
                    grabAndDisplayLastTen();
            }
        }
        private ProgressDialog pd;
        private void getcontacts()
        {
            if (!killed)
            {
                RunOnUiThread(() => pd = ProgressDialog.Show(context, Application.Resources.GetString(Resource.String.contactsTitle), 
				                                               Application.Resources.GetString(Resource.String.contactsRefreshing)));
                if (Contacts.ContactsUtil.contacts == null)
                    Contacts.ContactsUtil.contacts = new List<ContactDB>();
                List<Guid> excludeContactIDs = new List<Guid>();
                excludeContactIDs = dbm.GetAllContactsForOwner(AndroidData.CurrentUser.AccountID.ToString()).Select(s => s.ContactAccountID).ToList();
                if (excludeContactIDs.Count != 0)
                    saveProfileImages(excludeContactIDs);
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.ContactsGetListCompleted += Service_ContactsGetListCompleted;
                service.ContactsGetListAsync(AndroidData.CurrentUser.AccountID, excludeContactIDs, new Guid(AndroidData.ServiceAuthToken));
            }
        }

        private void saveProfileImages(List<Guid> users)
        {
            foreach (Guid user in users)
            {
                if (!Contacts.ContactsUtil.contactFilenames.Contains(user.ToString()))
                {
                    UserDB usr = dbm.GetUserWithAccountID(user.ToString());
                    if (usr.Picture.Length > 0)
                    {
                        File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, usr.AccountGuid), usr.Picture);
                        Contacts.ContactsUtil.contactFilenames.Add(user.ToString());
                    }
                }
            }
        }
		
        private void Service_ContactsGetListCompleted(object sender, ContactsGetListCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.ContactsGetListCompleted -= Service_ContactsGetListCompleted;

            if (null == e.Error)
            {
                Contact[] result = e.Result.ToArray();
                dbm.InserOrUpdateContacts(new List<Contact>(result));
                Contacts.ContactsUtil.contacts = dbm.GetAllContactsForOwner(AndroidData.CurrentUser.AccountID.ToString());
#if DEBUG
                foreach (ContactDB contact in Contacts.ContactsUtil.contacts)
                    System.Diagnostics.Debug.WriteLine(contact.ContactUser.FirstName + " " + contact.ContactUser.LastName);
#endif
                RunOnUiThread(delegate
                {
                    if (pd != null)
                        pd.Dismiss();
                });
                GetMessages();
                grabAndDisplayLastTen();
				
            }
        }

        private void getNewContacts(List<Guid>contactsToGet)
        {
            LOLConnectClient client = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            Queue<Guid> userQ = new Queue<Guid>();
            foreach (Guid guid in contactsToGet)
                userQ.Enqueue(guid);
            client.UserGetSpecificAsync(AndroidData.CurrentUser.AccountID, userQ.Peek(), new Guid(AndroidData.ServiceAuthToken), userQ);
            client.UserGetSpecificCompleted += (object sender, UserGetSpecificCompletedEventArgs e) => 
            {
                if (null == e.Error)
                {
                    LOLAccountManagement.User result = e.Result;
                    if (result.Errors.Count > 0)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Error retrieving user: {0}", StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors));
#endif
                    } else
                    {
                        Queue<Guid> userQQ = (Queue<Guid>)e.UserState;
						
                        if (!dbm.CheckUserExists(userQQ.Dequeue().ToString()))
                        {
                            dbm.InsertOrUpdateUser(result);
                            if (result.HasProfileImage)
                                getContactImage(userQQ.Peek(), true);
                        }
						
                        if (userQQ.Count > 0) 
                            client.UserGetSpecificAsync(AndroidData.CurrentUser.AccountID, userQQ.Peek(), new Guid(AndroidData.ServiceAuthToken), userQQ);
                    }
                }
            };
        }

        private Bitmap getContactImage(Guid accountId, bool getImages)
        {
            Bitmap blank = null;
            if (!getImages)
            {
                blank = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.defaultuserimage);
                return blank;
            }

            if (!Contacts.ContactsUtil.contactFilenames.Contains(accountId.ToString()))
            {
                LOLConnectClient client = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                client.UserGetImageDataCompleted += (object sender, UserGetImageDataCompletedEventArgs e) => {
                    if (e.Error == null)
                    {
						
                        if (e.Result.ImageData.Length == 0)
                            blank = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.defaultuserimage);
                        else
                        {
                            Contacts.ContactsUtil.contactFilenames.Add(accountId.ToString());
                            File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, e.Result.AccountID.ToString()), 
							                                       e.Result.ImageData);
                            blank = BitmapFactory.DecodeFile(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, accountId.ToString()));
                        }
                        //tcs.SetResult (blank);
                    }
                };
                client.UserGetImageDataAsync(AndroidData.CurrentUser.AccountID, accountId, new Guid(AndroidData.ServiceAuthToken));
            }
            return blank;
        }

        public void DismissModalPreviewDialog()
        {
            if (ModalPreviewDialog != null)
                RunOnUiThread(() => ModalPreviewDialog.Dismiss());
            ModalPreviewDialog = null;
        }
		
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
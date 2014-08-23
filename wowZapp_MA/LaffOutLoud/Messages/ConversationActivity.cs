using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using LOLApp_Common;
using LOLMessageDelivery;
using LOLAccountManagement;

namespace wowZapp.Messages
{
	public static class ConversationsUtil
	{
		public static UserDB userFrom
        { get; set; }
		public static ConversationInfo conversation
        { get; set; }
	}

	public class Conversations
	{
		public Conversations (UserDB userFrom, Context context)
		{
			ConversationsUtil.userFrom = userFrom;
			ConversationsUtil.conversation = null;
			startActivity (context);
		}

		public Conversations (ConversationInfo conversation, Context context)
		{
			ConversationsUtil.userFrom = null;
			ConversationsUtil.conversation = conversation;
			startActivity (context);
		}

		private void startActivity (Context c)
		{
			Intent i = new Intent (c, typeof(ConversationActivity));
			c.StartActivity (i);
		}
	}


	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public class ConversationActivity : Activity
	{
		private Context context;
		LinearLayout converse;
		private Dictionary<int, ContentPackItem> contentPackItems;
		private Dictionary<int, string> voiceFiles;
		private Dictionary<int, PollingStep> pollingSteps;
		private Dictionary<Guid, ContactDB> contacts;
		private bool saveContactAsked, checkForSentMessage;
		private MessageManager mmg;
		private DBManager dbm;
		private string isMe;

		private const int CHOOSE_NEW = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Conversations);

			ImageView btns = FindViewById<ImageView> (Resource.Id.imgNewUserHeader);
			TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
			ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
			context = header.Context;
			Header.headertext = Application.Context.Resources.GetString (Resource.String.converseMainTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;

			isMe = AndroidData.CurrentUser.FirstName + " " + AndroidData.CurrentUser.LastName;
			mmg = wowZapp.LaffOutOut.Singleton.mmg;
			dbm = wowZapp.LaffOutOut.Singleton.dbm;
			contentPackItems = new Dictionary<int,ContentPackItem> ();
			voiceFiles = new Dictionary<int,string> ();
			pollingSteps = new Dictionary<int,PollingStep> ();
			contacts = new Dictionary<Guid,ContactDB> ();

			mmg.MessageSendConfirmCompleted += MessageManager_MessageSendConfirmCompleted;
			wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;

			ImageButton btnAdd = FindViewById<ImageButton> (Resource.Id.btnAdd);
			btnAdd.Tag = 2;
			ImageButton btnHome = FindViewById<ImageButton> (Resource.Id.btnHome);
			btnHome.Tag = 1;
			ImageButton btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
			btnBack.Tag = 0;
			LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
			ImageButton[] buttons = new ImageButton[3];
			buttons [0] = btnBack;
			buttons [1] = btnHome;
			buttons [2] = btnAdd;
			ImageHelper.setupButtonsPosition (buttons, bottom, context);

			if (ConversationsUtil.userFrom != null) {
				UserDB lastFrom = ConversationInfo.GetSenderOfOldestMessage ();
				if (lastFrom == null)
					lastFrom = ConversationInfo.GetAllConversationParticipants () [0];
				Header.headertext = ConversationInfo.GetConversationPartipantsNameTitle ();
				Header.fontsize = 36f;
				ImageHelper.fontSizeInfo (header.Context);
				header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
				header.Text = Header.headertext;
				LinearLayout ll4 = FindViewById<LinearLayout> (Resource.Id.linearLayout4);
				btnAdd.Visibility = ViewStates.Invisible;
				btnBack.Visibility = ViewStates.Invisible;
				Button btnAll = new Button (context);
				Rect rect = new Rect ();
				btnHome.GetDrawingRect (rect);
				int xLeft = rect.Left;
				int yTop = rect.Top;
				btnAll.Visibility = ViewStates.Invisible;
				btnAll.SetWidth (400);
				btnAll.SetHeight (200);
				btnAll.Gravity = GravityFlags.CenterVertical;
				btnAll.Layout (xLeft, yTop, 400, 200);
				btnAll.SetPadding (30, 20, 30, 20);
				btnAll.SetTextColor (Color.Black);
				btnAll.Text = Application.Context.Resources.GetString (Resource.String.converseShowAllBtn);
				btnAll.Click += new EventHandler (btnAll_Click);
				ll4.AddView (btnAll);
				ll4.Invalidate ();
			}

			RunOnUiThread (delegate {
				contacts = dbm.GetAllContactsForOwner (AndroidData.CurrentUser.AccountID.ToString ()).ToDictionary (s => s.ContactAccountID, s => s);
				GetMessageRows ();
			});
		}

		private Dictionary<Guid, Messages.MessageInfo> MessageItems {
			get;
			set;
		}

		private UserDB UserFrom 
        { get; set; }

		private ConversationInfo ConversationInfo
        { get; set; }

		private void btnAll_Click (object s, EventArgs e)
		{
		}

		private void GetMessageRows ()
		{
			List<ConversationInfo> conversationInfo = ConversationInfo.DistributeToConversations (this.MessageItems.Values.ToList ());
			conversationInfo.Sort (delegate(ConversationInfo x, ConversationInfo y) {
				return y.GetLatestMessage ().Message.MessageSent.CompareTo (x.GetLatestMessage ().Message.MessageSent);
			});

			string ownerAccountID = AndroidData.CurrentUser.AccountID.ToString ();

			foreach (ConversationInfo eachConversationInfo in conversationInfo) {
				MessageInfo latestConversationMessage =
                    eachConversationInfo.GetLatestMessage ();

				bool isTextOnly = latestConversationMessage.IsTextOnly;

				string messageCountStr = eachConversationInfo.Messages.Count (s => s.Value.Message.MessageRecipientDBList.Count
                    (t => t.AccountGuid == ownerAccountID && !t.IsRead) > 0).ToString ();

				addConversation (latestConversationMessage.Message.MessageGuid, messageCountStr, eachConversationInfo, isTextOnly ? "BubbleCell" : "MediaMessageCell");

#if DEBUG
				System.Diagnostics.Debug.WriteLine ("latestConversationMessage.Message.MessageGuid = {0}, messageCountStr = {1}, eachConversationInfo = {2}, isTextOnly {3}",
                    latestConversationMessage.Message.MessageGuid, messageCountStr, eachConversationInfo, isTextOnly ? "BubbleCell" : "MediaMessageCell");
#endif
			}
		}

		private void addConversation (string messageGuid, string messageCounter, ConversationInfo converation, string convType)
		{
		}

		private void AppDelegate_ReceivedMessages (object sender, IncomingMessageEventArgs e)
		{
			List<MessageInfo> messageItems = new List<MessageInfo> ();
            
			foreach (LOLMessageDelivery.Message eachMessage in e.Messages) {
				MessageDB msgDB = MessageDB.ConvertFromMessage (eachMessage);
				MessageInfo msgInfo = new MessageInfo (msgDB, msgDB.FromAccountID == AndroidData.CurrentUser.AccountID ?
                                                      UserDB.ConvertFromUser (AndroidData.CurrentUser) :
                                                      dbm.GetUserWithAccountID (msgDB.FromAccountGuid));
				if (msgInfo != null)
					messageItems.Add (msgInfo);
			}//end foreach

			if (messageItems.Count > 0) {
				foreach (MessageInfo eachMessageInfo in messageItems) {
					this.MessageItems [eachMessageInfo.Message.MessageID] = eachMessageInfo;
				}
				RunOnUiThread (delegate {
					for (int i = 0; i < messageItems.Count; ++i) {
						createUI (messageItems [i].Message, messageItems [i].MessageUser);
					}
				});
			}//end if
		}

		private void loadProfilePicture (Guid contactId)
		{
			if (contactId != null) {
				ContactDB cont = new ContactDB ();
				if (contacts.ContainsKey (contactId))
					cont = contacts [contactId];

				byte[] imgdata = cont.ContactUser.Picture;
				Bitmap original = BitmapFactory.DecodeByteArray (imgdata, 0, imgdata.Length);
				Bitmap mask = Bitmap.CreateScaledBitmap (BitmapFactory.DecodeResource (Resources, Resource.Drawable.emptybackground), original.Width, original.Height, true);
				Bitmap result = Bitmap.CreateBitmap (mask.Width, mask.Height, Bitmap.Config.Argb8888);
				Canvas canv = new Canvas (result);
				Paint paint = new Paint (PaintFlags.AntiAlias);

				paint.SetXfermode (new PorterDuffXfermode (PorterDuff.Mode.DstIn));
				canv.DrawBitmap (original, 0, 0, null);
				canv.DrawBitmap (mask, 0, 0, paint);
				paint.SetXfermode (null);

				RunOnUiThread (delegate {
					ImageView pic = (ImageView)converse.FindViewWithTag (new Java.Lang.String ("profilepic_" + contactId.ToString ()));
					pic.SetImageBitmap (result);
					original.Dispose ();
					mask.Dispose ();
					paint.Dispose ();
					canv.Dispose ();
					result.Dispose ();
				});
			}
		}

		private void createUI (MessageDB message, UserDB contact)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine ("AccoundID (MC) = {0}", AndroidData.CurrentUser.AccountID.ToString ());
#endif
			int m = 0;
			string messager = "";
			if (message != null && contact != null) {
				RunOnUiThread (delegate {
					ImageView random = null;
					LinearLayout layout = new LinearLayout (context);
					layout.Orientation = Android.Widget.Orientation.Horizontal;
					layout.SetGravity (GravityFlags.CenterVertical);
					layout.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (10f, context));

					ImageView profilepic = new ImageView (context);
					profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (60f, context), (int)ImageHelper.convertDpToPixel (100f, context));
					profilepic.Tag = new Java.Lang.String ("profilepic_" + m.ToString ());

					if (contact == null)
						profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
					else {
						if (contact.Picture.Length > 0) {
							//profilepic.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromStream(new MemoryStream(eachContact.ContactUser.Picture), "Profile"));
							loadProfilePicture (contact.AccountID);
						} else {
							profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
						}

						//layout.AddView(profilepic);

						LinearLayout layout2 = new LinearLayout (context);
						layout2.Orientation = Orientation.Vertical;
						layout2.SetGravity (GravityFlags.Center);

						TextView name = new TextView (context);
						name.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (260f, context), (int)ImageHelper.convertDpToPixel (40f, context));
						name.Gravity = GravityFlags.Center;
						name.SetTextColor (Color.White);
						name.TextSize = 16f;
						name.Text = contact.FirstName + " " + contact.LastName;

						layout2.AddView (name);

						TextView text = new TextView (context);
						text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (240f, context), (int)ImageHelper.convertDpToPixel (60f, context));
						text.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
						text.Gravity = GravityFlags.CenterVertical;
						if (name.Text == isMe)
							text.SetBackgroundResource (Resource.Drawable.bubblesolidright);
						else
							text.SetBackgroundResource (Resource.Drawable.bubblesolidleft);

						text.TextSize = 16f;

						for (int e = 0; e < message.MessageStepDBList.Count; ++e) {
							if (!string.IsNullOrEmpty (message.MessageStepDBList [e].MessageText)) {
								messager = message.MessageStepDBList [e].MessageText;
								break;
							}
						}

						if (string.IsNullOrEmpty (messager))
							messager = "No text message found";

						int nolines = messager.Length / 27;
						text.SetHeight ((nolines + 1) * 20);

						text.SetTextColor (Android.Graphics.Color.Black);
						text.Text = messager;
						layout2.Clickable = true;
						layout2.AddView (text);
						if (message.MessageStepDBList.Count > 1) {
							LinearLayout layout3 = new LinearLayout (context);
							layout3.Orientation = Orientation.Horizontal;
							if (name.Text == isMe)
								layout3.SetGravity (GravityFlags.Right);
							else
								layout3.SetGravity (GravityFlags.Left);
							layout3.SetBackgroundResource (Resource.Drawable.attachmentspreviewbkgr);
							layout3.SetVerticalGravity (GravityFlags.CenterVertical);
							layout3.SetMinimumHeight (30);
							layout3.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);

							if (name.Text != isMe) {
								random = new ImageView (context);
								random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
								random.SetBackgroundResource (Resource.Drawable.playblack);
								//random.Click += delegate { PlayMessage(message); };
								layout3.AddView (random);
							}

							for (int i = 1; i < message.MessageStepDBList.Count; ++i) {
								switch (message.MessageStepDBList [i].StepType) {
								case LOLMessageDelivery.MessageStep.StepTypes.Animation:
									random = new ImageView (context);
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.drawicon);
									layout3.AddView (random);
									break;
								case LOLMessageDelivery.MessageStep.StepTypes.Comicon:
									random = new ImageView (context);
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.comicon);
									layout3.AddView (random);
									break;
								case LOLMessageDelivery.MessageStep.StepTypes.Comix:
									random = new ImageView (context);
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.comicmsgs);
									layout3.AddView (random);
									break;
								case LOLMessageDelivery.MessageStep.StepTypes.Emoticon:
									random = new ImageView (context);
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.emoticonmsg);
									layout3.AddView (random);
									break;
								case LOLMessageDelivery.MessageStep.StepTypes.Polling:
									random = new ImageView (context);
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.pollmsg);
									layout3.AddView (random);
									break;
								case LOLMessageDelivery.MessageStep.StepTypes.SoundFX:
									random = new ImageView (context);
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.soundfxmsg);
									layout3.AddView (random);
									break;
								case LOLMessageDelivery.MessageStep.StepTypes.Video:
									random = new ImageView (context);
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.videomsg);
									layout3.AddView (random);
									break;
								case LOLMessageDelivery.MessageStep.StepTypes.Voice:
									random = new ImageView (context);
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.voicemsg);
									layout3.AddView (random);
									break;
								}
							}
							if (name.Text == isMe) {
								random = new ImageView (context);
								random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
								random.SetBackgroundResource (Resource.Drawable.playblack);
								//random.Click += delegate { PlayMessage(message); };
								layout3.AddView (random);
							}
							layout2.AddView (layout3);
						}
						if (name.Text != isMe) {
							layout.AddView (profilepic);
							layout.AddView (layout2);
						} else {
							layout.AddView (layout2);
							layout.AddView (profilepic);
						}

						converse.AddView (layout);
						m++;
					}
				});
			}
		}

		private void ShowAll (object s, EventArgs e)
		{
			List<UserDB> convUsers = ConversationInfo.GetAllConversationParticipants ();
			convUsers = convUsers.OrderBy (t => t.LastName).OrderBy (t => t.FirstName).ToList ();
			// reorder form
		}

		private void btnAdd_Click (object s, EventArgs e)
		{
			Intent i = new Intent (this, typeof(ComposeMessageMainUtil));
			StartActivityForResult (i, CHOOSE_NEW);
		}

		private void MarkMessagesAsRead ()
		{
			string ownerID = AndroidData.CurrentUser.AccountID.ToString ();
			Queue<MessageDB> msgQ = new Queue<MessageDB> ();
			foreach (MessageDB eachMessageDB in ConversationInfo.Messages.Values.Select(s => s.Message)
                .Where(s => s.MessageRecipientDBList.Count(t => t.AccountGuid == ownerID && !t.IsRead) > 0))
				msgQ.Enqueue (eachMessageDB);

			if (msgQ.Count > 0) {
				LOLMessageClient service = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
				service.MessageMarkReadCompleted += Service_MessageMarkReadCompleted;
				service.MessageMarkReadAsync (msgQ.Peek ().MessageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, 
                    new Guid (AndroidData.ServiceAuthToken), msgQ);
			}
		}

		private void Service_MessageMarkReadCompleted (object s, MessageMarkReadCompletedEventArgs e)
		{
			LOLMessageClient service = (LOLMessageClient)s;
			if (e.Error == null) {
				Queue<MessageDB> msq = (Queue<MessageDB>)e.UserState;
				MessageDB messageDB = msq.Dequeue ();
				if (e.Result.ErrorNumber == "0" || string.IsNullOrEmpty (e.Result.ErrorNumber))
					dbm.MarkMessageRead (messageDB.MessageGuid, AndroidData.CurrentUser.AccountID.ToString ());
				if (msq.Count > 0)
					service.MessageMarkReadAsync (msq.Peek ().MessageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, 
                        new Guid (AndroidData.ServiceAuthToken), msq);
				else
					service.MessageMarkReadCompleted -= Service_MessageMarkReadCompleted;
			} else {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Exception when marking messages as read {0} - {1}", e.Error.Message, e.Error.StackTrace);
#endif
			}
		}

		private void AddUserAsContact ()
		{
			RunOnUiThread (delegate {
				Toast.MakeText (context, Resource.String.contactsAddingContact, ToastLength.Short).Show ();
			});
			Contact contact = new Contact ();
			contact.Blocked = false;
			contact.ContactAccountID = UserFrom.AccountID;
			contact.OwnerAccountID = AndroidData.CurrentUser.AccountID;
			LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
			service.ContactsSaveCompleted += Service_ContactsSaveCompleted;
			service.ContactsSaveAsync (contact, new Guid (AndroidData.ServiceAuthToken));
		}
        
		private void Service_ContactsSaveCompleted (object s, ContactsSaveCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)s;
			service.ContactsSaveCompleted -= Service_ContactsSaveCompleted;
			if (e.Error == null) {
				Contact result = e.Result;
				if (result.Errors.Count > 0) {
					RunOnUiThread (delegate {
						GeneralUtils.Alert (context, Application.Context.Resources.GetString (Resource.String.errorSaveContactTitle), Application.Context.Resources.GetString (Resource.String.errorSaveContactMessage));
					});
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Error saving contact - {0}", StringUtils.CreateErrorMessageFromGeneralErrors (result.Errors.ToArray ()));
#endif
				} else {
					result.ContactUser = UserDB.ConvertFromUserDB (UserFrom);
					dbm.InserOrUpdateContacts (new List<Contact> () { result });
				}
			} else {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Exception saving contact {0} - {1}", e.Error.Message, e.Error.StackTrace);
#endif
			}
		}

		private void CheckForSentMessage ()
		{
			MessageDB latest = dbm.GetLatestSentMessage (AndroidData.CurrentUser.AccountID.ToString ());
			if (latest != null) {
				MessageInfo msgInfo = new MessageInfo (latest, UserDB.ConvertFromUser (AndroidData.CurrentUser));
				if (ConversationInfo.ConversationID.Equals (msgInfo.GetConversationID ())) {
					if (!ConversationInfo.Messages.ContainsKey (latest.MessageID)) {
						ConversationInfo.Messages [latest.MessageID] = msgInfo;
						// reload view
					}
				}
			}
		}

		private void MessageManager_MessageSendConfirmCompleted (object s, MessageSendConfirmEventArgs e)
		{
			MessageDB message = e.Message;
			if (message.MessageConfirmed) {
				MessageInfo msgInfo = new MessageInfo (message, UserDB.ConvertFromUser (AndroidData.CurrentUser));
				if (ConversationInfo.IsInConversation (msgInfo) && !(ConversationInfo.Messages.ContainsKey (message.MessageID)))
					ConversationInfo.Messages.Add (message.MessageID, msgInfo);
			}
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
		}
	}
}
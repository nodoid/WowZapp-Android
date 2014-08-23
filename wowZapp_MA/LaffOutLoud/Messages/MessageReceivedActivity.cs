using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Android.Views;

using LOLApp_Common;
using LOLMessageDelivery;
using LOLAccountManagement;
using Android.Runtime;

namespace wowZapp.Messages
{
	public static class MessageReceivedUtil
	{
		public static MessageDB message
		{ get; set; }
		public static User userFrom
		{ get; set; }
		public static Context context
		{ get; set; }
		public static MessageEditType editType
		{ get; set; }
		public static bool readOnly
        { get; set; }
		public static bool FromMessages
       	{ get; set; }
		public static bool FromMessagesDone
       	{ get; set; }
	}

	public class MessageReceived
	{
		public MessageReceived (MessageDB message, Context c, bool readOnly = false)
		{
			MessageReceivedUtil.message = message;
			MessageReceivedUtil.context = c;
			MessageReceivedUtil.readOnly = readOnly;
			startActivity (c);
		}

		private void startActivity (Context c)
		{
			Intent i = new Intent (c, typeof(MessageReceivedActivity));
			i.PutExtra ("message", true);
			c.StartActivity (i);
		}
	}

	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]
	public partial class MessageReceivedActivity : Activity, IDisposable
	{
		private Context context;
		private LinearLayout listWrapper;
		private TextView header;
		private DBManager dbm;
		private ImageButton btnBack, btnAdd;
		private ImageView btns;
		private Dictionary<int, ContentPackItem> contentPackItems;
		private Dictionary<int, string> voiceFiles;
		private Dictionary<Guid, ContactDB> contacts;
		private Dictionary<int, PollingStep> pollSteps;
		private Dictionary<string, ConversationInfo> conversationItems;
		private bool checkForSentMessage, isConversation, markAsRead, createMultiple;
		private MessageManager mmg;
		private volatile bool fromLocalOnly;
		private string ContentPath, isMe;
		private ScrollView scroller;
		private ProgressDialog progress;
		public int cpUI, cUI;
		private List<Guid> getGuid;
		private int thumbImageWidth;
		private int thumbImageHeight;
		private List<byte[]> userImages;
		private Bitmap multipleContact;
		private Dialog ModalNewContact;
		private int toBeGrabbed;
		private List<Guid>newUsersToAdd;

		public Dictionary<Guid, MessageInfo> MessageItems {
			get;
			private set;
		}

		private UserDB UserFrom
		{ get; set; }
		
		private ConversationInfo conversationInfo
		{ get; set; }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			dbm = wowZapp.LaffOutOut.Singleton.dbm;
			mmg = wowZapp.LaffOutOut.Singleton.mmg;
			MessageItems = new Dictionary<Guid, MessageInfo> ();
			contentPackItems = new Dictionary<int, ContentPackItem> ();
			this.voiceFiles = new Dictionary<int, string> ();
			this.contacts = new Dictionary<Guid, ContactDB> ();
			this.pollSteps = new Dictionary<int, PollingStep> ();
			this.conversationItems = new Dictionary<string, ConversationInfo> ();
			ContentPath = wowZapp.LaffOutOut.Singleton.ContentDirectory;
			isMe = AndroidData.CurrentUser.FirstName + " " + AndroidData.CurrentUser.LastName;
			if (base.Intent.GetBooleanExtra ("message", false) == true) {
				SetContentView (Resource.Layout.previewSoundFX);
				context = MessageReceivedUtil.context;
				btns = FindViewById<ImageView> (Resource.Id.imgNewUserHeader);
				TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
				RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
				ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
				Header.headertext = Application.Context.Resources.GetString (Resource.String.messageViewingMessage);
				Header.fontsize = 36f;
#if DEBUG
				Console.WriteLine ("headertext = {0}, fontsize = {1}", Header.headertext, Header.fontsize);
				#endif
				ImageHelper.fontSizeInfo (header.Context);
				header.SetTextSize (Android.Util.ComplexUnitType.Pt, Header.fontsize);
				header.Text = Header.headertext;
				RunOnUiThread (delegate {
					progress = ProgressDialog.Show (context, Application.Context.Resources.GetString (Resource.String.messageViewingMessage),
												Application.Context.Resources.GetString (Resource.String.messageViewingPleaseWait));
				});
				processMessages (MessageReceivedUtil.message);
				PlayMessage (MessageReceivedUtil.message);
				markAsRead = true;
				enditall ();
			} else {
				SetContentView (Resource.Layout.MessageLists);
				btns = FindViewById<ImageView> (Resource.Id.imgNewUserHeader);
				TextView header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
				RelativeLayout relLayout = FindViewById<RelativeLayout> (Resource.Id.relativeLayout1);
				ImageHelper.setupTopPanel (btns, header, relLayout, header.Context);
				Header.headertext = Application.Context.Resources.GetString (Resource.String.messageListHeaderViewTitle);
				Header.fontsize = 36f;
#if DEBUG
				Console.WriteLine ("(else condition) headertext = {0}, fontsize = {1}", Header.headertext, Header.fontsize);
#endif
				ImageHelper.fontSizeInfo (header.Context);
				header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
				header.Text = Header.headertext;
				listWrapper = FindViewById<LinearLayout> (Resource.Id.linearListWrapper);
				context = listWrapper.Context;
				ViewGroup Parent = listWrapper;
				btnBack = FindViewById<ImageButton> (Resource.Id.btnBack);
				btnBack.Tag = 0;
				ImageButton btnHome = FindViewById<ImageButton> (Resource.Id.btnHome);
				btnHome.Tag = 1;
				btnAdd = FindViewById<ImageButton> (Resource.Id.btnAdd);
				btnAdd.Tag = 2;
				Messages.MessageReceivedUtil.FromMessagesDone = false;
				Messages.MessageReceivedUtil.FromMessages = true;
				
				LinearLayout bottom = FindViewById<LinearLayout> (Resource.Id.bottomHolder);
				ImageButton[] buttons = new ImageButton[3];
				buttons [0] = btnBack;
				buttons [1] = btnHome;
				buttons [2] = btnAdd;
				ImageHelper.setupButtonsPosition (buttons, bottom, context);
				
				scroller = FindViewById<ScrollView> (Resource.Id.scrollViewContainer);
				markAsRead = false;
				cUI = cpUI = 0;
				getGuid = new List<Guid> ();
				btnBack.Click += new EventHandler (btnBack_Click);
				btnHome.Click += delegate {
					wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
					Intent i = new Intent (this, typeof(Main.HomeActivity));
					i.SetFlags (ActivityFlags.ClearTop);
					StartActivity (i);
				};

				btnAdd.Click += new EventHandler (btnAdd_Click);
				createMultiple = false;
				isConversation = base.Intent.GetBooleanExtra ("list", false);
				if (MessageItems != null)
					MessageItems.Clear ();
				RunOnUiThread (delegate {
					progress = ProgressDialog.Show (context, Application.Context.Resources.GetString (Resource.String.messageRefreshingMessages),
												Application.Context.Resources.GetString (Resource.String.commonOneSec));
				});
				ThreadPool.QueueUserWorkItem (delegate {
					LoadContactsAndMessages (false);
				});
			}
			wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;
			this.thumbImageWidth = (int)ImageHelper.convertDpToPixel (56f, context);
			this.thumbImageHeight = (int)ImageHelper.convertDpToPixel (56f, context);
			//LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted += MessageManager_MessageSendConfirmCompleted;
		}

		private void AppDelegate_ReceivedMessages (object sender, IncomingMessageEventArgs e)
		{
			List <Messages.MessageInfo> messageItems = new List<Messages.MessageInfo> ();
			Guid me = AndroidData.CurrentUser.AccountID;
			
			RunOnUiThread (delegate {
				foreach (LOLMessageDelivery.Message eachMessage in e.Messages) {
					MessageDB msgDB = MessageDB.ConvertFromMessage (eachMessage);
					Messages.MessageInfo msgInfo = new Messages.MessageInfo (msgDB, msgDB.FromAccountID == me ? UserDB.ConvertFromUser (AndroidData.CurrentUser) :
					                                                         dbm.GetUserWithAccountID (msgDB.FromAccountGuid));
					
					if (msgInfo != null) {
						messageItems.Add (msgInfo);
					}
				}
				
				if (messageItems.Count > 0) {
					List<MessageDB> message = new List<MessageDB> ();
					foreach (Messages.MessageInfo eachMessageInfo in messageItems) {
						this.MessageItems [eachMessageInfo.Message.MessageID] = eachMessageInfo;
						message.Add (eachMessageInfo.Message);
					}
					dbm.InsertOrUpdateMessages (message);
					LoadContactsAndMessages (true);
				}
			});
		}

		private void AppDelegate_ReceivedConversationMessages (object sender, IncomingMessageEventArgs e)
		{
			foreach (LOLMessageDelivery.Message eachMessage in e.Messages) {
				MessageDB msgDB = MessageDB.ConvertFromMessage (eachMessage);
				MessageInfo msgInfo = new MessageInfo (msgDB, msgDB.FromAccountID == AndroidData.CurrentUser.AccountID ? UserDB.ConvertFromUser (AndroidData.CurrentUser) : 
														dbm.GetUserWithAccountID (msgDB.FromAccountGuid));
	
				if (conversationInfo.ConversationID.Equals (msgInfo.GetConversationID ()))
					conversationInfo.Messages [msgInfo.Message.MessageID] = msgInfo;
				commonConversation (conversationInfo.ConversationID);
			}
		}

		private void enditall ()
		{
			wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
			Finish ();
		}

		private void btnAdd_Click (object sender, EventArgs e)
		{
			wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
			Intent i = new Intent (this, typeof(Contacts.SelectContactsActivity));
			ComposeMessageMainUtil.returnTo = true;
			StartActivity (i);
		}

		private void btnAddBack_Click (object sender, EventArgs e)
		{
			btnAdd.Click += btnAdd_Click;
		}
		
		private void btnAddReply_Click (object sender, EventArgs e, List<UserDB> user)
		{
			Contacts.SelectContactsUtil.selectedUserContacts = user;
			wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
			Intent i = new Intent (this, typeof(ComposeMessageChooseContent));
			StartActivity (i);
		}

		private void btnBack_Click (object s, EventArgs e)
		{
			MessageItems.Clear ();
			wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
			if (MessageReceivedUtil.FromMessagesDone) {
				MessageReceivedUtil.FromMessagesDone = false;
				Intent i = new Intent (this, typeof(Main.HomeActivity));
				i.SetFlags (ActivityFlags.ClearTop);
				StartActivity (i);
			} else
				Finish ();
		}

		private void btnBackSide_Click (object s, EventArgs e)
		{
			btnBack.Click -= btnBackSide_Click;
			btnBack.Click += btnBack_Click;
			btnAdd.Click += btnAdd_Click;
			wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedConversationMessages;
			wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;
			isConversation = true;
			RunOnUiThread (delegate {
				GetRowsForMessages ();
				CreatePreviewUI ();
			});
		}
		
		private void processMessages (MessageDB message)
		{
			List<MessageInfo> messageItems = new List<MessageInfo> ();
			MessageInfo msgInfo = new MessageInfo (message, message.FromAccountID == AndroidData.CurrentUser.AccountID ?
													  UserDB.ConvertFromUser (AndroidData.CurrentUser) :
													  dbm.GetUserWithAccountID (message.FromAccountGuid));
			if (msgInfo != null)
				messageItems.Add (msgInfo);
			if (messageItems.Count > 0) {
				foreach (MessageInfo eachMessageInfo in messageItems) {
					this.MessageItems [eachMessageInfo.Message.MessageID] = eachMessageInfo;
				}
			}//end if
		}

		private void markMessageRead (ConversationInfo conversation)
		{
			string me = AndroidData.CurrentUser.AccountID.ToString ();
			Queue<MessageDB> msgQ = new Queue<MessageDB> ();
			foreach (MessageDB eachMessageDB in conversation.Messages.Values.Select (s=>s.Message)
														.Where(s=>s.MessageRecipientDBList.Count(t=>t.AccountGuid == me && !t.IsRead) >0 ))
				msgQ.Enqueue (eachMessageDB);
	
			if (msgQ.Count > 0) {
				LOLMessageClient service = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
				service.MessageMarkReadCompleted += Service_MessageMarkReadCompleted;
				service.MessageMarkReadAsync (msgQ.Peek ().MessageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, 
												new Guid (AndroidData.ServiceAuthToken), msgQ);
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

		private void LoadUserImage (UserDB user, ImageView profilePic)
		{
			RunOnUiThread (delegate {
				if (user.Picture.Length == 0)
					profilePic.SetImageResource (Resource.Drawable.defaultuserimage);
				else
					using (Bitmap img = ImageHelper.CreateUserProfileImageForDisplay(user.Picture, this.thumbImageWidth, this.thumbImageHeight, this.Resources)) {
						profilePic.SetImageBitmap (img);
					}//end using
			});
		}//end void LoadUserImage

		private void GetRowsForMessages ()
		{
			List<ConversationInfo> convInfo = ConversationInfo.DistributeToConversations (MessageItems.Values.ToList ());
			List<ConversationInfo> dupConvInfo = new List<ConversationInfo> ();
			dupConvInfo = convInfo.Where (x => x.Users.Count > 1).ToList ();

			dupConvInfo.Sort (delegate(ConversationInfo x, ConversationInfo y) {
				return y.GetLatestMessage ().Message.MessageSent.CompareTo (x.GetLatestMessage ().Message.MessageSent);
			});
			//FIXME: This is only a workaround

			foreach (ConversationInfo eachConversation in dupConvInfo) {
				this.conversationItems [eachConversation.ConversationID] = eachConversation;
			}
		}

		private byte[] getBufferFromPropertyFile (string filename)
		{
			string dataFile = System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ContentDirectory, filename);
			if (!File.Exists (dataFile)) {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("File {0} doesn't exist", dataFile);
#endif
				return new byte[0];
			}
			bool rv = false;
			byte[] dataBuffer = null;
			RunOnUiThread (delegate {
				dataBuffer = File.ReadAllBytes (dataFile);
				rv = dataBuffer == null ? true : false;
			});
			return rv == true ? new byte[0] : dataBuffer;
		}

		private List<ContentPackItemDB> getLocalContentPackItems (List<int> itemIds)
		{
			List<ContentPackItemDB> contentPackItems = dbm.GetContentPackItems (itemIds);
			List<ContentPackItemDB> toReturn = new List<ContentPackItemDB> ();
			foreach (ContentPackItemDB eachItem in contentPackItems) {
				if (checkContentPackItemDataExists (eachItem)) {
					eachItem.ContentPackItemIcon = getBufferFromPropertyFile (eachItem.ContentPackItemIconFile);
					eachItem.ContentPackData = getBufferFromPropertyFile (eachItem.ContentPackDataFile);
					toReturn.Add (eachItem);
				}
			}
			return toReturn;
		}

		private void createMultipleForContact (List<UserDB>contacts)
		{
			if (contacts == null) {
				multipleContact = BitmapFactory.DecodeResource (context.Resources, Resource.Drawable.defaultuserimage);
				return;
			}
			List<UserDB> myContacts = new List<UserDB> ();
			int e = 4;
			
			if (contacts.Count < 4)
				e = contacts.Count;
			for (int n = 0; n < e; ++n)
				myContacts.Add (contacts [n]);	
				
			if (userImages == null)
				userImages = new List<byte[]> ();
			else
				userImages.Clear ();
				
			foreach (UserDB user in myContacts) {
				if (user != null) {
					if (user.HasProfileImage || user.Picture.Length > 0)
						userImages.Add (user.Picture);
					else
						userImages.Add (new byte[0]);
				}
			}
			
			createImage (userImages);
		}
		
		private void createImage (List<byte[]> images)
		{
			Bitmap[] userImages = new Bitmap[4];
			Bitmap img = null;
			Bitmap blank = BitmapFactory.DecodeResource (context.Resources, Resource.Drawable.defaultuserimage);
			Bitmap scaledBlankSmall = null, scaledBlankLarge = null;
			int blankW = blank.Width;
			int blankH = blank.Height;
			scaledBlankSmall = Bitmap.CreateScaledBitmap (blank, blankW / 2, blankH / 2, false);
			scaledBlankLarge = Bitmap.CreateScaledBitmap (blank, blankW, blankH, false);
			if (images.Count < 2) {
				if (images.Count == 0) {
					multipleContact = blank;
					return;
				}
				if (images [0].Length != 0)
					multipleContact = ImageHelper.CreateUserProfileImageForDisplay (images [0], this.thumbImageWidth / 2, this.thumbImageHeight, this.Resources);
				else
					multipleContact = blank;
				return;
			}
			
			switch (images.Count) {
			case 2:
				img = ImageHelper.CreateUserProfileImageForDisplay (images [0], this.thumbImageWidth, this.thumbImageHeight / 2, this.Resources);
				userImages [0] = img == null ? blank : img;
				img = ImageHelper.CreateUserProfileImageForDisplay (images [1], this.thumbImageWidth, this.thumbImageHeight / 2, this.Resources);
				userImages [1] = img == null ? blank : img;
				break;
			case 3:
				img = ImageHelper.CreateUserProfileImageForDisplay (images [0], this.thumbImageWidth / 2, this.thumbImageHeight / 2, this.Resources);
				userImages [0] = img == null ? scaledBlankSmall : img;
				img = ImageHelper.CreateUserProfileImageForDisplay (images [2], this.thumbImageWidth / 2, this.thumbImageHeight, this.Resources);
				userImages [2] = img == null ? scaledBlankSmall : img;
				img = ImageHelper.CreateUserProfileImageForDisplay (images [1], this.thumbImageWidth / 2, this.thumbImageHeight / 2, this.Resources);
				userImages [1] = img == null ? scaledBlankLarge : img;
				break;
			case 4:
				img = ImageHelper.CreateUserProfileImageForDisplay (images [0], this.thumbImageWidth / 2, this.thumbImageHeight / 2, this.Resources);
				userImages [0] = img == null ? scaledBlankSmall : img;
				img = ImageHelper.CreateUserProfileImageForDisplay (images [1], this.thumbImageWidth / 2, this.thumbImageHeight / 2, this.Resources);
				userImages [1] = img == null ? scaledBlankSmall : img;
				img = ImageHelper.CreateUserProfileImageForDisplay (images [2], this.thumbImageWidth / 2, this.thumbImageHeight / 2, this.Resources);
				userImages [2] = img == null ? scaledBlankSmall : img;
				img = ImageHelper.CreateUserProfileImageForDisplay (images [3], this.thumbImageWidth / 2, this.thumbImageHeight / 2, this.Resources);
				userImages [3] = img == null ? scaledBlankSmall : img;
				break;
			}
			
			Bitmap cs = Bitmap.CreateBitmap (thumbImageWidth, thumbImageHeight, Bitmap.Config.Argb8888);
			Canvas comboImage = new Canvas (cs); 
			if (images.Count == 2) {
				comboImage.DrawBitmap (userImages [0], 0f, 0f, null);
				comboImage.DrawBitmap (userImages [1], 0f, (float)thumbImageHeight / 2, null);
			} else {
				if (images.Count == 3) {
					comboImage.DrawBitmap (userImages [0], 0f, 0f, null);
					comboImage.DrawBitmap (userImages [1], (float)thumbImageWidth / 2, 0f, null);
					comboImage.DrawBitmap (userImages [2], 0f, (float)thumbImageHeight / 2, null);
				} else {
					comboImage.DrawBitmap (userImages [0], 0f, 0f, null);
					comboImage.DrawBitmap (userImages [1], (float)thumbImageWidth / 2, 0f, null);
					comboImage.DrawBitmap (userImages [2], 0f, (float)thumbImageHeight / 2, null);
					comboImage.DrawBitmap (userImages [3], (float)thumbImageWidth / 2, (float)thumbImageHeight / 2, null);
				}
			}
			multipleContact = cs;
		}

		private string getVoiceRecordingFilename (Guid msgID, int stepNumber)
		{
			return System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ContentDirectory, string.Format (LOLConstants.VoiceRecordingFormat, msgID.ToString (), stepNumber));
		}

		private Dictionary<Guid, Dictionary<int, string>> getLocalVoiceFiles (List<Pair<Guid, List<int>>> voiceStepCriteria)
		{
			Dictionary<Guid, Dictionary<int, string>> toReturn = new Dictionary<Guid, Dictionary<int, string>> ();
			foreach (Pair<Guid, List<int>> eachItem in voiceStepCriteria) {
				Dictionary<int, string> eachMessageVoiceFiles = new Dictionary<int, string> ();
				foreach (int eachStep in eachItem.ItemB) {
					if (checkVoiceFileExists (eachItem.ItemA, eachStep))
						eachMessageVoiceFiles [eachStep] = getVoiceRecordingFilename (eachItem.ItemA, eachStep);
					else
						continue;

					if (eachMessageVoiceFiles.Count > 0)
						toReturn [eachItem.ItemA] = eachMessageVoiceFiles;
				}
			}
			return toReturn;
		}

		private bool checkContentPackItemDataExists (ContentPackItemDB item)
		{
			if (item != null) {
				return File.Exists (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ContentDirectory, item.ContentPackDataFile));
			} else
				return false;
		}

		private bool checkVoiceFileExists (Guid msgID, int step)
		{
			return File.Exists (getVoiceRecordingFilename (msgID, step));
		}

		private bool checkPhotoPollStepFileExists (PollingStepDB pollStep)
		{
			string contentpath = wowZapp.LaffOutOut.Singleton.ContentDirectory;
			if (pollStep != null) {
				bool exist = true;
				if (!string.IsNullOrEmpty (pollStep.PollingData1File))
					exist &= File.Exists (System.IO.Path.Combine (contentpath, pollStep.PollingData1File));
				if (!string.IsNullOrEmpty (pollStep.PollingData2File))
					exist &= File.Exists (System.IO.Path.Combine (contentpath, pollStep.PollingData2File));
				if (!string.IsNullOrEmpty (pollStep.PollingData3File))
					exist &= File.Exists (System.IO.Path.Combine (contentpath, pollStep.PollingData3File));
				if (!string.IsNullOrEmpty (pollStep.PollingData4File))
					exist &= File.Exists (System.IO.Path.Combine (contentpath, pollStep.PollingData4File));
				return exist;
			} else
				return false;
		}

		private List<PollingStepDB> getLocalPollingStepsForMessage (string messageGuid)
		{
			List<PollingStepDB> pollingSteps = dbm.GetPollingSteps (new List<string> () { messageGuid });
			List<PollingStepDB> toReturn = new List<PollingStepDB> ();
			foreach (PollingStepDB eachItem in pollingSteps) {
				if (string.IsNullOrEmpty (eachItem.PollingAnswer1)) {
					if (checkPhotoPollStepFileExists (eachItem)) {
						if (!string.IsNullOrEmpty (eachItem.PollingData1File))
							eachItem.PollingData1 = getBufferFromPropertyFile (eachItem.PollingData1File);
						if (!string.IsNullOrEmpty (eachItem.PollingData2File))
							eachItem.PollingData2 = getBufferFromPropertyFile (eachItem.PollingData2File);
						if (!string.IsNullOrEmpty (eachItem.PollingData3File))
							eachItem.PollingData3 = getBufferFromPropertyFile (eachItem.PollingData3File);
						if (!string.IsNullOrEmpty (eachItem.PollingData4File))
							eachItem.PollingData4 = getBufferFromPropertyFile (eachItem.PollingData4File);
					} else
						continue;
				}
			}
			return toReturn;
		}

		private void PlayMessage (MessageDB message)
		{
			voiceFiles.Clear ();
			contentPackItems.Clear ();
			pollSteps.Clear ();

			MessageInfo messageInfo = this.MessageItems [message.MessageID];

			ContentState dlState = new ContentState (message);
			contentPackItems = getLocalContentPackItems (dlState.ContentPackIDQ.ToList ())
				.ToDictionary (s => s.ContentPackItemID, s => ContentPackItemDB.ConvertFromContentPackItemDB (s));

			if (messageInfo.HasContentInfo) {
				RunOnUiThread (delegate {
					if (progress != null)
						progress.Dismiss ();
					this.PlayUnsentMessage (messageInfo.ContentInfo);
				});
			} else {
				Dictionary<Guid, Dictionary<int, string>> localVoiceFiles =
					getLocalVoiceFiles (new List<Pair<Guid, List<int>>> () { new Pair<Guid, List<int>>(dlState.Message.MessageID, dlState.VoiceIDQ.ToList()) });

				if (!localVoiceFiles.TryGetValue (dlState.Message.MessageID, out voiceFiles))
					voiceFiles = new Dictionary<int, string> ();

				pollSteps = getLocalPollingStepsForMessage (dlState.Message.MessageGuid)
					.ToDictionary (s => s.StepNumber, s => PollingStepDB.ConvertFromPollingStepDB (s));

				dlState.RemoveExistingItems (contentPackItems.Keys.ToList (), voiceFiles.Keys.ToList (), pollSteps.Keys.ToList ());
				if (dlState.HasContentForDownload) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("dlState has content for download");
#endif
					if (dlState.HasContentPackItems) {
#if DEBUG
						System.Diagnostics.Debug.WriteLine ("dlState has contentpackitems for download");
#endif
						LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
						service.ContentPackGetItemCompleted += Service_ContentPackGetItemCompleted;
						service.ContentPackGetItemAsync (dlState.ContentPackIDQ.Peek (), ContentPackItem.ItemSize.Small, AndroidData.CurrentUser.AccountID,
                            new Guid (AndroidData.ServiceAuthToken), dlState);
					} else
                        if (dlState.HasVoiceRecordings) {
#if DEBUG
						System.Diagnostics.Debug.WriteLine ("dlState has voicerecordings for download");
#endif
						LOLMessageClient service = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
						service.MessageGetStepDataCompleted += Service_MessageGetStepData;
						service.MessageGetStepDataAsync (dlState.Message.MessageID, dlState.VoiceIDQ.Peek (), new Guid (AndroidData.ServiceAuthToken), dlState);
					} else
                            if (dlState.HasPollingSteps) {
						RunOnUiThread (delegate {
#if DEBUG
							System.Diagnostics.Debug.WriteLine ("dlState has pollingsteps for download");
#endif
							LOLMessageClient service = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
							service.PollingStepGetCompleted += Service_PollingStepGetCompleted;
							service.PollingStepGetAsync (dlState.Message.MessageID, dlState.PollingIDQ.Peek (), AndroidData.CurrentUser.AccountID,
                                        new Guid (AndroidData.ServiceAuthToken), dlState);
						});
					}
				} else
					RunOnUiThread (delegate {
						StartPlayMessage (message);
					});
			}
		}

		private void StartPlayMessage (MessageDB message)
		{
			bool hasRespondedPollSteps = this.pollSteps.Count (s => s.Value.HasResponded) > 0;

			if (hasRespondedPollSteps) {
				RunOnUiThread (() => Toast.MakeText (context, Resource.String.pollGettingResults, ToastLength.Short).Show ());
				LOLMessageClient service = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
				service.PollingStepGetResultsListCompleted += Service_PollingStepGetResultsListCompleted;
				service.PollingStepGetResultsListAsync (message.MessageID, new Guid (AndroidData.ServiceAuthToken), message);
			} else {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("about to play the message");
#endif
				RunOnUiThread (delegate {
					if (progress != null)
						RunOnUiThread (() => progress.Dismiss ());
					List<UserDB> recipients = new List<UserDB> ();
					UserDB tmpUsr = null;
                    
					for (int m = 0; m < message.MessageRecipientDBList.Count; ++m) {
						tmpUsr = dbm.GetUserWithAccountID (message.MessageRecipientDBList [m].AccountGuid);
						if (tmpUsr != null)
							recipients.Add (tmpUsr);
					}

					tmpUsr = dbm.GetUserWithAccountID (message.FromAccountGuid);
					if (tmpUsr != null)
						recipients.Add (tmpUsr);
					MessagePlaybackController playbackController =
						new MessagePlaybackController (message.MessageStepDBList,
													  this.contentPackItems, this.voiceFiles, this.pollSteps, new Dictionary<int, LOLMessageSurveyResult> (), markAsRead, recipients, context);
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("we outa here");
#endif
				});
			}//end if else
		}//end void PlayMessage

		private void PlayUnsentMessage (ContentInfo contentInfo)
		{
			// TODO : Somesort of activity
			MessagePlaybackController playbackController = new MessagePlaybackController (contentInfo, this.contentPackItems, context);
		}//end void PlayUnsentMessage

		private void Service_PollingStepGetResultsListCompleted (object sender, PollingStepGetResultsListCompletedEventArgs e)
		{
			LOLMessageClient service = (LOLMessageClient)sender;
			service.PollingStepGetResultsListCompleted -= Service_PollingStepGetResultsListCompleted;

			if (null == e.Error) {
				LOLMessageSurveyResult[] results = e.Result.ToArray ();
				Dictionary<int, LOLMessageSurveyResult> pollResults = new Dictionary<int, LOLMessageSurveyResult> ();

				foreach (LOLMessageSurveyResult eachPollResult in results)
					pollResults [eachPollResult.StepNumber] = eachPollResult;

				MessageDB forMessage = (MessageDB)e.UserState;
				List<UserDB> recipients = new List<UserDB> ();
				for (int m = 0; m < forMessage.MessageRecipientDBList.Count; ++m)
					recipients.Add (dbm.GetUserWithAccountID (forMessage.MessageRecipientDBList [m].AccountGuid));
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("polling step results obtained - launching playback");
#endif
				RunOnUiThread (delegate {
					if (progress != null)
						RunOnUiThread (() => progress.Dismiss ());
					MessagePlaybackController playbackController = new MessagePlaybackController (forMessage.MessageStepDBList, 
                        this.contentPackItems, this.voiceFiles, this.pollSteps, pollResults, false, recipients, context);
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("and we're back in the room");
#endif
				});
			} else {
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Exception downloading polling results! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
			}//end if else
		}

		private void LoadContactsAndMessages (bool fromLocalOnly)
		{
			this.MessageItems.Clear ();
			conversationItems.Clear ();
			contacts.Clear ();
			this.contacts = dbm.GetAllContactsForOwner (AndroidData.CurrentUser.AccountID.ToString ()).ToDictionary (s => s.ContactAccountID, s => s);
			List<MessageDB> message = new List<MessageDB> ();
			message = dbm.GetAllMessagesForOwner (AndroidData.CurrentUser.AccountID.ToString ());
			List<MessageDB> messages = new List<MessageDB> ();
			List<UserDB> users = new List<UserDB> ();

			foreach (MessageDB eachMessageDB in message) {
				ContactDB msgContact = null;
				UserDB contactUser = null;
				MessageInfo msgInfoItem = null;

				if (eachMessageDB.FromAccountID != AndroidData.CurrentUser.AccountID) {
					if (this.contacts.TryGetValue (eachMessageDB.FromAccountID, out msgContact))
						contactUser = UserDB.ConvertFromUser (msgContact.ContactUser);
					else
						contactUser = dbm.GetUserWithAccountID (eachMessageDB.FromAccountGuid);

					msgInfoItem = new MessageInfo (eachMessageDB, contactUser);
				} else {
					contactUser = UserDB.ConvertFromUser (AndroidData.CurrentUser);
					msgInfoItem = new MessageInfo (eachMessageDB, contactUser);

					if (!eachMessageDB.MessageConfirmed) {
						ContentInfo contentInfo = dbm.GetContentInfoByMessageDBID (eachMessageDB.ID);
						msgInfoItem.ContentInfo = contentInfo;
					}
				}
				messages.Add (eachMessageDB);
				users.Add (contactUser);
				this.MessageItems [eachMessageDB.MessageID] = msgInfoItem;
				/*if (isConversation == false) {
					RunOnUiThread (delegate {
						createUI (messages, users);
					});
				}*/
			}

			if (fromLocalOnly) {
				RunOnUiThread (delegate {
					if (progress != null)
						progress.Dismiss ();
					GetRowsForMessages ();
					CreatePreviewUI ();
				});
			} else {
				List<Guid> excludeMessageGuids = new List<Guid> ();
				excludeMessageGuids = this.MessageItems.Values.Where (s => s.Message.MessageStepDBList.Count (t => t.StepID == default(Guid)) == 0)
					.Select (s => s.Message.MessageID).ToList ();

				LOLMessageClient service = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("**Will call MessageGetConversations with params: account id {0}, start date: new DateTime(1900, 1, 1), end date: new DateTime(2099, 1, 1), service auth token: {1}",
								  AndroidData.CurrentUser.AccountID, new Guid (AndroidData.ServiceAuthToken));
#endif
#if DEBUG
				if (null == excludeMessageGuids) {
				
					System.Diagnostics.Debug.WriteLine ("**Exclude message ids is NULL!");

				} else {
					System.Diagnostics.Debug.WriteLine ("**Exclude message ids count: {0}", excludeMessageGuids.Count);

					foreach (Guid eachMessageID in excludeMessageGuids) {
						System.Diagnostics.Debug.WriteLine ("**Each message id to exclude: {0}", eachMessageID);
					}//end foreach
				}//end if
				#endif

				service.MessageGetConversationsCompleted += Service_MessageGetConversationsCompleted;
				service.MessageGetConversationsAsync (AndroidData.CurrentUser.AccountID,
													 new DateTime (1900, 1, 1),
													 new DateTime (2099, 1, 1), 50,
													 excludeMessageGuids,
													 new Guid (AndroidData.ServiceAuthToken));
			}//end if else
		}//end void LoadContactsAndMessagesEx

		private void SaveContentPackItem (ContentPackItemDB pack)
		{
			pack.ContentPackDataFile = StringUtils.ConstructContentPackItemDataFile (pack.ContentPackItemID);
			pack.ContentPackItemIconFile = StringUtils.ConstructContentPackItemIconFilename (pack.ContentPackItemID);
			SaveContentPackItemBuffers (pack);
		}

		private void SaveContentPackItemBuffers (ContentPackItemDB contentPackItem)
		{
			if (contentPackItem.ContentPackItemIcon != null && contentPackItem.ContentPackItemIcon.Length > 0) {

				string iconFile = System.IO.Path.Combine (ContentPath, contentPackItem.ContentPackItemIconFile);

				if (File.Exists (iconFile))
					File.Delete (iconFile);

				byte[] iconData = contentPackItem.ContentPackItemIcon;
				try {
					File.WriteAllBytes (iconFile, iconData);
				} catch (IOException) {
#if(DEBUG)
					System.Diagnostics.Debug.WriteLine ("Error saving content pack item data file!");
#endif
				}
			}

			if (contentPackItem.ContentPackData != null && contentPackItem.ContentPackData.Length > 0) {
				string dataFile = System.IO.Path.Combine (this.ContentPath, contentPackItem.ContentPackDataFile);

				if (File.Exists (dataFile))
					File.Delete (dataFile);
				byte[] data = contentPackItem.ContentPackData;
				try {
					File.WriteAllBytes (dataFile, data);
				} catch (IOException) {
#if(DEBUG)
					System.Diagnostics.Debug.WriteLine ("Error saving content pack item data file!");
#endif
				}
			}//end if
		}

		private string SaveVoiceRecordingFile (byte[] voiceBuffer, Guid msgID, int stepNumber)
		{
			string dataFile = System.IO.Path.Combine (ContentPath, string.Format (LOLConstants.VoiceRecordingFormat, msgID.ToString (), stepNumber));
			try {
				File.WriteAllBytes (dataFile, voiceBuffer);
			} catch (IOException e) {
				#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Error saving voice message");
				#endif
			}
			return dataFile;
		}

		private void SavePhotoPollBuffers (PollingStepDB pollStepToSave)
		{
			if (null != pollStepToSave.PollingData1 && pollStepToSave.PollingData1.Length > 0)
				pollStepToSave.PollingData1File = StringUtils.ConstructPollingStepDataFile (1, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);

			if (null != pollStepToSave.PollingData2 && pollStepToSave.PollingData2.Length > 0)
				pollStepToSave.PollingData2File = StringUtils.ConstructPollingStepDataFile (2, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);

			if (null != pollStepToSave.PollingData3 && pollStepToSave.PollingData3.Length > 0)
				pollStepToSave.PollingData3File = StringUtils.ConstructPollingStepDataFile (3, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);

			if (null != pollStepToSave.PollingData4 && pollStepToSave.PollingData4.Length > 0)
				pollStepToSave.PollingData4File = StringUtils.ConstructPollingStepDataFile (4, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);

			SavePollingStepDataBuffers (pollStepToSave);
		}

		private void SavePollingStepDataBuffers (PollingStepDB pollingStep)
		{
			RunOnUiThread (delegate {
				for (int i = 1; i <= 4; i++) {
					string dataFile = string.Empty;
					byte[] buffer = null;

					switch (i) {
					case 1:
						if (!string.IsNullOrEmpty (pollingStep.PollingData1File)) {
							dataFile = System.IO.Path.Combine (ContentPath, pollingStep.PollingData1File);
							buffer = pollingStep.PollingData1;
						}
						break;
					case 2:
						if (!string.IsNullOrEmpty (pollingStep.PollingData2File)) {
							dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData2File);
							buffer = pollingStep.PollingData2;
						}
						break;
					case 3:
						if (!string.IsNullOrEmpty (pollingStep.PollingData3File)) {
							dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData3File);
							buffer = pollingStep.PollingData3;
						}
						break;
					case 4:
						if (!string.IsNullOrEmpty (pollingStep.PollingData4File)) {
							dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData4File);
							buffer = pollingStep.PollingData4;
						}
						break;
					}//end switch

					if (null != buffer && buffer.Length > 0) {
						try {
							File.WriteAllBytes (dataFile, buffer);
						} catch (IOException e) {
							#if DEBUG
							System.Diagnostics.Debug.WriteLine ("Unable to save polling step");
							#endif
						}
					}
				}
			});
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			GC.Collect ();
			LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted -= MessageManager_MessageSendConfirmCompleted;
		}
	}
}
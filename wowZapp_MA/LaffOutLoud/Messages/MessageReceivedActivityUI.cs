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
	public partial class MessageReceivedActivity
	{
		private Dialog contactsDialog;
	
		private void CreatePreviewUI ()
		{
			int m = 0;
			isConversation = false;
			header = FindViewById<TextView> (Resource.Id.txtFirstScreenHeader);
			Header.headertext = Application.Context.Resources.GetString (Resource.String.messageListHeaderViewTitle);
			Header.fontsize = 36f;
			ImageHelper.fontSizeInfo (header.Context);
			header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
			header.Text = Header.headertext;
			listWrapper.RemoveAllViewsInLayout ();
			
			Dictionary<string, MessageInfo> latestMessages = new Dictionary<string, MessageInfo> ();
			Dictionary<string, string> unreadMessageCounts = new Dictionary<string, string> ();
			string ownerAccountId = AndroidData.CurrentUser.AccountID.ToString ();
			foreach (ConversationInfo eachConversationInfo in this.conversationItems.Values) {
				MessageInfo latestMessage = eachConversationInfo.GetLatestMessage ();
				string messageCountStr = eachConversationInfo.Messages
					.Count (s => s.Value.Message.MessageRecipientDBList.Count (t => t.AccountGuid == ownerAccountId && !t.IsRead) > 0)
						.ToString ();
				latestMessages.Add (eachConversationInfo.ConversationID, latestMessage);
				unreadMessageCounts.Add (eachConversationInfo.ConversationID, messageCountStr);
			}
			
			if (getGuid != null)
				getGuid.Clear ();
			RunOnUiThread (delegate {
				foreach (KeyValuePair<string, MessageInfo> eachMessage in latestMessages) {
					eachMessage.Value.Message.MessageRecipientDBList = eachMessage.Value.Message.MessageRecipientDBList.Where (x => x != null).ToList ();
					
					string messager = string.Empty;
					string tmpName = string.Empty;
					ImageView random = null;
					LinearLayout.LayoutParams randomParams = null;
					LinearLayout layout = new LinearLayout (context);
					LinearLayout.LayoutParams layoutparams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
					layoutparams.SetMargins (0, 0, 0, (int)ImageHelper.convertDpToPixel (10f, context));
					layout.LayoutParameters = layoutparams;
					layout.Orientation = Android.Widget.Orientation.Horizontal;
					layout.SetGravity (GravityFlags.Center);
					layout.SetPadding ((int)ImageHelper.convertDpToPixel (5f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 
										(int)ImageHelper.convertDpToPixel (10f, context));
					
					ImageView profilepic = new ImageView (context);
					profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (55f, context), (int)ImageHelper.convertDpToPixel (100f, context));
					
					if (eachMessage.Key != null) {
						UserDB who = null;
						if (eachMessage.Value.MessageUser != null)
							who = eachMessage.Value.MessageUser;
						else
							who = null;
						
						if (who.AccountGuid == AndroidData.CurrentUser.AccountID.ToString ())
							who = dbm.GetUserWithAccountID (eachMessage.Value.Message.MessageRecipientDBList [0].AccountGuid);
						
						if (who == null) {
							#if DEBUG
							System.Diagnostics.Debug.WriteLine ("UserDB = null for {0}, bugging out", latestMessages);
							#endif
							m++;
						} else {
							profilepic.Tag = new Java.Lang.String ("profilepic_" + who.AccountID);
							profilepic.SetImageResource (Resource.Drawable.defaultuserimage);
							layout.AddView (profilepic);
							if (eachMessage.Value.Message.MessageRecipientDBList.Count > 1) {
								List<UserDB> imageList = new List<UserDB> ();
								foreach (MessageRecipientDB human in eachMessage.Value.Message.MessageRecipientDBList) {
									imageList.Add (dbm.GetUserWithAccountID (human.AccountGuid));
								}
								RunOnUiThread (delegate {
									createMultipleForContact (imageList);
									if (multipleContact != null)
										profilepic.SetImageBitmap (multipleContact);
									else
										profilepic.SetImageResource (Resource.Drawable.defaultuserimage);
								});
								
							} else {
								
								if (who.Picture.Length == 0 && who.HasProfileImage)
									getGuid.Add (who.AccountID);
								else {
									if (who.Picture.Length > 0)
										this.LoadUserImage (who, profilepic);
									else
										profilepic.SetImageResource (Resource.Drawable.defaultuserimage);
								}
							}
							LinearLayout layout2 = new LinearLayout (context);
							layout2.Orientation = Orientation.Vertical;
							layout2.SetGravity (GravityFlags.Left);
							
							using (TextView name = new TextView (context)) {
								name.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (240f, context), (int)ImageHelper.convertDpToPixel (25f, context));
								name.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), 0, (int)ImageHelper.convertDpToPixel (20f, context), 0);
								name.Gravity = GravityFlags.Left;
								name.SetTextColor (Color.White);
								name.TextSize = 16f;
								tmpName = who.FirstName + " " + who.LastName;
								if (eachMessage.Value.Message.MessageRecipientDBList.Count > 1) {
									if (eachMessage.Value.Message.MessageRecipientDBList.Count == 1 || eachMessage.Value.Message.MessageRecipientDBList.Count - 1 == 1)
										tmpName += "   1 other";
									else
										tmpName += "   " + string.Format ("{0} others", eachMessage.Value.Message.MessageRecipientDBList.Count - 1);
								}
							
								name.Text = tmpName;
								layout2.AddView (name);
							}
							
							if (eachMessage.Value.Message.MessageStepDBList.Count == 1 && eachMessage.Value.Message.MessageStepDBList [0].StepType == MessageStep.StepTypes.Text) {
								using (TextView txtMessage = new TextView (context)) {
									using (LinearLayout.LayoutParams txtMessageParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (220f, context), 
																												 (int)ImageHelper.convertDpToPixel (60f, context))) {
										txtMessageParams.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), 0, 0, 0);
										txtMessage.LayoutParameters = txtMessageParams;
									}
									txtMessage.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), (int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (20f, context), (int)ImageHelper.convertDpToPixel (10f, context));
									txtMessage.Gravity = GravityFlags.CenterVertical;
								
									if (eachMessage.Value.Message.FromAccountID == AndroidData.CurrentUser.AccountID)
										txtMessage.SetBackgroundResource (Resource.Drawable.bubblesolidright);
									else
										txtMessage.SetBackgroundResource (Resource.Drawable.bubblesolidleft);
								
									txtMessage.TextSize = 16f;
								
									for (int e = 0; e < eachMessage.Value.Message.MessageStepDBList.Count; ++e) {
										if (!string.IsNullOrEmpty (eachMessage.Value.Message.MessageStepDBList [e].MessageText)) {
											messager = eachMessage.Value.Message.MessageStepDBList [e].MessageText;
											break;
										}
									}
								
									if (string.IsNullOrEmpty (messager))
										messager = "";
								
									int nolines = (int)(ImageHelper.convertDpToPixel ((messager.Length / 27f) * 12f, context));
									txtMessage.SetHeight (nolines);
								
									txtMessage.SetTextColor (Android.Graphics.Color.Black);
									txtMessage.Text = messager;
									txtMessage.ContentDescription = eachMessage.Key;
									txtMessage.Click += ConversationItem_Clicked;
									layout2.AddView (txtMessage);
								}
							} else {
								int text = 0;
								for (int tt = 0; tt < eachMessage.Value.Message.MessageStepDBList.Count; ++tt) {
									if (eachMessage.Value.Message.MessageStepDBList [tt].StepType == MessageStep.StepTypes.Text)
										text++;
								}
								
								LinearLayout layout3 = new LinearLayout (context);
								LinearLayout.LayoutParams layout3params = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
								layout3params.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (5f, context), 0, (int)ImageHelper.convertDpToPixel (5f, context));
								layout3.LayoutParameters = layout3params;
								layout3.Orientation = Orientation.Horizontal;
								layout3.SetGravity (GravityFlags.Left);
								layout3.ContentDescription = eachMessage.Key;
								layout3.Click += ConversationLayItem_Clicked;
								layout3.SetBackgroundResource (Resource.Drawable.attachmentspreviewbkgr);
								layout3.SetVerticalGravity (GravityFlags.CenterVertical);
								layout3.SetMinimumHeight (30);
								layout3.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
								
								int end = eachMessage.Value.Message.MessageStepDBList.Count > 3 ? 3 : eachMessage.Value.Message.MessageStepDBList.Count;
								for (int i = 0; i < end; ++i) {
									switch (eachMessage.Value.Message.MessageStepDBList [i].StepType) {
									case LOLMessageDelivery.MessageStep.StepTypes.Text:
										if (text == 1) {
											using (TextView txtMessage = new TextView (context)) {
												using (LinearLayout.LayoutParams txtMessageParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (220f, context), 
																														(int)ImageHelper.convertDpToPixel (60f, context))) {
													txtMessageParams.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), 0, 0, 0);
													txtMessage.LayoutParameters = txtMessageParams;
												}
												txtMessage.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), (int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (20f, context), (int)ImageHelper.convertDpToPixel (10f, context));
												txtMessage.Gravity = GravityFlags.CenterVertical;
											
												if (eachMessage.Value.Message.FromAccountID == AndroidData.CurrentUser.AccountID)
													txtMessage.SetBackgroundResource (Resource.Drawable.bubblesolidright);
												else
													txtMessage.SetBackgroundResource (Resource.Drawable.bubblesolidleft);
											
												txtMessage.TextSize = 16f;
											
												for (int e = 0; e < eachMessage.Value.Message.MessageStepDBList.Count; ++e) {
													if (!string.IsNullOrEmpty (eachMessage.Value.Message.MessageStepDBList [e].MessageText)) {
														messager = eachMessage.Value.Message.MessageStepDBList [e].MessageText;
														break;
													}
												}
											
												if (string.IsNullOrEmpty (messager))
													messager = "No text message found";
											
												int nolines = (int)(ImageHelper.convertDpToPixel ((messager.Length / 27f) * 12f, context));
												txtMessage.SetHeight (nolines);
											
												txtMessage.SetTextColor (Android.Graphics.Color.Black);
												txtMessage.Text = messager;
												txtMessage.ContentDescription = eachMessage.Key;
												txtMessage.Click += ConversationItem_Clicked;
											
												layout2.AddView (txtMessage);
											}
										} else {
											if (text > 1) {
												using (random = new ImageView (context)) {
													using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
														randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
														random.LayoutParameters = randomParams;
													}
													random.ContentDescription = eachMessage.Key;
													random.Click += ConversationPicItem_Clicked;
													random.SetBackgroundResource (Resource.Drawable.icotext);
													layout3.AddView (random);
												}
											}
										}
										break;
									case LOLMessageDelivery.MessageStep.StepTypes.Animation:
										using (random = new ImageView (context)) {
											using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
												randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
												random.LayoutParameters = randomParams;
											}
											random.ContentDescription = eachMessage.Key;
											random.Click += ConversationPicItem_Clicked;
											random.SetBackgroundResource (Resource.Drawable.icoanimation);
											layout3.AddView (random);
										}
										break;
									case LOLMessageDelivery.MessageStep.StepTypes.Comicon:
										using (random = new ImageView (context)) {
											using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
												randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
												random.LayoutParameters = randomParams;
											}
											random.ContentDescription = eachMessage.Key;
											random.Click += ConversationPicItem_Clicked;
											random.SetBackgroundResource (Resource.Drawable.icocomicons);
											layout3.AddView (random);
										}
										break;
									case LOLMessageDelivery.MessageStep.StepTypes.Comix:
										using (random = new ImageView (context)) {
											using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
												randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
												random.LayoutParameters = randomParams;
											}
											random.ContentDescription = eachMessage.Key;
											random.Click += ConversationPicItem_Clicked;
											random.SetBackgroundResource (Resource.Drawable.icocomix);
											layout3.AddView (random);
										}
										break;
									case LOLMessageDelivery.MessageStep.StepTypes.Emoticon:
										using (random = new ImageView (context)) {
											using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
												randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
												random.LayoutParameters = randomParams;
											}
											random.ContentDescription = eachMessage.Key;
											random.Click += ConversationPicItem_Clicked;
											random.SetBackgroundResource (Resource.Drawable.icoemoticons);
											layout3.AddView (random);
										}
										break;
									case LOLMessageDelivery.MessageStep.StepTypes.Polling:
										using (random = new ImageView (context)) {
											using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
												randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
												random.LayoutParameters = randomParams;
											}
											random.ContentDescription = eachMessage.Key;
											random.Click += ConversationPicItem_Clicked;
											random.SetBackgroundResource (Resource.Drawable.icopolls);
											layout3.AddView (random);
										}
										break;
									case LOLMessageDelivery.MessageStep.StepTypes.SoundFX:
										using (random = new ImageView (context)) {
											using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
												randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
												random.LayoutParameters = randomParams;
											}
											random.ContentDescription = eachMessage.Key;
											random.Click += ConversationPicItem_Clicked;
											random.SetBackgroundResource (Resource.Drawable.icosoundfx);
											layout3.AddView (random);
										}
										break;
									case LOLMessageDelivery.MessageStep.StepTypes.Video:
										using (random = new ImageView (context)) {
											using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
												randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
												random.LayoutParameters = randomParams;
											}
											random.ContentDescription = eachMessage.Key;
											random.Click += ConversationPicItem_Clicked;
											random.SetBackgroundResource (Resource.Drawable.icovideo);
											layout3.AddView (random);
										}
										break;
									case LOLMessageDelivery.MessageStep.StepTypes.Voice:
										using (random = new ImageView (context)) {
											using (randomParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
												randomParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
												random.LayoutParameters = randomParams;
											}
											random.ContentDescription = eachMessage.Key;
											random.Click += ConversationPicItem_Clicked;
											random.SetBackgroundResource (Resource.Drawable.icovoice);
											layout3.AddView (random);
										}
										break;
									}
								}
								
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.playblack);
									random.ContentDescription = eachMessage.Value.Message.MessageGuid;
									random.Click += PlayButton_Clicked;
									layout3.AddView (random);
								}
								layout2.AddView (layout3);
							}
							
							LinearLayout layout4 = new LinearLayout (context);
							layout4.Orientation = Orientation.Vertical;
							layout4.SetGravity (GravityFlags.Right);
							
							using (TextView noMessages = new TextView (context)) {
								noMessages.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (240f, context), (int)ImageHelper.convertDpToPixel (20f, context));
								noMessages.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
								noMessages.SetTextColor (Color.White);
								noMessages.TextSize = 12f;
								noMessages.Text = unreadMessageCounts [eachMessage.Key] + " messages unread";
								noMessages.Gravity = GravityFlags.Right;
							
								layout4.AddView (noMessages);
							}
							layout2.AddView (layout4);
							
							layout.AddView (layout2);
							
							listWrapper.AddView (layout);
							m++;
						}
					}
				}
			});
			if (getGuid.Count > 0) {
				cpUI = 0;
				LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
				service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
				service.UserGetImageDataAsync (AndroidData.CurrentUser.AccountID, getGuid [0], new Guid (AndroidData.ServiceAuthToken));
			}
		}
		
		void PlayButton_Clicked (object sender, EventArgs e)
		{
			ImageView senderObj = (ImageView)sender;
			MessageInfo msgInfo = null;
			MessageReceivedUtil.readOnly = false;
			if (this.MessageItems.TryGetValue (new Guid (senderObj.ContentDescription), out msgInfo)) {
				this.RunOnUiThread (delegate {
					progress = ProgressDialog.Show (context, Application.Context.Resources.GetString (Resource.String.messageViewingMessage),
					                                Application.Context.Resources.GetString (Resource.String.messageViewingPleaseWait));
					PlayMessage (msgInfo.Message);
				});
			}//end if
		}
		
		private void ConversationPicItem_Clicked (object sender, EventArgs e)
		{
			ImageView senderObj = (ImageView)sender;
			string conversationID = senderObj.ContentDescription;
			commonConversation (conversationID);
		}
		
		private void ConversationLayItem_Clicked (object sender, EventArgs e)
		{
			LinearLayout senderObj = (LinearLayout)sender;
			string conversationID = senderObj.ContentDescription;
			commonConversation (conversationID);
		}
		
		private void commonConversation (string conversationID)
		{
			ConversationInfo conversation = null;
			
			List<MessageDB> messages = new List<MessageDB> ();
			List<UserDB> messageUsers = new List<UserDB> ();
			newUsersToAdd = new List<Guid> ();
			wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
			wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedConversationMessages;
			if (this.conversationItems.TryGetValue (conversationID, out conversation)) {
				RunOnUiThread (delegate {
					conversationInfo = conversation;
					UserDB user = null;
					UserDB senderOfOldestMessage = conversationInfo.GetSenderOfOldestMessage ();
					if (senderOfOldestMessage == null)
						senderOfOldestMessage = conversationInfo.GetAllConversationParticipants () [0];
					messages = conversationInfo.Messages.Values.Select (s => s.Message).ToList ();
					foreach (Guid guid in conversationInfo.Users.Where(s => s != AndroidData.CurrentUser.AccountID)) {
						user = dbm.GetUserWithAccountID (guid.ToString ());
						if (user != null)
							messageUsers.Add (user);
						else
							if (guid != AndroidData.CurrentUser.AccountID)
							newUsersToAdd.Add (guid);
					}
					btnBack.Click -= btnBack_Click;
					btnBack.Click += btnBackSide_Click;
					
					btnAdd.Click -= btnAdd_Click;
					btnAdd.Click += (object s, EventArgs ea) => {
						btnAddReply_Click (s, ea, messageUsers); };
					
					if (newUsersToAdd.Count != 0)
						AddNewUsers ();
					
					markMessageRead (conversationInfo);
					createUI (messages, messageUsers, "");
				});
			} else {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Conversation Info is null, bugging out");
#endif
				return;
			}
			MessageInfo msgInfo = null;
		}
		
		private int newUserCount;
		
		private void AddNewUsers ()
		{
			userDetails = new List<User> ();
			newUserCount = 0;
			toBeGrabbed = newUsersToAdd.Count;
			LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
			service.UserGetSpecificCompleted += Service_UserGetSpecificDone;
			service.UserGetSpecificAsync (AndroidData.CurrentUser.AccountID, newUsersToAdd [newUserCount], new Guid (AndroidData.ServiceAuthToken));
		}
		private List<User> userDetails;
		private void Service_UserGetSpecificDone (object s, UserGetSpecificCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)s;
			if (e.Error == null) {
				User result = e.Result;
				#if DEBUG
				if (result.Errors.Count > 0)
					System.Diagnostics.Debug.WriteLine ("Error getting user");
				#endif
				userDetails.Add (result);
				newUserCount++;
				if (newUserCount < toBeGrabbed)
					service.UserGetSpecificAsync (AndroidData.CurrentUser.AccountID, newUsersToAdd [newUserCount], new Guid (AndroidData.ServiceAuthToken));
				else {
					service.UserGetSpecificCompleted -= Service_UserGetSpecificDone;
					doTheAdding (userDetails);
				}
			}
		}
		
		private void doTheAdding (List<User>contacts)
		{
			
			foreach (User user in contacts) {
				RunOnUiThread (delegate {
					ModalNewContact = new Dialog (this, Resource.Style.lightbox_dialog);
					ModalNewContact.SetContentView (Resource.Layout.ModalNewContact);
			
					((Button)ModalNewContact.FindViewById (Resource.Id.btnAccept)).Click += delegate {
						if (user.Picture.Length > 0) {
							Contacts.ContactsUtil.contactFilenames.Add (user.AccountID.ToString ());
							File.WriteAllBytes (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, user.AccountID.ToString ()), user.Picture);
						}
						dbm.InsertOrUpdateUser (user);
						DismissModalDialog ();
					};
					((Button)ModalNewContact.FindViewById (Resource.Id.btnDecline)).Click += delegate {
						DismissModalDialog ();
					};
					((TextView)ModalNewContact.FindViewById (Resource.Id.txtContactName)).Text = user.FirstName + " " + user.LastName;
					ImageView image = ((ImageView)ModalNewContact.FindViewById (Resource.Id.imgContact));
					if (user.Picture.Length == 0)
						image.SetBackgroundResource (Resource.Drawable.defaultuserimage);
					else {
						Bitmap bm = BitmapFactory.DecodeResource (this.Resources, Resource.Drawable.defaultuserimage);
						MemoryStream ms = new MemoryStream ();
						bm.Compress (Bitmap.CompressFormat.Jpeg, 80, ms);
						byte[] img = ms.ToArray ();
						displayImage (img, image);
					}
				
					ModalNewContact.Show ();
				});
			}
		
			DismissModalDialog ();
		}
		
		private void DismissModalDialog ()
		{
			if (ModalNewContact != null)
				ModalNewContact.Dismiss ();
			ModalNewContact = null;
		}
		
		private void displayImage (byte[] image, ImageView contactPic)
		{
			if (image.Length > 0) {
				using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, thumbImageWidth, thumbImageHeight, this.Resources)) {
					RunOnUiThread (delegate {
						contactPic.SetImageBitmap (myBitmap);
					});
				}
			}
		}
		
		private void ConversationItem_Clicked (object sender, EventArgs e)
		{
			TextView senderObj = (TextView)sender;
			string conversationID = senderObj.ContentDescription;
			commonConversation (conversationID);
		}
		
		private void createUI (List<MessageDB> message, List<UserDB> contact, string nameTitle, bool clear = false)
		{
		
			message.Reverse ();
			contact.Reverse ();
			int m = 0;
			string messager = "";
			bool dd = false;
			if (message != null && contact != null) {
				
				if (clear == false)
					RunOnUiThread (() => listWrapper.RemoveAllViewsInLayout ());
				string othername = string.Empty;
				for (int i = 0; i < contact.Count; ++i) {
					if (string.IsNullOrEmpty (nameTitle))
						othername = contact [i].FirstName + " " + contact [i].LastName;
					else
						othername = nameTitle;
					if (isMe != othername) {
						RunOnUiThread (delegate {
							Header.headertext = othername;
							Header.fontsize = 36f;
							ImageHelper.fontSizeInfo (header.Context);
							header.SetTextSize (Android.Util.ComplexUnitType.Dip, Header.fontsize);
							header.Text = Header.headertext;
						});
						break;
					}
				}
				
				if (contact.Count > 1) {
					string toReturn = "";
					List<UserDB> sortedList = new List<UserDB> ();
					sortedList = contact.OrderBy (s => s.LastName).OrderBy (s => s.FirstName).ToList ();
					foreach (UserDB eachItem in sortedList)
						toReturn += string.Format ("{0} {1}, ", eachItem.FirstName, eachItem.LastName);
					int last = toReturn.LastIndexOf (", ");
					toReturn = toReturn.Remove (last);
					RunOnUiThread (delegate {
						using (LinearLayout btnlayout = new LinearLayout (context)) {
							btnlayout.Orientation = Android.Widget.Orientation.Vertical;
							btnlayout.SetGravity (GravityFlags.Center);
							btnlayout.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
							btnlayout.SetPadding ((int)ImageHelper.convertDpToPixel (5f, context), 0, (int)ImageHelper.convertDpToPixel (5f, context), (int)ImageHelper.convertDpToPixel (10f, context));
							
							using (TextView name = new TextView(context)) {
								name.Text = toReturn;
								name.SetTextSize (Android.Util.ComplexUnitType.Dip, 18f);
								name.SetTextColor (Color.Black);
								btnlayout.AddView (name);
							}
							
							using (Button showAll = new Button (context)) {
								showAll.Gravity = GravityFlags.CenterVertical;
								showAll.Text = Application.Context.Resources.GetString (Resource.String.messageShowAllInConversation);
								showAll.Click += (object sender, EventArgs e) => {
									showParticipants (sender, e, contact); };
								showAll.SetWidth ((int)ImageHelper.convertDpToPixel (180f, context));
								showAll.SetHeight ((int)ImageHelper.convertDpToPixel (30f, context));
								showAll.SetBackgroundResource (Resource.Drawable.button);
								btnlayout.AddView (showAll);
							}
							listWrapper.AddView (btnlayout);
						}
					});
				}
				
				if (getGuid != null)
					getGuid.Clear ();
				
				RunOnUiThread (delegate {				
					foreach (MessageDB messages in message) {
						string name = contact [m].FirstName + " " + contact [m].LastName;

						ImageView random = null;
						LinearLayout layout = new LinearLayout (context);
						layout.Orientation = Android.Widget.Orientation.Horizontal;
						layout.SetGravity (GravityFlags.CenterVertical);
						layout.SetPadding ((int)ImageHelper.convertDpToPixel (5f, context), 0, (int)ImageHelper.convertDpToPixel (5f, context), (int)ImageHelper.convertDpToPixel (10f, context));
						LinearLayout layout2 = new LinearLayout (context);
						layout2.Orientation = Orientation.Vertical;
						layout2.SetGravity (GravityFlags.Center);
						if (name == isMe)
							layout.AddView (contactUserInterface (messages, contact [m], true));
						else
							layout.AddView (contactUserInterface (messages, contact [m], false));
						
						
						/*ImageView profilepic = new ImageView (context);
						profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (55f, context), (int)ImageHelper.convertDpToPixel (100f, context));
						
						if (contact == null)
							profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
						else {
							profilepic.Tag = new Java.Lang.String ("profilepic_" + contact [m].AccountID);
							profilepic.SetImageResource (Resource.Drawable.defaultuserimage);
							layout.AddView (profilepic);
							if (contact [m].Picture.Length == 0 && contact [m].HasProfileImage)
								getGuid.Add (contact [m].AccountID);
							else {
								if (contact [m].Picture.Length > 0)
									LoadUserImage (contact [m], profilepic);
								else
									profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
							}
							
							
							
							TextView name = new TextView (context);
							name.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (230f, context), (int)ImageHelper.convertDpToPixel (40f, context));
							name.Gravity = GravityFlags.Center;
							name.SetTextColor (Color.White);
							name.TextSize = 16f;
							
							layout2.AddView (name);
							
							if (messages.MessageStepDBList.Count == 1 && messages.MessageStepDBList [0].StepType == MessageStep.StepTypes.Text) {
								TextView text = new TextView (context);
								text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (220f, context), (int)ImageHelper.convertDpToPixel (60f, context));
								text.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
								text.Gravity = GravityFlags.CenterVertical;
								if (name == isMe)
									text.SetBackgroundResource (Resource.Drawable.bubblesolidright);
								else
									text.SetBackgroundResource (Resource.Drawable.bubblesolidleft);
								
								text.TextSize = 16f;
								
								for (int e = 0; e < messages.MessageStepDBList.Count; ++e) {
									if (!string.IsNullOrEmpty (messages.MessageStepDBList [e].MessageText)) {
										messager = messages.MessageStepDBList [e].MessageText;
										break;
									}
								}
								
								if (string.IsNullOrEmpty (messager))
									messager = "No text message found";
								
								int nolines = (int)(ImageHelper.convertDpToPixel ((messager.Length / 27f) * 12f, context));
								text.SetHeight (nolines);
								
								text.SetTextColor (Android.Graphics.Color.Black);
								text.Text = messager;
								layout2.Clickable = true;
								layout2.AddView (text);
							} else {*/
						LinearLayout layout3 = new LinearLayout (context);
						layout3.Orientation = Orientation.Horizontal;
						if (name == isMe)
							layout3.SetGravity (GravityFlags.Right);
						else
							layout3.SetGravity (GravityFlags.Left);
						layout3.SetBackgroundResource (Resource.Drawable.attachmentspreviewbkgr);
						layout3.SetVerticalGravity (GravityFlags.CenterVertical);
						layout3.SetMinimumHeight (30);
						layout3.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
								
						if (name != isMe) {
							using (random = new ImageView (context)) {
								random.Tag = m;
								random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
								random.SetBackgroundResource (Resource.Drawable.playblack);
								random.ContentDescription = messages.MessageGuid;
								random.Click += PlayButton_Clicked;
									
								layout3.AddView (random);
							}
						}
								
						int textt = 0;
						for (int tt = 0; tt < messages.MessageStepDBList.Count; ++tt) {
							if (messages.MessageStepDBList [tt].StepType == MessageStep.StepTypes.Text)
								textt++;
						}
								
						for (int i = 0; i < messages.MessageStepDBList.Count; ++i) {
							switch (messages.MessageStepDBList [i].StepType) {
							case LOLMessageDelivery.MessageStep.StepTypes.Text:
								if (textt == 1) {
									TextView text = new TextView (context);
									text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (220f, context), (int)ImageHelper.convertDpToPixel (60f, context));
									text.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
									text.Gravity = GravityFlags.CenterVertical;
									if (name == isMe)
										text.SetBackgroundResource (Resource.Drawable.bubblesolidright);
									else
										text.SetBackgroundResource (Resource.Drawable.bubblesolidleft);
											
									text.TextSize = 16f;
											
									for (int e = 0; e < messages.MessageStepDBList.Count; ++e) {
										if (!string.IsNullOrEmpty (messages.MessageStepDBList [e].MessageText)) {
											messager = messages.MessageStepDBList [e].MessageText;
											break;
										}
									}
											
									if (string.IsNullOrEmpty (messager))
										messager = "No text message found";
											
									int nolines = (int)(ImageHelper.convertDpToPixel ((messager.Length / 27f) * 12f, context));
									text.SetHeight (nolines);
											
									text.SetTextColor (Android.Graphics.Color.Black);
									text.Text = messager;
									layout2.Clickable = true;
									layout2.AddView (text);
								} else {
									if (textt > 1) {
										using (random = new ImageView (context)) {
											random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
											random.SetBackgroundResource (Resource.Drawable.textmsg);
											layout3.AddView (random);
										}
									}
								}
								break;
							case LOLMessageDelivery.MessageStep.StepTypes.Animation:
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.drawicon);
									layout3.AddView (random);
								}
								break;
							case LOLMessageDelivery.MessageStep.StepTypes.Comicon:
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.comicon);
									layout3.AddView (random);
								}
								break;
							case LOLMessageDelivery.MessageStep.StepTypes.Comix:
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.comicmsgs);
									layout3.AddView (random);
								}
								break;
							case LOLMessageDelivery.MessageStep.StepTypes.Emoticon:
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.emoticonmsg);
									layout3.AddView (random);
								}
								break;
							case LOLMessageDelivery.MessageStep.StepTypes.Polling:
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.pollmsg);
									layout3.AddView (random);
								}
								break;
							case LOLMessageDelivery.MessageStep.StepTypes.SoundFX:
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.soundfxmsg);
									layout3.AddView (random);
								}
								break;
							case LOLMessageDelivery.MessageStep.StepTypes.Video:
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.videomsg);
									layout3.AddView (random);
								}
								break;
							case LOLMessageDelivery.MessageStep.StepTypes.Voice:
								using (random = new ImageView (context)) {
									random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
									random.SetBackgroundResource (Resource.Drawable.voicemsg);
									layout3.AddView (random);
								}
								break;
							}
						}
						if (name == isMe) {
							using (random = new ImageView (context)) {
								random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
								random.SetBackgroundResource (Resource.Drawable.playblack);
								random.Click += (object ss, EventArgs ee) => {
									random_Click (ss, ee, message); };
								layout3.AddView (random);
							}
						}
								
						layout2.AddView (layout3);
					
						layout.AddView (layout2);
							
						listWrapper.AddView (layout);
						if (m + 1 < contact.Count)
							m++;
					}
					//}
					
				});
				if (getGuid.Count > 0) {
					cpUI = 0;
					LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
					service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
					service.UserGetImageDataAsync (AndroidData.CurrentUser.AccountID, getGuid [0], new Guid (AndroidData.ServiceAuthToken));
				}
			}
		}
		
		private LinearLayout contactUserInterface (MessageDB message, UserDB contact, bool direction)
		{
			LinearLayout layout = new LinearLayout (context);
			layout.Orientation = Android.Widget.Orientation.Horizontal;
			layout.SetGravity (GravityFlags.CenterVertical);
			string messager = "";
			layout.SetPadding ((int)ImageHelper.convertDpToPixel (5f, context), 0, (int)ImageHelper.convertDpToPixel (5f, context), (int)ImageHelper.convertDpToPixel (10f, context));
			ImageView profilepic = new ImageView (context);
			if (!direction) {	
				profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (55f, context), (int)ImageHelper.convertDpToPixel (100f, context));
						
				if (contact == null)
					profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
				else {
					profilepic.Tag = new Java.Lang.String ("profilepic_" + contact.AccountID);
					profilepic.SetImageResource (Resource.Drawable.defaultuserimage);
					layout.AddView (profilepic);
					if (contact.Picture.Length == 0 && contact.HasProfileImage)
						getGuid.Add (contact.AccountID);
					else {
						if (contact.Picture.Length > 0)
							LoadUserImage (contact, profilepic);
						else
							profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
					}
							
					LinearLayout layout2 = new LinearLayout (context);
					layout2.Orientation = Orientation.Vertical;
					layout2.SetGravity (GravityFlags.Center);

					if (message.MessageStepDBList.Count == 1 && message.MessageStepDBList [0].StepType == MessageStep.StepTypes.Text) {
						TextView text = new TextView (context);
						text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (220f, context), (int)ImageHelper.convertDpToPixel (60f, context));
						text.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
						text.Gravity = GravityFlags.CenterVertical;
						text.SetBackgroundResource (Resource.Drawable.bubblesolidright);
						text.TextSize = 16f;
								
						for (int e = 0; e < message.MessageStepDBList.Count; ++e) {
							if (!string.IsNullOrEmpty (message.MessageStepDBList [e].MessageText)) {
								messager = message.MessageStepDBList [e].MessageText;
								break;
							}
						}
								
						if (string.IsNullOrEmpty (messager))
							messager = "No text message found";
								
						int nolines = (int)(ImageHelper.convertDpToPixel ((messager.Length / 27f) * 12f, context));
						text.SetHeight (nolines);
								
						text.SetTextColor (Android.Graphics.Color.Black);
						text.Text = messager;
						layout2.Clickable = true;
						layout2.AddView (text);
					}
					layout.AddView (layout2);
				}
			} else {	
				LinearLayout layout2 = new LinearLayout (context);
				layout2.Orientation = Orientation.Vertical;
				layout2.SetGravity (GravityFlags.Center);
					
				if (message.MessageStepDBList.Count == 1 && message.MessageStepDBList [0].StepType == MessageStep.StepTypes.Text) {
					TextView text = new TextView (context);
					text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (220f, context), (int)ImageHelper.convertDpToPixel (60f, context));
					text.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
					text.Gravity = GravityFlags.CenterVertical;
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
						
					int nolines = (int)(ImageHelper.convertDpToPixel ((messager.Length / 27f) * 12f, context));
					text.SetHeight (nolines);
						
					text.SetTextColor (Android.Graphics.Color.Black);
					text.Text = messager;
					layout2.Clickable = true;
					layout2.AddView (text);
				}
				profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (55f, context), (int)ImageHelper.convertDpToPixel (100f, context));
				layout.AddView (layout2);
				if (contact == null)
					profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
				else {
					profilepic.Tag = new Java.Lang.String ("profilepic_" + contact.AccountID);
					profilepic.SetImageResource (Resource.Drawable.defaultuserimage);
					layout.AddView (profilepic);
					if (contact.Picture.Length == 0 && contact.HasProfileImage)
						getGuid.Add (contact.AccountID);
					else {
						if (contact.Picture.Length > 0)
							LoadUserImage (contact, profilepic);
						else
							profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
					}
				}
			}
			return layout;
		}
		
		private void showParticipants (object s, EventArgs e, List<UserDB> contacts)
		{
			contactsDialog = new Dialog (this, Resource.Style.lightbox_dialog);
			contactsDialog.SetContentView (Resource.Layout.ModalContactsConversation);
			LinearLayout mainLayout = ((LinearLayout)contactsDialog.FindViewById (Resource.Id.contactMainLayout));
			Context localContext = mainLayout.Context;
			List<Guid> profilePicsToBeGrabbed = new List<Guid> ();
			RunOnUiThread (delegate {
				for (int n = 0; n < contacts.Count; n++) {
					LinearLayout layout = new LinearLayout (context);
					layout.Orientation = Android.Widget.Orientation.Horizontal;
					layout.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
					layout.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (10f, context));
					
					ImageView profilepic = new ImageView (context);
					profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (60f, context), (int)ImageHelper.convertDpToPixel (60f, context));
					profilepic.Tag = new Java.Lang.String ("profilepic_" + contacts [n].AccountID);
					if (contacts [n].HasProfileImage == true && contacts [n].Picture.Length == 0) {
						profilePicsToBeGrabbed.Add (contacts [n].AccountID);
					} else {
						if (contacts [n].Picture.Length > 0)
							LoadUserImage (contacts [n], profilepic);
						else
							profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage));
					}
					layout.AddView (profilepic);
					
					TextView text = new TextView (context);
					text.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (235f, context), (int)ImageHelper.convertDpToPixel (40f, context));
					text.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
					text.Gravity = GravityFlags.CenterVertical;
					text.TextSize = 16f;
					text.SetTextColor (Android.Graphics.Color.White);
					if (contacts [n].FirstName != "" || contacts [n].LastName != "") {
						text.Text = contacts [n].FirstName + " " + contacts [n].LastName;
					} else {
						text.Text = contacts [n].EmailAddress;
					}
					layout.AddView (text);
					
					mainLayout.AddView (layout);
				}
			});
			((Button)contactsDialog.FindViewById (Resource.Id.btnCancel)).Click += delegate {
				DismissModalPreviewDialog ();
			};
			
			if (profilePicsToBeGrabbed.Count > 0) {
				cpUI = 0;
				LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
				service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
				service.UserGetImageDataAsync (AndroidData.CurrentUser.AccountID, profilePicsToBeGrabbed [0], new Guid (AndroidData.ServiceAuthToken));
			}
			
			contactsDialog.Show ();
		}
		
		public void DismissModalPreviewDialog ()
		{
			if (contactsDialog != null)
				contactsDialog.Dismiss ();
				
			contactsDialog = null;
		}
		
		private void random_Click (object s, EventArgs e, List<MessageDB> message)
		{
			ImageView iv = (ImageView)s;
			int t = (int)iv.Tag;
			MessageDB messager = message [t];
			RunOnUiThread (delegate {
				progress = ProgressDialog.Show (context, Application.Context.Resources.GetString (Resource.String.messageViewingMessage),
				                                Application.Context.Resources.GetString (Resource.String.messageViewingPleaseWait));
			});
			PlayMessage (messager);
		}
	}
}


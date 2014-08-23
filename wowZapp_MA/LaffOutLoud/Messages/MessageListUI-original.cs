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

namespace wowZapp.Messages
{
	public partial class MessageList
	{
		private Dialog contactsDialog;
		private void CreateMessagesUI ()
		{
			List<UserDB> participants = new List<UserDB> ();
			bool moreThanOne = false, isCurrentMe = false;
			
			if (MessageConversations.clearView) {
				listWrapper.RemoveAllViews ();
				MessageConversations.clearView = false;
			}
			
			if (Contacts.SelectContactsUtil.selectedContacts.Count != 0)
				Contacts.SelectContactsUtil.selectedContacts.Clear ();
			
			if (MessageConversations.currentConversationMessages [0].MessageRecipientDBList.Count > 1) {
				moreThanOne = true;
				for (int m = 0; m < MessageConversations.currentConversationMessages[0].MessageRecipientDBList.Count; ++m) {
					if (MessageConversations.currentConversationMessages [0].MessageRecipientDBList [m].AccountGuid != AndroidData.CurrentUser.AccountID.ToString ()) {
						UserDB userDetails = dbm.GetUserWithAccountID (MessageConversations.currentConversationMessages [0].MessageRecipientDBList [m].AccountGuid);
						participants.Add (userDetails);
						ContactDB contact = new ContactDB ();
						contact.ContactUser = new LOLAccountManagement.User ();
						contact.ContactUser.AccountID = userDetails.AccountID;
						Contacts.SelectContactsUtil.selectedContacts.Add (contact);
					} else {
						UserDB userDetails = UserDB.ConvertFromUser (AndroidData.CurrentUser);
						participants.Add (userDetails);
					}
				}
			} else {
				UserDB userDetails = dbm.GetUserWithAccountID (MessageConversations.currentConversationMessages [0].MessageRecipientDBList [0].AccountGuid);
				ContactDB contact = new ContactDB ();
				contact.ContactUser = new LOLAccountManagement.User ();
				contact.ContactUser.AccountID = userDetails.AccountID;
				Contacts.SelectContactsUtil.selectedContacts.Add (contact);
			}
					
			if (moreThanOne) {
				string toReturn = "";
				List<UserDB> sortedList = new List<UserDB> ();
				sortedList = participants.OrderBy (s => s.LastName).OrderBy (s => s.FirstName).ToList ();
				foreach (UserDB eachItem in sortedList)
					toReturn += string.Format ("{0} {1}, ", eachItem.FirstName, eachItem.LastName);
				int last = toReturn.LastIndexOf (", ");
				toReturn = toReturn.Remove (last);
						
				using (LinearLayout btnlayout = new LinearLayout (context)) {
					btnlayout.Orientation = Android.Widget.Orientation.Vertical;
					btnlayout.SetGravity (GravityFlags.Center);
					btnlayout.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
					btnlayout.SetPadding ((int)ImageHelper.convertDpToPixel (5f, context), 0, (int)ImageHelper.convertDpToPixel (5f, context), (int)ImageHelper.convertDpToPixel (10f, context));
								
					using (TextView name = new TextView(context)) {
						name.Text = toReturn;
						name.SetTextSize (Android.Util.ComplexUnitType.Dip, 18f);
						name.SetTextColor (Color.Black);
						RunOnUiThread (() => btnlayout.AddView (name));
					}
								
					using (Button showAll = new Button (context)) {
						showAll.Gravity = GravityFlags.CenterVertical;
						showAll.Text = Application.Context.Resources.GetString (Resource.String.messageShowAllInConversation);
						showAll.Click += (object sender, EventArgs e) => {
							showParticipants (sender, e, participants); };
						showAll.SetWidth ((int)ImageHelper.convertDpToPixel (180f, context));
						showAll.SetHeight ((int)ImageHelper.convertDpToPixel (30f, context));
						showAll.SetBackgroundResource (Resource.Drawable.button);
						RunOnUiThread (() => btnlayout.AddView (showAll));
					}
					RunOnUiThread (() => listWrapper.AddView (btnlayout));
				}
						
			}
			foreach (MessageDB message in MessageConversations.currentConversationMessages) {
				isCurrentMe = message.FromAccountID != AndroidData.CurrentUser.AccountID ? false : true;
				LinearLayout shell = new LinearLayout (context);
				shell.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (imageSize [0] + 50f, context));
				shell.Orientation = Orientation.Horizontal;
				shell.SetPadding (0, 0, 0, (int)ImageHelper.convertDpToPixel (5f, context));
				
				UserDB whoAmI = new UserDB ();
				whoAmI = message.FromAccountID != AndroidData.CurrentUser.AccountID ? dbm.GetUserWithAccountID (message.FromAccountGuid) : UserDB.ConvertFromUser (AndroidData.CurrentUser);
				
				LinearLayout imageViewLayout = new LinearLayout (context);
				imageViewLayout.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
				imageViewLayout.SetGravity (GravityFlags.CenterVertical);
				ImageView userImage = new ImageView (context);
				if (Contacts.ContactsUtil.contactFilenames.Contains (message.FromAccountGuid)) {
					using (Bitmap bm = BitmapFactory.DecodeFile (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, message.FromAccountGuid))) {
						using (MemoryStream ms = new MemoryStream ()) {
							bm.Compress (Bitmap.CompressFormat.Jpeg, 80, ms);
							byte[] image = ms.ToArray ();
							using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)imageSize[0], (int)imageSize[1], this.Resources)) {
								RunOnUiThread (delegate {
									userImage.SetImageBitmap (myBitmap);
								});
							}
						}
					}
				} else {
					userImage.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (imageSize [0], context), (int)ImageHelper.convertDpToPixel (imageSize [1], context));
					userImage.SetScaleType (ImageView.ScaleType.FitXy);
					userImage.SetImageResource (Resource.Drawable.defaultuserimage);
				}
				RunOnUiThread (() => imageViewLayout.AddView (userImage));	
				
				LinearLayout messageShell = new LinearLayout (context);
				int left = (int)wowZapp.LaffOutOut.Singleton.ScreenXWidth - (int)ImageHelper.convertDpToPixel (imageSize [0] + 20f, context);
				int leftOver = (int)wowZapp.LaffOutOut.Singleton.ScreenXWidth - (int)ImageHelper.convertDpToPixel (imageSize [0] + 40f, context);
				messageShell.LayoutParameters = new ViewGroup.LayoutParams (!isCurrentMe ? LinearLayout.LayoutParams.FillParent : left, LinearLayout.LayoutParams.FillParent);
				messageShell.Orientation = Orientation.Vertical;
				
				LinearLayout from = new LinearLayout (context);
				using (LinearLayout.LayoutParams layParms = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel(24f, context))) {
					layParms.SetMargins (isCurrentMe ? 0 : (int)ImageHelper.convertDpToPixel (30f, context), 0, isCurrentMe ? (int)ImageHelper.convertDpToPixel (30f, context) : 0, 0);
					layParms.Weight = 10f;
					from.LayoutParameters = layParms;
				}
				
				TextView whoIsIt = new TextView (context);
				whoIsIt.SetTextColor (Color.Black);
				whoIsIt.SetTextSize (Android.Util.ComplexUnitType.Dip, 12f);
				whoIsIt.Gravity = !isCurrentMe ? GravityFlags.Left : GravityFlags.Right;
				whoIsIt.Text = whoAmI.FirstName + " " + whoAmI.LastName;
				RunOnUiThread (() => from.AddView (whoIsIt));
				
				LinearLayout messageBox = new LinearLayout (context);
				using (LinearLayout.LayoutParams linParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent)) {
					messageBox.LayoutParameters = linParams;
					linParams.Weight = 60f;
				}
				for (int m = 0; m < message.MessageStepDBList.Count; ++m) {
					if (message.MessageStepDBList [m].StepType == LOLMessageDelivery.MessageStep.StepTypes.Text) {
						TextView messageText = new TextView (context);
						using (LinearLayout.LayoutParams layParam = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent)) {
							messageText.LayoutParameters = layParam;
						}
						messageText.SetBackgroundResource (message.FromAccountID != AndroidData.CurrentUser.AccountID ? Resource.Drawable.bubblesolidleft : 
						                                   Resource.Drawable.bubblesolidright);
						int lr = (int)ImageHelper.convertDpToPixel (20f, context);                   
						int tb = lr / 2;
						messageText.SetPadding (lr, tb, lr, tb);
						messageText.SetTextColor (Color.White);
						messageText.SetTextSize (Android.Util.ComplexUnitType.Dip, 14f);
						string messager = message.MessageStepDBList [m].MessageText;
						if (messager.Length > 40)
							messager = messager.Substring (0, 40) + "...";
						messageText.Text = messager;
						messageText.ContentDescription = message.MessageGuid;
						RunOnUiThread (() => messageBox.AddView (messageText));
						break;
					}
				}
				
				LinearLayout messageLay = new LinearLayout (context);
				using (LinearLayout.LayoutParams messParam = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel(24f, context))) {
					messParam.SetMargins (isCurrentMe ? 0 : (int)ImageHelper.convertDpToPixel (30f, context), 0, isCurrentMe ? (int)ImageHelper.convertDpToPixel (30f, context) : 0, 0);
					messParam.Weight = 10f;
					messageLay.LayoutParameters = messParam;
				}
				
				TextView messageDate = new TextView (context);
				messageDate.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
				messageDate.SetTextColor (Color.Black);
				messageDate.SetTextSize (Android.Util.ComplexUnitType.Dip, 10f);
				messageDate.Gravity = message.FromAccountID != AndroidData.CurrentUser.AccountID ? GravityFlags.Left : GravityFlags.Right;
				messageDate.Text = message.MessageSent.ToShortTimeString () + ", " + message.MessageSent.ToShortDateString ();
				RunOnUiThread (() => messageLay.AddView (messageDate));
				
				LinearLayout messageItems = new LinearLayout (context);
				messageItems.Orientation = Orientation.Horizontal;
				float messageBarSize = wowZapp.LaffOutOut.Singleton.resizeFonts == true ? ImageHelper.convertPixelToDp (((50f * imageSize [0]) / 100), context) : 
					ImageHelper.convertDpToPixel (40f, context);
				using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)messageBarSize)) {
					layParams.SetMargins ((int)ImageHelper.convertDpToPixel (14f, context), (int)ImageHelper.convertDpToPixel (3.3f, context), 
					                      (int)ImageHelper.convertDpToPixel (12.7f, context), (int)ImageHelper.convertDpToPixel (4f, context));
					messageItems.LayoutParameters = layParams;
				}
				messageItems.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (3.3f, context), 
				                         (int)ImageHelper.convertDpToPixel (10f, context), 0);
				messageItems.SetGravity (GravityFlags.Left);
				messageItems = createMessageBar (messageItems, message, leftOver);
				
				RunOnUiThread (delegate {
					messageShell.AddView (from);
					messageShell.AddView (messageBox);
					messageShell.AddView (messageLay);
					messageShell.AddView (messageItems);
				
					if (whoAmI.AccountID == AndroidData.CurrentUser.AccountID) {
						shell.AddView (messageShell);
						shell.AddView (imageViewLayout);
					} else {
						shell.AddView (imageViewLayout);
						shell.AddView (messageShell);
					}
					listWrapper.AddView (shell);
				});
			}
			if (progress != null)
				RunOnUiThread (() => progress.Dismiss ());
		}
		
		private LinearLayout createMessageBar (LinearLayout mBar, MessageDB message, int leftOver)
		{
			RunOnUiThread (delegate {
				LinearLayout icons = new LinearLayout (context);
				ImageView random = null;
				icons.Orientation = Orientation.Horizontal;
				icons.SetGravity (GravityFlags.Left);
				icons.SetVerticalGravity (GravityFlags.CenterVertical);
				icons.SetMinimumHeight (30);
				int topPos = 0;
				if (wowZapp.LaffOutOut.Singleton.resizeFonts)
					topPos = 0;
				icons.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
				if (message.MessageStepDBList.Count == 0) {
					using (random = new ImageView (context)) {
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, leftOver - (int)ImageHelper.convertDpToPixel (30f, context), 0);
							random.LayoutParameters = lp;
						}
						random.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
						random.SetBackgroundResource (Resource.Drawable.playblack);
						random.ContentDescription = message.MessageGuid;
						random.Click += delegate {
							Messages.MessageReceived m = new Messages.MessageReceived (message, context);
						};
						icons.AddView (random);
					}
				} else {
					int end = message.MessageStepDBList.Count > 3 ? 3 : message.MessageStepDBList.Count;
					int iconSize = (int)ImageHelper.convertDpToPixel (34f, context);
					int toEnd = leftOver - (2 * iconSize) - (end * iconSize);
					for (int i = 0; i < end; ++i) {
						switch (message.MessageStepDBList [i].StepType) {
						case LOLMessageDelivery.MessageStep.StepTypes.Text:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.textmsg);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						case LOLMessageDelivery.MessageStep.StepTypes.Animation:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.drawicon);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						case LOLMessageDelivery.MessageStep.StepTypes.Comicon:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.comicon);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						case LOLMessageDelivery.MessageStep.StepTypes.Comix:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.comix);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						case LOLMessageDelivery.MessageStep.StepTypes.Emoticon:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.emoticon);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						case LOLMessageDelivery.MessageStep.StepTypes.Polling:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.polls);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						case LOLMessageDelivery.MessageStep.StepTypes.SoundFX:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.audiofile);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						case LOLMessageDelivery.MessageStep.StepTypes.Video:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.camera);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						case LOLMessageDelivery.MessageStep.StepTypes.Voice:
							using (random = new ImageView (context)) {
								using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
									lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
									random.LayoutParameters = lp;
								}
								random.SetBackgroundResource (Resource.Drawable.microphone);
								random.ContentDescription = message.MessageID.ToString ();
								random.Click += new EventHandler (imgMessage_Click);
								icons.AddView (random);
							}
							break;
						}
					}
					using (random = new ImageView (context)) {
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (toEnd, 0, 0, 0);
							random.LayoutParameters = lp;
						}
						random.SetBackgroundResource (Resource.Drawable.playblack);
						random.ContentDescription = message.MessageID.ToString ();
						random.Click += new EventHandler (random_Click);
						icons.AddView (random);
					}
				}
				mBar.AddView (icons);
			});
			return mBar;
		}
		
		private void imgMessage_Click (object s, EventArgs e)
		{
			ImageView senderObj = (ImageView)s;
			string conID = senderObj.ContentDescription;
			playOut (conID);
		}
		
		private void random_Click (object s, EventArgs e)
		{
			ImageView senderObj = (ImageView)s;
			string conID = senderObj.ContentDescription;
			MessageDB message = new MessageDB ();
			message = dbm.GetMessage (conID);
			MessageReceived player = new MessageReceived (message, context);
		}
		
		private void playOut (string message)
		{
			MessageDB mess = new MessageDB ();
			mess = dbm.GetMessage (message);
			Messages.ComposeMessageMainUtil.messageDB = mess;
			Intent i = new Intent (this, typeof(Messages.ComposeMessageMainActivity));
			i.PutExtra ("readonly", true);
			StartActivity (i);
		}
		
		private void showParticipants (object s, EventArgs e, List<UserDB> contacts)
		{
			contactsDialog = new Dialog (this, Resource.Style.lightbox_dialog);
			contactsDialog.SetContentView (Resource.Layout.ModalContactsConversation);
			LinearLayout mainLayout = ((LinearLayout)contactsDialog.FindViewById (Resource.Id.contactMainLayout));
			Context localContext = mainLayout.Context;
			List<Guid> profilePicsToBeGrabbed = new List<Guid> ();
			
			for (int n = 0; n < contacts.Count; n++) {
				LinearLayout layout = new LinearLayout (context);
				layout.Orientation = Android.Widget.Orientation.Horizontal;
				layout.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
				layout.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (10f, context));
					
				ImageView profilepic = new ImageView (context);
				profilepic.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (60f, context), (int)ImageHelper.convertDpToPixel (60f, context));
				profilepic.Tag = new Java.Lang.String ("profilepic_" + contacts [n].AccountID);
				if (contacts [n].Picture.Length == 0) {
					profilePicsToBeGrabbed.Add (contacts [n].AccountID);
				} else {
					if (contacts [n].Picture.Length > 0)
						LoadUserImage (contacts [n], profilepic);
					else
						RunOnUiThread (() => profilepic.SetImageDrawable (Application.Context.Resources.GetDrawable (Resource.Drawable.defaultuserimage)));
				}
				RunOnUiThread (() => layout.AddView (profilepic));
					
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
				RunOnUiThread (delegate {
					layout.AddView (text);
					mainLayout.AddView (layout);
				});
			}
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
		
		private void LoadUserImage (UserDB user, ImageView profilePic)
		{
			if (user.Picture.Length == 0)
				RunOnUiThread (() => profilePic.SetImageResource (Resource.Drawable.defaultuserimage));
			else
				using (Bitmap img = ImageHelper.CreateUserProfileImageForDisplay(user.Picture, (int)imageSize[0], (int)imageSize[0], this.Resources)) {
					RunOnUiThread (() => profilePic.SetImageBitmap (img));
				}//end using
		}
		
		public void DismissModalPreviewDialog ()
		{
			if (contactsDialog != null)
				contactsDialog.Dismiss ();
			
			contactsDialog = null;
		}
	}
}
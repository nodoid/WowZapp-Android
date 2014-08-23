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

namespace wowZapp.Messages
{
	public partial class Conversations
	{
		private Dialog ModalNewContact;
		private void CreateUI ()
		{
			if (MessageConversations.clearView) {
				RunOnUiThread (() => listWrapper.RemoveAllViewsInLayout ());
				MessageConversations.clearView = false;
			}
			int c = 0;
			List<int> unreadMessages = new List<int> ();
			int numberInConversation = 0;
			foreach (LOLMessageConversation conversation in MessageConversations.conversationsList) {
				unreadMessages.Add (conversation.MessageIDs.Count - conversation.ReadMessageIDs.Count);
				int t = 0;
			}
			if (unknownUsers == null)
				unknownUsers = new List<Guid> ();
			else
				unknownUsers.Clear ();
			                                                         
			
			List<Guid> unknownMessages = new List<Guid> ();
			foreach (MessageDB latestMessage in MessageConversations.initialMessages) {
				UserDB whoFrom = new UserDB ();
				whoFrom = dbm.GetUserWithAccountID (MessageConversations.conversationsList [c].Recipients [0].ToString ());
				if (latestMessage.MessageRecipientDBList.Count != 0) {
					if (whoFrom == null && latestMessage.MessageRecipientDBList [0].AccountGuid == AndroidData.CurrentUser.AccountID.ToString ())
						whoFrom = UserDB.ConvertFromUser (AndroidData.CurrentUser);
					if (whoFrom != null) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("c = {0}, Recipient[0] Guid = {1}, whoFrom name = {2} {3}", c, MessageConversations.conversationsList [c].Recipients [0], 
					                  whoFrom.FirstName, whoFrom.LastName);
						#endif
						List<UserDB> users = new List<UserDB> ();
						numberInConversation = latestMessage.MessageRecipientDBList.Count;
						for (int i = 0; i < (numberInConversation > 3 ? 3 : numberInConversation); ++i) {
							if (latestMessage.MessageRecipientDBList [i] != null) {
								UserDB current = dbm.GetUserWithAccountID (latestMessage.MessageRecipientDBList [i].AccountGuid.ToString ());
								if (current == null && latestMessage.MessageRecipientDBList [i].AccountGuid != AndroidData.CurrentUser.AccountID.ToString ())
									unknownUsers.Add (new Guid (latestMessage.MessageRecipientDBList [i].AccountGuid));
								else
									users.Add (current);
							}
						}

						int leftOver = (int)wowZapp.LaffOutOut.Singleton.ScreenXWidth - (int)ImageHelper.convertDpToPixel (imageSize [0] + 30f, context);
						LinearLayout shell = new LinearLayout (context);
					
						shell.Orientation = Orientation.Horizontal;
						shell.SetGravity (GravityFlags.CenterVertical);
						shell.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (imageSize [1] + 40f, context));
						shell.SetPadding (0, 0, 0, (int)ImageHelper.convertDpToPixel (5f, context));
				
						ImageView imageLayout = new ImageView (context);
						using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent)) {
							layParams.SetMargins (3, 3, 3, 3);
							imageLayout.LayoutParameters = layParams;
						}
						imageLayout.SetPadding ((int)ImageHelper.convertDpToPixel (5f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 
					                        (int)ImageHelper.convertDpToPixel (10f, context));
							
						imageLayout = generateUserImage (users);	
						RunOnUiThread (() => shell.AddView (imageLayout));
				
						LinearLayout information = new LinearLayout (context);
						information.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);
						information.Orientation = Orientation.Vertical;
					
						LinearLayout topLayout = new LinearLayout (context);
						topLayout.Orientation = Orientation.Vertical;
						using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel(28f, context))) {
							layParams.Weight = 10f;
							layParams.SetMargins (0, 0, 0, (int)ImageHelper.convertDpToPixel (3f, context));
							topLayout.LayoutParameters = layParams;
						}
					
						TextView whoAmI = new TextView (context);
						using (LinearLayout.LayoutParams layParams= new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent)) {
							layParams.SetMargins (10, 0, 10, 5);
							whoAmI.LayoutParameters = layParams;
						}
						string myName = whoFrom.FirstName + " " + whoFrom.LastName;
						if (numberInConversation != 1) {
							myName += string.Format (" and {0} other{1}", numberInConversation == 2 ? "1" : 
							                         (numberInConversation - 1).ToString (), numberInConversation == 2 ? "." : "s.");
						}
						whoAmI.SetTextColor (Android.Graphics.Color.Black);
						whoAmI.Text = myName;
						whoAmI.SetTextSize (Android.Util.ComplexUnitType.Dip, 12f);
						RunOnUiThread (() => topLayout.AddView (whoAmI));
					
				
						LinearLayout messageLayout = new LinearLayout (context);
						using (LinearLayout.LayoutParams linParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent)) {
							linParams.Weight = 80f;
							messageLayout.LayoutParameters = linParams;
						}
						messageLayout.Orientation = Orientation.Horizontal;
				
						TextView txtMessage = new TextView (context);
						using (LinearLayout.LayoutParams linParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel(70f, context))) {
							linParams.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (8f, context), 0);
							txtMessage.LayoutParameters = linParams;
						}
						txtMessage.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (5f, context), 
				                      (int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (5f, context));
				                      
						if (latestMessage.MessageStepDBList.Count == 1 && latestMessage.MessageStepDBList [0].StepType == MessageStep.StepTypes.Text) {
							txtMessage = messageTextBox (latestMessage, 0, leftOver);
						
						} else {
					
							for (int n = 0; n < latestMessage.MessageStepDBList.Count; ++n) {
								if (latestMessage.MessageStepDBList [n].StepType == MessageStep.StepTypes.Text) {
									txtMessage = messageTextBox (latestMessage, n, leftOver);
									break;
								}
							}
						}
					
						txtMessage.ContentDescription = latestMessage.MessageID.ToString ();
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("ContentDesctription = {0}, ID = {1}", txtMessage.ContentDescription, latestMessage.ID);
						#endif
						txtMessage.Click += textMessage_Click;
						RunOnUiThread (() => messageLayout.AddView (txtMessage));

						LinearLayout bottomLayout = new LinearLayout (context);
						using (LinearLayout.LayoutParams linParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent)) {
							linParams.SetMargins (0, (int)ImageHelper.convertDpToPixel (3f, context), 0, 0);
							bottomLayout.LayoutParameters = linParams;
						}				
						bottomLayout.SetGravity (GravityFlags.Right);
				
						TextView txtMessageUnread = new TextView (context);
						using (LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel(12f, context))) {
							layoutParams.SetMargins (10, 0, 10, 0);
							txtMessageUnread.LayoutParameters = layoutParams;
						}
						txtMessageUnread.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
						txtMessageUnread.SetTextColor (Color.Black);
						txtMessageUnread.SetTextSize (Android.Util.ComplexUnitType.Dip, 10f);
						if (unreadMessages [c] == 0)
							txtMessageUnread.Text = "0 unread messages.";
						else
							txtMessageUnread.Text = string.Format ("({0}) unread message{1}", unreadMessages [c], unreadMessages [c] == 1 ? "." : "s.");
						txtMessageUnread.Gravity = GravityFlags.Right;
						RunOnUiThread (() => bottomLayout.AddView (txtMessageUnread));
					
						LinearLayout messageItems = new LinearLayout (context);
						if (latestMessage.MessageStepDBList.Count > 0) {
							
							messageItems.Orientation = Orientation.Horizontal;
							float messageBarSize = wowZapp.LaffOutOut.Singleton.resizeFonts == true ? ImageHelper.convertPixelToDp (((55f * imageSize [0]) / 100), context) : ImageHelper.convertDpToPixel (40f, context);
							using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams (leftOver, (int)messageBarSize)) {
								layParams.SetMargins ((int)ImageHelper.convertDpToPixel (14f, context), (int)ImageHelper.convertDpToPixel (3.3f, context), (int)ImageHelper.convertDpToPixel (12.7f, context), (int)ImageHelper.convertDpToPixel (4f, context));
								messageItems.LayoutParameters = layParams;
							}
							messageItems.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (3.3f, context), (int)ImageHelper.convertDpToPixel (10f, context), 0);
							messageItems.SetGravity (GravityFlags.Left);
							messageItems.SetBackgroundResource (Resource.Drawable.attachmentspreviewbkgr);
							messageItems = createMessageBar (messageItems, latestMessage, leftOver);
							messageItems.ContentDescription = latestMessage.MessageGuid;
							messageItems.Click += messageBar_Click;
						} else {
							messageItems.Orientation = Orientation.Horizontal;
							float messageBarSize = wowZapp.LaffOutOut.Singleton.resizeFonts == true ? ImageHelper.convertPixelToDp (((55f * imageSize [0]) / 100), context) : ImageHelper.convertDpToPixel (40f, context);
							using (LinearLayout.LayoutParams layParams = new LinearLayout.LayoutParams (leftOver, (int)messageBarSize)) {
								layParams.SetMargins ((int)ImageHelper.convertDpToPixel (14f, context), (int)ImageHelper.convertDpToPixel (3.3f, context), (int)ImageHelper.convertDpToPixel (12.7f, context), (int)ImageHelper.convertDpToPixel (4f, context));
								messageItems.LayoutParameters = layParams;
								messageItems.SetBackgroundResource (Resource.Drawable.attachmentspreviewbkgr);
							}
						}						
						RunOnUiThread (delegate {
							information.AddView (topLayout);
							if (!string.IsNullOrEmpty (txtMessage.Text))
								information.AddView (messageLayout);
							if (latestMessage.MessageStepDBList.Count > 1)
								information.AddView (messageItems);
							information.AddView (bottomLayout);

							shell.AddView (information);
							listWrapper.AddView (shell);
						});
						
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("done a loop");
						#endif
					}
				}
				if (c < MessageConversations.initialMessages.Count - 2)
					c++;
					
			}
			//});

			if (unknownUsers.Count != 0) {
				Contacts.AddUnknownUser auu = new Contacts.AddUnknownUser (unknownUsers, context);
			}
			if (progress != null)
				RunOnUiThread (() => progress.Dismiss ());
		}
		
		private TextView messageTextBox (MessageDB message, int position, int leftOver)
		{
			TextView txtMessage = new TextView (context);
			using (LinearLayout.LayoutParams txtMessageParams = new LinearLayout.LayoutParams (leftOver, 
			                                                                            (int)ImageHelper.convertDpToPixel (60f, context))) {
				txtMessageParams.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), 0, 0, 0);
				txtMessage.LayoutParameters = txtMessageParams;
			}
			txtMessage.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), (int)ImageHelper.convertDpToPixel (10f, context), 
			                       (int)ImageHelper.convertDpToPixel (20f, context), (int)ImageHelper.convertDpToPixel (10f, context));
			txtMessage.Gravity = GravityFlags.CenterVertical;
			txtMessage.SetBackgroundResource (Resource.Drawable.bubblesolidleft);
			txtMessage.TextSize = 16f;
			
			if (position < message.MessageStepDBList.Count) {
				if (string.IsNullOrEmpty (message.MessageStepDBList [position].MessageText))
					txtMessage.Text = "";
				else
					txtMessage.Text = message.MessageStepDBList [position].MessageText;
			}
			int nolines = (int)(ImageHelper.convertDpToPixel ((txtMessage.Text.Length / 27f) * 12f, context));
			txtMessage.SetHeight (nolines);
			txtMessage.SetTextColor (Android.Graphics.Color.Black);
			return txtMessage;
		}
		
		private LinearLayout createMessageBar (LinearLayout mBar, MessageDB message, int leftOver)
		{
			LinearLayout icons = new LinearLayout (context);
			icons.Orientation = Orientation.Horizontal;
			icons.SetGravity (GravityFlags.Left);
			icons.SetVerticalGravity (GravityFlags.CenterVertical);
			icons.SetMinimumHeight (30);

			icons.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
			if (message.MessageStepDBList.Count == 0) {
				ImageView random1 = new ImageView (context);
				using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
					lp.SetMargins (0, 0, leftOver - (int)ImageHelper.convertDpToPixel (30f, context), 0);
					random1.LayoutParameters = lp;
				}
				random1.LayoutParameters = new ViewGroup.LayoutParams ((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context));
				random1.SetBackgroundResource (Resource.Drawable.playblack);
				random1.ContentDescription = message.MessageGuid;
				random1.Click += delegate {
					Messages.MessageReceived m = new Messages.MessageReceived (message, context);
				};
				RunOnUiThread (() => icons.AddView (random1));
			} else {
				int end = message.MessageStepDBList.Count > 3 ? 3 : message.MessageStepDBList.Count;
				int iconSize = (int)ImageHelper.convertDpToPixel (34f, context);
				int toEnd = leftOver - (2 * iconSize) - (end * iconSize);
				for (int i = 0; i < end; ++i) {
					switch (message.MessageStepDBList [i].StepType) {
					case LOLMessageDelivery.MessageStep.StepTypes.Text:
						ImageView random2 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random2.LayoutParameters = lp;
						}
						random2.SetBackgroundResource (Resource.Drawable.textmsg);
						random2.ContentDescription = message.MessageID.ToString ();
						random2.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random2));
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Animation:
						ImageView random3 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random3.LayoutParameters = lp;
						}
						random3.SetBackgroundResource (Resource.Drawable.drawicon);
						random3.ContentDescription = message.MessageID.ToString ();
						random3.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random3));
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Comicon:
						ImageView random4 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random4.LayoutParameters = lp;
						}
						random4.SetBackgroundResource (Resource.Drawable.comicon);
						random4.ContentDescription = message.MessageID.ToString ();
						random4.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random4));
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Comix:
						ImageView random5 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random5.LayoutParameters = lp;
						}
						random5.SetBackgroundResource (Resource.Drawable.comix);
						random5.ContentDescription = message.MessageID.ToString ();
						random5.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random5));
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Emoticon:
						ImageView random6 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random6.LayoutParameters = lp;
						}
						random6.SetBackgroundResource (Resource.Drawable.emoticon);
						random6.ContentDescription = message.MessageID.ToString ();
						random6.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random6));
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Polling:
						ImageView random7 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random7.LayoutParameters = lp;
						}
						random7.SetBackgroundResource (Resource.Drawable.polls);
						random7.ContentDescription = message.MessageID.ToString ();
						random7.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random7));	
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.SoundFX:
						ImageView random8 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random8.LayoutParameters = lp;
						}
						random8.SetBackgroundResource (Resource.Drawable.audiofile);
						random8.ContentDescription = message.MessageID.ToString ();
						random8.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random8));
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Video:
						ImageView random9 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random9.LayoutParameters = lp;
						}
						random9.SetBackgroundResource (Resource.Drawable.camera);
						random9.ContentDescription = message.MessageID.ToString ();
						random9.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random9));
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Voice:
						ImageView randomA = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							randomA.LayoutParameters = lp;
						}
						randomA.SetBackgroundResource (Resource.Drawable.microphone);
						randomA.ContentDescription = message.MessageID.ToString ();
						randomA.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (randomA));
						break;
					}
				}
				ImageView randomp = new ImageView (context);
				using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
					lp.SetMargins (toEnd, 0, 0, 0);
					randomp.LayoutParameters = lp;
				}
				randomp.SetBackgroundResource (Resource.Drawable.playblack);
				randomp.ContentDescription = message.MessageID.ToString ();
				randomp.Click += new EventHandler (random_Click);
				RunOnUiThread (() => icons.AddView (randomp));
			}
			RunOnUiThread (() => mBar.AddView (icons));
			return mBar;
		}
				
		private ImageView generateUserImage (List<UserDB>users)
		{			
			Bitmap blank = BitmapFactory.DecodeResource (context.Resources, Resource.Drawable.defaultuserimage);
			Bitmap smallBlank = Bitmap.CreateScaledBitmap (blank, (int)imageSize [0] / 2, (int)imageSize [0] / 2, false);
			System.IO.MemoryStream ms = new System.IO.MemoryStream ();
			smallBlank.Compress (Bitmap.CompressFormat.Jpeg, 80, ms);
			byte[] img = ms.ToArray ();
			
			float imageXY = imageSize [0] / 2;
			ImageView blankIV = new ImageView (context);
			blankIV.LayoutParameters = new LinearLayout.LayoutParams ((int)imageSize [0], (int)imageSize [1]);
			blankIV.SetImageResource (Resource.Drawable.defaultuserimage);
			blankIV.SetScaleType (ImageView.ScaleType.FitXy);
			
			List<UserDB> newUsers = users.Where (s => s.AccountID != AndroidData.CurrentUser.AccountID && s != null).ToList ();
			
			if (newUsers.Count == 0 || (newUsers.Count == 1 && newUsers [0].Picture.Length == 0))
				return blankIV;
		
			List<byte[]> userImages = new List<byte[]> ();
			for (int n = 0; n < newUsers.Count; ++n) {
				if (newUsers [n].Picture.Length > 0) {
					using (Bitmap sm = BitmapFactory.DecodeByteArray(newUsers[n].Picture, 0, newUsers[n].Picture.Length)) {
						using (Bitmap smScale = Bitmap.CreateScaledBitmap(sm, (int)imageXY, (int)imageXY, false)) {
							using (System.IO.MemoryStream mm = new System.IO.MemoryStream()) {
								smScale.Compress (Bitmap.CompressFormat.Jpeg, 80, mm);
								userImages.Add (mm.ToArray ());
							}
						}
					}
				} else {
					if (Contacts.ContactsUtil.contactFilenames.Contains (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, newUsers [n].AccountGuid))) {
						using (Bitmap small = BitmapFactory.DecodeFile (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, newUsers [n].AccountGuid))) {
							using (Bitmap smallUser = Bitmap.CreateScaledBitmap (small, (int)imageXY, (int)imageXY, false)) {
								using (System.IO.MemoryStream m = new System.IO.MemoryStream ()) {
									smallUser.Compress (Bitmap.CompressFormat.Jpeg, 80, m);
									userImages.Add (m.ToArray ());
								}
							}
						}
					} else
						userImages.Add (img);
				}
			}

			Bitmap [] bitmaps = new Bitmap[userImages.Count];
		
			for (int n = 0; n < userImages.Count; ++n) {
				bitmaps [n] = ImageHelper.CreateUserProfileImageForDisplay (userImages [n], (int)imageXY, (int)imageXY, this.Resources);
				if (bitmaps [n] == null)
					bitmaps [n] = blank;
			}
		
			if (userImages.Count == 1 && users.Count == 1) {
				blankIV.SetImageBitmap (bitmaps [0]);
				return blankIV;
			}
		
			int diff = users.Count - userImages.Count;

			Bitmap cs = Bitmap.CreateBitmap ((int)imageSize [0], (int)imageSize [1], Bitmap.Config.Argb8888);
			Canvas comboImage = new Canvas (cs); 
			switch (userImages.Count) {
			case 2:
				comboImage.DrawBitmap (bitmaps [0], 0f, 0f, null);
				comboImage.DrawBitmap (bitmaps [1], imageXY, 0f, null);
				if (diff != 0) {
					switch (diff) {
					case 1:
						comboImage.DrawBitmap (smallBlank, 0f, imageXY, null);
						break;
					case 2:
						comboImage.DrawBitmap (smallBlank, 0f, imageXY, null);
						comboImage.DrawBitmap (smallBlank, imageXY, imageXY, null);
						break;
					}
				}
				break;
			case 3:
				comboImage.DrawBitmap (bitmaps [0], 0f, 0f, null);
				comboImage.DrawBitmap (bitmaps [1], imageXY, 0f, null);
				comboImage.DrawBitmap (bitmaps [2], 0f, imageXY, null);
				if (diff == 1)
					comboImage.DrawBitmap (smallBlank, imageXY, imageXY, null);
				break;
			case 4:
				comboImage.DrawBitmap (bitmaps [0], 0f, 0f, null);
				comboImage.DrawBitmap (bitmaps [1], imageXY, 0f, null);
				comboImage.DrawBitmap (bitmaps [2], 0f, imageXY, null);
				comboImage.DrawBitmap (bitmaps [3], imageXY, imageXY, null);
				break;
			}
			blankIV.SetImageBitmap (cs);

			return blankIV;
		}
		
		private void displayImage (byte[] image, ImageView contactPic)
		{
			if (image.Length > 0) {
				using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)imageSize[0], (int)imageSize[1], this.Resources)) {
					RunOnUiThread (delegate {
						contactPic.SetImageBitmap (myBitmap);
					});
				}
			}
		}
	}
}
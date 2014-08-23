using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using LOLApp_Common;
using LOLMessageDelivery;
using WZCommon;

namespace wowZapp.Main
{		
	public partial class HomeActivity : Activity
	{
		private string shortenBubble (MessageDB message)
		{
			string toReturn = "";
			int n = -1;
			for (int t = 0; t < message.MessageStepDBList.Count; ++t) {
				#if DEBUG
				System.Diagnostics.Debug.WriteLine (message.MessageStepDBList [t].StepType);
				#endif
				if (message.MessageStepDBList [t].StepType == LOLMessageDelivery.MessageStep.StepTypes.Text) {
					n = t;
					continue;
				}
			}
			if (n != -1) {
				if (message.MessageStepDBList [n].MessageText.Length > 35)
					toReturn = message.MessageStepDBList [n].MessageText.Substring (0, 35) + "...";
				else
					toReturn = message.MessageStepDBList [n].MessageText;
			}
			return toReturn;
		}
		
		private void grabAndDisplayLastTen ()
		{
			messageListAll = dbm.GetTopTenUnreadMessages (AndroidData.CurrentUser.AccountID.ToString ());
			if (messageListAll.Count == 0)
				return;
			
			posn = 0;
			scroller = new System.Timers.Timer ();
			scroller.Interval = 2000;
			scroller.Elapsed += new System.Timers.ElapsedEventHandler (scroller_Elapsed);
			scroller.Start ();
			UserDB tmp = dbm.GetUserWithAccountID (messageListAll [posn].FromAccountID.ToString ());
			if (tmp != null && messageListAll [posn].MessageStepDBList.Count > 0) {
				string messager = shortenBubble (messageListAll [posn]);
				generateMessageBarAndAnimate (messager, messageListAll [posn], tmp, true);
			}
		}
		
		private void displayImage (byte[] image, ImageView contactPic)
		{
			if (image.Length > 0) {
				using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, picSize, picSize, this.Resources)) {
					try {
						RunOnUiThread (() => contactPic.SetImageBitmap (myBitmap));
					} catch (System.NullReferenceException) {
						#if DEBUG
						Console.WriteLine ("Null reference hit");
						#endif
					}
				}
			}
			GC.Collect ();
		}
		
		private void generateMessageBarAndAnimate (string message, MessageDB msgList, UserDB contact, bool shutUp = false)
		{
			//RunOnUiThread (delegate {
			bool rpong = false;
			if (shutUp != true) {
				AudioPlayer ap = new AudioPlayer (context);
				Android.Content.Res.AssetManager am = this.Assets;
				ap.playFromAssets (am, "incoming.mp3");
			}
			Guid grabGuid = Guid.Empty;
			RunOnUiThread (() => messageBar.SetPadding ((int)ImageHelper.convertDpToPixel (5f, context), 0, (int)ImageHelper.convertDpToPixel (5f, context), 
			                                            (int)ImageHelper.convertDpToPixel (10f, context)));
			
			int leftOver = Convert.ToInt32 (wowZapp.LaffOutOut.Singleton.ScreenXWidth - (picSize + 40));
			LinearLayout.LayoutParams layParams;
			if (contact == null)
				throw new Exception ("Contact is null in generateMessageBarAndAnimate");
			
			ImageView profilePic = new ImageView (context);
			using (layParams = new LinearLayout.LayoutParams (picSize, picSize)) {
				layParams.SetMargins ((int)ImageHelper.convertDpToPixel (15f, context), (int)ImageHelper.convertDpToPixel (20f, context), 0, 0);
				profilePic.LayoutParameters = layParams;
			}
			profilePic.SetBackgroundResource (Resource.Drawable.defaultuserimage);
			profilePic.Tag = new Java.Lang.String ("profilepic_" + contact.AccountID);
			
			RunOnUiThread (() => messageBar.AddView (profilePic));
			
			if (Contacts.ContactsUtil.contactFilenames.Contains (contact.AccountGuid)) {
				rpong = true;
				using (Bitmap bm = BitmapFactory.DecodeFile (System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ImageDirectory, contact.AccountGuid))) {
					using (MemoryStream ms = new MemoryStream ()) {
						bm.Compress (Bitmap.CompressFormat.Jpeg, 80, ms);
						byte[] image = ms.ToArray ();
						displayImage (image, profilePic);
					}
				}
			} else {
				if (contact.Picture.Length == 0)
					grabGuid = contact.AccountID;
				else {
					if (contact.Picture.Length > 0)
						loadProfilePicture (contact, profilePic);
				}
			}
			
			LinearLayout fromTVH = new LinearLayout (context);
			fromTVH.Orientation = Orientation.Vertical;
			fromTVH.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);
			
			float textNameSize = wowZapp.LaffOutOut.Singleton.resizeFonts == true ? (int)ImageHelper.convertPixelToDp (((25 * picSize) / 100), context) : 16f;
			TextView textFrom = new TextView (context);
			using (layParams = new LinearLayout.LayoutParams (leftOver, (int)textNameSize)) {
				layParams.SetMargins ((int)ImageHelper.convertDpToPixel (9.3f, context), (int)ImageHelper.convertPixelToDp (textNameSize - 1, context), 0, 0);
				textFrom.LayoutParameters = layParams;
			}
			float fontSize = wowZapp.LaffOutOut.Singleton.resizeFonts == true ? ImageHelper.convertPixelToDp (((12 * picSize) / 100), context) : ImageHelper.convertPixelToDp (14f, context);
			textFrom.SetTextSize (Android.Util.ComplexUnitType.Dip, fontSize);
			
			textFrom.SetTextColor (Color.Black);
			if (contact != null)
				textFrom.Text = contact.FirstName + " " + contact.LastName;
			else
				textFrom.Text = "Ann Onymouse";
			RunOnUiThread (() => fromTVH.AddView (textFrom));
			
			float textMessageSize = wowZapp.LaffOutOut.Singleton.resizeFonts == true ? ImageHelper.convertPixelToDp (((70 * picSize) / 100), context) : ImageHelper.convertDpToPixel (39f, context);
			TextView textMessage = new TextView (context);
			using (layParams = new LinearLayout.LayoutParams (leftOver, (int)textMessageSize)) {
				layParams.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10, context), 0);
				textMessage.LayoutParameters = layParams;
			}
			float fontSize2 = wowZapp.LaffOutOut.Singleton.resizeFonts == true ? ImageHelper.convertPixelToDp (((12 * picSize) / 100), context) : ImageHelper.convertPixelToDp (16f, context);
			textMessage.SetTextSize (Android.Util.ComplexUnitType.Dip, fontSize2);
			textMessage.SetTextColor (Color.White);
			
			textMessage.SetPadding ((int)ImageHelper.convertDpToPixel (20f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
			if (!string.IsNullOrEmpty (message))
				textMessage.SetBackgroundResource (Resource.Drawable.bubblesolidleft);
			textMessage.Text = message != string.Empty ? message : "";
			textMessage.ContentDescription = msgList.MessageGuid;
			textMessage.Click += new EventHandler (textMessage_Click);
			RunOnUiThread (() => fromTVH.AddView (textMessage));
			//}
			
			if (msgList != null) {
				LinearLayout messageItems = new LinearLayout (context);
				messageItems.Orientation = Orientation.Horizontal;
				float messageBarSize = wowZapp.LaffOutOut.Singleton.resizeFonts == true ? ImageHelper.convertPixelToDp (((35 * picSize) / 100), context) : ImageHelper.convertDpToPixel (40f, context);
				using (layParams = new LinearLayout.LayoutParams (leftOver, (int)messageBarSize)) {
					layParams.SetMargins ((int)ImageHelper.convertDpToPixel (14f, context), (int)ImageHelper.convertDpToPixel (3.3f, context), (int)ImageHelper.convertDpToPixel (12.7f, context), (int)ImageHelper.convertDpToPixel (4f, context));
					messageItems.LayoutParameters = layParams;
				}
				messageItems.SetPadding ((int)ImageHelper.convertDpToPixel (10f, context), (int)ImageHelper.convertDpToPixel (3.3f, context), (int)ImageHelper.convertDpToPixel (10f, context), 0);
				messageItems.SetGravity (GravityFlags.Left);
				messageItems = createMessageBar (messageItems, msgList, leftOver);
				RunOnUiThread (() => fromTVH.AddView (messageItems));
			}
			
			RunOnUiThread (() => messageBar.AddView (fromTVH));
			
			RunOnUiThread (delegate {
				hsv.RemoveAllViews ();
				hsv.AddView (messageBar);
			});
			
			if (grabGuid != null) {
				LOLConnectClient service = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
				service.UserGetImageDataCompleted += Service_UserGetImageDataCompleted;
				service.UserGetImageDataAsync (AndroidData.CurrentUser.AccountID, grabGuid, new Guid (AndroidData.ServiceAuthToken));
			}
			//});                
			RunOnUiThread (delegate {
				Handler handler = new Handler ();
				handler.PostDelayed (new Action (() =>
				{
					hsv.SmoothScrollTo (newX, 0);
				}), 2000);
			});
		}
		
		private void loadProfilePicture (UserDB user, ImageView image)
		{
			if (user.Picture.Length == 0)
				RunOnUiThread (() => image.SetImageResource (Resource.Drawable.defaultuserimage));
			else
				using (Bitmap img = ImageHelper.CreateUserProfileImageForDisplay(user.Picture, picSize, picSize, this.Resources)) {
					RunOnUiThread (() => image.SetImageBitmap (img));
				}
			GC.Collect ();
		}
		
		private void messagebox ()
		{
			List<MessageDB> message = new List<MessageDB> ();
			message = dbm.GetAllMessagesForOwner (AndroidData.CurrentUser.AccountID.ToString ());
			//});
			int m = message.Count;
			#if DEBUG
			System.Diagnostics.Debug.WriteLine ("message stored : {0}", m);
			#endif
			if (m > 0) {
				Messages.MessageInfo msgInfoItem = null;
				UserDB contactUser = null;
				foreach (MessageDB eachMessageDB in message) {
					if (eachMessageDB.FromAccountID != AndroidData.CurrentUser.AccountID) {
						contactUser = dbm.GetUserWithAccountID (eachMessageDB.FromAccountGuid);
						msgInfoItem = new Messages.MessageInfo (eachMessageDB, contactUser);
					} else {
						contactUser = UserDB.ConvertFromUser (AndroidData.CurrentUser);
						msgInfoItem = new Messages.MessageInfo (eachMessageDB, contactUser);
						
						if (!eachMessageDB.MessageConfirmed) {
							ContentInfo contentInfo = dbm.GetContentInfoByMessageDBID (eachMessageDB.ID);
							msgInfoItem.ContentInfo = contentInfo;
						}
					}
					
					if (message.Count > 0) {
						string messager = string.Empty;
						if (msgInfoItem.Message.MessageStepDBList [0].MessageText.Length > 45)
							messager = msgInfoItem.Message.MessageStepDBList [0].MessageText.Substring (0, 44);
						else
							messager = msgInfoItem.Message.MessageStepDBList [0].MessageText;
						generateMessageBarAndAnimate (messager, message [message.Count - 1], contactUser);
					}
				}
			}
			//});
		}
		
		private LinearLayout createMessageBar (LinearLayout mBar, MessageDB message, int leftOver)
		{
			LinearLayout icons = new LinearLayout (context);
			icons.Orientation = Orientation.Horizontal;
			icons.SetGravity (GravityFlags.Left);
			icons.SetVerticalGravity (GravityFlags.CenterVertical);
			icons.SetMinimumHeight (30);
			int topPos = 0;
			if (wowZapp.LaffOutOut.Singleton.resizeFonts)
				topPos = 0;
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
				//}
			} else {
				int end = message.MessageStepDBList.Count > 3 ? 3 : message.MessageStepDBList.Count;
				int iconSize = (int)ImageHelper.convertDpToPixel (34f, context);
				int toEnd = leftOver - (2 * iconSize) - (end * iconSize);
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("toEnd = {0}, end = {1}, iconSize = {2}, leftOver = {3}", toEnd, end, iconSize, leftOver);
#endif
				for (int i = 0; i < end; ++i) {
					switch (message.MessageStepDBList [i].StepType) {
					case LOLMessageDelivery.MessageStep.StepTypes.Text:
						ImageView random2 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random2.LayoutParameters = lp;
						}
						random2.SetBackgroundResource (Resource.Drawable.textmsg);
						random2.ContentDescription = message.MessageGuid;
						random2.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random2));
						//}
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Animation:
						ImageView random3 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random3.LayoutParameters = lp;
						}
						random3.SetBackgroundResource (Resource.Drawable.drawicon);
						random3.ContentDescription = message.MessageGuid;
						random3.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random3));
						//}
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Comicon:
						ImageView random4 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random4.LayoutParameters = lp;
						}
						random4.SetBackgroundResource (Resource.Drawable.comicon);
						random4.ContentDescription = message.MessageGuid;
						random4.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random4));
						//}
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Comix:
						ImageView random5 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random5.LayoutParameters = lp;
						}
						random5.SetBackgroundResource (Resource.Drawable.comix);
						random5.ContentDescription = message.MessageGuid;
						random5.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random5));
						//}
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Emoticon:
						ImageView random6 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random6.LayoutParameters = lp;
						}
						random6.SetBackgroundResource (Resource.Drawable.emoticon);
						random6.ContentDescription = message.MessageGuid;
						random6.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random6));
						//}
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Polling:
						ImageView random7 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random7.LayoutParameters = lp;
						}
						random7.SetBackgroundResource (Resource.Drawable.polls);
						random7.ContentDescription = message.MessageGuid;
						random7.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random7));
						//}
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.SoundFX:
						ImageView random8 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random8.LayoutParameters = lp;
						}
						random8.SetBackgroundResource (Resource.Drawable.audiofile);
						random8.ContentDescription = message.MessageGuid;
						random8.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random8));
						//}
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Video:
						ImageView random9 = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							random9.LayoutParameters = lp;
						}
						random9.SetBackgroundResource (Resource.Drawable.camera);
						random9.ContentDescription = message.MessageGuid;
						random9.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (random9));
						//}
						break;
					case LOLMessageDelivery.MessageStep.StepTypes.Voice:
						ImageView randomA = new ImageView (context);
						using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
							lp.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (1f, context), 0);
							randomA.LayoutParameters = lp;
						}
						randomA.SetBackgroundResource (Resource.Drawable.microphone);
						randomA.ContentDescription = message.MessageGuid;
						randomA.Click += new EventHandler (imgMessage_Click);
						RunOnUiThread (() => icons.AddView (randomA));
						//}
						break;
					}
				}
				ImageView randomB = new ImageView (context);
				using (LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams((int)ImageHelper.convertDpToPixel (30f, context), (int)ImageHelper.convertDpToPixel (30f, context))) {
					lp.SetMargins (toEnd, 0, 0, 0);
					randomB.LayoutParameters = lp;
				}
				randomB.SetBackgroundResource (Resource.Drawable.playblack);
				randomB.ContentDescription = message.MessageGuid;
				randomB.Click += new EventHandler (random_Click);
				RunOnUiThread (() => icons.AddView (randomB));
				//}
			}
			RunOnUiThread (() => mBar.AddView (icons));
			//});
			return mBar;
		}
		
		private void aboutUs ()
		{
			ModalPreviewDialog = new Dialog (this, Resource.Style.lightbox_dialog);
			ModalPreviewDialog.SetContentView (Resource.Layout.ModalAboutUsCompany);
			string [] company = Resources.GetStringArray (Resource.Array.aboutusTrendaAddress);
			string [] spork = Resources.GetStringArray (Resource.Array.aboutusSpork);
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textCompany1)).Text = company [0];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textCompany2)).Text = company [1];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textCompany3)).Text = company [2];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textCompany4)).Text = company [3];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textDevelop1)).Text = spork [0];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textDevelop2)).Text = spork [1];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textDevelop3)).Text = spork [2];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textDevelop4)).Text = spork [3];
			
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textPublishDate)).Text = wowZapp.LaffOutOut.Singleton.Published;
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textVersion)).Text = wowZapp.LaffOutOut.Singleton.Version;
			
			((Button)ModalPreviewDialog.FindViewById (Resource.Id.btnDevs)).Click += delegate {
				DismissModalPreviewDialog ();
				showDevs ();
			};
			((Button)ModalPreviewDialog.FindViewById (Resource.Id.btnClose)).Click += delegate {
				DismissModalPreviewDialog ();
				scroller.Start ();
				wowZapp.LaffOutOut.Singleton.EnableMessageTimer ();
				wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;
			};
			
			ModalPreviewDialog.Show ();
		}
		
		private void showDevs ()
		{
			ModalPreviewDialog = new Dialog (this, Resource.Style.lightbox_dialog);
			ModalPreviewDialog.SetContentView (Resource.Layout.ModalAboutUsDeveloper);
			
			string [] developers = Resources.GetStringArray (Resource.Array.developersList);
			
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textProMang)).Text = developers [0];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textAndroid)).Text = developers [1];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textIOS1)).Text = developers [2];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textIOS2)).Text = developers [1];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textWeb1)).Text = developers [3];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textWeb2)).Text = developers [4];
			((TextView)ModalPreviewDialog.FindViewById (Resource.Id.textGraphics)).Text = developers [4];
			
			((Button)ModalPreviewDialog.FindViewById (Resource.Id.btnClose)).Click += delegate {
				DismissModalPreviewDialog ();
				scroller.Start ();
				wowZapp.LaffOutOut.Singleton.EnableMessageTimer ();
				wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;
			};
			
			ModalPreviewDialog.Show ();
		}
	}
}


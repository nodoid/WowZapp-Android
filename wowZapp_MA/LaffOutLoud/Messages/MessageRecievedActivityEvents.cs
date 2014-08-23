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
		/*void MessageManager_MessageSendConfirmCompleted (object sender, MessageSendConfirmEventArgs e)
		{
			MessageDB msgDB = e.Message;
			MessageInfo msgInfo = new MessageInfo (msgDB, UserDB.ConvertFromUser (AndroidData.CurrentUser));
			
			this.MessageItems [msgInfo.Message.MessageID] = msgInfo;
			
			if (!isConversation) {
				this.RunOnUiThread (delegate {
					
					if (progress != null) {
						progress.Dismiss ();
					}//end if
					this.createUI (new List<MessageDB> () { msgDB }, new List<UserDB> () { msgInfo.MessageUser }, "", true);
				});
				
			} else {
				
				RunOnUiThread (delegate {
					
					if (progress != null) {
						progress.Dismiss ();
					}//end if
					
					this.GetRowsForMessages ();
					this.LoadContactsAndMessages (true);
					
				});
				
			}//end if else
		}*/
		
		private void Service_ContactsSaveCompleted (object s, ContactsSaveCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)s;
			service.ContactsSaveCompleted -= Service_ContactsSaveCompleted;
			if (e.Error == null) {
				Contact result = e.Result;
				if (result.Errors.Count > 0) {
					RunOnUiThread (delegate {
						Alert (context, Application.Context.Resources.GetString (Resource.String.errorSaveContactTitle), Application.Context.Resources.GetString (Resource.String.errorSaveContactMessage));
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
		
		private void Service_UserGetImageDataCompleted (object sender, UserGetImageDataCompletedEventArgs e)
		{
			#if DEBUG
			System.Diagnostics.Debug.WriteLine ("UserGetImageDataCompleted!");
#endif
			LOLConnectClient service = (LOLConnectClient)sender;
			if (e.Result.Errors.Count == 0) {
				using (Bitmap userImage = ImageHelper.CreateUserProfileImageForDisplay(e.Result.ImageData, this.thumbImageWidth, this.thumbImageHeight, this.Resources)) {
					ImageView pic = (ImageView)listWrapper.FindViewWithTag (new Java.Lang.String ("profilepic_" + e.Result.AccountID));
					if (pic != null)
						RunOnUiThread (() => pic.SetImageBitmap (userImage));
				}//end using userImage
				cpUI++;
				dbm.UpdateUserImage (e.Result.AccountID.ToString (), e.Result.ImageData);
				if (cpUI < getGuid.Count)
					service.UserGetImageDataAsync (AndroidData.CurrentUser.AccountID, getGuid [cpUI], new Guid (AndroidData.ServiceAuthToken));
				else
					service.UserGetImageDataCompleted -= Service_UserGetImageDataCompleted;
			}
		}
		
		private void Service_ContentPackGetItemCompleted (object sender, ContentPackGetItemCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)sender;
			
			if (null == e.Error) {
				ContentPackItem result = e.Result;
				
				if (result.Errors.Count > 0) {
#if(DEBUG)
					System.Diagnostics.Debug.WriteLine ("Error in getting content pack items! {0}", StringUtils.CreateErrorMessageFromGeneralErrors (result.Errors.ToArray ()));
#endif
				} else {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Content pack. Result = {0}", result);
#endif
					this.contentPackItems [result.ContentPackItemID] = result;
					ContentPackItemDB contentPackItem = ContentPackItemDB.ConvertFromContentPackItem (result);
					RunOnUiThread (delegate {
						SaveContentPackItem (contentPackItem);
						dbm.InsertOrUpdateContentPackItems (new List<ContentPackItemDB> () { contentPackItem });
					});
				}//end if else
				
				ContentState stateObj = (ContentState)e.UserState;
				stateObj.ContentPackIDQ.Dequeue ();
				
				if (stateObj.HasContentPackItems) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Contentpack get item completed async");
#endif
					
					service.ContentPackGetItemAsync (stateObj.ContentPackIDQ.Peek (),
					                                 ContentPackItem.ItemSize.Small,
					                                 AndroidData.CurrentUser.AccountID,
					                                 new Guid (AndroidData.ServiceAuthToken), stateObj);
				} else
				if (stateObj.HasVoiceRecordings) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Contentpack get item completed - has voice recordings");
#endif
					service.ContentPackGetItemCompleted -= Service_ContentPackGetItemCompleted;
					
					LOLMessageClient msgService = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
					msgService.MessageGetStepDataCompleted += Service_MessageGetStepData;
					msgService.MessageGetStepDataAsync (stateObj.Message.MessageID, stateObj.VoiceIDQ.Peek (), new Guid (AndroidData.ServiceAuthToken), stateObj);
				} else
				if (stateObj.HasPollingSteps) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Contentpack get item completed - has poll steps");
#endif
					service.ContentPackGetItemCompleted -= Service_ContentPackGetItemCompleted;
					
					LOLMessageClient msgService = new LOLMessageClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
					msgService.PollingStepGetCompleted += Service_PollingStepGetCompleted;
					msgService.PollingStepGetAsync (stateObj.Message.MessageID,
					                                stateObj.PollingIDQ.Peek (),
					                                AndroidData.CurrentUser.AccountID,
					                                new Guid (AndroidData.ServiceAuthToken), stateObj);
				} else {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Contentpack get item completed - startplaymessage");
#endif
					service.ContentPackGetItemCompleted -= Service_ContentPackGetItemCompleted;
					RunOnUiThread (delegate {
						StartPlayMessage (stateObj.Message);
					});
				}//end if else
			} else {
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Exception in getting content pack items! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
			}//end if else
		}
		
		private void Service_MessageGetStepData (object sender, MessageGetStepDataCompletedEventArgs e)
		{
			LOLMessageClient service = (LOLMessageClient)sender;
			
			if (null == e.Error) {
				byte[] result = e.Result;
				ContentState stateObj = (ContentState)e.UserState;
				int forStepNumber = stateObj.VoiceIDQ.Dequeue ();
				
				if (null == result || result.Length == 0) {
					#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Error downloading message data for step number: {0}, MessageID: {1}!", forStepNumber, stateObj.Message.MessageID);
					#endif
				} else
					this.voiceFiles [forStepNumber] = SaveVoiceRecordingFile (result, stateObj.Message.MessageID, forStepNumber);
				
				if (stateObj.HasVoiceRecordings) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("messagegetstepdata - has voice recordings");
#endif
					service.MessageGetStepDataAsync (stateObj.Message.MessageID, stateObj.VoiceIDQ.Peek (), new Guid (AndroidData.ServiceAuthToken), stateObj);
				} else
				if (stateObj.HasPollingSteps) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("messagegetstepdata - has polling steps");
#endif
					service.MessageGetStepDataCompleted -= Service_MessageGetStepData;
					
					service.PollingStepGetCompleted += Service_PollingStepGetCompleted;
					service.PollingStepGetAsync (stateObj.Message.MessageID,
					                             stateObj.PollingIDQ.Peek (),
					                             AndroidData.CurrentUser.AccountID,
					                             new Guid (AndroidData.ServiceAuthToken), stateObj);
				} else {
#if DEBUG
					System.Diagnostics.Debug.WriteLine ("messagegetstepdata - starting play message");
#endif
					service.MessageGetStepDataCompleted -= Service_MessageGetStepData;
					RunOnUiThread (delegate {
						StartPlayMessage (stateObj.Message);
					});
				}//end if else
			} else {
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Exception in getting voice recording! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
			}//end if
		}
		
		private void Service_MessageGetConversationsCompleted (object sender, MessageGetConversationsCompletedEventArgs e)
		{
			LOLMessageClient service = (LOLMessageClient)sender;
			service.MessageGetConversationsCompleted -= Service_MessageGetConversationsCompleted;
			
			if (null == e.Error) {
				LOLMessageDelivery.Message[] result = e.Result.ToArray ();
				List<MessageDB> msgList = new List<MessageDB> ();
				UserDB contactUser = null;
				foreach (LOLMessageDelivery.Message eachMessage in result) {
					if (eachMessage.Errors.Count > 0) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Error retrieving message: {0}", StringUtils.CreateErrorMessageFromMessageGeneralErrors (eachMessage.Errors.ToArray ()));
						#endif
					} else {
						contactUser = null;
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("**Message id received: {0}", eachMessage.MessageID);
						#endif
						
						MessageDB msgDB = MessageDB.ConvertFromMessage (eachMessage);
						msgList.Add (msgDB);
						
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Message in conversation received! {0}", msgDB);
						#endif
						
						contactUser = msgDB.FromAccountID == AndroidData.CurrentUser.AccountID ? UserDB.ConvertFromUser (AndroidData.CurrentUser) :
							dbm.GetUserWithAccountID (msgDB.FromAccountGuid);
						
						this.MessageItems [eachMessage.MessageID] = new MessageInfo (msgDB, contactUser);
					}//end if else
				}//end foreach
				
				if (msgList.Count > 0)
					dbm.InsertOrUpdateMessages (msgList);
				
				// Get message users
				Pair<Queue<Guid>, Queue<Guid>> stateObj = new Pair<Queue<Guid>, Queue<Guid>> (new Queue<Guid> (), new Queue<Guid> ());
				foreach (MessageInfo eachMessageInfo in this.MessageItems.Values.Where(s => s.Message.FromAccountID != AndroidData.CurrentUser.AccountID)) {
					if (!dbm.CheckUserExists (eachMessageInfo.Message.FromAccountGuid)) {
						stateObj.ItemA.Enqueue (eachMessageInfo.Message.MessageID);
						stateObj.ItemB.Enqueue (eachMessageInfo.Message.FromAccountID);
					}//end if
				}//end foreach
				
				if (stateObj.ItemA.Count > 0) {
					LOLConnectClient userService = new LOLConnectClient (LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
					userService.UserGetSpecificCompleted += Service_UserGetSpecificCompletedForSenders;
					userService.UserGetSpecificAsync (AndroidData.CurrentUser.AccountID, stateObj.ItemB.Peek (), new Guid (AndroidData.ServiceAuthToken), stateObj);
				} else {
					fromLocalOnly = true;
					RunOnUiThread (delegate {
						if (progress != null)
							progress.Dismiss ();
						GetRowsForMessages ();
						CreatePreviewUI ();
					});
				}//end if else
			} else {
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Exception in MessageGetConversationsCompleted! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
			}//end if else
		}
		
		private void Service_UserGetSpecificCompletedForSenders (object sender, UserGetSpecificCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)sender;
			
			if (null == e.Error) {
				User result = e.Result;
				Pair<Queue<Guid>, Queue<Guid>> stateObj = (Pair<Queue<Guid>, Queue<Guid>>)e.UserState;
				
				Guid messageID = stateObj.ItemA.Dequeue ();
				Guid userID = stateObj.ItemB.Dequeue ();
				
				if (result.Errors.Count > 0) {
					#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Error downloading user! {0}", StringUtils.CreateErrorMessageFromGeneralErrors (result.Errors.ToArray ()));
					#endif
				} else {
					dbm.InsertOrUpdateUser (result);
					// Assign the user to the corresponding message.
					this.MessageItems [messageID].MessageUser = UserDB.ConvertFromUser (result);
				}//end if else
				
				if (stateObj.ItemA.Count > 0)
					service.UserGetSpecificAsync (AndroidData.CurrentUser.AccountID, stateObj.ItemB.Peek (), new Guid (AndroidData.ServiceAuthToken), stateObj);
				else {
					service.UserGetSpecificCompleted -= Service_UserGetSpecificCompletedForSenders;
					fromLocalOnly = true;
					RunOnUiThread (delegate {
						if (progress != null)
							progress.Dismiss ();
						GetRowsForMessages ();
						CreatePreviewUI ();
					});
				}//end if else
			} else {
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Exception retrieving message sender! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
			}//end if else
		}
		
		private void Service_UserGetSpecificCompleted (object sender, UserGetSpecificCompletedEventArgs e)
		{
			LOLConnectClient service = (LOLConnectClient)sender;
			
			if (null == e.Error) {
				User result = e.Result;
				if (result.Errors.Count > 0) {
					#if DBEUG
						System.Diagnostics.Debug.WriteLine ("Error retrieving user: {0}", StringUtils.CreateErrorMessageFromGeneralErrors (result.Errors.ToArray ()));
					#endif
				} else
					dbm.InsertOrUpdateUser (result);
				
				Pair<int, Pair<int, Queue<Guid>>> stateObj = (Pair<int, Pair<int, Queue<Guid>>>)e.UserState;
				stateObj.ItemB.ItemB.Dequeue ();
				
				if (stateObj.ItemB.ItemB.Count > 0)
					service.UserGetSpecificAsync (AndroidData.CurrentUser.AccountID, stateObj.ItemB.ItemB.Peek (), new Guid (AndroidData.ServiceAuthToken), stateObj);
				else {
					service.UserGetSpecificCompleted -= Service_UserGetSpecificCompleted;
					RunOnUiThread (delegate {
						if (progress != null)
							progress.Dismiss ();
					});
				}
			} else {
				service.UserGetSpecificCompleted -= Service_UserGetSpecificCompleted;
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Exception in UserGetSpecific! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
			}//end if else
		}
		
		private void Service_PollingStepGetCompleted (object sender, PollingStepGetCompletedEventArgs e)
		{
			LOLMessageClient service = (LOLMessageClient)sender;
			
			if (null == e.Error) {
				RunOnUiThread (delegate {
					PollingStep result = e.Result;
					ContentState stateObj = (ContentState)e.UserState;
					int stepNumber = stateObj.PollingIDQ.Dequeue ();
					
					if (result.Errors.Count > 0) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Error downloading polling step: {0} Message: {1}", stepNumber, StringUtils.CreateErrorMessageFromMessageGeneralErrors (result.Errors.ToArray ()));
						#endif
					} else {
						PollingStepDB pollStepDB = PollingStepDB.ConvertFromPollingStep (result);
						if (string.IsNullOrEmpty (pollStepDB.PollingAnswer1))
							SavePhotoPollBuffers (pollStepDB);
						
						dbm.InsertOrUpdatePollingSteps (new List<PollingStepDB> () { pollStepDB });
						this.pollSteps [stepNumber] = result;
					}//end if else
					
					if (stateObj.HasPollingSteps) {
						service.PollingStepGetAsync (stateObj.Message.MessageID,
						                             stateObj.PollingIDQ.Peek (),
						                             AndroidData.CurrentUser.AccountID,
						                             new Guid (AndroidData.ServiceAuthToken), stateObj);
					} else {
						service.PollingStepGetCompleted -= Service_PollingStepGetCompleted;
						RunOnUiThread (delegate {
							StartPlayMessage (stateObj.Message);
						});
					}
				});
			} else {
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Exception in getting polling step! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
			}//end if else
		}
		
		private void Service_MessageMarkReadCompleted (object sender, MessageMarkReadCompletedEventArgs e)
		{
			LOLMessageClient service = (LOLMessageClient)sender;
			if (e.Error == null) {
				Queue<MessageDB> msgQ = (Queue<MessageDB>)e.UserState;
				MessageDB messageDB = msgQ.Dequeue ();
				if (e.Result.ErrorNumber == "0" || string.IsNullOrEmpty (e.Result.ErrorNumber)) {
					dbm.MarkMessageRead (messageDB.MessageGuid, AndroidData.CurrentUser.AccountID.ToString ());
					if (msgQ.Count > 0)
						service.MessageMarkReadAsync (msgQ.Peek ().MessageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, new Guid (AndroidData.ServiceAuthToken), msgQ);
					else
						service.MessageMarkReadCompleted -= Service_MessageMarkReadCompleted;
				}
			} else {
				#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Exception when marking messages as read {0} {1}", e.Error.Message, e.Error.StackTrace);
				#endif
			}
		}
		
		private void MessageManager_MessageSendConfirmCompleted (object sender, MessageSendConfirmEventArgs e)
		{
			MessageDB message = e.Message;
			if (message.MessageConfirmed) {
				MessageInfo msgInfo = new MessageInfo (message, UserDB.ConvertFromUser (AndroidData.CurrentUser));
				if (conversationInfo.IsInConversation (msgInfo) && !conversationInfo.Messages.ContainsKey (message.MessageID)) {
					conversationInfo.Messages.Add (message.MessageID, msgInfo);
					if (!isConversation)
						createUI (new List<MessageDB> (){msgInfo.Message}, new List<UserDB> (){msgInfo.MessageUser}, "", true);
					else
						CreatePreviewUI ();
				}
			}
		}
		
	}
}


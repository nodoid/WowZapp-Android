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

using LOLAccountManagement;
using LOLMessageDelivery;
using LOLApp_Common;
using WZCommon;
using LOLMessageDelivery.Classes.LOLAnimation;

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
        public MessageReceived(MessageDB message, Context c, bool readOnly = false)
        {
            MessageReceivedUtil.message = message;
            MessageReceivedUtil.context = c;
            MessageReceivedUtil.readOnly = readOnly;
            startActivity(c);
        }
			
        private void startActivity(Context c)
        {
            Intent i = new Intent(c, typeof(MessageReceivedActivity));
            i.PutExtra("message", true);
            c.StartActivity(i);
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
        private Dictionary<int, AnimationInfo> animationStep;
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
			
        public Dictionary<Guid, MessageInfo> MessageItems
        {
            get;
            private set;
        }
			
        private UserDB UserFrom
			{ get; set; }
			
        private ConversationInfo conversationInfo
			{ get; set; }
			
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            dbm = wowZapp.LaffOutOut.Singleton.dbm;
            mmg = wowZapp.LaffOutOut.Singleton.mmg;
            MessageItems = new Dictionary<Guid, MessageInfo>();
            contentPackItems = new Dictionary<int, ContentPackItem>();
            this.voiceFiles = new Dictionary<int, string>();
            this.contacts = new Dictionary<Guid, ContactDB>();
            this.pollSteps = new Dictionary<int, PollingStep>();
            this.conversationItems = new Dictionary<string, ConversationInfo>();
            this.animationStep = new Dictionary<int, AnimationInfo>();
            ContentPath = wowZapp.LaffOutOut.Singleton.ContentDirectory;
            isMe = AndroidData.CurrentUser.FirstName + " " + AndroidData.CurrentUser.LastName;
            SetContentView(Resource.Layout.previewSoundFX);
            context = MessageReceivedUtil.context;
            btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
            UserDB user = dbm.GetUserWithAccountID(MessageReceivedUtil.message.FromAccountGuid);
            if (user == null)
                user = UserDB.ConvertFromUser(AndroidData.CurrentUser);
            MessageReceivedUtil.userFrom = UserDB.ConvertFromUserDB(user);
            Header.headertext = user.FirstName + " " + user.LastName;
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Pt, Header.fontsize);
            header.Text = Header.headertext;
            RunOnUiThread(delegate
            {
                progress = ProgressDialog.Show(context, Application.Context.Resources.GetString(Resource.String.messageViewingMessage),
						                                Application.Context.Resources.GetString(Resource.String.messageViewingPleaseWait));
            });
            processMessages(MessageReceivedUtil.message);
            PlayMessage(MessageReceivedUtil.message);
            markAsRead = true;
            Finish();
        }	
		
        private void processMessages(MessageDB message)
        {
            List<MessageInfo> messageItems = new List<MessageInfo>();
            MessageInfo msgInfo = new MessageInfo(message, message.FromAccountID == AndroidData.CurrentUser.AccountID ?
			                                       UserDB.ConvertFromUser(AndroidData.CurrentUser) :
			                                       dbm.GetUserWithAccountID(message.FromAccountGuid));
            if (msgInfo != null)
                messageItems.Add(msgInfo);
            if (messageItems.Count > 0)
            {
                foreach (MessageInfo eachMessageInfo in messageItems)
                {
                    this.MessageItems [eachMessageInfo.Message.MessageID] = eachMessageInfo;
                }
            }//end if
        }
		
        private void PlayMessage(MessageDB message)
        {
            voiceFiles.Clear();
            contentPackItems.Clear();
            pollSteps.Clear();
            animationStep.Clear();
			
            MessageInfo messageInfo = this.MessageItems [message.MessageID];
			
            ContentState dlState = new ContentState(message);
            contentPackItems = getLocalContentPackItems(dlState.ContentPackIDQ.ToList())
				.ToDictionary(s => s.ContentPackItemID, s => ContentPackItemDB.ConvertFromContentPackItemDB(s));
			
            if (messageInfo.HasContentInfo)
            {
                RunOnUiThread(delegate
                {
                    if (progress != null)
                        progress.Dismiss();
                    this.PlayUnsentMessage(messageInfo.ContentInfo);
                });
            } else
            {
                Dictionary<Guid, Dictionary<int, string>> localVoiceFiles =
					getLocalVoiceFiles(new List<Pair<Guid, List<int>>>() { new Pair<Guid, List<int>>(dlState.Message.MessageID, dlState.VoiceIDQ.ToList()) });
				
                if (!localVoiceFiles.TryGetValue(dlState.Message.MessageID, out voiceFiles))
                    voiceFiles = new Dictionary<int, string>();
				
                pollSteps = getLocalPollingStepsForMessage(dlState.Message.MessageGuid)
					.ToDictionary(s => s.StepNumber, s => PollingStepDB.ConvertFromPollingStepDB(s));
				
                List<int> animationStepNumbers = 
                    dlState.Message.MessageStepDBList
                        .Where(s => s.StepType == MessageStep.StepTypes.Animation)
                        .Select(s => s.StepNumber)
                        .ToList();

                dlState.RemoveExistingItems(contentPackItems.Keys.ToList(), voiceFiles.Keys.ToList(), pollSteps.Keys.ToList(), animationStep.Keys.ToList());
                if (dlState.HasContentForDownload)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("dlState has content for download");
#endif
                    if (dlState.HasContentPackItems)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("dlState has contentpackitems for download");
#endif
                        LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                        service.ContentPackGetItemCompleted += Service_ContentPackGetItemCompleted;
                        service.ContentPackGetItemAsync(dlState.ContentPackIDQ.Peek(), ContentPackItem.ItemSize.Small, AndroidData.CurrentUser.AccountID,
						                                 new Guid(AndroidData.ServiceAuthToken), dlState);
                    } else
					if (dlState.HasVoiceRecordings)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("dlState has voicerecordings for download");
#endif
                        LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                        service.MessageGetStepDataCompleted += Service_MessageGetStepData;
                        service.MessageGetStepDataAsync(dlState.Message.MessageID, dlState.VoiceIDQ.Peek(), AndroidData.CurrentUser.AccountID, 
                                                         new Guid(AndroidData.ServiceAuthToken), dlState);
                    } else
					if (dlState.HasPollingSteps)
                    {
                        RunOnUiThread(delegate
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("dlState has pollingsteps for download");
#endif
                            LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                            service.PollingStepGetCompleted += Service_PollingStepGetCompleted;
                            service.PollingStepGetAsync(dlState.Message.MessageID, dlState.PollingIDQ.Peek(), AndroidData.CurrentUser.AccountID,
							                             new Guid(AndroidData.ServiceAuthToken), dlState);
                        });
                    } else if (dlState.HasAnimationSteps)
                    {
                        
                        LOLMessageClient msgService = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                        msgService.AnimationStepGetCompleted += Service_AnimationStepGetCompleted;
                        msgService.AnimationStepGetAsync(dlState.Message.MessageID,
                                                         dlState.AnimationIDQ.Peek(),
                                                         AndroidData.CurrentUser.AccountID,
                                                         new Guid(AndroidData.ServiceAuthToken), dlState);
                        
                    }
                } else
                    RunOnUiThread(delegate
                    {
                        StartPlayMessage(message);
                    });
            }
        }
		
        private void Service_AnimationStepGetCompleted(object sender, AnimationStepGetCompletedEventArgs e)
        {
            
            LOLMessageClient service = (LOLMessageClient)sender;
            
            if (null == e.Error)
            {
                
                AnimationStep result = e.Result;
                ContentState stateObj = (ContentState)e.UserState;
                int stepNumber = stateObj.AnimationIDQ.Dequeue();
                
                if (result.Errors.Count > 0)
                {
                    
                    #if(DEBUG)
                    Console.WriteLine("Error downloading animation step: {0} Message: {1}", stepNumber, StringUtils.CreateErrorMessageFromMessageGeneralErrors(result.Errors));
                    #endif
                    
                } else
                {
                    
                    this.animationStep [stepNumber] = result.CreateAnimationInfo();
                    
                    dbm.InsertOrUpdateAnimation(this.animationStep [stepNumber]);
                    
                }//end if else
                
                if (stateObj.HasAnimationSteps)
                {
                    
                    service.AnimationStepGetAsync(stateObj.Message.MessageID,
                                                  stateObj.AnimationIDQ.Peek(),
                                                  AndroidData.CurrentUser.AccountID,
                                                  new Guid(AndroidData.ServiceAuthToken), stateObj);
                    
                } else
                {     
                    service.AnimationStepGetCompleted -= Service_AnimationStepGetCompleted;
                    //this.PlayMessage (stateObj.Message);  
                }//end if else   
            } else
            {
                #if(DEBUG)
                Console.WriteLine("Exception in getting animation step! {0}--{1}", e.Error.Message, e.Error.StackTrace);
                #endif
            }//end if else 
        }

        private List<ContentPackItemDB> getLocalContentPackItems(List<int> itemIds)
        {
            List<ContentPackItemDB> contentPackItems = dbm.GetContentPackItems(itemIds);
            List<ContentPackItemDB> toReturn = new List<ContentPackItemDB>();
            foreach (ContentPackItemDB eachItem in contentPackItems)
            {
                if (checkContentPackItemDataExists(eachItem))
                {
                    eachItem.ContentPackItemIcon = getBufferFromPropertyFile(eachItem.ContentPackItemIconFile);
                    eachItem.ContentPackData = getBufferFromPropertyFile(eachItem.ContentPackDataFile);
                    toReturn.Add(eachItem);
                }
            }
            return toReturn;
        }
		
        private string getVoiceRecordingFilename(Guid msgID, int stepNumber)
        {
            return System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, string.Format(LOLConstants.VoiceRecordingFormat, msgID.ToString(), stepNumber));
        }
		
        private Dictionary<Guid, Dictionary<int, string>> getLocalVoiceFiles(List<Pair<Guid, List<int>>> voiceStepCriteria)
        {
            Dictionary<Guid, Dictionary<int, string>> toReturn = new Dictionary<Guid, Dictionary<int, string>>();
            foreach (Pair<Guid, List<int>> eachItem in voiceStepCriteria)
            {
                Dictionary<int, string> eachMessageVoiceFiles = new Dictionary<int, string>();
                foreach (int eachStep in eachItem.ItemB)
                {
                    if (checkVoiceFileExists(eachItem.ItemA, eachStep))
                        eachMessageVoiceFiles [eachStep] = getVoiceRecordingFilename(eachItem.ItemA, eachStep);
                    else
                        continue;
					
                    if (eachMessageVoiceFiles.Count > 0)
                        toReturn [eachItem.ItemA] = eachMessageVoiceFiles;
                }
            }
            return toReturn;
        }
		
        private bool checkContentPackItemDataExists(ContentPackItemDB item)
        {
            if (item != null)
            {
                return File.Exists(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, item.ContentPackDataFile));
            } else
                return false;
        }
		
        private bool checkVoiceFileExists(Guid msgID, int step)
        {
            return File.Exists(getVoiceRecordingFilename(msgID, step));
        }
		
        private bool checkPhotoPollStepFileExists(PollingStepDB pollStep)
        {
            string contentpath = wowZapp.LaffOutOut.Singleton.ContentDirectory;
            if (pollStep != null)
            {
                bool exist = true;
                if (!string.IsNullOrEmpty(pollStep.PollingData1File))
                    exist &= File.Exists(System.IO.Path.Combine(contentpath, pollStep.PollingData1File));
                if (!string.IsNullOrEmpty(pollStep.PollingData2File))
                    exist &= File.Exists(System.IO.Path.Combine(contentpath, pollStep.PollingData2File));
                if (!string.IsNullOrEmpty(pollStep.PollingData3File))
                    exist &= File.Exists(System.IO.Path.Combine(contentpath, pollStep.PollingData3File));
                if (!string.IsNullOrEmpty(pollStep.PollingData4File))
                    exist &= File.Exists(System.IO.Path.Combine(contentpath, pollStep.PollingData4File));
                return exist;
            } else
                return false;
        }
		
        private List<PollingStepDB> getLocalPollingStepsForMessage(string messageGuid)
        {
            List<PollingStepDB> pollingSteps = dbm.GetPollingSteps(new List<string>() { messageGuid });
            List<PollingStepDB> toReturn = new List<PollingStepDB>();
            foreach (PollingStepDB eachItem in pollingSteps)
            {
                if (string.IsNullOrEmpty(eachItem.PollingAnswer1))
                {
                    if (checkPhotoPollStepFileExists(eachItem))
                    {
                        if (!string.IsNullOrEmpty(eachItem.PollingData1File))
                            eachItem.PollingData1 = getBufferFromPropertyFile(eachItem.PollingData1File);
                        if (!string.IsNullOrEmpty(eachItem.PollingData2File))
                            eachItem.PollingData2 = getBufferFromPropertyFile(eachItem.PollingData2File);
                        if (!string.IsNullOrEmpty(eachItem.PollingData3File))
                            eachItem.PollingData3 = getBufferFromPropertyFile(eachItem.PollingData3File);
                        if (!string.IsNullOrEmpty(eachItem.PollingData4File))
                            eachItem.PollingData4 = getBufferFromPropertyFile(eachItem.PollingData4File);
                    } else
                        continue;
                }
            }
            return toReturn;
        }
		
        private byte[] getBufferFromPropertyFile(string filename)
        {
            string dataFile = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, filename);
            if (!File.Exists(dataFile))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("File {0} doesn't exist", dataFile);
#endif
                return new byte[0];
            }
            bool rv = false;
            byte[] dataBuffer = null;
            RunOnUiThread(delegate
            {
                dataBuffer = File.ReadAllBytes(dataFile);
                rv = dataBuffer == null ? true : false;
            });
            return rv == true ? new byte[0] : dataBuffer;
        }
		
        private void SaveContentPackItem(ContentPackItemDB pack)
        {
            pack.ContentPackDataFile = StringUtils.ConstructContentPackItemDataFile(pack.ContentPackItemID);
            pack.ContentPackItemIconFile = StringUtils.ConstructContentPackItemIconFilename(pack.ContentPackItemID);
            SaveContentPackItemBuffers(pack);
        }
		
        private void SaveContentPackItemBuffers(ContentPackItemDB contentPackItem)
        {
            if (contentPackItem.ContentPackItemIcon != null && contentPackItem.ContentPackItemIcon.Length > 0)
            {
                RunOnUiThread(delegate
                {
                    string iconFile = System.IO.Path.Combine(ContentPath, contentPackItem.ContentPackItemIconFile);
					
                    if (File.Exists(iconFile))
                        File.Delete(iconFile);
					
                    byte[] iconData = contentPackItem.ContentPackItemIcon;
                    try
                    {
                        File.WriteAllBytes(iconFile, iconData);
                    } catch (IOException)
                    {
#if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("Error saving content pack item data file!");
#endif
                    }
                });
            }
			
            if (contentPackItem.ContentPackData != null && contentPackItem.ContentPackData.Length > 0)
            {
                RunOnUiThread(delegate
                {
                    string dataFile = System.IO.Path.Combine(this.ContentPath, contentPackItem.ContentPackDataFile);
					
                    if (File.Exists(dataFile))
                        File.Delete(dataFile);
                    byte[] data = contentPackItem.ContentPackData;
                    try
                    {
                        File.WriteAllBytes(dataFile, data);
                    } catch (IOException)
                    {
#if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("Error saving content pack item data file!");
#endif
                    }
                });
            }//end if
        }
		
        private string SaveVoiceRecordingFile(byte[] voiceBuffer, Guid msgID, int stepNumber)
        {
            string dataFile = System.IO.Path.Combine(ContentPath, string.Format(LOLConstants.VoiceRecordingFormat, msgID.ToString(), stepNumber));
            RunOnUiThread(delegate
            {
                try
                {
                    File.WriteAllBytes(dataFile, voiceBuffer);
                } catch (IOException e)
                {
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("Error saving voice message");
                    #endif
                }
            });
            return dataFile;
        }
		
        private void SavePhotoPollBuffers(PollingStepDB pollStepToSave)
        {
            if (null != pollStepToSave.PollingData1 && pollStepToSave.PollingData1.Length > 0)
                pollStepToSave.PollingData1File = StringUtils.ConstructPollingStepDataFile(1, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);
			
            if (null != pollStepToSave.PollingData2 && pollStepToSave.PollingData2.Length > 0)
                pollStepToSave.PollingData2File = StringUtils.ConstructPollingStepDataFile(2, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);
			
            if (null != pollStepToSave.PollingData3 && pollStepToSave.PollingData3.Length > 0)
                pollStepToSave.PollingData3File = StringUtils.ConstructPollingStepDataFile(3, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);
			
            if (null != pollStepToSave.PollingData4 && pollStepToSave.PollingData4.Length > 0)
                pollStepToSave.PollingData4File = StringUtils.ConstructPollingStepDataFile(4, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);
			
            SavePollingStepDataBuffers(pollStepToSave);
        }
		
        private void SavePollingStepDataBuffers(PollingStepDB pollingStep)
        {
            RunOnUiThread(delegate
            {
                for (int i = 1; i <= 4; i++)
                {
                    string dataFile = string.Empty;
                    byte[] buffer = null;
					
                    switch (i)
                    {
                        case 1:
                            if (!string.IsNullOrEmpty(pollingStep.PollingData1File))
                            {
                                dataFile = System.IO.Path.Combine(ContentPath, pollingStep.PollingData1File);
                                buffer = pollingStep.PollingData1;
                            }
                            break;
                        case 2:
                            if (!string.IsNullOrEmpty(pollingStep.PollingData2File))
                            {
                                dataFile = System.IO.Path.Combine(this.ContentPath, pollingStep.PollingData2File);
                                buffer = pollingStep.PollingData2;
                            }
                            break;
                        case 3:
                            if (!string.IsNullOrEmpty(pollingStep.PollingData3File))
                            {
                                dataFile = System.IO.Path.Combine(this.ContentPath, pollingStep.PollingData3File);
                                buffer = pollingStep.PollingData3;
                            }
                            break;
                        case 4:
                            if (!string.IsNullOrEmpty(pollingStep.PollingData4File))
                            {
                                dataFile = System.IO.Path.Combine(this.ContentPath, pollingStep.PollingData4File);
                                buffer = pollingStep.PollingData4;
                            }
                            break;
                    }//end switch
					
                    if (null != buffer && buffer.Length > 0)
                    {
                        try
                        {
                            File.WriteAllBytes(dataFile, buffer);
                        } catch (IOException e)
                        {
                            #if DEBUG
                            System.Diagnostics.Debug.WriteLine("Unable to save polling step");
                            #endif
                        }
                    }
                }
            });
        }
		
        private void StartPlayMessage(MessageDB message)
        {
            bool hasRespondedPollSteps = this.pollSteps.Count(s => s.Value.HasResponded) > 0;
			
            if (hasRespondedPollSteps)
            {
                RunOnUiThread(() => Toast.MakeText(context, Resource.String.pollGettingResults, ToastLength.Short).Show());
                LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                service.PollingStepGetResultsListCompleted += Service_PollingStepGetResultsListCompleted;
                service.PollingStepGetResultsListAsync(message.MessageID, AndroidData.CurrentUser.AccountID, 
                                                        new Guid(AndroidData.ServiceAuthToken), message);
            } else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("about to play the message");
#endif
                RunOnUiThread(delegate
                {
                    if (progress != null)
                        RunOnUiThread(() => progress.Dismiss());
                    List<UserDB> recipients = new List<UserDB>();
                    UserDB tmpUsr = null;
					
                    for (int m = 0; m < message.MessageRecipientDBList.Count; ++m)
                    {
                        tmpUsr = dbm.GetUserWithAccountID(message.MessageRecipientDBList [m].AccountGuid);
                        if (tmpUsr != null)
                            recipients.Add(tmpUsr);
                    }
					
                    tmpUsr = dbm.GetUserWithAccountID(message.FromAccountGuid);
                    if (tmpUsr != null)
                        recipients.Add(tmpUsr);
                    MessagePlaybackController playbackController =
						new MessagePlaybackController(message.MessageStepDBList,
						                               this.contentPackItems, this.voiceFiles, this.pollSteps, new Dictionary<int, LOLMessageSurveyResult>(), markAsRead, recipients, context);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("we outa here");
#endif
                });
            }//end if else
        }//end void PlayMessage
		
        private void PlayUnsentMessage(ContentInfo contentInfo)
        {
            // TODO : Somesort of activity
            MessagePlaybackController playbackController = new MessagePlaybackController(contentInfo, this.contentPackItems, context);
        }//end void PlayUnsentMessage
		
        private void Service_PollingStepGetResultsListCompleted(object sender, PollingStepGetResultsListCompletedEventArgs e)
        {
            LOLMessageClient service = (LOLMessageClient)sender;
            service.PollingStepGetResultsListCompleted -= Service_PollingStepGetResultsListCompleted;
			
            if (null == e.Error)
            {
                LOLMessageSurveyResult[] results = e.Result.ToArray();
                Dictionary<int, LOLMessageSurveyResult> pollResults = new Dictionary<int, LOLMessageSurveyResult>();
				
                foreach (LOLMessageSurveyResult eachPollResult in results)
                    pollResults [eachPollResult.StepNumber] = eachPollResult;
				
                MessageDB forMessage = (MessageDB)e.UserState;
                List<UserDB> recipients = new List<UserDB>();
                for (int m = 0; m < forMessage.MessageRecipientDBList.Count; ++m)
                    recipients.Add(dbm.GetUserWithAccountID(forMessage.MessageRecipientDBList [m].AccountGuid));
#if DEBUG
                System.Diagnostics.Debug.WriteLine("polling step results obtained - launching playback");
#endif
                RunOnUiThread(delegate
                {
                    if (progress != null)
                        RunOnUiThread(() => progress.Dismiss());
                    MessagePlaybackController playbackController = new MessagePlaybackController(forMessage.MessageStepDBList, 
					                                                                              this.contentPackItems, this.voiceFiles, this.pollSteps, pollResults, false, recipients, context);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("and we're back in the room");
#endif
                });
            } else
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception downloading polling results! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
            }//end if else
        }
		
        private void Service_ContentPackGetItemCompleted(object sender, ContentPackGetItemCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
			
            if (null == e.Error)
            {
                ContentPackItem result = e.Result;
				
                if (result.Errors.Count > 0)
                {
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Error in getting content pack items! {0}", StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors));
#endif
                } else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Content pack. Result = {0}", result);
#endif
                    this.contentPackItems [result.ContentPackItemID] = result;
                    ContentPackItemDB contentPackItem = ContentPackItemDB.ConvertFromContentPackItem(result);
                    RunOnUiThread(delegate
                    {
                        SaveContentPackItem(contentPackItem);
                        dbm.InsertOrUpdateContentPackItems(new List<ContentPackItemDB>() { contentPackItem });
                    });
                }//end if else
				
                ContentState stateObj = (ContentState)e.UserState;
                stateObj.ContentPackIDQ.Dequeue();
				
                if (stateObj.HasContentPackItems)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Contentpack get item completed async");
#endif
					
                    service.ContentPackGetItemAsync(stateObj.ContentPackIDQ.Peek(),
					                                 ContentPackItem.ItemSize.Small,
					                                 AndroidData.CurrentUser.AccountID,
					                                 new Guid(AndroidData.ServiceAuthToken), stateObj);
                } else
				if (stateObj.HasVoiceRecordings)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Contentpack get item completed - has voice recordings");
#endif
                    service.ContentPackGetItemCompleted -= Service_ContentPackGetItemCompleted;
					
                    LOLMessageClient msgService = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                    msgService.MessageGetStepDataCompleted += Service_MessageGetStepData;
                    msgService.MessageGetStepDataAsync(stateObj.Message.MessageID, stateObj.VoiceIDQ.Peek(), AndroidData.CurrentUser.AccountID, 
                                                        new Guid(AndroidData.ServiceAuthToken), stateObj);
                } else
				if (stateObj.HasPollingSteps)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Contentpack get item completed - has poll steps");
#endif
                    service.ContentPackGetItemCompleted -= Service_ContentPackGetItemCompleted;
					
                    LOLMessageClient msgService = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                    msgService.PollingStepGetCompleted += Service_PollingStepGetCompleted;
                    msgService.PollingStepGetAsync(stateObj.Message.MessageID,
					                                stateObj.PollingIDQ.Peek(),
					                                AndroidData.CurrentUser.AccountID,
					                                new Guid(AndroidData.ServiceAuthToken), stateObj);
                } else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Contentpack get item completed - startplaymessage");
#endif
                    service.ContentPackGetItemCompleted -= Service_ContentPackGetItemCompleted;
                    RunOnUiThread(delegate
                    {
                        StartPlayMessage(stateObj.Message);
                    });
                }//end if else
            } else
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception in getting content pack items! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
            }//end if else
        }
		
        private void Service_MessageGetStepData(object sender, MessageGetStepDataCompletedEventArgs e)
        {
            LOLMessageClient service = (LOLMessageClient)sender;
			
            if (null == e.Error)
            {
                byte[] result = e.Result;
                ContentState stateObj = (ContentState)e.UserState;
                int forStepNumber = stateObj.VoiceIDQ.Dequeue();
				
                if (null == result || result.Length == 0)
                {
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("Error downloading message data for step number: {0}, MessageID: {1}!", forStepNumber, stateObj.Message.MessageID);
#endif
                } else
                    this.voiceFiles [forStepNumber] = SaveVoiceRecordingFile(result, stateObj.Message.MessageID, forStepNumber);
				
                if (stateObj.HasVoiceRecordings)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("messagegetstepdata - has voice recordings");
#endif
                    service.MessageGetStepDataAsync(stateObj.Message.MessageID, stateObj.VoiceIDQ.Peek(), AndroidData.CurrentUser.AccountID, 
                                                     new Guid(AndroidData.ServiceAuthToken), stateObj);
                } else
				if (stateObj.HasPollingSteps)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("messagegetstepdata - has polling steps");
#endif
                    service.MessageGetStepDataCompleted -= Service_MessageGetStepData;
					
                    service.PollingStepGetCompleted += Service_PollingStepGetCompleted;
                    service.PollingStepGetAsync(stateObj.Message.MessageID,
					                             stateObj.PollingIDQ.Peek(),
					                             AndroidData.CurrentUser.AccountID,
					                             new Guid(AndroidData.ServiceAuthToken), stateObj);
                } else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("messagegetstepdata - starting play message");
#endif
                    service.MessageGetStepDataCompleted -= Service_MessageGetStepData;
                    RunOnUiThread(delegate
                    {
                        StartPlayMessage(stateObj.Message);
                    });
                }//end if else
            } else
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception in getting voice recording! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
            }//end if
        }
		
        private void Service_PollingStepGetCompleted(object sender, PollingStepGetCompletedEventArgs e)
        {
            LOLMessageClient service = (LOLMessageClient)sender;
			
            if (null == e.Error)
            {
                RunOnUiThread(delegate
                {
                    PollingStep result = e.Result;
                    ContentState stateObj = (ContentState)e.UserState;
                    int stepNumber = stateObj.PollingIDQ.Dequeue();
					
                    if (result.Errors.Count > 0)
                    {
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("Error downloading polling step: {0} Message: {1}", stepNumber, 
                                                           StringUtils.CreateErrorMessageFromMessageGeneralErrors(result.Errors));
                        #endif
                    } else
                    {
                        PollingStepDB pollStepDB = PollingStepDB.ConvertFromPollingStep(result);
                        if (string.IsNullOrEmpty(pollStepDB.PollingAnswer1))
                            SavePhotoPollBuffers(pollStepDB);
						
                        dbm.InsertOrUpdatePollingSteps(new List<PollingStepDB>() { pollStepDB });
                        this.pollSteps [stepNumber] = result;
                    }//end if else
					
                    if (stateObj.HasPollingSteps)
                    {
                        service.PollingStepGetAsync(stateObj.Message.MessageID,
						                             stateObj.PollingIDQ.Peek(),
						                             AndroidData.CurrentUser.AccountID,
						                             new Guid(AndroidData.ServiceAuthToken), stateObj);
                    } else
                    {
                        service.PollingStepGetCompleted -= Service_PollingStepGetCompleted;
                        RunOnUiThread(delegate
                        {
                            StartPlayMessage(stateObj.Message);
                        });
                    }
                });
            } else
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception in getting polling step! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
            }//end if else
        }
    }
}


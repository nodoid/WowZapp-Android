using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LOLAccountManagement;
using LOLApp_Common;
using LOLMessageDelivery;
using LOLMessageDelivery.Classes.LOLAnimation;

using WZCommon;

namespace wowZapp.Messages
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal")]			
    public partial class MessageList : Activity
    {
        private DBManager dbm;
        private LinearLayout listWrapper;
        private Context context;
        private ScrollView scroller;
        private ProgressDialog progress;
        private float[] imageSize;
        private MessageManager mmg;
        private List<UserDB> messageUsers;
        private Dictionary<int, ContentPackItem> contentPackItems;
        private Dictionary<int, string> voiceFiles;
        private Dictionary<Guid, ContactDB> contacts;
        private Dictionary<int, PollingStep> pollSteps;
        private Dictionary<int, AnimationInfo> animationItems;
        private bool markAsRead, clearView;
        private int cpUI;
        private List<Guid> getGuid;
        private string ContentPath;
		
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            dbm = wowZapp.LaffOutOut.Singleton.dbm;
            SetContentView(Resource.Layout.MessageLists);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
			
            Header.headertext = MessageConversations.firstUserName;
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;
            clearView = false;
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
			
            if (messageUsers == null)
                messageUsers = new List<UserDB>();
            else
                messageUsers.Clear();
				
            if (getGuid == null)
                getGuid = new List<Guid>();
            else
                getGuid.Clear();
			
            LinearLayout bottom = FindViewById<LinearLayout>(Resource.Id.bottomHolder);
            ImageButton[] buttons = new ImageButton[3];
            buttons [0] = btnBack;
            buttons [1] = btnHome;
            buttons [2] = btnAdd;
            ImageHelper.setupButtonsPosition(buttons, bottom, context);
            ContentPath = wowZapp.LaffOutOut.Singleton.ContentDirectory;
            scroller = FindViewById<ScrollView>(Resource.Id.scrollViewContainer);
            btnBack.Click += delegate
            {
                wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
                Intent resultData = new Intent();
                SetResult(Result.Ok, resultData);
                Finish();
            };
            //
			
            btnHome.Click += delegate
            {
                wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
                Intent i = new Intent(this, typeof(Main.HomeActivity));
                i.SetFlags(ActivityFlags.ClearTop);
                StartActivity(i);
            };
			
            btnAdd.Click += delegate
            {
                wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;	
                Intent i = new Intent(this, typeof(ComposeMessageChooseContent));
                ComposeMessageMainUtil.returnTo = true;
                StartActivity(i);
            };

            RunOnUiThread(delegate
            {
                progress = ProgressDialog.Show(context, Application.Context.Resources.GetString(Resource.String.messageRefreshingMessages),
				                                Application.Context.Resources.GetString(Resource.String.commonOneSec));
            });
		
            wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;
		
            float tSize = ImageHelper.convertDpToPixel(56f, context);
            imageSize = new float[2];
            imageSize [0] = imageSize [1] = tSize;
            if (wowZapp.LaffOutOut.Singleton.resizeFonts)
                imageSize = ImageHelper.getNewSizes(imageSize, context);
            RunOnUiThread(() => CreateMessagesUI());
            LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted += MessageManager_MessageSendConfirmCompleted;
        }
	
        private void AppDelegate_ReceivedMessages(object sender, IncomingMessageEventArgs e)
        {
            List <Messages.MessageInfo> messageItems = new List<Messages.MessageInfo>();
            Guid me = AndroidData.CurrentUser.AccountID;
            foreach (LOLMessageDelivery.Message eachMessage in e.Messages)
            {
                MessageConversations.storedMessages.Add(MessageDB.ConvertFromMessage(eachMessage));
                MessageConversations.clearView = true;
                CreateMessagesUI();
            }
        }
		
        private void markMessageRead(ConversationInfo conversation)
        {
            string me = AndroidData.CurrentUser.AccountID.ToString();
            Queue<MessageDB> msgQ = new Queue<MessageDB>();
            foreach (MessageDB eachMessageDB in conversation.Messages.Values.Select (s=>s.Message)
			         .Where(s=>s.MessageRecipientDBList.Count(t=>t.AccountGuid == me && !t.IsRead) >0 ))
                msgQ.Enqueue(eachMessageDB);
			
            if (msgQ.Count > 0)
            {
                LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                service.MessageMarkReadCompleted += Service_MessageMarkReadCompleted;
                service.MessageMarkReadAsync(msgQ.Peek().MessageID, AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, 
				                              new Guid(AndroidData.ServiceAuthToken), msgQ);
            }
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
		
        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Messages.MessageReceivedUtil.FromMessages = false;
            MessageConversations.clearView = true;
            CreateMessagesUI();
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
            LaffOutOut.Singleton.mmg.MessageSendConfirmCompleted -= MessageManager_MessageSendConfirmCompleted;
        }
    }
}


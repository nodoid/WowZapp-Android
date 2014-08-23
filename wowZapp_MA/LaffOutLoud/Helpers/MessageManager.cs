using System;
using System.Collections.Generic;
using System.Linq;
using LOLApp_Common;
using LOLMessageDelivery;
using LOLMessageDelivery.Classes.LOLAnimation;
using System.Threading;

using Android.Content;
using Android.Widget;
using Android.App;

using WZCommon;

namespace wowZapp
{
    public class MessageManager : Activity
    {
        public MessageManager()
        {
            this.pContentInfoQ = new Queue<ContentInfo>();
            this.contentInfoQLock = new object();
            dbm = wowZapp.LaffOutOut.Singleton.dbm;
            ccm = wowZapp.LaffOutOut.Singleton.ccm;
        }

        private Queue<ContentInfo> pContentInfoQ;
        private object contentInfoQLock;
        private volatile bool pIsSendingMessage;
        private readonly int maxRetries = 2;
        private DBManager dbm;
        private ContentCacheManager ccm;

        public event EventHandler<MessageSendConfirmEventArgs> MessageSendConfirmCompleted;

        public Queue<ContentInfo> ContentInfoQ
        {
            get
            {
                lock (this.contentInfoQLock)
                {
                    return this.pContentInfoQ;
                }//end lock
            }//end get
        }//end Queue<ContentInfo> ContentInfoQ

        public bool IsSendingMessage
        {
            get
            {
                return this.pIsSendingMessage;
            }
            private set
            {
                this.pIsSendingMessage = value;
            }//end get set
        }//end bool IsSendingMessage

        Context context;

        public void QueueMessage(ContentInfo contentInfo, bool isNew, Context c)
        {
            context = c;
            if (isNew)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    dbm.InsertOrUpdateContentInfoItems(new List<ContentInfo>() { contentInfo });
                });
            }//end if

            this.ContentInfoQ.Enqueue(contentInfo);

            if (!this.IsSendingMessage)
            {

                this.IsSendingMessage = true;

                ContentInfo toSend = this.ContentInfoQ.Dequeue();
                toSend.Message.MessageSent = DateTime.Now;

                try
                {
                    LOLMessageClient service = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                    service.MessageCreateCompleted += Service_MessageCreateCompleted;
                    service.MessageCreateAsync(contentInfo.Message,
                                           contentInfo.Message.MessageSteps,
                                           contentInfo.Recipients, AndroidData.CurrentUser.AccountID,
                                           new Guid(AndroidData.ServiceAuthToken), toSend);
                } catch (Exception e)
                {
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("Exception thrown - {0}--{1}", e.Message, e.StackTrace);
                    #endif
                }

            }//end if else

        }//end void QueueMessage

        private void SendNextMessage(LOLMessageClient service)
        {
            if (!AndroidData.IsAppActive)
            {
                List<ContentInfo> cacheList = new List<ContentInfo>();
                int qCount = this.ContentInfoQ.Count;
                for (int i = 0; i < qCount; i++)
                    cacheList.Add(this.ContentInfoQ.Dequeue());

                dbm.InsertOrUpdateContentInfoItems(cacheList);
                this.IsSendingMessage = false;
                return;
            }

            if (!this.IsSendingMessage)
            {

                if (this.ContentInfoQ.Count > 0)
                {

                    ContentInfo toSend = this.ContentInfoQ.Dequeue();
                    toSend.Message.MessageSent = DateTime.Now;
                    #if DEBUG
                    //RunOnUiThread (() =>System.Diagnostics.Debug.WriteLine ("just do something here"));
                    #endif
                    this.IsSendingMessage = true;

                    if (!toSend.IsFailed && toSend.Retries++ < this.maxRetries)
                    {
                        service.MessageCreateCompleted += Service_MessageCreateCompleted;
                        service.MessageCreateAsync(toSend.Message, toSend.Message.MessageSteps, toSend.Recipients, AndroidData.CurrentUser.AccountID, 
                                                    new Guid(AndroidData.ServiceAuthToken), toSend);
                    } else
                    {
                        if (toSend.Retries++ >= this.maxRetries)
                        {
                            // If the max number of retries has been reached, cache the message content and proceed.
#if(DEBUG)
                            System.Diagnostics.Debug.WriteLine("Max retries: {0}. Will cache message for later.", toSend.Retries);
#endif
                            dbm.InsertOrUpdateContentInfoItems(new List<ContentInfo>() { toSend });
                            this.IsSendingMessage = false;
                            this.SendNextMessage(service);
                        } else
                        {
                            this.SendNextStep(toSend, service);
                        }//end if else
                    }//end if else
                } else
                {
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Finished sending messages!");
#endif
                }//end if else
            }//end if
        }//end void SendNextMessage

        private void SendNextStep(ContentInfo contentInfo, LOLMessageClient service)
        {
#if(DEBUG)
            System.Diagnostics.Debug.WriteLine("Send next step!");
#endif
            if (contentInfo.ContentState != null)
            {
                if (contentInfo.ContentState.HasVoiceRecordings)
                {
                    this.SendVoiceData(contentInfo, service);
                } else 
				if (contentInfo.ContentState.HasPollingSteps)
                {
                    this.SendPollingSteps(contentInfo, service);
                } else
                {
                    service.MessageConfirmSendCompleted += Service_MessageConfirmSendCompleted;
                    service.MessageConfirmSendAsync(contentInfo.Message.MessageID, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken), contentInfo);
                }
            }
        }

        private void Service_MessageConfirmSendCompleted(object sender, MessageConfirmSendCompletedEventArgs e)
        {
#if(DEBUG)
            System.Diagnostics.Debug.WriteLine("Message confirm send!");
#endif

            LOLMessageClient service = (LOLMessageClient)sender;
            service.MessageConfirmSendCompleted -= Service_MessageConfirmSendCompleted;

            if (null == e.Error)
            {
                ContentInfo contentInfo = (ContentInfo)e.UserState;

                if (e.Result.ErrorType == ErrorsManagement.SystemTypesErrorMessage.NoErrorDetected)
                {
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Message sent successfully!");
#endif
                    dbm.MarkMessageSent(contentInfo.Message.MessageID.ToString());
                    contentInfo.Message.MessageConfirmed = true;
                    dbm.DeleteContentInfoIfExists(contentInfo);
                    this.IsSendingMessage = false;
                    RunOnUiThread(() => Toast.MakeText(context, Resource.String.sendMessageMessageSent, ToastLength.Short).Show());
                    if (MessageSendConfirmCompleted != null)
                        MessageSendConfirmCompleted(this, new MessageSendConfirmEventArgs(dbm.GetMessage(contentInfo.Message.MessageID.ToString())));
                } else
                {
                    GeneralError result = e.Result;
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Message sending failed!");
                    System.Diagnostics.Debug.WriteLine(result.ToString());
#endif
                    contentInfo.IsFailed = true;
                    this.ContentInfoQ.Enqueue(contentInfo);
                    RunOnUiThread(() => Toast.MakeText(context, Resource.String.errorMessageSendFail, ToastLength.Short).Show());
                    this.IsSendingMessage = false;
				
                }//end if else
            } else
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception confirming message send! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
            }//end if else
            this.SendNextMessage(service);
        }
		

        private string SaveVoiceRecordingFile(byte[] voiceBuffer, Guid msgID, int stepNumber)
        {
            string dataFile = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, string.Format(LOLConstants.VoiceRecordingFormat,
                msgID.ToString(), stepNumber));

            try
            {
                System.IO.File.WriteAllBytes(dataFile, voiceBuffer);
            } catch (System.IO.IOException ex)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("Error saving voice recording file! {0}", ex.ToString());
                #endif
                RunOnUiThread(() => Toast.MakeText(context, Resource.String.errorSendingVoiceMessage, ToastLength.Short).Show());
            }
            return dataFile;
        }

        private void SendVoiceData(ContentInfo contentInfo, LOLMessageClient service)
        {
#if(DEBUG)
            System.Diagnostics.Debug.WriteLine("Send voice data!");
#endif

            int stepNumber = contentInfo.ContentState.VoiceIDQ.Peek();
            if (contentInfo.VoiceRecordings.Count != 0)
            {
                SaveVoiceRecordingFile(contentInfo.VoiceRecordings [stepNumber], contentInfo.Message.MessageID, stepNumber);

                // Upload voice recordings
                service.MessageStepDataSaveCompleted += Service_MessageStepDataSaveCompleted;
                service.MessageStepDataSaveAsync(contentInfo.Message.MessageID, stepNumber, contentInfo.VoiceRecordings [stepNumber], 
                                                  AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken), contentInfo);
            } else
                this.SendNextMessage(service);
        }//end void SendVoiceData

        private void SendPollingSteps(ContentInfo contentInfo, LOLMessageClient service)
        {
#if(DEBUG)
            System.Diagnostics.Debug.WriteLine("Send polling steps!");
#endif

            PollingStep pollStepToSend = contentInfo.PollingSteps [contentInfo.ContentState.PollingIDQ.Peek()];
            pollStepToSend.MessageID = contentInfo.Message.MessageID;
            contentInfo.PollingSteps [contentInfo.ContentState.PollingIDQ.Peek()] = pollStepToSend;

            if (pollStepToSend is PollingStepDB)
            {
                PollingStepDB converted = (PollingStepDB)pollStepToSend;
                pollStepToSend = PollingStepDB.ConvertFromPollingStepDB(converted);
            }//end if

            // Upload step data
            service.PollingStepSaveCompleted += Service_PollingStepSaveCompleted;
            service.PollingStepSaveAsync(pollStepToSend, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken), contentInfo);
        }//end void SendPollingSteps

        private void Service_MessageCreateCompleted(object sender, MessageCreateCompletedEventArgs e)
        {
#if(DEBUG)
            System.Diagnostics.Debug.WriteLine("Message create completed!");
#endif

            LOLMessageClient service = (LOLMessageClient)sender;
            service.MessageCreateCompleted -= Service_MessageCreateCompleted;

            if (null == e.Error)
            {
                Message result = e.Result;
                ContentInfo contentInfo = (ContentInfo)e.UserState;

                if (result.Errors.Count > 0)
                {
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Error sending message! {0}", StringUtils.CreateErrorMessageFromMessageGeneralErrors(result .Errors).ToString());
#endif
                    this.IsSendingMessage = false;
                    contentInfo.IsMessageCreateFailed = true;
                    RunOnUiThread(() => Toast.MakeText(context, Resource.String.errorMessageSendFail, ToastLength.Short).Show());
                    this.ContentInfoQ.Enqueue(contentInfo);
                } else
                {
                    contentInfo.Message.MessageID = result.MessageID;

                    MessageDB messageDB = MessageDB.ConvertFromMessage(result);
                    messageDB.MessageStepDBList = MessageDB.ConvertFromMessage(contentInfo.Message).MessageStepDBList;
                    foreach (MessageStepDB eachStep in messageDB.MessageStepDBList)
                    {
                        eachStep.MessageID = messageDB.MessageID;
                    }//end foreach

                    dbm.InsertOutgoingMessage(messageDB);

                    ContentState ulState = new ContentState(messageDB);
                    contentInfo.ContentState = ulState;

                    this.SendNextStep(contentInfo, service);
                }//end if else
            } else
            {
                this.IsSendingMessage = false;
            }//end if else

            this.SendNextMessage(service);
        }

        private void Service_MessageStepDataSaveCompleted(object sender, MessageStepDataSaveCompletedEventArgs e)
        {
#if(DEBUG)
            System.Diagnostics.Debug.WriteLine("Message step data save completed!");
#endif

            LOLMessageClient service = (LOLMessageClient)sender;
            service.MessageStepDataSaveCompleted -= Service_MessageStepDataSaveCompleted;
            ContentInfo contentInfo = (ContentInfo)e.UserState;

            if (null == e.Error)
            {
                LOLMessageDelivery.GeneralError result = e.Result;

                if (string.IsNullOrEmpty(result.ErrorNumber) || result.ErrorNumber == "0")
                {
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Voice recording uploaded!");
#endif

                    contentInfo.ContentState.VoiceIDQ.Dequeue();
                    this.SendNextStep(contentInfo, service);
                } else
                {
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Error uploading voice recording! {0}--{1}--{2}", result.ErrorNumber, result.ErrorDescription, result.ErrorLocation);
#endif
                    contentInfo.IsFailed = true;
                    this.ContentInfoQ.Enqueue(contentInfo);
                    this.IsSendingMessage = false;
                    RunOnUiThread(() => Toast.MakeText(context, Resource.String.errorUploadingVoiceMessage, ToastLength.Short).Show());
                    this.SendNextMessage(service);
                }//end if else
            } else
            {
                this.IsSendingMessage = false;
                contentInfo.IsFailed = true;
                this.ContentInfoQ.Enqueue(contentInfo);

                this.SendNextMessage(service);
            }//end if else
        }

        private void Service_PollingStepSaveCompleted(object sender, PollingStepSaveCompletedEventArgs e)
        {
#if(DEBUG)
            System.Diagnostics.Debug.WriteLine("Polling step save completed!");
#endif

            LOLMessageClient service = (LOLMessageClient)sender;
            service.PollingStepSaveCompleted -= Service_PollingStepSaveCompleted;

            ContentInfo contentInfo = (ContentInfo)e.UserState;

            if (null == e.Error)
            {
                List<GeneralError> result = new List<GeneralError>();
                result = e.Result;

                if (null != result && result.Count > 0)
                {
#if(DEBUG)
                    System.Diagnostics.Debug.WriteLine("Error sending polling step! {0}", StringUtils.CreateErrorMessageFromMessageGeneralErrors(result).ToString());
#endif
                    this.IsSendingMessage = false;
                    contentInfo.IsFailed = true;
                    this.ContentInfoQ.Enqueue(contentInfo);
                    RunOnUiThread(() => Toast.MakeText(context, Resource.String.errorUploadPollingMessage, ToastLength.Short).Show());
                    this.SendNextMessage(service);
                } else
                {
                    int stepNumber = contentInfo.ContentState.PollingIDQ.Dequeue();
                    PollingStepDB pollStepSent = PollingStepDB.ConvertFromPollingStep(contentInfo.PollingSteps [stepNumber]);

                    // Save polling step and data
                    if (string.IsNullOrEmpty(pollStepSent.PollingAnswer1))
                    {
                        ccm.SavePhotoPollBuffers(pollStepSent);
                    }//end if

                    // Save to database
                    dbm.InsertNewPollingSteps(new List<PollingStepDB>() { pollStepSent });
                }//end if else

                this.SendNextStep(contentInfo, service);
            } else
            {
                this.IsSendingMessage = false;
                contentInfo.IsFailed = true;
                this.ContentInfoQ.Enqueue(contentInfo);

                this.SendNextMessage(service);
            }//end if else
        }

        private void SendAnimationSteps(ContentInfo contentInfo, LOLMessageClient service)
        {
            
            #if(DEBUG)
            Console.WriteLine("Send animation steps!");
            #endif
            
            AnimationInfo animationInfoToSend = contentInfo.AnimationItems [contentInfo.ContentState.AnimationIDQ.Peek()];
            animationInfoToSend.MessageID = contentInfo.Message.MessageID;
            contentInfo.AnimationItems [contentInfo.ContentState.AnimationIDQ.Peek()] = animationInfoToSend;
            
            foreach (FrameInfo eachFrameInfo in animationInfoToSend.FrameItems.Values)
            {
                Console.WriteLine("In frame: {0}", eachFrameInfo.ID);
                foreach (LayerInfo eachLayerItem in eachFrameInfo.Layers.Values)
                {
                    Console.WriteLine("\tIn layer: {0}", eachLayerItem.ID);
                    foreach (TransitionInfo eachTrItem in eachLayerItem.Transitions.Values)
                    {
                        foreach (TransitionEffectSettings efSetting in eachTrItem.Settings.Values)
                        {
                            Console.WriteLine("\t\tSetting: {0}. Duration: {1}.", efSetting.EffectType, efSetting.Duration);
                        }//end foreach
                    }//end foreach
                }//end foreach
            }//end foreach
            AnimationStep animStep = animationInfoToSend.CreateAnimationStep();
            foreach (AnimationTransition eachTransition in animStep.TransitionItems)
            {
                Console.WriteLine("Step transition duration: {0}", eachTransition.TransitionDuration);
                
                foreach (KeyValuePair<AnimationTypesTransitionEffectType, double> eachSetting in eachTransition.EffectDelays)
                {
                    Console.WriteLine("\tStep duration: {0}", eachTransition.EffectDurations [eachSetting.Key]);
                    Console.WriteLine("\tStep delay: {0}", eachTransition.EffectDelays [eachSetting.Key]);
                    Console.WriteLine("\tStep rotation: {0}", eachTransition.EffectRotations [eachSetting.Key]);
                }//end foreach
            }//end foreach
            
            // Upload animation step
            service.AnimationStepSaveCompleted += Service_AnimationStepSaveCompleted;
            service.AnimationStepSaveAsync(animStep, 
                                           AndroidData.CurrentUser.AccountID, 
                                           new Guid(AndroidData.ServiceAuthToken), contentInfo);
            
        }//end void SendAnimationSteps
        
        
        
        
        private void Service_AnimationStepSaveCompleted(object sender, AnimationStepSaveCompletedEventArgs e)
        {
            #if(DEBUG)
            Console.WriteLine("Animation step save completed!");
            #endif
            
            LOLMessageClient service = (LOLMessageClient)sender;
            service.AnimationStepSaveCompleted -= Service_AnimationStepSaveCompleted;
            
            ContentInfo contentInfo = (ContentInfo)e.UserState;
            
            if (null == e.Error)
            {
                
                GeneralError result = e.Result;
                
                if (result.ErrorType != ErrorsManagement.SystemTypesErrorMessage.NoErrorDetected)
                {
                    
                    #if(DEBUG)
                    Console.WriteLine("Error sending Animation step! {0}", StringUtils.CreateErrorMessageFromMessageGeneralErrors(new List<GeneralError>() { result }));
                    #endif
                    this.IsSendingMessage = false;
                    contentInfo.IsFailed = true;
                    this.ContentInfoQ.Enqueue(contentInfo);
                    
                    this.SendNextMessage(service);
                    
                } else
                {
                    
                    int stepNumber = contentInfo.ContentState.AnimationIDQ.Dequeue();
                    
                    contentInfo.AnimationItems [stepNumber].IsSent = true;
                    contentInfo.AnimationItems [stepNumber].IsEditing = false;
                    
                    dbm.InsertOrUpdateAnimation(contentInfo.AnimationItems [stepNumber]);
                    
                }//end if else
                
                this.SendNextStep(contentInfo, service);
                
            } else
            {
                
                this.IsSendingMessage = false;
                contentInfo.IsFailed = true;
                this.ContentInfoQ.Enqueue(contentInfo);
                
                this.SendNextMessage(service);
                
            }//end if else
        }
        
        
        

    }
}

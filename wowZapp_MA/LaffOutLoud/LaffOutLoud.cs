using Android.App;
using Android.Content;
using Android.Runtime;

using LOLMessageDelivery;
using LOLApp_Common;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using WZCommon;

#if DEBUG
using System.IO;
#endif

namespace wowZapp
{
    //[Application(Label = "@string/ApplicationName", Theme = "@android:style/Theme.NoTitleBar" )]
    public class LaffOutOut/* : Application*/
    {
        public static LaffOutOut Singleton;
        private Context context;
        //public LaffOutOut(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        public LaffOutOut(Context c)
        {
            context = c;
            AndroidData.SetTeleManager(c);
            LaffOutOut.Singleton = this;
            ContentDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Version = "0.991";
            Published = "5th Feb 2013";

            this.dbm = new DBManager();
            try
            {
                this.dbm.SetupDB();
            } catch (Exception ex)
            {
                #if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Database failed to be created! {0}--{1}", ex.Message, ex.StackTrace);
                #endif
            }//end try catch
            this.ccm = new ContentCacheManager();
            this.mmg = new MessageManager();
            
            if (LaffOutOut.Singleton == null)
                LaffOutOut.Singleton = this;
            
            #if DEBUG
            if (!System.IO.Directory.Exists(Android.OS.Environment.ExternalStorageDirectory + "/wz"))
                System.IO.Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory + "/wz");
            File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "LOL.db"), Android.OS.Environment.ExternalStorageDirectory + "/wz/lol.db", true);
            #endif
        }

        public string ContentDirectory, ImageDirectory, Version, Published;

        public DBManager dbm;
        public ContentCacheManager ccm;
        public MessageManager mmg;

        public EventHandler OnGlobalReceived;
        public event EventHandler<IncomingMessageEventArgs> ReceivedMessages; 
        private volatile bool isMsgInProgress;
        private LOLMessageClient messageService;
        public bool isTablet, resizeFonts;
        public float bigger;
        public double ScreenXWidth, ScreenYHeight;

        /*public override void OnTerminate()
        {
            base.OnTerminate();
            System.Environment.Exit(-1);
        }*/

        /*public override void OnCreate()
        {
            base.OnCreate();

            this.dbm = new DBManager();
            try
            {
                this.dbm.SetupDB();
            } catch (Exception ex)
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Database failed to be created! {0}--{1}", ex.Message, ex.StackTrace);
#endif
            }//end try catch
            this.ccm = new ContentCacheManager();
            this.mmg = new MessageManager();

            if (LaffOutOut.Singleton == null)
                LaffOutOut.Singleton = this;

            #if DEBUG
            if (!System.IO.Directory.Exists(Android.OS.Environment.ExternalStorageDirectory + "/wz"))
                System.IO.Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory + "/wz");
            File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "LOL.db"), Android.OS.Environment.ExternalStorageDirectory + "/wz/lol.db", true);
            #endif
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }*/

        public bool IsAppActive
        {
            get;
            private set;
        }

        public bool MessageTimer
        {
            get;
            private set;
        }

        private System.Timers.Timer timer, locationUpdate;

        public void EnableMessageTimer()
        {
            //if (this.MessageTimer == null)
            //{
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Starting message timer");
#endif
            timer = new System.Timers.Timer();
            timer.Interval = 15000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();
            //}
        }
		
        public void EnableLocationTimer()
        {
            locationUpdate = new System.Timers.Timer();
            locationUpdate.Interval = 300000; // 30 mins
            locationUpdate.Elapsed += new System.Timers.ElapsedEventHandler(locationUpdate_Elapsed);
            locationUpdate.Start();
        }

        public void DisableMessageTimer()
        {
            timer.Stop();
        }

        public void DisableLocationUpdates()
        {
            locationUpdate.Stop();
        }


        public void CheckForUnsentMessages(Context context)
        {
            List<ContentInfo> unsent = new List<ContentInfo>();
            unsent = dbm.GetAllContentInfo();

            foreach (ContentInfo eachContentInfo in unsent)
            {
                mmg.QueueMessage(eachContentInfo, false, context);
            }//end foreach
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
#if(DEBUG)
            System.Diagnostics.Debug.WriteLine("Downloading messages!");
#endif
            // If another call is in progress, skip this one
            if (!this.isMsgInProgress)
            {

                this.isMsgInProgress = true;
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Actually doing something now. this.messageService = {0}", this.messageService);
#endif

                List<Guid> excludeMessageIDs = new List<Guid>();
                excludeMessageIDs = dbm.GetUnreadMessages(false, AndroidData.CurrentUser.AccountID.ToString()).Select(s => s.MessageID).ToList();

                if (null == this.messageService)
                {
                    this.messageService = new LOLMessageClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLMessageEndpoint);
                }//end if
                this.messageService.MessageGetNewCompleted += Service_MessageGetNewCompleted;
                this.messageService.MessageGetNewAsync(AndroidData.CurrentUser.AccountID, AndroidData.NewDeviceID, excludeMessageIDs, 
				                                       new Guid(AndroidData.ServiceAuthToken));
            }
        }

        public void Service_MessageGetNewCompleted(object sender, MessageGetNewCompletedEventArgs e)
        {
            LOLMessageClient service = (LOLMessageClient)sender;
            service.MessageGetNewCompleted -= Service_MessageGetNewCompleted;

            if (null == e.Error)
            {
                Message[] result = e.Result.ToArray();

#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Received {0} new messages!", result.Length);
#endif

                List<Message> msgList = new List<Message>();
                Queue<Guid> userQ = new Queue<Guid>();
                foreach (Message eachMessage in result)
                {
                    if (eachMessage.Errors.Count > 0)
                    {
#if(DEBUG)
                        System.Diagnostics.Debug.WriteLine("Error retrieving new messages: {0}",
                                          StringUtils.CreateErrorMessageFromMessageGeneralErrors(eachMessage.Errors));
#endif
                    } else
                    {
                        msgList.Add(eachMessage);
                        if (!dbm.CheckUserExists(eachMessage.FromAccountID.ToString()))
                        {
                            userQ.Enqueue(eachMessage.FromAccountID);
                        }
                    }
                }
                // Insert the messages to the database

                List<MessageDB> msgListForDB = new List<MessageDB>();
                msgList.ForEach(s => msgListForDB.Add(MessageDB.ConvertFromMessage(s)));
                dbm.InsertOrUpdateMessages(msgListForDB);

                // Trigger the event only if the app is active.
#if DEBUG
                System.Diagnostics.Debug.WriteLine("IsAppActive = {0}, msgList.Count = {1}", AndroidData.IsAppActive, msgList.Count);
#endif
                if (AndroidData.IsAppActive == true && msgList.Count > 0)
                {
                    if (null != this.ReceivedMessages)
                    {
                        this.ReceivedMessages(this, new IncomingMessageEventArgs(msgList));
                    }//end if
                }//end if

                if (userQ.Count > 0)
                {
                    LOLConnectClient clientService = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                    clientService.UserGetSpecificCompleted += Service_UserGetSpecificCompleted;
                    clientService.UserGetSpecificAsync(AndroidData.CurrentUser.AccountID, userQ.Peek(), new Guid(AndroidData.ServiceAuthToken), userQ);
                } else
                    this.isMsgInProgress = false;
            } else
            {
                this.isMsgInProgress = false;
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception receiving message! {0}--{1}",
                                  e.Error.Message, e.Error.StackTrace);
#endif
            }//end if else
        }

        private void Service_UserGetSpecificCompleted(object sender, UserGetSpecificCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("In Service_UserGetSpecificCompleted");
            #endif
            if (null == e.Error)
            {
                LOLAccountManagement.User result = e.Result;
                if (result.Errors.Count > 0)
                {
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine("Error retrieving user: {0}", StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors));
                    #endif
                } else
                {
                    Queue<Guid> userQ = (Queue<Guid>)e.UserState;

                    if (!dbm.CheckUserExists(userQ.Dequeue().ToString()))
                    {
                        // Insert the user to the database
                        dbm.InsertOrUpdateUser(result);
                    }//end if

                    if (userQ.Count > 0)
                    {
                        service.UserGetSpecificAsync(AndroidData.CurrentUser.AccountID, userQ.Peek(), new Guid(AndroidData.ServiceAuthToken), userQ);
                    } else
                    {
                        service.UserGetSpecificCompleted -= Service_UserGetSpecificCompleted;
                        this.isMsgInProgress = false;
                    }
                }
            } else
            {
                this.isMsgInProgress = false;
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception fetching user! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
            }
        }
		
        public void locationUpdate_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (network.checkForNetwork)
            {
                Intent i = new Intent(context, typeof(geolocation));
                context.StartActivity(i);
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                Task.Factory.StartNew(() => {
					
                    Task<LOLAccountManagement.GeneralError> task = 
						service.CallAsyncMethod<LOLAccountManagement.GeneralError, UserUpdateLocationCompletedEventArgs>((s, h, b) => {
							
                        if (b)
                        {
                            s.UserUpdateLocationCompleted += h;
                        } else
                        {
                            s.UserUpdateLocationCompleted -= h;
                        }//end if else
							
                    }, (s) => {
							
                        s.UserUpdateLocationAsync(AndroidData.CurrentUser.AccountID, 
							                           AndroidData.GeoLocation [0], 
							                           AndroidData.GeoLocation [1], 
							                           new Guid(AndroidData.ServiceAuthToken));
							
                    });
					
                    try
                    {	
                        LOLAccountManagement.GeneralError result = task.Result;
                        if (result.ErrorType != LOLCodeLibrary.ErrorsManagement.SystemTypesErrorMessage.NoErrorDetected)
                        {
#if DEBUG
                            Console.WriteLine("Error updating user's location! {0}", result.ErrorType);
#endif
                        }
						
                    } catch (Exception ex)
                    {
#if DEBUG 
                        Console.WriteLine("Exception updating user's location. {0}--{1}", ex.Message, ex.StackTrace);
#endif
                    } finally
                    {
                        service.Close();
                    }
                });
            }
        }
    }
}
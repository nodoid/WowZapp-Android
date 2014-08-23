using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SQLite;
using LOLMessageDelivery;
using LOLAccountManagement;
using System.Drawing;
using LOLCodeLibrary.ErrorsManagement;
using LOLCodeLibrary;
using LOLMessageDelivery.Classes.LOLAnimation;
using WZCommon;

namespace WZCommon
{

    public class DBManager
    {

        #region Constructors

        public DBManager()
        {

            this.dbLock = new object();

            string documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            this.pDBPath = Path.Combine(documents, "LOL.db");
            this.pAnimationDBPath = Path.Combine(documents, "AnimationData.db");

        }

        #endregion Constructors



        #region Fields

        private string pDBPath;
        private string pAnimationDBPath;
        private object dbLock;

        #endregion Fields



        #region Properties

        public string DBPath
        {

            get
            {

                return this.pDBPath;

            }//end get

        }//end string DBPath





        public string AnimationDBPath
        {

            get
            {

                return this.pAnimationDBPath;

            } //end get

        }//end string AnimationDBPath

        #endregion Properties




        #region Public methods

        public bool SetupDB()
        {

            lock (this.dbLock)
            {

                try
                {

                    // LOL db
                    using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                    {

                        sqlCon.CreateTable<UserDB>();
                        sqlCon.CreateTable<ContactDB>();
                        sqlCon.CreateTable<ContactOAuthDB>();
                        sqlCon.CreateTable<ContentPackDB>();
                        sqlCon.CreateTable<ContentPackItemDB>();
                        sqlCon.CreateTable<MessageDB>();
                        sqlCon.CreateTable<MessageRecipientDB>();
                        sqlCon.CreateTable<MessageStepDB>();
                        sqlCon.CreateTable<PollingStepDB>();

                        sqlCon.CreateTable<ContentInfo>();
                        sqlCon.CreateTable<ContentState>();
                        sqlCon.CreateTable<MessageStepDBCache>();
                        sqlCon.CreateTable<MessageRecipientDBCache>();
                        sqlCon.CreateTable<PollingStepDBCache>();
                        sqlCon.CreateTable<VoiceCache>();

                        sqlCon.Execute(WZConstants.DBClauseVacuum);

                    }//end using sqlCon

                    // AnimationData db
                    using (SQLiteConnection sqlCon = new SQLiteConnection(this.AnimationDBPath))
                    {

                        sqlCon.CreateTable<AnimationInfo>();
                        sqlCon.CreateTable<FrameInfo>();
                        sqlCon.CreateTable<LayerInfo>();
                        sqlCon.CreateTable<DrawingInfo>();
                        sqlCon.CreateTable<BrushItem>();
                        sqlCon.CreateTable<TransitionInfo>();
                        sqlCon.CreateTable<TransitionEffectSettings>();
                        sqlCon.CreateTable<PathPointDB>();

                        sqlCon.CreateTable<AnimationAudioInfo>();

                        sqlCon.CreateTable<UndoInfo>();

                        // Create tables here
                        sqlCon.Execute(WZConstants.DBClauseVacuum);

                    }//end using sqlCon

                    return true;

                } catch (SQLiteException ex)
                {
                    throw ex;
                } catch (Exception ex)
                {
                    throw ex;
                } 
                /*finally
                {

                    if (File.Exists(this.DBPath))
                    {
                        // Mark the database to be excluded from iCloud backups.
                        NSError error = NSFileManager.SetSkipBackupAttribute(this.DBPath, true);

                        if (null != error)
                        {
                            Console.WriteLine("Could not mark LOL DB's SkipBackupAttribute: {0}", error.LocalizedDescription);
                            error.Dispose();
                        }//end if

                    }//end if


                    if (File.Exists(this.AnimationDBPath))
                    {

                        // Mark the database to be excluded from iCloud backups.
                        NSError error = NSFileManager.SetSkipBackupAttribute(this.AnimationDBPath, true);

                        if (null != error)
                        {
                            Console.WriteLine("Could not mark AnimationData DB's SkipBackupAttribute: {0}", error.LocalizedDescription);
                            error.Dispose();
                        }//end if

                    }//end if

                }//end try catch finally
                */
            }//end lock
            
        }//end void SetupDB





        public void CleanUpDB()
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        sqlCon.Execute("DELETE FROM UserDB");
                        sqlCon.Execute("DELETE FROM ContactDB");
                        sqlCon.Execute("DELETE FROM ContactOAuthDB");
                        sqlCon.Execute("DELETE FROM ContentPackDB");
                        sqlCon.Execute("DELETE FROM ContentPackItemDB");
                        sqlCon.Execute("DELETE FROM MessageDB");
                        sqlCon.Execute("DELETE FROM MessageStepDB");

                        sqlCon.Commit();

                        sqlCon.Execute(WZConstants.DBClauseVacuum);

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in CleanUpDB! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void CleanUpDB




        #region Messages

        /// <summary>
        /// Gets all the unread messages from the database.
        /// </summary>
        /// <returns>
        /// A List<MessageDB> containing the unread messages.
        /// </returns>
        /// <param name='hasUser'>
        /// If set to true, it will only return messages for which the sender already exists in the database.
        /// </param>
        public List<MessageDB> GetUnreadMessages(bool hasUser, string ownerAccountGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<MessageDB> unreadMessages =
                        hasUser ?
                            sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE MessageGuid IN " +
                        "(SELECT MessageGuid FROM MessageRecipientDB WHERE IsRead=? AND AccountGuid=?) AND " +
                        "FromAccountGuid IN (SELECT AccountGuid FROM UserDB) ORDER BY MessageSent DESC", false, ownerAccountGuid) :
                        sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE MessageGuid IN " +
                        "(SELECT MessageGuid FROM MessageRecipientDB WHERE IsRead=? AND AccountGuid=?) ORDER BY MessageSent DESC", false, ownerAccountGuid);

                    unreadMessages.ForEach(msg =>
                    {

                        msg.MessageStepDBList =
                            sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", msg.MessageGuid);
                        msg.MessageRecipientDBList =
                            sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", msg.MessageGuid);

                    });

                    return unreadMessages;

                }//end using sqlCon

            }//end lock


        }//end List<MessageDB> GetAllMessages




        /// <summary>
        /// Inserts newly received messages to the database.
        /// </summary>
        /// <param name='messageList'>
        /// The newly received messages.
        /// </param>
        public void InsertNewMessages(List<Message> messageList)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();

                    try
                    {

                        Type messageDBType = typeof(MessageDB);
                        foreach (Message eachMessage in messageList)
                        {

                            MessageDB eachMessageDb = MessageDB.ConvertFromMessage(eachMessage);
                            sqlCon.Insert(eachMessageDb, messageDBType);

                            sqlCon.InsertAll(eachMessageDb.MessageStepDBList);
                            sqlCon.InsertAll(eachMessageDb.MessageRecipientDBList);

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {
#if(DEBUG)
                        Console.WriteLine("Error in InsertNewMessages! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();
                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertNewMessages



        public void DeleteMessages(List<MessageDB> messages)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        foreach (MessageDB eachMessageDB in messages)
                        {

                            eachMessageDB.MessageStepDBList.ForEach(step => {

                                sqlCon.Execute("DELETE FROM MessageStepDB WHERE StepID=?", step.StepGuid);

                            });

                            eachMessageDB.MessageRecipientDBList.ForEach(s => {

                                sqlCon.Execute("DELETE FROM MessageRecipientDB WHERE AccountGuid=? AND MessageGuid=?", s.AccountGuid, s.MessageGuid);

                            });

                            sqlCon.Execute("DELETE FROM MessageDB WHERE MessageGuid=?", eachMessageDB.MessageGuid);

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in DeleteMessages! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end using lock

        }//end void DeleteMessages




        public void InsertOrUpdateMessages(List<MessageDB> messageList)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        Type messageDBType = typeof(MessageDB);
                        Type messageStepDBType = typeof(MessageStepDB);
                        Type messageRecipientDBType = typeof(MessageRecipientDB);

                        foreach (MessageDB eachMessage in messageList)
                        {

                            if (sqlCon.Execute("UPDATE MessageDB SET " +
                                "MessageGuid=?, " +
                                "FromAccountGuid=?, " +
                                "IsOutgoing=?, " +
                                "MessageSent=? " +
                                "WHERE MessageGuid=?",
							                   eachMessage.MessageGuid,
							                   eachMessage.FromAccountGuid,
							                   eachMessage.IsOutgoing,
							                   eachMessage.MessageSent,
							                   eachMessage.MessageGuid) == 0)
                            {

                                sqlCon.Insert(eachMessage, messageDBType);

                            }//end if

                            eachMessage.MessageStepDBList.ForEach(step => {

                                if (sqlCon.Execute("UPDATE MessageStepDB SET " +
                                    "MessageGuid=?, " +
                                    "StepGuid=?, " +
                                    "ContentPackItemID=?, " +
                                    "MessageText=?, " +
                                    "StepNumber=?, " +
                                    "StepType=? " +
                                    "WHERE MessageGuid=? AND StepNumber=?",
								                   step.MessageGuid,
								                   step.StepGuid,
								                   step.ContentPackItemID,
								                   step.MessageText,
								                   step.StepNumber,
								                   step.StepType,
								                   step.MessageGuid,
								                   step.StepNumber) == 0)
                                {

                                    sqlCon.Insert(step, messageStepDBType);

                                }//end if

                            });

                            eachMessage.MessageRecipientDBList.ForEach(rcp => {

                                if (sqlCon.Execute("UPDATE MessageRecipientDB SET " +
                                    "AccountGuid=?, " +
                                    "MessageGuid=?, " +
                                    "IsRead=? " +
                                    "WHERE MessageGuid=? AND AccountGuid=?",
								                   rcp.AccountGuid,
								                   rcp.MessageGuid,
								                   rcp.IsRead,
								                   rcp.MessageGuid,
								                   rcp.AccountGuid) == 0)
                                {
                                    sqlCon.Insert(rcp, messageRecipientDBType);
                                }//end if

                            });

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in InsertOrUpdateMessages! {0}--{1}", ex.Message, ex.StackTrace);
#endif

                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertOrUpdateMessages





        public void InsertOutgoingMessage(MessageDB message)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();

                    try
                    {

                        message.IsOutgoing = true;
                        sqlCon.Insert(message, typeof(MessageDB));
                        sqlCon.InsertAll(message.MessageStepDBList);
                        sqlCon.InsertAll(message.MessageRecipientDBList);

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in InsertOutgoingMessage! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertOutgoingMessage





        public bool CheckHasUnreadMessages(string accountGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

//                    TableQuery<MessageRecipientDB> recipientTable = sqlCon.Table<MessageRecipientDB>();
//                    return recipientTable.Count(s => s.AccountGuid == accountGuid &&
//                                                !s.IsRead) > 0;

                    List<MessageRecipientDB> recipients = 
						sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE AccountGuid=? " +
                        "AND IsRead=?", accountGuid, false);

                    return recipients.Count > 0;

                }//end using sqlCon

            }//end lock

        }//end bool CheckHasUnreadMessages




        /// <summary>
        /// Gets the top ten unread messages, provided that the sender data exist in the database.
        /// </summary>
        /// <returns>
        /// The top ten unread messages.
        /// </returns>
        public List<MessageDB> GetTopTenUnreadMessages(string ownerAccountGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<MessageDB> tenUnreadMessages =
                        sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE MessageGuid IN (SELECT MessageGuid FROM MessageRecipientDB WHERE IsRead=? AND AccountGuid=?) AND " +
                        "FromAccountGuid IN (SELECT AccountGuid FROM UserDB) ORDER BY MessageSent DESC LIMIT 10", false, ownerAccountGuid);
                    tenUnreadMessages.ForEach(msg =>
                    {

                        msg.MessageStepDBList =
                            sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", msg.MessageGuid);
                        msg.MessageRecipientDBList =
                            sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", msg.MessageGuid);

                    });

                    return tenUnreadMessages;

                }//end using sqlCon

            }//end lock

        }//end MessageDB GetNewestUnreadMessage




        public void MarkMessageRead(string messageGuid, string ownerAccountID)
        {

#if(DEBUG)
            Console.WriteLine("Will mark message read! {0}", messageGuid);
#endif

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        if (sqlCon.Execute("UPDATE MessageRecipientDB SET IsRead=? WHERE MessageGuid=? AND AccountGuid=?", true, messageGuid, ownerAccountID) == 0)
                        {

                            MessageRecipientDB msgRcp = new MessageRecipientDB()
                            {

                                AccountGuid = ownerAccountID,
                                MessageGuid = messageGuid,
                                IsRead = true

                            };

#if(DEBUG)
                            Console.WriteLine("Created a new message recipient! {0}", msgRcp);
#endif

                            sqlCon.Insert(msgRcp, typeof(MessageRecipientDB));

                        }//end if
                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in MarkMessageRead! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void MarkMessageRead




        public void MarkMessageSent(string messageGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        sqlCon.Execute("UPDATE MessageDB SET MessageConfirmed=? WHERE MessageGuid=?", true, messageGuid);
                        sqlCon.Commit();


                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in MarkMessageSent! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void MarkMessageSent




        public MessageDB GetMessage(string messageGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    MessageDB msg =
                        sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE MessageGuid=?", messageGuid)
                            .FirstOrDefault();

                    if (null != msg)
                    {
                        msg.MessageStepDBList =
                            sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", messageGuid);
                        msg.MessageRecipientDBList =
                            sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", messageGuid);
                    }//end if

                    return msg;

                }//end using sqlCon

            }//end lock

        }//end MessageDB GetMessage




        /// <summary>
        /// Gets all messages a user has sent.
        /// </summary>
        /// <param name='userAccountGuid'>
        /// The user account id for which to get the messages
        /// </param>
        [Obsolete("Use \"GetAllSentMessagesForUserToOwner\" instead.")]
        public List<MessageDB> GetAllMessagesForUser(string userAccountGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

//                    TableQuery<MessageDB> messageTable = sqlCon.Table<MessageDB>();
//
//                    List<MessageDB> otherMessages =
//                        messageTable
//                            .Where(s => s.FromAccountGuid == otherAccountGuid)
//                            .ToList();
                    List<MessageDB> otherMessages = 
						sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE FromAccountGuid=?", userAccountGuid);

                    List<MessageDB> toReturn = new List<MessageDB>();
                    toReturn.AddRange(otherMessages);

                    // Sort the output list
                    toReturn.Sort(delegate(MessageDB x, MessageDB y)
                    {

                        return x.MessageSent.CompareTo(y.MessageSent);

                    });

                    // Add the MessageSteps
                    toReturn.ForEach(msg =>
                    {

                        msg.MessageStepDBList =
                            sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", msg.MessageGuid);
                        msg.MessageRecipientDBList =
                            sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", msg.MessageGuid);

                    });

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end List<MessageDB> GetAllMessagesForUser



        /// <summary>
        /// Gets all sent a user has sent to the owner.
        /// </summary>
        /// <param name='fromAccountGuid'>
        /// The user that has sent the messages.
        /// </param>
        /// <param name='ownerAccountGuid'>
        /// Owner account GUID. Normally, this should be the user that is currently logged in.
        /// </param>
        public List<MessageDB> GetAllSentMessagesForUserToOwner(string fromAccountGuid, string ownerAccountGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<MessageDB> messages = 
						sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE FromAccountGuid=? AND " +
                        "MessageGuid IN (SELECT MessageGuid FROM MessageRecipientDB WHERE AccountGuid=?)",
						                        fromAccountGuid, ownerAccountGuid);

                    messages.Sort(delegate(MessageDB x, MessageDB y)
                    {

                        return x.MessageSent.CompareTo(y.MessageSent);
					
                    });

                    // Add the MessageSteps
                    messages.ForEach(msg =>
                    {

                        msg.MessageStepDBList =
                            sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", msg.MessageGuid);
                        msg.MessageRecipientDBList =
                            sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", msg.MessageGuid);

                    });

                    return messages;

                }//end using sqlCon

            }//end lock

        }//end List<MessageDB> GetAllSentMessagesForUserToOwner



        [Obsolete()]
        public List<MessageDB> GetAllReceivedMessages(string ownerAccountID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<MessageDB> messages =
                        sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE FromAccountGuid<>? ORDER BY MessageSent DESC", ownerAccountID);

                    messages.ForEach(msg =>
                    {

                        msg.MessageStepDBList =
                            sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", msg.MessageGuid);
                        msg.MessageRecipientDBList =
                            sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", msg.MessageGuid);

                    });

                    return messages;

                }//end using sqlCon

            }//end lock 

        }//end List<MessageDB> GetAllReceivedMessages



        [Obsolete("Use \"GetAllMessagesForOwner\" instead.")]
        public List<MessageDB> GetAllMessages()
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<MessageDB> messages = 
						sqlCon.Query<MessageDB>("SELECT * FROM MessageDB ORDER BY MessageSent DESC");

                    messages.ForEach(msg => {

                        msg.MessageStepDBList = 
							sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", msg.MessageGuid);
                        msg.MessageRecipientDBList = 
							sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", msg.MessageGuid);

                    });

                    return messages;

                }//end using sqlCon

            }//end lock

        }//end List<MessageDB> GetAllMessages



        /// <summary>
        /// Gets all messages the owner user has sent or received.
        /// </summary>
        /// <param name='ownerAccountGuid'>
        /// Owner account GUID. Normally, this should be the user that is currently logged in.
        /// </param>
        public List<MessageDB> GetAllMessagesForOwner(string ownerAccountGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<MessageDB> toReturn = 
						sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE FromAccountGuid=?", ownerAccountGuid);
                    List<MessageRecipientDB> ownerRecipient = 
						sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE AccountGuid=?", ownerAccountGuid);

                    ownerRecipient.ForEach(rcp => {

                        toReturn.AddRange(
							sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE MessageGuid=?", rcp.MessageGuid)
                        );

                    });

                    toReturn.ForEach(msg => {

                        msg.MessageRecipientDBList = 
							sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", msg.MessageGuid);
                        msg.MessageStepDBList = 
							sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", msg.MessageGuid);

                    });

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end List<MessageDB> GetAllMessagesForOwner




        public List<MessageDB> GetMessagesByGuid(List<Guid> messageGuidList, out List<Guid> notFound)
        {

            notFound = new List<Guid>();

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<MessageDB> toReturn = new List<MessageDB>();

                    foreach (Guid eachGuid in messageGuidList)
                    {

                        string msgIDStr = eachGuid.ToString();

                        MessageDB msg = 
							sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE MessageGuid=?", msgIDStr)
								.FirstOrDefault();

                        if (null != msg)
                        {

                            toReturn.Add(msg);

                            msg.MessageRecipientDBList = 
								sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", msgIDStr);

                            msg.MessageStepDBList = 
								sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", msgIDStr);

                        } else
                        {

                            notFound.Add(eachGuid);

                        }//end if else

                    }//end foreach

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end List<MessageDB> GetMessagesByGuid






        /// <summary>
        /// Gets the last message date time for user.
        /// </summary>
        /// <returns>
        /// The last message date time for user, or null.
        /// </returns>
        /// <param name='accountID'>
        /// The account ID for the user whose messages to check.
        /// </param>
        /// <param name='convertToLocalDateTime'>
        /// Set to true to convert the DateTime to local.
        /// </param>
        public DateTime? GetLastMessageDateTimeForUser(string accountID, bool convertToLocalDateTime)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    TableQuery<MessageDB> messageTable = sqlCon.Table<MessageDB>();
                    MessageDB latestMessage = messageTable
                        .MaxItem<MessageDB, DateTime>(s => s.MessageSent);

                    if (null == latestMessage)
                    {
                        return null;
                    } else
                    {
                        return convertToLocalDateTime ?
                            latestMessage.MessageSent.ToLocalTime() :
                                latestMessage.MessageSent;

                    }//end if else

                }//end using sqlCon

            }//end lock

        }//end DateTime? GetLastMessageDateTimeForUser




        public MessageDB GetLatestSentMessage(string fromAccountGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    TableQuery<MessageDB> messageQuery = 
						sqlCon.Table<MessageDB>();

                    MessageDB toReturn = 
						messageQuery
							.Where(s => s.FromAccountGuid == fromAccountGuid)
							.MaxItem<MessageDB, DateTime>(s => s.MessageSent);

                    if (null != toReturn)
                    {

                        toReturn.MessageStepDBList = 
							sqlCon.Query<MessageStepDB>("SELECT * FROM MessageStepDB WHERE MessageGuid=?", toReturn.MessageGuid);
                        toReturn.MessageRecipientDBList = 
							sqlCon.Query<MessageRecipientDB>("SELECT * FROM MessageRecipientDB WHERE MessageGuid=?", toReturn.MessageGuid);

                    }//end if

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end MessageDB GetLatestSentMessage





        public Queue<Guid> GetNotExistingMessageIDs(List<Guid> msgIDList)
        {
			
            lock (this.dbLock)
            {
				
                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {
					
                    sqlCon.Execute(WZConstants.DBClauseSyncOff);
					
                    Queue<Guid> toReturn = new Queue<Guid>();
                    foreach (Guid eachMsgID in msgIDList)
                    {
						
                        MessageDB msgDB =
							sqlCon.Query<MessageDB>("SELECT MessageGuid FROM MessageDB WHERE MessageGuid=?", eachMsgID.ToString())
								.FirstOrDefault();
						
                        if (null == msgDB)
                        {

                            toReturn.Enqueue(eachMsgID);

                        }//end if
						
                    }//end foreach
					
                    return toReturn;
					
                }//end using sqlCon
				
            }//end lock
			
        }//end Queue<Guid> GetExistingUserIDs




        #endregion Messages
        //
        //
        //
        //
        #region Users & Contacts

        public bool CheckUserExists(string userAccountID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<UserDB> userList = 
						sqlCon.Query<UserDB>("SELECT * FROM UserDB WHERE AccountGuid=?", userAccountID);

                    return userList.Count == 1;

                }//end using sqlCon

            }//end lock

        }//end bool CheckUserExists



        public Queue<Guid> GetNotExistingUserIDs(List<Guid> userIDList)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    Queue<Guid> toReturn = new Queue<Guid>();
                    foreach (Guid eachUserID in userIDList)
                    {

                        UserDB userDB =
							sqlCon.Query<UserDB>("SELECT AccountGuid FROM UserDB WHERE AccountGuid=?", eachUserID.ToString())
								.FirstOrDefault();

                        if (null == userDB)
                        {
                            toReturn.Enqueue(eachUserID);
                        }//end if

                    }//end foreach

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end Queue<Guid> GetExistingUserIDs



        [Obsolete("Use \"CheckContactExistsForOwner\" instead.")]
        public bool CheckContactExists(string contactAccountID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<ContactDB> contacts = 
						sqlCon.Query<ContactDB>("SELECT * FROM ContactDB WHERE ContactAccountGuid=?", contactAccountID);

                    return contacts.Count == 1;

                }//end using sqlCon

            }//end using lock

        }//end bool CheckContactExists




        /// <summary>
        /// Checks if a contact exists for the owner user.
        /// </summary>
        /// <param name='contactAccountID'>
        /// The contact's account ID to check.
        /// </param>
        /// <param name='ownerAccountID'>
        /// Owner account ID. Normally, this should be the logged in user.
        /// </param>
        public bool CheckContactExistsForOwner(string contactAccountID, string ownerAccountID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<ContactDB> contacts = 
						sqlCon.Query<ContactDB>("SELECT * FROM ContactDB WHERE ContactAccountGuid=? AND " +
                        "OwnerAccountGuid=?", contactAccountID, ownerAccountID);

                    return contacts.Count == 1;

                }//end using sqlCon

            }//end lock

        }//end bool CheckContactExistsForOwner




        public void InsertOrUpdateUser(User user)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        UserDB userDB = UserDB.ConvertFromUser(user);
                        if (sqlCon.Execute("UPDATE UserDB SET " +
                            "AccountActive=?, " +
                            "AccountGuid=?, " +
                            "DateOfBirth=?, " +
                            "EmailAddress=?, " +
                            "FirstName=?, " +
                            "LastName=?, " +
                            "Password=?, " +
                            "Picture=?, " +
                            "PictureUrl=?, " +
                            "HasProfileImage=? " +
                            "WHERE AccountGuid=?",
                                               userDB.AccountActive,
                                               userDB.AccountGuid,
                                               userDB.DateOfBirth,
                                               userDB.EmailAddress,
                                               userDB.FirstName,
                                               userDB.LastName,
                                               userDB.Password,
                                               userDB.Picture,
                                               userDB.PictureURL,
							                   userDB.HasProfileImage,
                                               userDB.AccountGuid) == 0)
                        {

                            sqlCon.Insert(userDB, typeof(UserDB));
                        }//end if

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in InsertOrUpdateContact! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertOrUpdateUser



        public UserDB GetUserWithAccountID(string accountID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    return sqlCon.Query<UserDB>("SELECT * FROM UserDB WHERE AccountGuid=?", accountID)
                        .FirstOrDefault();

                }//end using sqlCon

            }//end lock

        }//end UserDB GetUserWithAccountID



        public void InserOrUpdateContacts(List<Contact> contacts)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);
                    sqlCon.BeginTransaction();

                    try
                    {

                        Type contactDBType = typeof(ContactDB);
                        Type authDBType = typeof(ContactOAuthDB);
                        Type userDBType = typeof(UserDB);
                        foreach (Contact eachContact in contacts)
                        {

                            ContactDB contactDB = ContactDB.ConvertFromContact(eachContact);

                            // Insert or update ContactDB
                            if (sqlCon.Execute("UPDATE ContactDB SET " +
                                "Blocked=?, " +
                                "ContactAccountGuid=?, " +
                                "ContactGuid=?, " +
                                "OwnerAccountGuid=? " +
                                "WHERE ContactGuid=?",
                                               contactDB.Blocked,
                                               contactDB.ContactAccountGuid,
                                               contactDB.ContactGuid,
                                               contactDB.OwnerAccountGuid,
                                               contactDB.ContactGuid) == 0)
                            {
                                sqlCon.Insert(contactDB, contactDBType);
                            }//end if

                            // Insert or update ContactOAuthItems
                            if (contactDB.ContactOAuthItems.Count > 0)
                            {

                                foreach (ContactOAuthDB eachAuthDB in contactDB.ContactOAuthItems)
                                {

                                    if (sqlCon.Execute("UPDATE ContactOAuthDB SET " +
                                        "ContactGuid=?, " +
                                        "OAuthID=?, " +
                                        "OAuthType=? " +
                                        "WHERE ContactGuid=?",
                                                       contactDB.ContactGuid,
                                                           eachAuthDB.OAuthID,
                                                           eachAuthDB.OAuthType,
                                                           contactDB.ContactGuid) == 0)
                                    {
                                        sqlCon.Insert(eachAuthDB, authDBType);
                                    }//end if

                                }//end foreach

                            }//end if

                            // Insert or update ContactUser
                            UserDB contactUser = UserDB.ConvertFromUser(contactDB.ContactUser);

                            if (sqlCon.Execute("UPDATE UserDB SET " +
                                "AccountActive=?, " +
                                "AccountGuid=?, " +
                                "DateOfBirth=?, " +
                                "EmailAddress=?, " +
                                "FirstName=?, " +
                                "LastName=?, " +
                                "Password=?, " +
                                "Picture=?, " +
                                "PictureUrl=?, " +
                                "HasProfileImage=? " +
                                "WHERE AccountGuid=?",
                                               contactUser.AccountActive,
                                               contactUser.AccountGuid,
                                               contactUser.DateOfBirth,
                                               contactUser.EmailAddress,
                                               contactUser.FirstName,
                                               contactUser.LastName,
                                               contactUser.Password,
                                               contactUser.Picture,
                                               contactUser.PictureURL,
							                   contactUser.HasProfileImage,
                                               contactUser.AccountGuid) == 0)
                            {

                                sqlCon.Insert(contactUser, userDBType);
                            }//end if

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in InsertOrUpdateContacts! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch


                }//end using sqlCon

            }//end lock

        }//end void InsertOrUpdateContacts



        [Obsolete("Use DeleteContactForOwner instead.")]
        public void DeleteContact(ContactDB contact)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        // Delete OAuths for the contact
                        sqlCon.Execute("DELETE FROM ContactOAuthDB WHERE ContactGuid=?", contact.ContactGuid);

                        // Delete the Contact object
                        sqlCon.Execute("DELETE FROM ContactDB WHERE ContactGuid=?", contact.ContactGuid);

                        // Delete the User object
                        sqlCon.Execute("DELETE FROM UserDB WHERE AccountGuid=?", contact.ContactUser.AccountID.ToString());

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in DeleteContact! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void DeleteContact




        /// <summary>
        /// Deletes the contact that belongs to the owner.
        /// </summary>
        /// <param name='contact'>
        /// The contact to delete.
        /// </param>
        public void DeleteContactForOwner(ContactDB contact)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        // Delete OAuths for the contact
                        sqlCon.Execute("DELETE FROM ContactOAuthDB WHERE ContactGuid=? AND " +
                            "ContactGuid IN (SELECT ContactGuid FROM ContactDB WHERE OwnerAccountGuid=?)",
						               contact.ContactGuid, contact.OwnerAccountGuid);

                        // Delete the Contact object
                        sqlCon.Execute("DELETE FROM ContactDB WHERE ContactGuid=? AND OwnerAccountGuid=?",
						               contact.ContactGuid, contact.OwnerAccountGuid);

                        //NOTE: No need to delete the user object for the contact (maybe).
                        // Delete the User object
//                        sqlCon.Execute("DELETE FROM UserDB WHERE AccountGuid=?", contact.ContactUser.AccountID.ToString());

                        sqlCon.Commit();


                    } catch (Exception ex)
                    {

                        sqlCon.Rollback();

#if(DEBUG)
                        Console.WriteLine("Error in DeleteContactForOwner! {0}--{1}", ex.Message, ex.StackTrace);
#endif

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void DeleteContactForOwner





        public void DeleteContacts(List<Guid> contactIDs)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        foreach (Guid eachContactID in contactIDs)
                        {

                            string eachContactGuid = eachContactID.ToString();
                            // Delete ContactOAuthDB items
                            sqlCon.Execute("DELETE FROM ContactOAuthDB WHERE ContactGuid=?", eachContactGuid);
                            // Delete contact's user object
                            sqlCon.Execute("DELETE FROM UserDB WHERE AccountGuid IN (SELECT ContactAccountGuid FROM ContactDB WHERE ContactGuid=?)", eachContactGuid);
                            // Delete Contact object
                            sqlCon.Execute("DELETE FROM ContactDB WHERE ContactGuid=?", eachContactGuid);

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in DeleteContacts! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void DeleteContacts




        [Obsolete("Use GetAllContactsForOwner instead.")]
        public List<ContactDB> GetAllContacts()
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<ContactDB> toReturn =
                        sqlCon.Query<ContactDB>("SELECT * FROM ContactDB");

                    toReturn.ForEach(contact =>
                    {

                        contact.ContactOAuthItems =
                            sqlCon.Query<ContactOAuthDB>("SELECT * FROM ContactOAuthDB WHERE ContactGuid=?", contact.ContactGuid);
                        contact.ContactUser =
                            UserDB.ConvertFromUserDB(sqlCon.Query<UserDB>("SELECT * FROM UserDB WHERE AccountGuid=?", contact.ContactAccountGuid)
                                                     .First());

                    });

                    return toReturn;

                }//end sqlCon

            }//end lock

        }//end List<ContactDB> GetAllContacts





        public List<ContactDB> GetAllContactsForOwner(string ownerAccountGuid)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<UserDB> contactUsers = 
						sqlCon.Query<UserDB>("SELECT * FROM UserDB WHERE AccountGuid IN (SELECT ContactAccountGuid FROM ContactDB WHERE OwnerAccountGuid=?) " +
                        "ORDER BY LastName, FirstName", ownerAccountGuid);

                    List<ContactDB> contacts = new List<ContactDB>();
                    foreach (UserDB eachUser in contactUsers)
                    {

                        ContactDB contact = 
							sqlCon.Query<ContactDB>("SELECT * FROM ContactDB WHERE ContactAccountGuid=?", eachUser.AccountGuid).FirstOrDefault();

                        contact.ContactUser = UserDB.ConvertFromUserDB(eachUser);
                        contact.ContactOAuthItems = 
							sqlCon.Query<ContactOAuthDB>("SELECT * FROM ContactOAuthDB WHERE ContactGuid=?", contact.ContactGuid);

                        contacts.Add(contact);

                    }//end foreach

                    return contacts;

                }//end using sqlCon
            }//end lock

        }//end List<ContactDB> GetAllContactsForOwner



        /// <summary>
        /// Deletes all contacts in the database and replaces them with the provided list.
        /// </summary>
        /// <param name='withContacts'>
        /// The list of contacts to replace the existing one.
        /// </param>
        [Obsolete("Use RefreshContactsForOwner instead.")]
        public void RefreshContacts(List<Contact> withContacts)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        // Delete all contact oauth items
                        sqlCon.Execute("DELETE FROM ContactOAuthDB");

                        // Delete all contact items
                        sqlCon.Execute("DELETE FROM ContactDB");

                        // Insert contacts
                        Type contactDBType = typeof(ContactDB);
                        Type contactOAuthDBType = typeof(ContactOAuthDB);
                        Type userDBType = typeof(UserDB);

                        foreach (Contact eachContact in withContacts)
                        {
                            ContactDB contactDB = ContactDB.ConvertFromContact(eachContact);
                            sqlCon.Insert(contactDB, contactDBType);
                            // Insert the contact auth db item
                            contactDB.ContactOAuthItems.ForEach(s => {

                                sqlCon.Insert(s, contactOAuthDBType);

                            });

                            // Insert Or Update user object
                            UserDB contactUser = UserDB.ConvertFromUser(contactDB.ContactUser);

                            if (sqlCon.Execute("UPDATE UserDB SET " +
                                "AccountActive=?, " +
                                "AccountGuid=?, " +
                                "DateOfBirth=?, " +
                                "EmailAddress=?, " +
                                "FirstName=?, " +
                                "LastName=?, " +
                                "Password=?, " +
                                "Picture=?, " +
                                "PictureUrl=?, " +
                                "HasProfileImage=? " +
                                "WHERE AccountGuid=?",
                                               contactUser.AccountActive,
                                               contactUser.AccountGuid,
                                               contactUser.DateOfBirth,
                                               contactUser.EmailAddress,
                                               contactUser.FirstName,
                                               contactUser.LastName,
                                               contactUser.Password,
                                               contactUser.Picture,
                                               contactUser.PictureURL,
							                   contactUser.HasProfileImage,
                                               contactUser.AccountGuid) == 0)
                            {

                                sqlCon.Insert(contactUser, userDBType);
                            }//end if

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error refreshing contacts in database! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void RefreshContacts




        /// <summary>
        /// Deletes all contacts that belong to the owner and replaces them with the supplied contact list.
        /// </summary>
        /// <param name='withContacts'>
        /// Contacts to replace the deleted ones with.
        /// </param>
        /// <param name='ownerAccountID'>
        /// Owner account GUID. Normally, this should be the currently logged in user.
        /// </param>
        public void RefreshContactsForOwner(List<Contact> withContacts, string ownerAccountID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        // Delete all ContactOAuth items for owner
                        sqlCon.Execute("DELETE FROM ContactOAuthDB WHERE ContactGuid IN " +
                            "(SELECT ContactGuid FROM ContactDB WHERE OwnerAccountGuid=?)", ownerAccountID);

                        // Delete all Contacts for owner
                        sqlCon.Execute("DELETE FROM ContactDB WHERE OwnerAccountGuid=?", ownerAccountID);

                        // Insert contacts
                        Type contactDBType = typeof(ContactDB);
                        Type contactOAuthDBType = typeof(ContactOAuthDB);
                        Type userDBType = typeof(UserDB);

                        foreach (Contact eachContact in withContacts)
                        {
                            ContactDB contactDB = ContactDB.ConvertFromContact(eachContact);
                            sqlCon.Insert(contactDB, contactDBType);
                            // Insert the contact auth db item
                            contactDB.ContactOAuthItems.ForEach(s => {

                                sqlCon.Insert(s, contactOAuthDBType);

                            });

                            // Insert Or Update user object
                            UserDB contactUser = UserDB.ConvertFromUser(contactDB.ContactUser);

                            if (sqlCon.Execute("UPDATE UserDB SET " +
                                "AccountActive=?, " +
                                "AccountGuid=?, " +
                                "DateOfBirth=?, " +
                                "EmailAddress=?, " +
                                "FirstName=?, " +
                                "LastName=?, " +
                                "Password=?, " +
                                "Picture=?, " +
                                "PictureUrl=?, " +
                                "HasProfileImage=? " +
                                "WHERE AccountGuid=?",
                                               contactUser.AccountActive,
                                               contactUser.AccountGuid,
                                               contactUser.DateOfBirth,
                                               contactUser.EmailAddress,
                                               contactUser.FirstName,
                                               contactUser.LastName,
                                               contactUser.Password,
                                               contactUser.Picture,
                                               contactUser.PictureURL,
							                   contactUser.HasProfileImage,
                                               contactUser.AccountGuid) == 0)
                            {

                                sqlCon.Insert(contactUser, userDBType);
                            }//end if

                        }//end foreach

                        sqlCon.Commit();


                    } catch (Exception ex)
                    {

                        sqlCon.Rollback();

#if(DEBUG)
                        Console.WriteLine("Error in RefreshContactsForOwner! {0}--{1}", ex.Message, ex.StackTrace);
#endif

                    }//end try catch


                }//end using sqlCon

            }//end lock

        }//end void RefreshContactsForOwner





        public void UpdateUserImage(string accountGuid, byte[] imageBuffer)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        sqlCon.Execute("UPDATE UserDB SET Picture=? WHERE AccountGuid=?", imageBuffer, accountGuid);

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

                        Console.WriteLine("Error in UpdateUserImage! {0}--{1}", ex.Message, ex.StackTrace);

                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void UpdateUserImage


        #endregion Users & Contacts
        //
        //
        //
        //
        #region Content

        public void InsertOrUpdateContentPacks(List<ContentPackDB> contentPacks)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        Type contentPackDBType = typeof(ContentPackDB);
                        foreach (ContentPackDB eachContentPack in contentPacks)
                        {

                            if (sqlCon.Execute("UPDATE ContentPackDB SET " +
                                "ContentPackAdFile=?, " +
                                "ContentPackAvailableDate=?, " +
                                "ContentPackDescription=?, " +
                                "ContentPackEndDate=?, " +
                                "ContentPackIconFile=?, " +
                                "ContentPackID=?, " +
                                "ContentPackIsFree=?, " +
                                "ContentPackPrice=?, " +
                                "ContentPackSaleEndDate=?, " +
                                "ContentPackSalePrice=?, " +
                                "ContentPackTitle=?, " +
                                "ContentPackTypeID=? " +
                                "WHERE ContentPackID=?",
                                               eachContentPack.ContentPackAdFile,
                                               eachContentPack.ContentPackAvailableDate,
                                               eachContentPack.ContentPackDescription,
                                               eachContentPack.ContentPackEndDate,
                                               eachContentPack.ContentPackIconFile,
                                               eachContentPack.ContentPackID,
                                               eachContentPack.ContentPackIsFree,
                                               eachContentPack.ContentPackPrice,
                                               eachContentPack.ContentPackSaleEndDate,
                                               eachContentPack.ContentPackSalePrice,
                                               eachContentPack.ContentPackTitle,
                                               eachContentPack.ContentPackTypeID,
                                               eachContentPack.ContentPackID) == 0)
                            {

                                sqlCon.Insert(eachContentPack, contentPackDBType);
                            }//end if

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in InsertNewContentPacks! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertNewContentPacks





        public void InsertOrUpdateContentPackItems(List<ContentPackItemDB> contentPackItems)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        Type contentPackItemType = typeof(ContentPackItemDB);

                        foreach (ContentPackItemDB eachContentPack in contentPackItems)
                        {

                            if (sqlCon.Execute("UPDATE ContentPackItemDB SET " +
                                "ContentItemTitle=?, " +
                                "ContentPackDataFile=?, " +
                                "ContentPackID=?, " +
                                "ContentPackItemIconFile=?, " +
                                "ContentPackItemID=?, " +
                                "LastDateTimeUsed=? " +
                                "WHERE ContentPackItemID=?",
                                               eachContentPack.ContentItemTitle,
                                               eachContentPack.ContentPackDataFile,
                                               eachContentPack.ContentPackID,
                                               eachContentPack.ContentPackItemIconFile,
                                               eachContentPack.ContentPackItemID,
							                   eachContentPack.LastDateTimeUsed,
                                               eachContentPack.ContentPackItemID) == 0)
                            {

                                sqlCon.Insert(eachContentPack, contentPackItemType);

                            }//end foreach

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in InsertNewContentPackItems! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertNewContentPackItems





        public ContentPackDB GetContentPack(int contentPackID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    return sqlCon.Query<ContentPackDB>("SELECT * FROM ContentPackDB WHERE ContentPackID=?", contentPackID)
						.FirstOrDefault();

                }//end using sqlCon

            }//end lock

        }//end ContentPackDB GetContentPack




        public ContentPackItemDB GetContentPackItem(int contentPackItemID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    return sqlCon.Query<ContentPackItemDB>("SELECT * FROM ContentPackItemDB WHERE ContentPackItemID=?", contentPackItemID)
						.FirstOrDefault();

                }//end using sqlCon

            }//end lock

        }//end ContentPackItemDB GetContentPackItem




        public List<ContentPackDB> GetAllContentPacks(GenericEnumsContentPackType packType)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    return sqlCon.Query<ContentPackDB>("SELECT * FROM ContentPackDB WHERE ContentPackTypeID=?", packType);

                }//end using sqlCon

            }//end lock

        }//end List<ContentPackDB> GetAllContentPacks




        public List<ContentPackItemDB> GetAllContentPackItems(int forContentPackID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    return sqlCon.Query<ContentPackItemDB>("SELECT * FROM ContentPackItemDB WHERE ContentPackID=?", forContentPackID);

                }//end sqlCon

            }//end lock

        }//end List<ContentPackItemDB> GetAllContentPackItems



        public List<ContentPackItemDB> GetContentPackItems(List<int> contentPackItemIDs)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<ContentPackItemDB> toReturn = new List<ContentPackItemDB>();
                    foreach (int contentPackItemID in contentPackItemIDs)
                    {

                        toReturn.AddRange(sqlCon.Query<ContentPackItemDB>("SELECT * FROM ContentPackItemDB WHERE ContentPackItemID=?", contentPackItemID));

                    }//end foreach

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end List<ContentPackItemDB> GetContentPackItems





        public void UpdateContentPackItemUsageDate(int contentPackItemID, DateTime dateTime)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        sqlCon.Execute("UPDATE ContentPackItemDB SET LastDateTimeUsed=? WHERE ContentPackItemID=?", dateTime, contentPackItemID);

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

                        sqlCon.Rollback();

#if(DEBUG)
                        Console.WriteLine("Error updating content pack item date! {0}--{1}", ex.Message, ex.StackTrace);
#endif

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void UpdateContentPackItemUsageDate





        public List<ContentPackItemDB> GetContentPackItemsOlderThan(DateTime ago)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<ContentPackItemDB> toReturn = 
						sqlCon.Query<ContentPackItemDB>("SELECT * FROM ContentPackItemDB WHERE LastDateTimeUsed < ?", ago);

                    return toReturn;

                }//end using


            }//end lock

        }//end List<ContentPackItemDB> GetContentPackItemsOlderThan





        public void DeleteContentPackItems(List<ContentPackItemDB> items)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        foreach (ContentPackItemDB eachPackItem in items)
                        {
                            sqlCon.Execute("DELETE FROM ContentPackItemDB WHERE ID=?", eachPackItem.ID);
                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

                        sqlCon.Rollback();
#if(DEBUG)
                        Console.WriteLine("Error deleting content pack item! {0}--{1}", ex.Message, ex.StackTrace);
#endif

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void DeleteContentPackItems






        public void InsertNewPollingSteps(List<PollingStepDB> pollingSteps)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        sqlCon.InsertAll(pollingSteps);

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in InsertNewPollingSteps! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertNewPollingSteps



        public void InsertOrUpdatePollingSteps(List<PollingStepDB> pollingSteps)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        Type pollingStepDBType = typeof(PollingStepDB);
                        foreach (PollingStepDB eachItem in pollingSteps)
                        {

                            if (sqlCon.Execute("UPDATE PollingStepDB SET " +
                                "MessageGuid=?, " +
                                "PollingAnswer1=?, " +
                                "PollingAnswer2=?, " +
                                "PollingAnswer3=?, " +
                                "PollingAnswer4=?, " +
                                "PollingData1File=?, " +
                                "PollingData2File=?, " +
                                "PollingData3File=?, " +
                                "PollingData4File=?, " +
                                "PollingQuestion=?, " +
                                "StepNumber=? " +
                                "WHERE MessageGuid=? AND StepNumber=?",
							                   eachItem.MessageGuid,
							                   eachItem.PollingAnswer1,
							                   eachItem.PollingAnswer2,
							                   eachItem.PollingAnswer3,
							                   eachItem.PollingAnswer4,
							                   eachItem.PollingData1File,
							                   eachItem.PollingData2File,
							                   eachItem.PollingData3File,
							                   eachItem.PollingData4File,
							                   eachItem.PollingQuestion,
							                   eachItem.StepNumber,
							                   eachItem.MessageGuid,
							                   eachItem.StepNumber) == 0)
                            {
                                sqlCon.Insert(eachItem, pollingStepDBType);
                            }//end if

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error in InsertOrUpdatePollingSteps! {0}--{1}", ex.Message, ex.StackTrace);
#endif

                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertOrUpdatePollingSteps




        public List<PollingStepDB> GetAllPollingSteps()
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    return sqlCon.Query<PollingStepDB>("SELECT * FROM PollingStepDB");

                }//end using sqlCon

            }//end lock

        }//end List<PollingStepDB> GetAllPollingSteps




        public List<PollingStepDB> GetPollingSteps(List<string> messageGuids)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<PollingStepDB> toReturn = new List<PollingStepDB>();

                    foreach (string eachMessageGuid in messageGuids)
                    {

                        toReturn.AddRange(sqlCon.Query<PollingStepDB>("SELECT * FROM PollingStepDB WHERE MessageGuid=?", eachMessageGuid));

                    }//end foreach

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end List<PollingStepDB> GetPollingSteps




        public PollingStepDB GetPollingStep(string messageID, int stepNumber)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    return sqlCon.Query<PollingStepDB>("SELECT * FROM PollingStepDB WHERE MessageGuid=? AND StepNumber=?", messageID, stepNumber)
						.FirstOrDefault();

                }//end using sqlCon

            }//end lock 

        }//end PollingStepDB GetPollingStep




        public void SetPollingStepHasResponded(string messageID, int stepNumber)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.Execute("UPDATE PollingStepDB SET HasResponded=? WHERE MessageGuid=? AND StepNumber=?", true, messageID, stepNumber);

                }//end using sqlCon

            }//end lock

        }//end





        public void DeletePollingStep(PollingStepDB pollingStep)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        sqlCon.Execute("DELETE FROM PollingStepDB WHERE MessageGuid=? AND StepNumber=?", pollingStep.MessageGuid, pollingStep.StepNumber);
                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

                        Console.WriteLine("Error in DeletePollingStep! {0}--{1}", ex.Message, ex.StackTrace);
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void DeletePollingStep



        public void InsertOrUpdateContentInfoItems(List<ContentInfo> contentInfoItems)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        Type messageDBType = typeof(MessageDB);
                        Type contentInfoType = typeof(ContentInfo);
                        Type voiceCacheType = typeof(VoiceCache);
                        Type pollingStepDBCacheType = typeof(PollingStepDBCache);
                        Type messageStepDBCacheType = typeof(MessageStepDBCache);
                        Type messageRecipientDBCacheType = typeof(MessageRecipientDBCache);
                        Type contentStateType = typeof(ContentState);

                        foreach (ContentInfo eachContentInfo in contentInfoItems)
                        {

                            if (sqlCon.Execute("UPDATE MessageDB SET " +
                                "MessageGuid=?, " +
                                "FromAccountGuid=?, " +
                                "IsOutgoing=?, " +
                                "MessageSent=? " +
                                "WHERE ID=?",
							                   eachContentInfo.Message.MessageID.ToString(),
							                   eachContentInfo.Message.FromAccountID.ToString(),
							                   true,
							                   eachContentInfo.Message.MessageSent,
							                   eachContentInfo.MessageDBID) == 0)
                            {

                                MessageDB msgDB = MessageDB.ConvertFromMessage(eachContentInfo.Message);
                                sqlCon.Insert(msgDB, messageDBType);
                                eachContentInfo.MessageDBID = msgDB.ID;

                            }//end if



                            if (sqlCon.Execute("UPDATE ContentInfo SET " +
                                "MessageDBID=?, " +
                                "IsFailed=?, " +
                                "IsMessageCreateFailed=? " +
                                "WHERE ID=?",
							                   eachContentInfo.MessageDBID,
							                   eachContentInfo.IsFailed,
							                   eachContentInfo.IsMessageCreateFailed,
							                   eachContentInfo.ID) == 0)
                            {

                                sqlCon.Insert(eachContentInfo, contentInfoType);

                            }//end if



                            foreach (KeyValuePair<int, byte[]> eachVoiceCache in eachContentInfo.VoiceRecordings)
                            {

                                bool isSent = eachContentInfo.ContentState != null ? 
									!eachContentInfo.ContentState.VoiceIDQ.Contains(eachVoiceCache.Key) : 
										false;

                                if (sqlCon.Execute("UPDATE VoiceCache SET " +
                                    "ContentInfoID=?, " +
                                    "StepNumber=?, " +
                                    "VoiceData=?, " +
                                    "IsSent=? " +
                                    "WHERE ContentInfoID=? AND StepNumber=?",
								                   eachContentInfo.ID,
								                   eachVoiceCache.Key,
								                   eachVoiceCache.Value,
								                   isSent,
								                   eachContentInfo.ID,
								                   eachVoiceCache.Key) == 0)
                                {

                                    VoiceCache voiceCache = new VoiceCache();
                                    voiceCache.ContentInfoID = eachContentInfo.ID;
                                    voiceCache.StepNumber = eachVoiceCache.Key;
                                    voiceCache.VoiceData = eachVoiceCache.Value;
                                    voiceCache.IsSent = isSent;

                                    sqlCon.Insert(voiceCache, voiceCacheType);

                                }//end if

                            }//end foreach



                            foreach (KeyValuePair<int, PollingStep> eachPollingStep in eachContentInfo.PollingSteps)
                            {

                                bool isSent = eachContentInfo.ContentState != null ?
									!eachContentInfo.ContentState.PollingIDQ.Contains(eachPollingStep.Key) :
										false;

                                if (sqlCon.Execute("UPDATE PollingStepDBCache SET " +
                                    "ContentInfoID=?, " +
                                    "MessageGuid=?, " +
                                    "PollingData1=?, " +
                                    "PollingData2=?, " +
                                    "PollingData3=?, " +
                                    "PollingData4=?, " +
                                    "HasResponded=?, " +
                                    "PollingAnswer1=?, " +
                                    "PollingAnswer2=?, " +
                                    "PollingAnswer3=?, " +
                                    "PollingAnswer4=?, " +
                                    "PollingQuestion=?, " +
                                    "StepNumber=?, " +
                                    "IsSent=? " +
                                    "WHERE ContentInfoID=? AND StepNumber=?",
								                   eachContentInfo.ID,
								                   eachContentInfo.Message.MessageID.ToString(),
								                   eachPollingStep.Value.PollingData1,
								                   eachPollingStep.Value.PollingData2,
								                   eachPollingStep.Value.PollingData3,
								                   eachPollingStep.Value.PollingData4,
								                   eachPollingStep.Value.HasResponded,
								                   eachPollingStep.Value.PollingAnswer1,
								                   eachPollingStep.Value.PollingAnswer2,
								                   eachPollingStep.Value.PollingAnswer3,
								                   eachPollingStep.Value.PollingAnswer4,
								                   eachPollingStep.Value.PollingQuestion,
								                   eachPollingStep.Key,
								                   isSent,
								                   eachContentInfo.ID, eachPollingStep.Key) == 0)
                                {


                                    PollingStepDBCache pollStepCache = new PollingStepDBCache();
                                    pollStepCache.ContentInfoID = eachContentInfo.ID;
                                    pollStepCache.HasResponded = eachPollingStep.Value.HasResponded;
                                    pollStepCache.IsSent = isSent;
                                    pollStepCache.MessageID = eachContentInfo.Message.MessageID;
                                    pollStepCache.PollingAnswer1 = eachPollingStep.Value.PollingAnswer1;
                                    pollStepCache.PollingAnswer2 = eachPollingStep.Value.PollingAnswer2;
                                    pollStepCache.PollingAnswer3 = eachPollingStep.Value.PollingAnswer3;
                                    pollStepCache.PollingAnswer4 = eachPollingStep.Value.PollingAnswer4;
                                    pollStepCache.PollingData1 = eachPollingStep.Value.PollingData1;
                                    pollStepCache.PollingData2 = eachPollingStep.Value.PollingData2;
                                    pollStepCache.PollingData3 = eachPollingStep.Value.PollingData3;
                                    pollStepCache.PollingData4 = eachPollingStep.Value.PollingData4;
                                    pollStepCache.PollingQuestion = eachPollingStep.Value.PollingQuestion;
                                    pollStepCache.StepNumber = eachPollingStep.Key;

                                    sqlCon.Insert(pollStepCache, pollingStepDBCacheType);

                                }//end if

                            }//end foreach



                            foreach (MessageStep eachMessageStep in eachContentInfo.Message.MessageSteps)
                            {

                                if (sqlCon.Execute("UPDATE MessageStepDBCache SET " +
                                    "MessageDBID=?, " +
                                    "MessageGuid=?, " +
                                    "StepGuid=?, " +
                                    "ContentPackItemID=?, " +
                                    "MessageText=?, " +
                                    "StepNumber=?, " +
                                    "StepType=? " +
                                    "WHERE MessageDBID=? AND StepNumber=?",
								                   eachContentInfo.MessageDBID,
								                   eachContentInfo.Message.MessageID.ToString(),
								                   eachMessageStep.StepID.ToString(),
								                   eachMessageStep.ContentPackItemID,
								                   eachMessageStep.MessageText,
								                   eachMessageStep.StepNumber,
								                   eachMessageStep.StepType,
								                   eachContentInfo.MessageDBID,
								                   eachMessageStep.StepNumber) == 0)
                                {

                                    MessageStepDBCache msgStepCache = new MessageStepDBCache();
                                    msgStepCache.ContentPackItemID = eachMessageStep.ContentPackItemID;
                                    msgStepCache.MessageDBID = eachContentInfo.MessageDBID;
                                    msgStepCache.MessageID = eachContentInfo.Message.MessageID;
                                    msgStepCache.MessageText = eachMessageStep.MessageText;
                                    msgStepCache.StepID = eachMessageStep.StepID;
                                    msgStepCache.StepNumber = eachMessageStep.StepNumber;
                                    msgStepCache.StepType = eachMessageStep.StepType;

                                    sqlCon.Insert(msgStepCache, messageStepDBCacheType);

                                }//end if

                            }//end foreach



                            foreach (Guid eachRcpID in eachContentInfo.Recipients)
                            {

                                if (sqlCon.Execute("UPDATE MessageRecipientDBCache SET " +
                                    "MessageDBID=?, " +
                                    "AccountGuid=?, " +
                                    "MessageGuid=?, " +
                                    "IsRead=? " +
                                    "WHERE MessageDBID=? AND AccountGuid=?",
								                   eachContentInfo.MessageDBID,
								                   eachRcpID.ToString(),
								                   eachContentInfo.Message.MessageID.ToString(),
								                   false,
								                   eachContentInfo.MessageDBID,
								                   eachRcpID.ToString()) == 0)
                                {

                                    MessageRecipientDBCache rcpCache = new MessageRecipientDBCache();
                                    rcpCache.AccountGuid = eachRcpID.ToString();
                                    rcpCache.IsRead = false;
                                    rcpCache.MessageDBID = eachContentInfo.MessageDBID;
                                    rcpCache.MessageGuid = eachContentInfo.Message.MessageID.ToString();

                                    sqlCon.Insert(rcpCache, messageRecipientDBCacheType);

                                }//end if

                            }//end foreach



                            if (null != eachContentInfo.ContentState)
                            {
                                if (sqlCon.Execute("UPDATE ContentState SET " +
                                    "ContentInfoID=? WHERE ContentInfoID=?",
								                   eachContentInfo.ID, eachContentInfo.ID) == 0)
                                {
                                    sqlCon.Insert(eachContentInfo.ContentState, contentStateType);
                                }//end if
                            }//end if

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error saving content info item! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void InsertOrUpdateContentInfoItems





        public List<ContentInfo> GetAllContentInfo()
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    List<ContentInfo> toReturn = 
						sqlCon.Query<ContentInfo>("SELECT * FROM ContentInfo");

                    foreach (ContentInfo eachContentInfo in toReturn)
                    {

                        MessageDB msg = sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE ID=?", eachContentInfo.MessageDBID)
							.FirstOrDefault();
                        eachContentInfo.Message = MessageDB.ConvertFromMessageDB(msg);

                        List<MessageStepDBCache> msgSteps = 
							sqlCon.Query<MessageStepDBCache>("SELECT * FROM MessageStepDBCache WHERE MessageDBID=?", eachContentInfo.MessageDBID);
                        eachContentInfo.Message.MessageSteps = new List<MessageStep>();

                        for (int i = 0; i < msgSteps.Count; i++)
                        {
                            eachContentInfo.Message.MessageSteps [i] = MessageStepDBCache.ConvertFromMessageStepDB(msgSteps [i]);

                        }//end for

                        List<MessageRecipientDBCache> msgRcp = 
							sqlCon.Query<MessageRecipientDBCache>("SELECT * FROM MessageRecipientDBCache WHERE MessageDBID=?", eachContentInfo.MessageDBID);
                        eachContentInfo.Recipients = new List<Guid>();
                        eachContentInfo.Message.MessageRecipients = new List<Message.MessageRecipient>();

                        for (int i = 0; i < msgRcp.Count; i++)
                        {
                            eachContentInfo.Recipients [i] = new Guid(msgRcp [i].AccountGuid);
                            eachContentInfo.Message.MessageRecipients [i] = new Message.MessageRecipient() {

								AccountID = eachContentInfo.Recipients[i],
								IsRead = false

							};
                        }//end for

                        List<VoiceCache> voiceRecordings =
							sqlCon.Query<VoiceCache>("SELECT * FROM VoiceCache WHERE ContentInfoID=?", eachContentInfo.ID);
                        eachContentInfo.VoiceRecordings = 
							voiceRecordings.ToDictionary(s => s.StepNumber, s => s.VoiceData);

                        List<PollingStepDBCache> pollingSteps = 
							sqlCon.Query<PollingStepDBCache>("SELECT * FROM PollingStepDBCache WHERE ContentInfoID=?", eachContentInfo.ID);
                        eachContentInfo.PollingSteps = 
							pollingSteps.ToDictionary(s => s.StepNumber, s => PollingStepDB.ConvertFromPollingStepDB(s));

                        if (sqlCon.Query<ContentState>("SELECT * FROM ContentState WHERE ContentInfoID=?", eachContentInfo.ID).Count == 1)
                        {

                            ContentState contentState = new ContentState(MessageDB.ConvertFromMessage(eachContentInfo.Message));
                            //TODO: Set animation items to remove in DBManager.GetAllContentInfo()!
#if(DEBUG)
                            Console.WriteLine("TODO: Set animation items to remove in DBManager.GetAllContentInfo()!");
#endif
                            contentState.RemoveExistingItems(null, 
							                                 new List<int>(voiceRecordings
							                                 .Where(s => s.IsSent)
							                                 .Select(s => s.StepNumber)), 
							                                 new List<int>(pollingSteps
							                                 .Where(s => s.IsSent)
							                                 .Select(s => s.StepNumber)), null);

                            eachContentInfo.ContentState = contentState;

                        }//end if

                    }//end foreach

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end List<ContentInfo> GetAllContentInfo




        public void DeleteContentInfoIfExists(ContentInfo contentInfo)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        ContentInfo contentInfoToDelete = 
							sqlCon.Query<ContentInfo>("SELECT * FROM ContentInfo WHERE ID=?", contentInfo.ID)
								.FirstOrDefault();

                        if (null != contentInfoToDelete)
                        {

                            // Take care of the message first.
                            MessageDB messageDB = sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE ID=?", contentInfo.MessageDBID)
								.FirstOrDefault();
                            if (messageDB.MessageID == default(Guid))
                            {
                                // Delete it, because the message has already been inserted from message create.
                                sqlCon.Execute("DELETE FROM MessageDB WHERE ID=?", messageDB.ID);
                            }//end if

                            sqlCon.Execute("DELETE FROM VoiceCache WHERE ContentInfoID=?", contentInfo.ID);
                            sqlCon.Execute("DELETE FROM PollingStepDBCache WHERE ContentInfoID=?", contentInfo.ID);
                            sqlCon.Execute("DELETE FROM MessageRecipientDBCache WHERE MessageDBID=?", messageDB.ID);
                            sqlCon.Execute("DELETE FROM MessageStepDBCache WHERE MessageDBID=?", messageDB.ID);
                            sqlCon.Execute("DELETE FROM ContentState WHERE ContentInfoID=?", contentInfo.ID);

                            sqlCon.Execute("DELETE FROM ContentInfo WHERE ID=?", contentInfo.ID);

                        }//end if

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

#if(DEBUG)
                        Console.WriteLine("Error deleting content info! {0}--{1}", ex.Message, ex.StackTrace);
#endif
                        sqlCon.Rollback();

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void DeleteContentInfo




        [Obsolete("Does not handle Animation objects")]
        public ContentInfo GetContentInfoByMessageDBID(int messageDBID)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.DBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    ContentInfo toReturn = 
						sqlCon.Query<ContentInfo>("SELECT * FROM ContentInfo WHERE MessageDBID=?", messageDBID)
							.FirstOrDefault();

                    if (null != toReturn)
                    {

                        MessageDB msg = sqlCon.Query<MessageDB>("SELECT * FROM MessageDB WHERE ID=?", toReturn.MessageDBID)
							.FirstOrDefault();
                        toReturn.Message = MessageDB.ConvertFromMessageDB(msg);

                        List<MessageStepDBCache> msgSteps = 
							sqlCon.Query<MessageStepDBCache>("SELECT * FROM MessageStepDBCache WHERE MessageDBID=?", toReturn.MessageDBID);
                        toReturn.Message.MessageSteps = new List<MessageStep>();

                        for (int i = 0; i < msgSteps.Count; i++)
                        {
                            toReturn.Message.MessageSteps [i] = MessageStepDBCache.ConvertFromMessageStepDB(msgSteps [i]);
                        }//end for

                        List<MessageRecipientDBCache> msgRcp = 
							sqlCon.Query<MessageRecipientDBCache>("SELECT * FROM MessageRecipientDBCache WHERE MessageDBID=?", toReturn.MessageDBID);
                        toReturn.Recipients = new List<Guid>();
                        toReturn.Message.MessageRecipients = new List<Message.MessageRecipient>();

                        for (int i = 0; i < msgRcp.Count; i++)
                        {
                            toReturn.Recipients [i] = new Guid(msgRcp [i].AccountGuid);
                            toReturn.Message.MessageRecipients [i] = new Message.MessageRecipient() {

								AccountID = toReturn.Recipients[i],
								IsRead = false

							};
                        }//end for

                        List<VoiceCache> voiceRecordings =
							sqlCon.Query<VoiceCache>("SELECT * FROM VoiceCache WHERE ContentInfoID=?", toReturn.ID);
                        toReturn.VoiceRecordings = 
							voiceRecordings.ToDictionary(s => s.StepNumber, s => s.VoiceData);

                        List<PollingStepDBCache> pollingSteps = 
							sqlCon.Query<PollingStepDBCache>("SELECT * FROM PollingStepDBCache WHERE ContentInfoID=?", toReturn.ID);
                        toReturn.PollingSteps = 
							pollingSteps.ToDictionary(s => s.StepNumber, s => PollingStepDB.ConvertFromPollingStepDB(s));

                        // No need for content state here
                        //					if (sqlCon.Query<ContentState>("SELECT * FROM ContentState WHERE ContentInfoID=?", toReturn.ID).Count == 1)
                        //					{
                        //
                        //						ContentState contentState = new ContentState(MessageDB.ConvertFromMessage(toReturn.Message));
                        //						contentState.RemoveExistingItems(null, 
                        //						                                 new List<int>(voiceRecordings
                        //						                                 .Where(s => s.IsSent)
                        //						                                 .Select(s => s.StepNumber)), 
                        //						                                 new List<int>(pollingSteps
                        //						                                 .Where(s => s.IsSent)
                        //						                                 .Select(s => s.StepNumber)));
                        //
                        //						toReturn.ContentState = contentState;
                        //
                        //					}//end if

                    }//end if

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end ContentInfoGetContentInfoByMessageDBID

        #endregion Content




		#region Animations

        public void InsertOrUpdateAnimation(AnimationInfo animationInfo)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.AnimationDBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    Type animationInfoType = typeof(AnimationInfo);
                    Type frameInfoType = typeof(FrameInfo);
                    Type layerInfoType = typeof(LayerInfo);
                    Type drInfoType = typeof(DrawingInfo);
                    Type trInfoType = typeof(TransitionInfo);
                    Type trSettingsType = typeof(TransitionEffectSettings);
                    Type brushItemType = typeof(BrushItem);
					
                    Type audioItemType = typeof(AnimationAudioInfo);

                    sqlCon.BeginTransaction();

                    try
                    {

                        //
                        // AnimationInfo
                        //
                        if (sqlCon.Execute("UPDATE AnimationInfo SET " +
                            "MessageGuid=?, " +
                            "StepNumber=?, " +
                            "CreatedOn=?, " +
                            "OriginalCanvasSizeWidth=?, " +
                            "OriginalCanvasSizeHeight=?, " +
                            "IsEditing=?, " +
                            "IsSent=? " +
                            "WHERE DBID=?",
						                   animationInfo.MessageGuid,
						                   animationInfo.StepNumber,
						                   animationInfo.CreatedOn,
						                   animationInfo.OriginalCanvasSizeWidth,
						                   animationInfo.OriginalCanvasSizeHeight,
						                   animationInfo.IsEditing,
						                   animationInfo.IsSent,
						                   animationInfo.DBID) == 0)
                        {

                            sqlCon.Insert(animationInfo, animationInfoType);

                        }//end if

                        foreach (FrameInfo eachFrameInfo in animationInfo.FrameItems.Values)
                        {

                            eachFrameInfo.AnimationDBID = animationInfo.DBID;
                            if (sqlCon.Execute("UPDATE FrameInfo SET " +
                                "ID=?, " +
                                "AnimationDBID=? " +
                                "WHERE DBID=?",
							                   eachFrameInfo.ID,
							                   eachFrameInfo.AnimationDBID,
							                   eachFrameInfo.DBID) == 0)
                            {

                                sqlCon.Insert(eachFrameInfo, frameInfoType);

                            }//end if

                            foreach (LayerInfo eachLayerInfo in eachFrameInfo.Layers.Values)
                            {

                                eachLayerInfo.FrameDBID = eachFrameInfo.DBID;
                                if (sqlCon.Execute("UPDATE LayerInfo SET " +
                                    "ID=?, " +
                                    "FrameDBID=?, " +
                                    "CanvasSizeWidth=?, " +
                                    "CanvasSizeHeight=?, " +
                                    "IsCanvasActive=? " +
                                    "WHERE DBID=?",
								                   eachLayerInfo.ID,
								                   eachLayerInfo.FrameDBID,
								                   eachLayerInfo.CanvasSizeWidth,
								                   eachLayerInfo.CanvasSizeHeight,
								                   eachLayerInfo.IsCanvasActive,
								                   eachLayerInfo.DBID) == 0)
                                {

                                    sqlCon.Insert(eachLayerInfo, layerInfoType);

                                }//end if

                                foreach (DrawingInfo eachDrawingInfo in eachLayerInfo.DrawingItems.Values)
                                {

                                    eachDrawingInfo.LayerDBID = eachLayerInfo.DBID;
                                    if (null != eachDrawingInfo.Brush)
                                    {

                                        if (sqlCon.Execute("UPDATE BrushItem SET " +
                                            "Thickness=?, " +
                                            "BrushType=?, " +
                                            "BrushImageBuffer=?, " +
                                            "BrushColorBuffer=?, " +
                                            "InactiveBrushColorBuffer=?, " +
                                            "IsSprayBrushActive=? " +
                                            "WHERE DBID=?",
										                   eachDrawingInfo.Brush.Thickness,
										                   eachDrawingInfo.Brush.BrushType,
										                   eachDrawingInfo.Brush.BrushImageBuffer,
										                   eachDrawingInfo.Brush.BrushColorBuffer,
										                   eachDrawingInfo.Brush.InactiveBrushColorBuffer,
										                   eachDrawingInfo.Brush.IsSprayBrushActive,
										                   eachDrawingInfo.Brush.DBID) == 0)
                                        {
                                            sqlCon.Insert(eachDrawingInfo.Brush, brushItemType);
                                        }//end sqlCon

                                        eachDrawingInfo.BrushItemDBID = eachDrawingInfo.Brush.DBID;
                                    }//end if

                                    if (sqlCon.Execute("UPDATE DrawingInfo SET " +
                                        "DrawingID=?, " +
                                        "LayerDBID=?, " +
                                        "BrushItemDBID=?, " +
                                        "LineColorBuffer=?, " +
                                        "DrawingType=?, " +
                                        "ImageBuffer=?, " +
                                        "ImageFrameX=?, " +
                                        "ImageFrameY=?, " +
                                        "ImageFrameWidth=?, " +
                                        "ImageFrameHeight=?, " +
                                        "RotationAngle=?, " +
                                        "RotatedImageBoxX=?, " +
                                        "RotatedImageBoxY=?, " +
                                        "RotatedImageBoxWidth=?, " +
                                        "RotatedImageBoxHeight=?, " +
                                        "ContentPackItemID=?, " +
                                        "CalloutText=?, " +
                                        "CalloutTextRectX=?, " +
                                        "CalloutTextRectY=?, " +
                                        "CalloutTextRectWidth=?, " +
                                        "CalloutTextRectHeight=? " +
                                        "WHERE DBID=?",
									                   eachDrawingInfo.DrawingID,
									                   eachDrawingInfo.LayerDBID,
									                   eachDrawingInfo.BrushItemDBID,
									                   eachDrawingInfo.LineColorBuffer,
									                   eachDrawingInfo.DrawingType,
									                   eachDrawingInfo.ImageBuffer,
									                   eachDrawingInfo.ImageFrameX,
									                   eachDrawingInfo.ImageFrameY,
									                   eachDrawingInfo.ImageFrameWidth,
									                   eachDrawingInfo.ImageFrameHeight,
									                   eachDrawingInfo.RotationAngle,
									                   eachDrawingInfo.RotatedImageBoxX,
									                   eachDrawingInfo.RotatedImageBoxY,
									                   eachDrawingInfo.RotatedImageBoxWidth,
									                   eachDrawingInfo.RotatedImageBoxHeight,
									                   eachDrawingInfo.ContentPackItemID,
									                   eachDrawingInfo.CalloutText,
									                   eachDrawingInfo.CalloutTextRectX,
									                   eachDrawingInfo.CalloutTextRectY,
									                   eachDrawingInfo.CalloutTextRectWidth,
									                   eachDrawingInfo.CalloutTextRectHeight,
									                   eachDrawingInfo.DBID) == 0)
                                    {

                                        sqlCon.Insert(eachDrawingInfo, drInfoType);

                                    }//end if

                                    if (eachDrawingInfo.DrawingType == DrawingLayerType.Drawing)
                                    {

                                        List<PathPointDB> drPoints = eachDrawingInfo.GetPathPointsForDB();
                                        // DELETE path points for this drawing item. No need to try to update each one individually.
                                        sqlCon.Execute("DELETE FROM PathPointDB WHERE DrawingInfoDBID=?", eachDrawingInfo.DBID);
                                        // And INSERT
                                        sqlCon.InsertAll(drPoints);

                                    }//end if

                                }//end foreach

                                foreach (TransitionInfo eachTrInfo in eachLayerInfo.Transitions.Values)
                                {
									
                                    eachTrInfo.LayerDBID = eachLayerInfo.DBID;
                                    if (sqlCon.Execute("UPDATE TransitionInfo SET " +
                                        "ID=?, " +
                                        "LayerDBID=?, " +
                                        "FadeOpacity=?, " +
                                        "RotationAngle=?, " +
                                        "EndSizeWidth=?, " +
                                        "EndSizeHeight=?, " +
                                        "EndSizeFixedPointX=?, " +
                                        "EndSizeFixedPointY=?, " +
                                        "EndLocationX=?, " +
                                        "EndLocationY=? " +
                                        "WHERE DBID=?", 
									                   eachTrInfo.ID,
									                   eachTrInfo.LayerDBID,
									                   eachTrInfo.FadeOpacity,
									                   eachTrInfo.RotationAngle,
									                   eachTrInfo.EndSizeWidth,
									                   eachTrInfo.EndSizeHeight,
									                   eachTrInfo.EndSizeFixedPointX,
									                   eachTrInfo.EndSizeFixedPointY,
									                   eachTrInfo.EndLocationX,
									                   eachTrInfo.EndLocationY,
									                   eachTrInfo.DBID) == 0)
                                    {

                                        sqlCon.Insert(eachTrInfo, trInfoType);

                                    }//end if
									
                                    foreach (TransitionEffectSettings eachTrSetting in eachTrInfo.Settings.Values)
                                    {
										
                                        eachTrSetting.TransitionInfoDBID = eachTrInfo.DBID;
                                        if (sqlCon.Execute("UPDATE TransitionEffectSettings SET " +
                                            "TransitionID=?, " +
                                            "TransitionInfoDBID=?, " +
                                            "FrameID=?, " +
                                            "LayerID=?, " +
                                            "EffectType=?, " +
                                            "Duration=?, " +
                                            "RotationCount=?, " +
                                            "Delay=? " +
                                            "WHERE DBID=?",
										                  eachTrSetting.TransitionID,
										                  eachTrSetting.TransitionInfoDBID,
										                  eachTrSetting.FrameID,
										                  eachTrSetting.LayerID,
										                  eachTrSetting.EffectType,
										                  eachTrSetting.Duration,
										                  eachTrSetting.RotationCount,
										                  eachTrSetting.Delay,
										                  eachTrSetting.DBID) == 0)
                                        {

                                            sqlCon.Insert(eachTrSetting, trSettingsType);

                                        }//end if
										
                                    }//end foreach
									
                                }//end foreach

                            }//end foreach

                        }//end foreach

                        foreach (AnimationAudioInfo eachAudioItem in animationInfo.AudioItems.Values)
                        {

                            FrameInfo frameItem = null;
                            if (animationInfo.FrameItems.TryGetValue(eachAudioItem.FrameID, out frameItem))
                            {
                                eachAudioItem.FrameDBID = frameItem.DBID;
                            }//end if

                            if (sqlCon.Execute("UPDATE AnimationAudioInfo SET " +
                                "ID=?, " +
                                "FrameID=?, " +
                                "FrameDBID=?, " +
                                "AudioBuffer=?, " +
                                "Duration=? " +
                                "WHERE DBID=?",
							                   eachAudioItem.ID,
							                   eachAudioItem.FrameID,
							                   eachAudioItem.FrameDBID,
							                   eachAudioItem.AudioBuffer,
							                   eachAudioItem.Duration,
							                   eachAudioItem.DBID) == 0)
                            {

                                sqlCon.Insert(eachAudioItem, audioItemType);

                            }//end if

                        }//end foreach

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

                        sqlCon.Rollback();

                        throw ex;

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void SaveAnimation




        /// <summary>
        /// Loads an AnimationInfo object from the database
        /// </summary>
        /// <returns>An AnimationInfo object</returns>
        /// <param name="idStepNumber">A Pair containing the animation's MessageID and StepNumber. Set to null to return the currently editing animation.</param>
        public AnimationInfo LoadAnimation(Pair<string, int> idStepNumber)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.AnimationDBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    AnimationInfo animationInfo = null;

                    if (null == idStepNumber)
                    {

                        animationInfo = 
							sqlCon.Query<AnimationInfo>("SELECT * FROM AnimationInfo WHERE IsEditing=?", true)
								.FirstOrDefault();

                    } else
                    {

                        animationInfo = 
							sqlCon.Query<AnimationInfo>("SELECT * FROM AnimationInfo WHERE MessageGuid=? AND StepNumber=?",
							                            idStepNumber.ItemA, idStepNumber.ItemB)
								.FirstOrDefault();

                    }//end if else


                    if (null == animationInfo)
                    {
                        return null;
                    } else
                    {

                        List<FrameInfo> frameItems = 
							sqlCon.Query<FrameInfo>("SELECT * FROM FrameInfo WHERE AnimationDBID=?", animationInfo.DBID);

                        foreach (FrameInfo eachFrameInfo in frameItems)
                        {

                            List<LayerInfo> layerItems = 
								sqlCon.Query<LayerInfo>("SELECT * FROM LayerInfo WHERE FrameDBID=?", eachFrameInfo.DBID);

                            foreach (LayerInfo eachLayerItem in layerItems)
                            {

                                List<DrawingInfo> drItems = 
									sqlCon.Query<DrawingInfo>("SELECT * FROM DrawingInfo WHERE LayerDBID=?", eachLayerItem.DBID);

                                foreach (DrawingInfo eachDrInfo in drItems)
                                {

                                    if (eachDrInfo.BrushItemDBID > 0)
                                    {

                                        eachDrInfo.Brush = 
											sqlCon.Get<BrushItem>(eachDrInfo.BrushItemDBID);

                                    }//end if

                                    if (eachDrInfo.DrawingType == DrawingLayerType.Drawing)
                                    {

                                        List<PathPointDB> pathPoints = 
											sqlCon.Query<PathPointDB>("SELECT * FROM PathPointDB WHERE DrawingInfoDBID=? " +
                                            "ORDER BY SortOrder", eachDrInfo.DBID);

                                        eachDrInfo.SetPathPointsFromDB(pathPoints);

                                    }//end if

                                    eachLayerItem.DrawingItems [eachDrInfo.DrawingID] = eachDrInfo;

                                }//end foreach

                                List<TransitionInfo> transitions = 
									sqlCon.Query<TransitionInfo>("SELECT * FROM TransitionInfo WHERE LayerDBID=?", eachLayerItem.DBID);

                                foreach (TransitionInfo eachTrInfo in transitions)
                                {

                                    List<TransitionEffectSettings> efSettings = 
										sqlCon.Query<TransitionEffectSettings>("SELECT * FROM TransitionEffectSettings WHERE TransitionInfoDBID=?", eachTrInfo.DBID);

                                    foreach (TransitionEffectSettings eachEfSetting in efSettings)
                                    {

                                        eachTrInfo.Settings [eachEfSetting.EffectType] = eachEfSetting;

                                    }//end foreach

                                    eachLayerItem.Transitions [eachTrInfo.ID] = eachTrInfo;

                                }//end foreach

                                eachFrameInfo.Layers [eachLayerItem.ID] = eachLayerItem;

                            }//end foreach

                            animationInfo.FrameItems [eachFrameInfo.ID] = eachFrameInfo;

                            List<AnimationAudioInfo> frameAudioItems = 
								sqlCon.Query<AnimationAudioInfo>("SELECT * FROM AnimationAudioInfo WHERE FrameDBID=?", eachFrameInfo.DBID);

                            foreach (AnimationAudioInfo eachAudioInfo in frameAudioItems)
                            {
                                animationInfo.AudioItems [eachAudioInfo.ID] = eachAudioInfo;
                            }//end foreach

                        }//end foreach

                        return animationInfo;

                    }//end if else
                }//end using sqlCon

            }//end lock

        }//end AnimationInfo LoadAnimation






        public void SaveAnimationUndoInfo(List<UndoInfo> undoInfo)
        {

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.AnimationDBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    sqlCon.BeginTransaction();

                    try
                    {

                        sqlCon.Execute("DELETE FROM UndoInfo");

                        sqlCon.InsertAll(undoInfo);

                        sqlCon.Commit();

                    } catch (Exception ex)
                    {

                        sqlCon.Rollback();

#if(DEBUG)
                        Console.WriteLine("Error inserting undo info objects! {0}--{1}", ex.Message, ex.StackTrace);
#endif

                    }//end try catch

                }//end using sqlCon

            }//end lock

        }//end void SaveAnimationUndoInfo




        public List<UndoInfo> LoadAnimationUndoInfo()
        {

            List<UndoInfo> toReturn = null;

            lock (this.dbLock)
            {

                using (SQLiteConnection sqlCon = new SQLiteConnection(this.AnimationDBPath))
                {

                    sqlCon.Execute(WZConstants.DBClauseSyncOff);

                    toReturn = 
						sqlCon.Query<UndoInfo>("SELECT * FROM UndoInfo ORDER BY SortOrder DESC");

                    return toReturn;

                }//end using sqlCon

            }//end lock

        }//end List<UndoInfo> LoadAnimationUndoInfo


		#region Obsolete Animation methods

        //		[Obsolete("", true)]
        //		public void SaveAnimation (Dictionary<int, FrameInfo> frameItems, Dictionary<int, AnimationAudioInfo> audioItems)
        //		{
        //
        //			lock (this.dbLock) 
        //			{
        //
        //				using (SQLiteConnection sqlCon = new SQLiteConnection(this.AnimationDBPath)) 
        //				{
        //
        //					sqlCon.Execute (WZConstants.DBClauseSyncOff);
        //
        //					// Setup types
        //					Type animationInfoType = typeof(AnimationInfo);
        //					Type frameInfoType = typeof(FrameInfo);
        //					Type layerInfoType = typeof(LayerInfo);
        //					Type drInfoType = typeof(DrawingInfo);
        //					Type trInfoType = typeof(TransitionInfo);
        //					Type trSettingsType = typeof(TransitionEffectSettings);
        //					Type brushItemType = typeof(BrushItem);
        //
        //					Type audioItemType = typeof(AnimationAudioInfo);
        //
        //					sqlCon.BeginTransaction ();
        //
        //					try 
        //					{
        //
        //						// First delete all data
        //						//TODO: This should change
        //						sqlCon.Execute ("DELETE FROM FrameInfo");
        //						sqlCon.Execute ("DELETE FROM LayerInfo");
        //						sqlCon.Execute ("DELETE FROM DrawingInfo");
        //						sqlCon.Execute ("DELETE FROM BrushItem");
        //						sqlCon.Execute ("DELETE FROM TransitionInfo");
        //						sqlCon.Execute ("DELETE FROM TransitionEffectSettings");
        //						sqlCon.Execute ("DELETE FROM PathPointDB");
        //						sqlCon.Execute ("DELETE FROM AnimationAudioInfo");
        //
        //						foreach (FrameInfo eachFrameInfo in frameItems.Values) 
        //						{
        //
        //							sqlCon.Insert (eachFrameInfo, frameInfoType);
        //
        //							foreach (LayerInfo eachLayerInfo in eachFrameInfo.Layers.Values) 
        //							{
        //
        //								eachLayerInfo.FrameDBID = eachFrameInfo.DBID;
        //								sqlCon.Insert (eachLayerInfo, layerInfoType);
        //
        //								foreach (DrawingInfo eachDrItem in eachLayerInfo.DrawingItems.Values) 
        //								{
        //
        //									eachDrItem.LayerDBID = eachLayerInfo.DBID;
        //									if (null != eachDrItem.Brush) 
        //									{
        //										sqlCon.Insert (eachDrItem.Brush, brushItemType);
        //										eachDrItem.BrushItemDBID = eachDrItem.Brush.DBID;
        //									}//end if
        //
        //									sqlCon.Insert (eachDrItem, drInfoType);
        //
        //									if (eachDrItem.DrawingType == DrawingLayerType.Drawing) 
        //									{
        //										sqlCon.InsertAll (eachDrItem.GetPathPointsForDB ());
        //									}//end if
        //
        //								}//end foreach
        //
        //								foreach (TransitionInfo eachTrInfo in eachLayerInfo.Transitions.Values) 
        //								{
        //
        //									eachTrInfo.LayerDBID = eachLayerInfo.DBID;
        //									sqlCon.Insert (eachTrInfo, trInfoType);
        //
        //									foreach (TransitionEffectSettings eachTrSetting in eachTrInfo.Settings.Values) 
        //									{
        //
        //										eachTrSetting.TransitionInfoDBID = eachTrInfo.DBID;
        //										sqlCon.Insert (eachTrSetting, trSettingsType);
        //
        //									}//end foreach
        //
        //								}//end foreach
        //
        //							}//end foreach
        //
        //						}//end foreach
        //
        //
        //						foreach (AnimationAudioInfo eachAudioItem in audioItems.Values) 
        //						{
        //
        //							FrameInfo frameItem = null;
        //							if (frameItems.TryGetValue (eachAudioItem.FrameID, out frameItem)) 
        //							{
        //								eachAudioItem.FrameDBID = frameItem.ID;
        //							}//end if
        //
        //							sqlCon.Insert (eachAudioItem, audioItemType);
        //
        //						}//end foreach
        //
        //						sqlCon.Commit ();
        //
        //					} catch (Exception ex) {
        //
        //						sqlCon.Rollback ();
        //#if(DEBUG)
        //						Console.WriteLine ("Error saving animation! {0}--{1}", ex.Message, ex.StackTrace);
        //#endif
        //						throw ex;
        //					}//end try catch
        //
        //				}//end using sqlCon
        //
        //			}//end lock
        //
        //		}//end void SaveAnimation

        //		[Obsolete("", true)]
        //		public Dictionary<int, FrameInfo> LoadAnimation ()
        //		{
        //
        //			Dictionary<int, FrameInfo> toReturn = new Dictionary<int, FrameInfo> ();
        //
        //			lock (this.dbLock) {
        //
        //				using (SQLiteConnection sqlCon = new SQLiteConnection(this.AnimationDBPath)) {
        //
        //					sqlCon.Execute (WZConstants.DBClauseSyncOff);
        //
        //					List<FrameInfo> frameItems = 
        //						sqlCon.Query<FrameInfo> ("SELECT * FROM FrameInfo");
        //
        //					foreach (FrameInfo eachFrameInfo in frameItems) {
        //
        //						List<LayerInfo> layers = 
        //							sqlCon.Query<LayerInfo> ("SELECT * FROM LayerInfo WHERE FrameDBID=?", eachFrameInfo.DBID);
        //
        //						foreach (LayerInfo eachLayerInfo in layers) {
        //
        //							List<DrawingInfo> drawingItems = 
        //								sqlCon.Query<DrawingInfo> ("SELECT * FROM DrawingInfo WHERE LayerDBID=?", eachLayerInfo.DBID);
        //
        //							foreach (DrawingInfo eachDrInfo in drawingItems) {
        //
        //								if (eachDrInfo.BrushItemDBID > 0) {
        //
        //									eachDrInfo.Brush = 
        //										sqlCon.Get<BrushItem> (eachDrInfo.BrushItemDBID);
        //
        //								}//end if
        //
        //								List<PathPointDB> pathPoints = 
        //									sqlCon.Query<PathPointDB> ("SELECT * FROM PathPointDB WHERE DrawingInfoDBID=? " +
        //									"ORDER BY SortOrder", eachDrInfo.DBID);
        //
        //								eachDrInfo.SetPathPointsFromDB (pathPoints);
        //
        //								eachLayerInfo.DrawingItems [eachDrInfo.DrawingID] = eachDrInfo;
        //							}//end foreach
        //
        //							List<TransitionInfo> transitions = 
        //								sqlCon.Query<TransitionInfo> ("SELECT * FROM TransitionInfo WHERE LayerDBID=?", eachLayerInfo.DBID);
        //
        //							foreach (TransitionInfo eachTrInfo in transitions) {
        //
        //								List<TransitionEffectSettings> trSettings = 
        //									sqlCon.Query<TransitionEffectSettings> ("SELECT * FROM TransitionEffectSettings WHERE TransitionInfoDBID=?", eachTrInfo.DBID);
        //
        //								foreach (TransitionEffectSettings eachTrSetting in trSettings) {
        //
        //									eachTrInfo.Settings [eachTrSetting.EffectType] = eachTrSetting;
        //
        //								}//end foreach
        //
        //								eachLayerInfo.Transitions [eachTrInfo.ID] = eachTrInfo;
        //							}//end foreach
        //
        //							eachFrameInfo.Layers [eachLayerInfo.ID] = eachLayerInfo;
        //						}//end foreach
        //
        //						toReturn [eachFrameInfo.ID] = eachFrameInfo;
        //					}//end foreach
        //
        //				}//end using sqlCon
        //
        //				return toReturn;
        //
        //			}//end lock
        //
        //		}//end Dictionary<int, FrameInfo> LoadAnimation
		
		
		
        //		[Obsolete("", true)]
        //		public Dictionary<int, AnimationAudioInfo> LoadAnimationAudio ()
        //		{
        //
        //			Dictionary<int, AnimationAudioInfo> toReturn = null;
        //
        //			lock (this.dbLock) {
        //
        //				using (SQLiteConnection sqlCon = new SQLiteConnection(this.AnimationDBPath)) {
        //
        //					sqlCon.Execute (WZConstants.DBClauseSyncOff);
        //
        //					List<AnimationAudioInfo> audioItems = 
        //						sqlCon.Query<AnimationAudioInfo> ("SELECT * FROM AnimationAudioInfo");
        //
        //					toReturn = new Dictionary<int, AnimationAudioInfo> (audioItems.Count);
        //					audioItems.ForEach (s => toReturn.Add (s.ID, s));
        //
        //					return toReturn;
        //
        //				}//end using sqlCon
        //
        //			}//end lock
        //
        //		}//end Dictionary<int, AnimationAudioInfo> LoadAnimationAudio

		#endregion Obsolete Animation methods

		#endregion Animations


        #endregion Public methods

    }

}
// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Linq;
using LOLAccountManagement;
using LOLMessageDelivery;
using SQLite;
using LOLMessageDelivery.Classes.LOLAnimation;
using WZCommon;

using PreserveProps = Android.Runtime.PreserveAttribute;

namespace WZCommon
{
    [PreserveProps(AllMembers=true)]
    public class ContentInfo
    {

		#region Constructors

        public ContentInfo(Message message, List<Guid> recipients)
        {
            this.Message = message;
            this.Recipients = recipients;

            this.VoiceRecordings = new Dictionary<int, byte[]>();
            this.PollingSteps = new Dictionary<int, PollingStep>();
            this.AnimationItems = new Dictionary<int, AnimationInfo>();
        }



        public ContentInfo()
        {
        }//end default ctor

		#endregion Contructors




		#region Properties

        [PrimaryKey, AutoIncrement]
        public int ID
        {
            get;
            private set;
        }//end int ID


        [Indexed]
        public int MessageDBID
        {
            get;
            set;
        }//end int MessageDBID


        [Ignore]
        public Message Message
        {
            get;
            set;
        }//end Message Message



        [Ignore]
        public List<Guid> Recipients
        {
            get;
            set;
        }//end List<Guid> Recipients



        [Ignore]
        public Dictionary<int, byte[]> VoiceRecordings
        {
            get;
            set;
        }//end Dictionary<int, string> VoiceRecordings



        [Ignore]
        public Dictionary<int, PollingStep> PollingSteps
        {
            get;
            set;
        }//end Dictionary<int, PollingStep> PollingSteps



        [Ignore]
        public Dictionary<int, AnimationInfo> AnimationItems
        {
            get;
            set;
        }//end Dictionary<int, AnimationItems> AnimationItems



        public bool IsFailed
        {
            get;
            set;
        }//end bool IsFailed



        public bool IsMessageCreateFailed
        {
            get;
            set;
        }//end bool IsMessageCreateFailed




        [Ignore]
        public ContentState ContentState
        {
            get;
            set;
        }//end ContentState ContentState



        [Ignore]
        public int Retries
        {
            get;
            set;
        }//end int Retries

		#endregion Properties



		#region Overrides

        public override string ToString()
        {
            return string.Format("[ContentInfo: ID={0}, MessageDBID={1}, Message={2}, Recipients={3}, VoiceRecordings={4}, PollingSteps={5}, IsFailed={6}, IsMessageCreateFailed={7}, ContentState={8}]", ID, MessageDBID, Message, Recipients, VoiceRecordings, PollingSteps, IsFailed, IsMessageCreateFailed, ContentState);
        }

		#endregion Overrides

    }//end class ContentInfo



    public class VoiceCache
    {

        public VoiceCache()
        {

        }//end ctor



        [PrimaryKey, AutoIncrement]
        public int ID
        {
            get;
            private set;
        }//end int ID



        [Indexed]
        public int ContentInfoID
        {
            get;
            set;
        }//end int ContentInfoID



        public int StepNumber
        {
            get;
            set;
        }//end int StepNumber



        public byte[] VoiceData
        {
            get;
            set;
        }//end byte[] VoiceData



        public bool IsSent
        {
            get;
            set;
        }//end bool IsSent

    }//end class VoiceCache





    public class PollingStepDBCache : PollingStepDB
    {

        public PollingStepDBCache()
        {
        }//end ctor



        [PrimaryKey, AutoIncrement]
        public new int ID
        {
            get;
            private set;
        }//end int ID



        [Indexed]
        public int ContentInfoID
        {
            get;
            set;
        }//end int ContentInfoID



        public bool IsSent
        {
            get;
            set;
        }//end bool IsSent



        [Ignore]
        public new string PollingData1File
        {
            get
            {
                return base.PollingData1File;
            }
            set
            {
                base.PollingData1File = value;
            }//end get set
        }//end new string PollingData1File


        [Ignore]
        public new string PollingData2File
        {
            get
            {
                return base.PollingData2File;
            }
            set
            {
                base.PollingData2File = value;
            }//end get set
        }//end new string PollingData1File



        [Ignore]
        public new string PollingData3File
        {
            get
            {
                return base.PollingData3File;
            }
            set
            {
                base.PollingData3File = value;
            }//end get set
        }//end new string PollingData1File



        [Ignore]
        public new string PollingData4File
        {
            get
            {
                return base.PollingData4File;
            }
            set
            {
                base.PollingData4File = value;
            }//end get set
        }//end new string PollingData1File



        public new byte[] PollingData1
        {
            get
            {
                return base.PollingData1;
            }
            set
            {
                base.PollingData1 = value;
            }//end get set

        }//end new byte[] PollingData1



        public new byte[] PollingData2
        {
            get
            {
                return base.PollingData2;
            }
            set
            {
                base.PollingData2 = value;
            }//end get set

        }//end new byte[] PollingData1



        public new byte[] PollingData3
        {
            get
            {
                return base.PollingData3;
            }
            set
            {
                base.PollingData3 = value;
            }//end get set

        }//end new byte[] PollingData1



        public new byte[] PollingData4
        {
            get
            {
                return base.PollingData4;
            }
            set
            {
                base.PollingData4 = value;
            }//end get set

        }//end new byte[] PollingData1

    }//end class PollingStepDBCache



    public class MessageStepDBCache : MessageStepDB
    {

        public MessageStepDBCache()
        {

        }//end MessageStepDBCache



        [PrimaryKey, AutoIncrement]
        public new int ID
        {
            get;
            private set;
        }//end int ID


        [Indexed]
        public int MessageDBID
        {
            get;
            set;
        }//end int MessageDBID

    }//end class MessageStepDBCache



    public class MessageRecipientDBCache : MessageRecipientDB
    {

        public MessageRecipientDBCache()
        {
        }//end ctor


        [PrimaryKey, AutoIncrement]
        public new int ID
        {
            get;
            private set;
        }//end int ID



        [Indexed]
        public int MessageDBID
        {
            get;
            set;
        }//end int MessageDBID

    }//end class MessageRecipientDBCache


}


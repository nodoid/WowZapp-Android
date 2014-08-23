// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using SQLite;
using System.Linq;
using LOLMessageDelivery;
using System.Collections.Generic;

namespace WZCommon
{

	public class MessageDB : Message
	{

        #region Constructors

		public MessageDB ()
		{
		}

        #endregion Constructors



        #region Properties

		/// <summary>
		/// Gets the db primary key of the MessageItem.
		/// </summary>
		[PrimaryKey, AutoIncrement]
		public int ID {
			get;
			private set;
		}//end int ID



		public string MessageGuid 
		{
			get 
			{
				if (this.MessageID == default(Guid)) 
				{
					return Guid.Empty.ToString();
				}//end if

				return this.MessageID.ToString ();
			}
			set 
			{
				this.MessageID = new Guid (value);
			}//end get set

		}//end string MessageGuid



		public string FromAccountGuid {
			get {
				if (this.FromAccountID == default(Guid)) {
					return Guid.Empty.ToString();
				}//end if

				return this.FromAccountID.ToString ();
			}
			set {
				this.FromAccountID = new Guid (value);
			}//end get set

		}//end string FromAccountGuid




		public bool IsOutgoing {
			get;
			set;
		}//end bool

        #endregion Properties



        #region Overrides

		public override string ToString ()
		{
			return string.Format ("[MessageDB: ID={0}, MessageGuid={1}, FromAccountGuid={2}, IsOutgoing={3}, Errors={4}, MessageID={5}, FromAccountID={6}, MessageSteps={7}, MessageStepDBList={8}]", ID, MessageGuid, FromAccountGuid, IsOutgoing, Errors, MessageID, FromAccountID, MessageSteps, MessageStepDBList);
		}

        #endregion Overrides



        #region SQLite ignores

		[Ignore]
		public new List<GeneralError> Errors {
			get {
				return base.Errors;
			}
			set {
				base.Errors = value;
			}//end get set

		}//end new GeneralError[] Errors



		[Ignore]
		public new Guid MessageID {
			get {
				return base.MessageID;
			}
			set {
				base.MessageID = value;
			}//end get set

		}//end new Guid MessageID


		[Ignore]
		public new Guid FromAccountID {
			get {
				return base.FromAccountID;
			}
			set {
				base.FromAccountID = value;
			}//end get set

		}//end new Guid FromAccountID



		[Ignore]
		public new List<MessageStep> MessageSteps {
			get {
				return base.MessageSteps;
			}
			set {
				base.MessageSteps = value;
			}//end get set

		}//end new MessageStep[] MessageSteps




		[Ignore]
		public List<MessageStepDB> MessageStepDBList {
			get;
			set;
		}//end List<MessageStepDB> MessageStepDBList




		[Ignore]
		public new List<Message.MessageRecipient> MessageRecipients {
			get {
				return base.MessageRecipients;
			}
			set {
				base.MessageRecipients = value;
			}//end get set

		}//end new Message.MessageRecipient[]




		[Ignore]
		public List<MessageRecipientDB> MessageRecipientDBList 
		{
			get;
			set;
		}//end List<MessageRecipientDB> MessageRecipientDBList

        #endregion SQLite ignores



        #region Static members

		public static Message ConvertFromMessageDB (MessageDB item)
		{

			Message toReturn = new Message ();

			toReturn.Errors = item.Errors;
			toReturn.FromAccountID = item.FromAccountID;
			toReturn.MessageID = item.MessageID;
			toReturn.MessageSent = item.MessageSent;
			toReturn.MessageConfirmed = item.MessageConfirmed;
            
			if (null != item.MessageStepDBList &&
				item.MessageStepDBList.Count > 0) {
				MessageStep[] messageSteps = new MessageStep[item.MessageStepDBList.Count];
				for (int i = 0; i < item.MessageStepDBList.Count; i++) {
					messageSteps [i] = MessageStepDB.ConvertFromMessageStepDB (item.MessageStepDBList [i]);
				}//end for

				toReturn.MessageSteps = messageSteps.ToList ();
			}//end if

			if (null != item.MessageRecipientDBList &&
				item.MessageRecipientDBList.Count > 0) {
				Message.MessageRecipient[] recipients = new MessageRecipient[item.MessageRecipientDBList.Count];
				for (int i = 0; i < item.MessageRecipientDBList.Count; i++) {
					recipients [i] = MessageRecipientDB.ConvertFromMessageRecipientDB (item.MessageRecipientDBList [i]);
				}//end for

				toReturn.MessageRecipients = recipients.ToList ();
			}//end if

			return toReturn;

		}//end static Message FromMessageItem




		public static MessageDB ConvertFromMessage (Message item)
		{

			MessageDB toReturn = new MessageDB ();
			toReturn.Errors = item.Errors;
			toReturn.FromAccountID = item.FromAccountID;
			toReturn.MessageID = item.MessageID;
			toReturn.MessageSent = item.MessageSent;
			toReturn.MessageSteps = item.MessageSteps;
			toReturn.MessageConfirmed = item.MessageConfirmed;
			toReturn.MessageStepDBList = new List<MessageStepDB> ();

			if (null != item.MessageSteps &&
				item.MessageSteps.Count > 0) 
			{

				foreach (MessageStep eachMessageStep in item.MessageSteps) 
				{
					toReturn.MessageStepDBList.Add (MessageStepDB.ConvertFromMessageStep (eachMessageStep));
				}//end foreach

			}//end if

			toReturn.MessageRecipientDBList = new List<MessageRecipientDB> ();

			if (null != item.MessageRecipients &&
				item.MessageRecipients.Count > 0) 
			{
				foreach (Message.MessageRecipient eachMessageRecipient in item.MessageRecipients) 
				{

					MessageRecipientDB msgRcpDB = MessageRecipientDB.ConvertFromMessageRecipient (eachMessageRecipient);
					msgRcpDB.MessageGuid = toReturn.MessageGuid;
					toReturn.MessageRecipientDBList.Add (msgRcpDB);

				}//end foreach
			}//end if

			return toReturn;

		}//end static MessageDB ToMessageDB
        #endregion Static members


	}
}

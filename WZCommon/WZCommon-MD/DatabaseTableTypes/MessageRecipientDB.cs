// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using LOLMessageDelivery;
using SQLite;


namespace WZCommon
{

	[Serializable()]
	public class MessageRecipientDB
	{
		#region Constructors

		public MessageRecipientDB ()
		{
		}

		#endregion Constructors



		#region Properties

		[PrimaryKey, AutoIncrement]
		public int ID
		{
			get;
			private set;
		}//end int ID



		public string AccountGuid
		{
			get;
			set;

		}//end string AccountGuid



		public string MessageGuid
		{
			get;
			set;
		}//end string MessageGuid




		public bool IsRead
		{
			get;
			set;
		}//end bool IsRead

		#endregion Properties



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[MessageRecipientDB: ID={0}, AccountGuid={1}, MessageGuid={2}, IsRead={3}]", ID, AccountGuid, MessageGuid, IsRead);
		}

		#endregion Overrides



		#region Static members

		public static Message.MessageRecipient ConvertFromMessageRecipientDB(MessageRecipientDB item)
		{

			Message.MessageRecipient toReturn = new Message.MessageRecipient();
			toReturn.AccountID = new Guid(item.AccountGuid);
			toReturn.IsRead = item.IsRead;

			return toReturn;

		}//end static Message.MessageRecipient ConvertFromMessageRecipientDB



		public static MessageRecipientDB ConvertFromMessageRecipient(Message.MessageRecipient item)
		{

			MessageRecipientDB toReturn = new MessageRecipientDB();
			toReturn.AccountGuid = item.AccountID.ToString();
			toReturn.IsRead = item.IsRead;

			return toReturn;

		}//end static MessageRecipientDB ConvertFromMessageRecipient

		#endregion Static members

	}
}


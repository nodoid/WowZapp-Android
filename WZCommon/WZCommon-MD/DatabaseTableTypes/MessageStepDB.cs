// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using LOLMessageDelivery;
using LOLMessageDelivery.Classes.LOLAnimation;
using SQLite;
using System.Collections.Generic;

namespace WZCommon
{
	
	public class MessageStepDB : MessageStep
	{

		#region Constructors

		public MessageStepDB ()
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



		[Indexed]
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



		public string StepGuid 
		{
			get 
			{
				if (this.StepID == default(Guid)) 
				{
					return Guid.Empty.ToString();
				}//end if

				return this.StepID.ToString ();
			}
			set 
			{
				this.StepID = new Guid (value);
			}//end get set

		}//end string StepGuid





		[MaxLength(500)]
		public new string MessageText 
		{
			get 
			{
				return base.MessageText;
			}
			set 
			{
				base.MessageText = value;
			}//end get set

		}//end new string MessageText

		#endregion Properties



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[MessageStepDB: ID={0}, MessageGuid={1}, StepGuid={2}, MessageText={3}, Errors={4}, MessageID={5}, StepID={6}]", ID, MessageGuid, StepGuid, MessageText, Errors, MessageID, StepID);
		}

		#endregion Overrides



		#region SQLite ignores

		[Ignore]
		public new List<GeneralError> Errors 
		{
			get 
			{
				return base.Errors;
			}
			set 
			{
				base.Errors = value;
			}//end get set

		}//end new GeneralError[] Errors



		[Ignore]
		public new Guid MessageID 
		{
			get 
			{
				return base.MessageID;
			}
			set 
			{
				base.MessageID = value;
			}//end get set
		}//end new Guid MessageID



		[Ignore]
		public new Guid StepID 
		{
			get 
			{
				return base.StepID;
			}
			set 
			{
				base.StepID = value;
			}//end get set

		}//end new Guid StepID

		#endregion SQLite ignores



		#region Static members

		public static MessageStep ConvertFromMessageStepDB (MessageStepDB item)
		{

			MessageStep toReturn = new MessageStep ();
			toReturn.ContentPackItemID = item.ContentPackItemID;
			toReturn.Errors = item.Errors;
			toReturn.MessageID = item.MessageID;
			toReturn.MessageText = item.MessageText;
			toReturn.StepID = item.StepID;
			toReturn.StepNumber = item.StepNumber;
			toReturn.StepType = item.StepType;

			return toReturn;

		}//end static MessageStep FromMessageStepItem



		public static MessageStepDB ConvertFromMessageStep (MessageStep item)
		{

			MessageStepDB toReturn = new MessageStepDB ();
			toReturn.ContentPackItemID = item.ContentPackItemID;
			toReturn.Errors = item.Errors;
			toReturn.MessageID = item.MessageID;
			toReturn.MessageText = item.MessageText;
			toReturn.StepID = item.StepID;
			toReturn.StepNumber = item.StepNumber;
			toReturn.StepType = item.StepType;

			return toReturn;

		}//end static MessageStepDB ConvertFromMessageStep

		#endregion Static members
	}
}


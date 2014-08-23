using System;
using System.Collections.Generic;
using System.Linq;
using LOLMessageDelivery;
using LOLApp_Common;
using WZCommon;
namespace wowZapp.Messages
{
	public class MessageInfo
	{
		public MessageInfo (MessageDB message, UserDB msgFrom)
		{
			this.Message = message;
			this.MessageUser = msgFrom;
			pStepTypesForStrip = new List<MessageStep.StepTypes> ();
			IsTextOnlyChecked = false;
		}//end ctor

		private List<MessageStep.StepTypes> pStepTypesForStrip;
		private bool IsTextOnlyChecked, pIsTextOnly;
		private ContentInfo pContentInfo;
	
		public MessageDB Message {
			get;
			private set;
		}

		public UserDB MessageUser {
			get;
			set;
		}

		public BubbleOrientation BubbleOrientation {
			get {
				return this.MessageUser.AccountID ==
					AndroidData.CurrentUser.AccountID ?
                        BubbleOrientation.Right :
                        BubbleOrientation.Left;
			}//end get
		}

		public bool IsTextOnly {
			get {
				if (!IsTextOnlyChecked)
					pIsTextOnly = this.Message.MessageStepDBList.Count (s => s.StepType == MessageStep.StepTypes.Text) ==
						this.Message.MessageStepDBList.Count;
				return pIsTextOnly;
			}//end get
		}//end bool IsTextOnly

		/// <summary>
		/// Gets up to the first 3 step types for the message.
		/// </summary>
		public List<MessageStep.StepTypes> StepTypesForStrip {
			get {
				if (pStepTypesForStrip.Count == 0) {
					if (this.Message.MessageStepDBList.Count > 3) {
						pStepTypesForStrip =
                        this.Message.MessageStepDBList
                            .Take (3)
                            .Select (s => s.StepType)
                            .ToList ();
					} else {
						pStepTypesForStrip =
                        this.Message.MessageStepDBList
                            .Select (s => s.StepType)
                            .ToList ();
					}//end if else
				}
				return pStepTypesForStrip;
			}//end get
		}//end List<MessageStep.StepTypes> StepTypesForStrip

		public bool IsConversationWith (MessageInfo otherMessage)
		{
			if (Message.MessageRecipientDBList.Count == otherMessage.Message.MessageRecipientDBList.Count) {
				bool toReturn = true;
				List<Guid> myUsers = GetAllUserAccountIDs ();
				List<Guid> otherUsers = otherMessage.GetAllUserAccountIDs ();
				for (int i = 0; i < myUsers.Count; ++i)
					toReturn &= myUsers [i].Equals (otherUsers [i]);
				return toReturn;
			} else
				return false;
		}//end bool IsConversationWith

		public List<Guid> GetAllUserAccountIDs ()
		{
			List<Guid> toReturn = new List<Guid> ();
			toReturn.Add (this.Message.FromAccountID);
			foreach (MessageRecipientDB eachItem in this.Message.MessageRecipientDBList)
				toReturn.Add (new Guid (eachItem.AccountGuid));

			toReturn.Sort (delegate(Guid x, Guid y) {
				return x.CompareTo (y);
			});
			return toReturn;

		}//end List<Guid> GetAllUserAccountIDs

		/// <summary>
		/// Gets the conversation unique ID.
		/// It is calculated by the participants' user IDs
		/// </summary>
		public string GetConversationID ()
		{
			string toReturn = string.Empty;
			List<string> userGuids = new List<string> ();
			this.GetAllUserAccountIDs ().ForEach (s => userGuids.Add (s.ToString ()));

			userGuids.Sort (delegate(string x, string y) {
				return x.CompareTo (y);
			});

			userGuids.ForEach (s => toReturn += s);
			return toReturn;
		}//end string GetconversationID

		public override string ToString ()
		{
			return string.Format ("[MessageInfo: Message={0}, MessageUser={1}, BubbleOrientation={2}, IsTextOnly={3}, StepTypesForStrip={4}]", Message, MessageUser, BubbleOrientation, IsTextOnly, StepTypesForStrip);
		}

		public bool HasContentInfo {
			get;
			private set;
		}//end bool HasContentInfo

		public ContentInfo ContentInfo {
			get {
				return this.pContentInfo;
			}
			set {
				this.pContentInfo = value;
				if (null != this.pContentInfo) {
					this.HasContentInfo = true;
					this.Message.MessageRecipientDBList = new List<MessageRecipientDB> ();
					this.Message.MessageStepDBList = new List<MessageStepDB> ();

					foreach (Message.MessageRecipient eachRecipient in this.pContentInfo.Message.MessageRecipients) {
						this.Message.MessageRecipientDBList.Add (MessageRecipientDB.ConvertFromMessageRecipient (eachRecipient));
					}//end foreach

					foreach (MessageStep eachMessageStep in this.pContentInfo.Message.MessageSteps) {
						this.Message.MessageStepDBList.Add (MessageStepDB.ConvertFromMessageStep (eachMessageStep));
					}//end foreach

				} else {
					this.HasContentInfo = false;
				}//end if else
			}//end get set
		}
	}
}
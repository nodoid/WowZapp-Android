using System;
using System.Collections.Generic;
using LOLApp_Common;
using LOLMessageDelivery;
using WZCommon;

namespace wowZapp.Messages
{
	public class ConversationInfoLight
	{
		public ConversationInfoLight (Guid localID, LOLMessageConversation messageConversation)
		{
			this.MessageConversation = messageConversation;
			this.Participants = new Dictionary<Guid, UserDB> ();
					
			this.LocalID = localID;
		}

		public Guid LocalID {
			get;
			private set;
		}//end Guid LocalID

		public LOLMessageConversation MessageConversation {
			get;
			private set;
		}//end LOLMessageConversation MessageConversation

		public MessageInfo LatestMessage {
			get;
			set;
		}//end MessageInfo LatestMessage

		public Dictionary<Guid, UserDB> Participants {
			get;
			private set;
		}//end List<UserDB> Participants


		public string ContactNameTitle {
			get;
			set;
		}//end string ContactNameTitle

		public string UnreadMessagesTitle {
			get;
			set;
		}//end string UnreadMessagesTitle

		public string BubbleText {
			get;
			set;
		}//end string BubbleText

	}
}

using System;
using System.Collections.Generic;
using System.Linq;

using LOLApp_Common;
using WZCommon;

namespace wowZapp.Messages
{
	public class ConversationInfo
	{
		private DBManager dbm;
		
		public ConversationInfo (List<Guid> users, Dictionary<Guid, MessageInfo> messages)
		{
			this.Users = users;
			this.Messages = messages;
			this.ConversationID = this.GetConversationID ();
			dbm = wowZapp.LaffOutOut.Singleton.dbm;
		}
		
		public string ConversationID {
			get;
			private set;
		}//end string ConversationID
		
		public List<Guid> Users {
			get;
			private set;
		}//end List<Guid> Users
		
		public Dictionary<Guid, MessageInfo> Messages {
			get;
			private set;
		}//end Dictionary<Guid, MessageInfo>
		
		public MessageInfo GetLatestMessage ()
		{
			
			if (this.Messages.Count == 1) {
				return this.Messages.First ().Value;
			} else {
				return this.Messages
					.MaxItem<KeyValuePair<Guid, MessageInfo>, DateTime> (s => s.Value.Message.MessageSent).Value;		
			}//end if else
		}//end MessageInfo GetLatestMessage
		
		public string GetConversationPartipantsNameTitle ()
		{
			string toReturn = string.Empty;
			List<UserDB> sortedList = new List<UserDB> ();
			sortedList = GetAllConversationParticipants ().OrderBy (s => s.LastName).OrderBy (s => s.FirstName).ToList ();
			foreach (UserDB eachItem in sortedList)
				toReturn += string.Format ("{0} {1}, ", eachItem.FirstName, eachItem.LastName);
			int last = toReturn.LastIndexOf (", ");
			toReturn = toReturn.Remove (last);
			
			return toReturn;
		}
		
		public UserDB GetSenderOfLatestMessage ()
		{
			List<MessageInfo> messageInfoList =
				this.Messages.Values
					.Where (s => s.Message.FromAccountID != AndroidData.CurrentUser.AccountID)
					.ToList ();
			
			if (messageInfoList.Count > 0) {
				return messageInfoList
					.MaxItem<MessageInfo, DateTime> (s => s.Message.MessageSent)
						.MessageUser;
				
			} else {
				return null;
			}//end if ese
		}//end UserDB GetSenderOfLatestMessage
		
		public UserDB GetSenderOfOldestMessage ()
		{
			List<MessageInfo> messageInfoList = new List<MessageInfo> ();
			messageInfoList =
				this.Messages.Values.Where (s => s.Message.FromAccountID != AndroidData.CurrentUser.AccountID).ToList ();
			
			if (messageInfoList.Count > 0)
				return messageInfoList.MinItem<MessageInfo, DateTime> (s => s.Message.MessageSent).MessageUser;
			else
				return null;
		}//end UserDB GetSenderOfOlderMessage
		
		public List<UserDB> GetAllConversationParticipants ()
		{
			List<UserDB> toReturn = new List<UserDB> ();
			foreach (Guid eachGuid in this.Users.Where(s => s != AndroidData.CurrentUser.AccountID)) {
				toReturn.Add (dbm.GetUserWithAccountID (eachGuid.ToString ()));
			}//end foreach
			return toReturn;
		}//end List<UserDB> GetAllMessageRecipients
		
		public bool IsInConversation (MessageInfo messageInfo)
		{
			return this.ConversationID.Equals (messageInfo.GetConversationID ());
		}//end bool IsInConversation
		
		public string GetConversationParticipantsNameTitle ()
		{
			string toReturn = string.Empty;
			List<UserDB> sortedUserList = new List<UserDB> ();
			sortedUserList =
				this.GetAllConversationParticipants ().OrderBy (s => s.LastName)
					.OrderBy (s => s.FirstName)
					.ToList ();
			
			foreach (UserDB eachItem in sortedUserList) {
				toReturn += string.Format ("{0} {1}, ", eachItem.FirstName, eachItem.LastName);
			}//end foreach
			
			int lastIndex = toReturn.LastIndexOf (", ");
			toReturn = toReturn.Remove (lastIndex);
			
			return toReturn;
		}//end string GetConversationParticipantsNameTitle
		
		public override int GetHashCode ()
		{
			int hashCode = 0;
			foreach (Guid eachGuid in this.Users) {
				hashCode ^= eachGuid.GetHashCode ();
			}//end foreach
			
			return hashCode;
		}
		
		public override bool Equals (object obj)
		{
			ConversationInfo otherObj = obj as ConversationInfo;
			if (null == otherObj) {
				return false;
			} else {
				if (this.Users.Count == otherObj.Users.Count) {
					bool equals = true;
					for (int i = 0; i < this.Users.Count; i++) {
						equals &= this.Users [i].Equals (otherObj.Users [i]);
					}//end for
					return equals;
				} else {
					return false;
				}//end if else
			}//end if else
		}
		
		public override string ToString ()
		{
			return string.Format ("[ConversationInfo: ConversationID={0}, Users={1}, Messages={2}]", ConversationID, Users, Messages);
		}
		
		public static bool operator == (ConversationInfo first, ConversationInfo second)
		{
			return first.Equals (second);
		}//end static bool operator
		
		public static bool operator != (ConversationInfo first, ConversationInfo second)
		{
			return !first.Equals (second);
		}//end static bool operator
		
		private string GetConversationID ()
		{
			string toReturn = string.Empty;
			List<string> userGuids = new List<string> ();
			this.Users.ForEach (s => userGuids.Add (s.ToString ()));
			
			userGuids.Sort (delegate(string x, string y) {
				return x.CompareTo (y);
			});
			
			userGuids.ForEach (s => toReturn += s);
			
			return toReturn;
		}//end string GetconversationID
		
		public static List<ConversationInfo> DistributeToConversations (List<MessageInfo> fromMessages)
		{
			Dictionary<string, ConversationInfo> toReturn = new Dictionary<string, ConversationInfo> ();
			for (int i = 0; i < fromMessages.Count; i++) {
				MessageInfo first = fromMessages [i];
				Dictionary<Guid, MessageInfo> conversationMessages = new Dictionary<Guid, MessageInfo> ();
				conversationMessages [first.Message.MessageID] = fromMessages [i];
				
				for (int j = i; j < fromMessages.Count; j++) {
					MessageInfo second = fromMessages [j];
					if (first.Message.MessageID != second.Message.MessageID && first.IsConversationWith (second)) {
						conversationMessages [second.Message.MessageID] = second;
					}//end if
				}//end for
				
				Dictionary<Guid, Guid> users = new Dictionary<Guid, Guid> ();
				string conversationKey = string.Empty;
				foreach (KeyValuePair<Guid, MessageInfo> eachItem in conversationMessages) {
					List<Guid> messageUsers = eachItem.Value.GetAllUserAccountIDs ();
					messageUsers.ForEach (s =>
					{
						if (!users.ContainsKey (s)) {
							users [s] = s;
							conversationKey += s.ToString ();
						}//end if
					});
				}//end foreach
				
				ConversationInfo conversationInfo = null;
				if (toReturn.TryGetValue (conversationKey, out conversationInfo)) {
					foreach (KeyValuePair<Guid, MessageInfo> eachMessageInfo in conversationMessages) {
						conversationInfo.Messages [eachMessageInfo.Key] = eachMessageInfo.Value;
					}//end foreach
					toReturn [conversationKey] = conversationInfo;
				} else {
					toReturn.Add (conversationKey, new ConversationInfo (users.Values.ToList (), conversationMessages));
				}//end if else
			}//end for
			
			return toReturn.Values.ToList ();
		}
	}
}

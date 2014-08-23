// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using LOLAccountManagement;
using SQLite;
using PreserveProps = Android.Runtime.PreserveAttribute;

namespace LOLApp_Common
{
	[PreserveProps(AllMembers=true)]
	public class ContactOAuthDB
	{

		#region Constructors

		public ContactOAuthDB ()
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



		public string OAuthID
		{
			get;
			set;
		}//end string OAuthID



		public AccountOAuth.OAuthTypes OAuthType
		{
			get;
			set;
		}//end AccountOAuth.OAuthTypes OAuthType



		[Indexed]
		public string ContactGuid
		{
			get;
			set;
		}//end string ContactGuid

		#endregion Properties



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[ContactOAuthDB: ID={0}, OAuthID={1}, OAuthType={2}, ContactGuid={3}]", ID, OAuthID, OAuthType, ContactGuid);
		}

		#endregion Overrides



		#region Static members

		public static Contact.ContactOAuth ConvertFromContactOAuthDB(ContactOAuthDB item)
		{

			Contact.ContactOAuth toReturn = new Contact.ContactOAuth();
			toReturn.OAuthID = item.OAuthID;
			toReturn.OAuthType = item.OAuthType;

			return toReturn;

		}//end static Contact.ContactOAuth ConvertFromContactOAuthDB




		public static ContactOAuthDB ConvertFromContactOAuth(Contact.ContactOAuth item)
		{

			ContactOAuthDB toReturn = new ContactOAuthDB();
			toReturn.OAuthID = item.OAuthID;
			toReturn.OAuthType = item.OAuthType;

			return toReturn;

		}//end static ContactOAuthDB ConvertFromContactOAuth

		#endregion Static members
	}
}


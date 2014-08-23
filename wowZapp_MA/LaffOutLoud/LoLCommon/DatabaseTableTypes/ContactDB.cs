// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using LOLAccountManagement;
using SQLite;
using System.Linq;
using System.Collections.Generic;
using PreserveProps = Android.Runtime.PreserveAttribute;

namespace LOLApp_Common
{

	[PreserveProps(AllMembers=true)]
	public class ContactDB : Contact
	{

		#region Constructors

		public ContactDB ()
		{
			this.ContactOAuthItems = new List<ContactOAuthDB> ();
		}

		#endregion Constructors



		#region Properties

		[PrimaryKey, AutoIncrement]
		public int ID {
			get;
			private set;
		}//end int ID




		[Indexed]
		public string ContactAccountGuid {
			get {

				if (this.ContactAccountID == default(Guid)) {
					return LOLConstants.DefaultGuid.ToString ();
				}//end if

				return this.ContactAccountID.ToString ();

			}
			set {
				this.ContactAccountID = new Guid (value);
			}//end get set

		}//end string ContactAccountGuid




		public string ContactGuid {
			get {

				if (this.ContactID == default(Guid)) {
					return LOLConstants.DefaultGuid.ToString ();
				}//end if

				return this.ContactID.ToString ();
			}
			set {
				this.ContactID = new Guid (value);
			}//end string ContactGuid

		}//end string ContactGuid



		[Indexed]
		public string OwnerAccountGuid {
			get {
				if (this.OwnerAccountID == default(Guid)) {
					return LOLConstants.DefaultGuid.ToString ();
				}//end if

				return this.OwnerAccountID.ToString ();

			}
			set {

				this.OwnerAccountID = new Guid (value);

			}//end get set

		}//end string OwnerAccountGuid

		#endregion Properties



		#region SQLite ignores

		[Ignore]
		/*
		public new GeneralError[] Errors {
			get {
				return base.Errors.ToArray ();
			}
			set {
				base.Errors = value.Select (s => s).ToList ();
			}//end get set

		}//end new GeneralError[] Errors*/

public new List<GeneralError> Errors {
			get { return base.Errors;}
			set { base.Errors = value;}
		}


		[Ignore]
		public new Guid ContactAccountID {
			get {
				return base.ContactAccountID;
			}
			set {
				base.ContactAccountID = value;
			}//end get set

		}//end new Guid ContactAccountID



		[Ignore]
		public new Guid ContactID {
			get {
				return base.ContactID;
			}
			set {
				base.ContactID = value;
			}//end get set

		}//end new Guid ContactID



		[Ignore]
		public new Guid OwnerAccountID {
			get {
				return base.OwnerAccountID;
			}
			set {
				base.OwnerAccountID = value;
			}//end get set

		}//end new Guid OwnerAccountID



		[Ignore]
		/*public new Contact.ContactOAuth[] ContactOAuths {
			get {
				return base.ContactOAuths.ToArray ();
			}
			set {
				base.ContactOAuths = value.Select (s => s).ToList ();
			}//end get set

		}*/
		public new List<Contact.ContactOAuth> ContactOAuths {
			get{ return base.ContactOAuths;}
			set{ base.ContactOAuths = value;}
		}




		[Ignore]
		public List<ContactOAuthDB> ContactOAuthItems {
			get;
			set;
		}//end List<ContactOauthItem> ContactOAuthItems




		[Ignore]
		public new User ContactUser {
			get {
				return base.ContactUser;
			}
			set {
				base.ContactUser = value;
			}//end get set

		}//end new User ContactUser



		[Ignore]
		public DateTime? LastMessageSent {
			get;
			set;
		}//end DateTime? LastMessageSent

		#endregion SQLite ignores



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[ContactDB: ID={0}, ContactAccountGuid={1}, ContactGuid={2}, OwnerAccountGuid={3}, Errors={4}, ContactAccountID={5}, ContactID={6}, OwnerAccountID={7}, ContactOAuths={8}, ContactOAuthItems={9}, ContactUser={10}]", ID, ContactAccountGuid, ContactGuid, OwnerAccountGuid, Errors, ContactAccountID, ContactID, OwnerAccountID, ContactOAuths, ContactOAuthItems, ContactUser);
		}

		#endregion Overrides



		#region Static members

		public static Contact ConvertFromContactDB (ContactDB item)
		{

			Contact toReturn = new Contact ();
			toReturn.Blocked = item.Blocked;
			toReturn.ContactAccountID = item.ContactAccountID;
			toReturn.ContactID = item.ContactID;
			toReturn.ContactOAuths = item.ContactOAuths;
			toReturn.ContactUser = item.ContactUser;
			toReturn.Errors = item.Errors;
			toReturn.OwnerAccountID = item.OwnerAccountID;
			toReturn.DateLastUpdated = item.DateLastUpdated;
			toReturn.DateCreated = item.DateCreated;
			return toReturn;

		}//end static Contact FromContactItem

		public static ContactDB ConvertFromContact (Contact item)
		{

			ContactDB toReturn = new ContactDB ();
			toReturn.Blocked = item.Blocked;
			toReturn.ContactAccountID = item.ContactAccountID;
			toReturn.ContactID = item.ContactID;

			if (null != item.ContactOAuths) {

				toReturn.ContactOAuthItems = new List<ContactOAuthDB> (item.ContactOAuths.Count);

				foreach (Contact.ContactOAuth eachOAuth in item.ContactOAuths) {
					ContactOAuthDB contactOAuthDB = ContactOAuthDB.ConvertFromContactOAuth (eachOAuth);
					contactOAuthDB.ContactGuid = toReturn.ContactGuid;
					toReturn.ContactOAuthItems.Add (contactOAuthDB);
				}//end foreach

			}//end if

			toReturn.ContactUser = item.ContactUser;
			toReturn.Errors = item.Errors;
			toReturn.OwnerAccountID = item.OwnerAccountID;
			toReturn.DateLastUpdated = item.DateLastUpdated;
			toReturn.DateCreated = item.DateCreated;
			return toReturn;

		}//end static ContactDB ConvertFromContact

		#endregion Static members
	}
}


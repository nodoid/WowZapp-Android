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
	public class UserDB : User
	{

		#region Constructors

		public UserDB ()
		{

		}

		#endregion Constructors



		#region Properties

		[PrimaryKey, AutoIncrement]
		public int ID {
			get;
			private set;
		}//end int ID




		public string AccountGuid {
			get {
				if (this.AccountID == default(Guid)) {
					return LOLConstants.DefaultGuid.ToString ();
				}//end if

				return this.AccountID.ToString ();
			}
			set {
				this.AccountID = new Guid (value);
			}//end get set

		}//end string AccountGuid

		#endregion Properties



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[UserDB: ID={0}, AccountGuid={1}, Errors={2}, AccountID={3}]", ID, AccountGuid, Errors, AccountID);
		}

		#endregion Overrides



		#region SQLite ignores

		[Ignore]
		public new List<GeneralError> Errors {
			get { return base.Errors;}
			set { base.Errors = value;}
		}


		[Ignore]
		public new Guid AccountID {
			get {
				return base.AccountID;
			}
			set {
				base.AccountID = value;
			}//end get set

		}//end new Guid AccountID

		#endregion SQLite ignores



		#region Static members

		public static User ConvertFromUserDB (UserDB item)
		{

			User toReturn = new User ();
			toReturn.AccountActive = item.AccountActive;
			toReturn.AccountID = item.AccountID;
			toReturn.DateOfBirth = item.DateOfBirth;
			toReturn.EmailAddress = item.EmailAddress;
			toReturn.Errors = item.Errors == null ? null : item.Errors;
			toReturn.FirstName = item.FirstName;
			toReturn.LastName = item.LastName;
			toReturn.Password = item.Password;
			toReturn.Picture = item.Picture;
			toReturn.PictureURL = item.PictureURL;
			toReturn.AllowLocationSearch = item.AllowLocationSearch;
			toReturn.AllowSearch = item.AllowSearch;
			toReturn.Description = item.Description;
			toReturn.DateCreated = item.DateCreated;
			toReturn.Latitude = item.Latitude;
			toReturn.Longitude = item.Longitude;
			toReturn.ShowLocation = item.ShowLocation;
			toReturn.UserGender = item.UserGender;
			toReturn.UserName = item.UserName;

			return toReturn;

		}//end static USer FromUserItem




		public static UserDB ConvertFromUser (User item)
		{

			UserDB toReturn = new UserDB ();
			toReturn.AccountActive = item.AccountActive;
			toReturn.AccountID = item.AccountID;
			toReturn.DateOfBirth = item.DateOfBirth;
			toReturn.EmailAddress = item.EmailAddress;
			toReturn.Errors = item.Errors == null ? null : item.Errors;
			toReturn.FirstName = item.FirstName;
			toReturn.LastName = item.LastName;
			toReturn.Password = item.Password;
			toReturn.Picture = item.Picture;
			toReturn.PictureURL = item.PictureURL;
			toReturn.AllowLocationSearch = item.AllowLocationSearch;
			toReturn.AllowSearch = item.AllowSearch;
			toReturn.Description = item.Description;
			toReturn.DateCreated = item.DateCreated;
			toReturn.Latitude = item.Latitude;
			toReturn.Longitude = item.Longitude;
			toReturn.ShowLocation = item.ShowLocation;
			toReturn.UserGender = item.UserGender;
			toReturn.UserName = item.UserName;
			return toReturn;

		}//end static UserDB ConvertFromUser

		#endregion Static members
	}
}


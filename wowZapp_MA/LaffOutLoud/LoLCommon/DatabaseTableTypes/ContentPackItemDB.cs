// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using LOLAccountManagement;
using SQLite;
using System.Linq;
using System.Collections.Generic;
using System;
using PreserveProps = Android.Runtime.PreserveAttribute;
namespace LOLApp_Common
{

	[PreserveProps(AllMembers=true)]
	public class ContentPackItemDB : ContentPackItem
	{

		#region Constructors

		public ContentPackItemDB ()
		{
			this.Errors = new GeneralError[0].ToList ();
		}

		#endregion Constructors



		#region Properties

		[PrimaryKey, AutoIncrement]
		public int ID {
			get;
			private set;
		}//end int ID



		public string ContentPackDataFile {
			get;
			set;
		}//end string ContentPackDataFile




		public string ContentPackItemIconFile {
			get;
			set;
		}//end string ContentPackItemIconFile



		public DateTime LastDateTimeUsed {
			get;
			set;
		}//end DateTime LastDateTimeUsed

		#endregion Properties



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[ContentPackItemDB: ID={0}, ContentPackDataFile={1}, ContentPackItemIconFile={2}, Errors={3}, ContentPackData={4}, ContentPackItemIcon={5}]", ID, ContentPackDataFile, ContentPackItemIconFile, Errors, ContentPackData, ContentPackItemIcon);
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
		public new byte[] ContentPackData {
			get {
				return base.ContentPackData;
			}
			set {
				base.ContentPackData = value;
			}//end get set

		}//end new byte[] ContentPackData




		[Ignore]
		public new byte[] ContentPackItemIcon {
			get {
				return base.ContentPackItemIcon;
			}
			set {
				base.ContentPackItemIcon = value;
			}//end get set

		}//end new byte[] ContentPackItemIcon


		#endregion SQLite ignores





		#region Static members

		public static ContentPackItem ConvertFromContentPackItemDB (ContentPackItemDB item)
		{

			ContentPackItem toReturn = new ContentPackItem ();
			toReturn.ContentItemTitle = item.ContentItemTitle;
			toReturn.ContentPackData = item.ContentPackData;
			toReturn.ContentPackID = item.ContentPackID;
			toReturn.ContentPackItemIcon = item.ContentPackItemIcon;
			toReturn.ContentPackItemID = item.ContentPackItemID;
			toReturn.Errors = item.Errors;

			return toReturn;

		}//end ContentPackItem ConvertFromContentPackItemDB




		public static ContentPackItemDB ConvertFromContentPackItem (ContentPackItem item)
		{

			ContentPackItemDB toReturn = new ContentPackItemDB ();
			toReturn.ContentItemTitle = item.ContentItemTitle;
			toReturn.ContentPackData = item.ContentPackData;
			toReturn.ContentPackID = item.ContentPackID;
			toReturn.ContentPackItemIcon = item.ContentPackItemIcon;
			toReturn.ContentPackItemID = item.ContentPackItemID;
			toReturn.Errors = item.Errors;

			return toReturn;

		}//end static ContentPackItemDb ConvertFromContentPackItem

		#endregion Static members
	}
}


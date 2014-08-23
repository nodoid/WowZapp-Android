// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using LOLAccountManagement;
using System.Linq;
using SQLite;
using System.Collections.Generic;
using PreserveProps = Android.Runtime.PreserveAttribute;

namespace LOLApp_Common
{
	[PreserveProps(AllMembers=true)]
	public class ContentPackDB : ContentPack
	{
		#region Constructors

		public ContentPackDB ()
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





		public string ContentPackAdFile {
			get;
			set;
		}//end string ContentPackAdFile




		public string ContentPackIconFile {
			get;
			set;
		}//end string ContentPackIconFile

		#endregion Properties



		#region SQLite ignores

		[Ignore]
		/*public new GeneralError[] Errors {
			get {
				return base.Errors.ToArray ();
			}
			set {
				base.Errors = value.Select (s => s).ToList ();
			}//end get set

		}*/
		public new List<GeneralError> Errors {
			get{ return base.Errors;}
			set{ base.Errors = value;}
		}




		[Ignore]
		public new byte[] ContentPackAd {
			get {
				return base.ContentPackAd;
			}
			set {
				base.ContentPackAd = value;
			}//end get set

		}//end new byte[] ContentPackAd




		[Ignore]
		public new byte[] ContentPackIcon {
			get {
				return base.ContentPackIcon;
			}
			set {
				base.ContentPackIcon = value;
			}//end get set

		}//end new byte[] ContentPackIcon


		#endregion SQLite ignores



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[ContentPackDB: ID={0}, ContentPackAdFile={1}, ContentPackIconFile={2}, Errors={3}, ContentPackAd={4}, ContentPackIcon={5}]", ID, ContentPackAdFile, ContentPackIconFile, Errors, ContentPackAd, ContentPackIcon);
		}

		#endregion Overrides



		#region Public methods



		#endregion Public methods



		#region Static members

		public static ContentPack ConvertFromContentPackDB (ContentPackDB item)
		{

			ContentPack toReturn = new ContentPack ();
			toReturn.ContentPackAd = item.ContentPackAd;
			toReturn.ContentPackAvailableDate = item.ContentPackAvailableDate;
			toReturn.ContentPackDescription = item.ContentPackDescription;
			toReturn.ContentPackEndDate = item.ContentPackEndDate;
			toReturn.ContentPackIcon = item.ContentPackIcon;
			toReturn.ContentPackID = item.ContentPackID;
			toReturn.ContentPackIsFree = item.ContentPackIsFree;
			toReturn.ContentPackPrice = item.ContentPackPrice;
			toReturn.ContentPackSaleEndDate = item.ContentPackSaleEndDate;
			toReturn.ContentPackSalePrice = item.ContentPackSalePrice;
			toReturn.ContentPackTitle = item.ContentPackTitle;
			toReturn.ContentPackTypeID = item.ContentPackTypeID;
			toReturn.Errors = item.Errors.ToList ();

			return toReturn;

		}//end static ContentPack ConvertFromContentPackDB




		public static ContentPackDB ConvertFromContentPack (ContentPack item)
		{

			ContentPackDB toReturn = new ContentPackDB ();
			toReturn.ContentPackAd = item.ContentPackAd;
			toReturn.ContentPackAvailableDate = item.ContentPackAvailableDate;
			toReturn.ContentPackDescription = item.ContentPackDescription;
			toReturn.ContentPackEndDate = item.ContentPackEndDate;
			toReturn.ContentPackIcon = item.ContentPackIcon;
			toReturn.ContentPackID = item.ContentPackID;
			toReturn.ContentPackIsFree = item.ContentPackIsFree;
			toReturn.ContentPackPrice = item.ContentPackPrice;
			toReturn.ContentPackSaleEndDate = item.ContentPackSaleEndDate;
			toReturn.ContentPackSalePrice = item.ContentPackSalePrice;
			toReturn.ContentPackTitle = item.ContentPackTitle;
			toReturn.ContentPackTypeID = item.ContentPackTypeID;
			toReturn.Errors = item.Errors;

			return toReturn;

		}//end static ContentPackDB ConvertFromContentPack

		#endregion Static members
	}
}


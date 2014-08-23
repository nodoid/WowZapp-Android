// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using SQLite;
using LOLMessageDelivery;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace WZCommon
{

	public class PollingStepDB : PollingStep
	{
		#region Constructors

		public PollingStepDB ()
		{
		}

		#endregion Constructors



		#region Properties

		[PrimaryKey, AutoIncrement]
		public int ID {
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
			set {

				this.MessageID = new Guid (value);

			}//end get set

		}//end string MessageGuid



		public string PollingData1File 
		{
			get;
			set;
		}//end string PollingData1File



		public string PollingData2File 
		{
			get;
			set;
		}//end string PollingData2File



		public string PollingData3File {
			get;
			set;
		}//end string PollingData3File



		public string PollingData4File {
			get;
			set;
		}//end string PollingData4File

		#endregion Properties



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
		public new Guid MessageID {
			get {
				return base.MessageID;
			}
			set {
				base.MessageID = value;
			}//end get set

		}//end new Guid MessageID



		[Ignore]
		public new byte[] PollingData1 {
			get {
				return base.PollingData1;
			}
			set {
				base.PollingData1 = value;
			}//end get set

		}//end new byte[] PollingData1



		[Ignore]
		public new byte[] PollingData2 {
			get {
				return base.PollingData2;
			}
			set {
				base.PollingData2 = value;
			}//end get set

		}//end new byte[] PollingData2




		[Ignore]
		public new byte[] PollingData3 {
			get {
				return base.PollingData3;
			}
			set {
				base.PollingData3 = value;
			}//end get set

		}//end new byte[] PollingData3



		[Ignore]
		public new byte[] PollingData4 {
			get {
				return base.PollingData4;
			}
			set {
				base.PollingData4 = value;
			}//end get set

		}//end new byte[] PollingData4


		#endregion SQLite ignores



		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[PollingStepDB: ID={0}, MessageGuid={1}, PollingData1File={2}, PollingData2File={3}, PollingData3File={4}, PollingData4File={5}, Errors={6}, MessageID={7}, PollingData1={8}, PollingData2={9}, PollingData3={10}, PollingData4={11}]", ID, MessageGuid, PollingData1File, PollingData2File, PollingData3File, PollingData4File, Errors, MessageID, PollingData1, PollingData2, PollingData3, PollingData4);
		}

		#endregion Overrides



		#region Static members

		public static PollingStep ConvertFromPollingStepDB (PollingStepDB item)
		{

			PollingStep toReturn = new PollingStep ();
			toReturn.Errors = item.Errors;
			toReturn.MessageID = item.MessageID;
			toReturn.PollingAnswer1 = item.PollingAnswer1;
			toReturn.PollingAnswer2 = item.PollingAnswer2;
			toReturn.PollingAnswer3 = item.PollingAnswer3;
			toReturn.PollingAnswer4 = item.PollingAnswer4;
			toReturn.PollingData1 = item.PollingData1;
			toReturn.PollingData2 = item.PollingData2;
			toReturn.PollingData3 = item.PollingData3;
			toReturn.PollingData4 = item.PollingData4;
			toReturn.PollingQuestion = item.PollingQuestion;
			toReturn.StepNumber = item.StepNumber;
			toReturn.HasResponded = item.HasResponded;

			return toReturn;

		}//end static PollingStep ConvertFromPollingStepDB




		public static PollingStepDB ConvertFromPollingStep (PollingStep item)
		{

			PollingStepDB toReturn = new PollingStepDB ();
			toReturn.Errors = item.Errors;
			toReturn.MessageID = item.MessageID;
			toReturn.PollingAnswer1 = item.PollingAnswer1;
			toReturn.PollingAnswer2 = item.PollingAnswer2;
			toReturn.PollingAnswer3 = item.PollingAnswer3;
			toReturn.PollingAnswer4 = item.PollingAnswer4;
			toReturn.PollingData1 = item.PollingData1;
			toReturn.PollingData2 = item.PollingData2;
			toReturn.PollingData3 = item.PollingData3;
			toReturn.PollingData4 = item.PollingData4;
			toReturn.PollingQuestion = item.PollingQuestion;
			toReturn.StepNumber = item.StepNumber;
			toReturn.HasResponded = item.HasResponded;

			return toReturn;

		}//end static PollingStepDb ConvertFromPollingStep


		#endregion Static members
	}
}


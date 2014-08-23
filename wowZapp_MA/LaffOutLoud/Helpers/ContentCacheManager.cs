using System;
using System.Collections.Generic;
using System.Linq;
using LOLApp_Common;
using System.IO;
using LOLAccountManagement;

using WZCommon;
namespace wowZapp
{
	public class ContentCacheManager
	{
		public ContentCacheManager ()
		{
			this.cacheLock = new object ();
			dbm = wowZapp.LaffOutOut.Singleton.dbm;
			ContentPath = wowZapp.LaffOutOut.Singleton.ContentDirectory;
		}

		private object cacheLock;
		private DBManager dbm;
		private string ContentPath;

		private bool CheckContentPackDataExist (ContentPackDB contentPack)
		{
			if (null != contentPack) {
				// We want both the icon and ad files.
				return
                    File.Exists (Path.Combine (ContentPath, contentPack.ContentPackIconFile)) &&
					File.Exists (Path.Combine (ContentPath, contentPack.ContentPackAdFile));
			} else
				return false;
		}//end bool CheckContentPackDataExist

		private bool CheckContentPackItemDataExist (ContentPackItemDB contentPackItem)
		{
			if (null != contentPackItem) {
				// We want both the icon and data files.
				return
                    File.Exists (Path.Combine (ContentPath, contentPackItem.ContentPackItemIconFile)) &&
					File.Exists (Path.Combine (ContentPath, contentPackItem.ContentPackDataFile));
			} else
				return false;
		}//end bool CheckContentPackItemDataExist

		private bool CheckContentPackItemIconExist (ContentPackItemDB contentPackItem)
		{
			if (null != contentPackItem) {
				return File.Exists (Path.Combine (ContentPath, contentPackItem.ContentPackItemIconFile));
			} else {
				return false;
			}//end if else
		}//end if

		private bool CheckVoiceFileExists (Guid msgID, int stepNumber)
		{
			return File.Exists (GetVoiceRecordingFilename (msgID, stepNumber));
		}//end bool CheckVoiceFileExists

		private bool CheckPhotoPollStepFilesExist (PollingStepDB pollingStep)
		{
			if (null != pollingStep) {
				bool exist = true;
				if (!string.IsNullOrEmpty (pollingStep.PollingData1File))
					exist &= File.Exists (Path.Combine (ContentPath, pollingStep.PollingData1File));

				if (!string.IsNullOrEmpty (pollingStep.PollingData2File))
					exist &= File.Exists (Path.Combine (ContentPath, pollingStep.PollingData2File));

				if (!string.IsNullOrEmpty (pollingStep.PollingData3File))
					exist &= File.Exists (Path.Combine (ContentPath, pollingStep.PollingData3File));

				if (!string.IsNullOrEmpty (pollingStep.PollingData4File))
					exist &= File.Exists (Path.Combine (ContentPath, pollingStep.PollingData4File));
				return exist;
			} else
				return false;
		}//end bool CheckPhotoPollStepFilesExist

		public List<ContentPackDB> GetAllLocalContentPacks (LOLCodeLibrary.GenericEnumsContentPackType contentPackType)
		{
			lock (this.cacheLock) {
				List<ContentPackDB> allContentPacks =
                    dbm.GetAllContentPacks (contentPackType);
				List<ContentPackDB> toReturn = new List<ContentPackDB> ();

				foreach (ContentPackDB eachContentPack in allContentPacks) {
					if (this.CheckContentPackDataExist (eachContentPack)) {
						eachContentPack.ContentPackIcon = GetBufferFromPropertyFile (eachContentPack.ContentPackIconFile);
						eachContentPack.ContentPackAd = GetBufferFromPropertyFile (eachContentPack.ContentPackAdFile);

						toReturn.Add (eachContentPack);
					}//end if
				}//end foreach

				return toReturn;
			}//end lock
		}//end List<ContentPackDB> GetLocalContentPacks

		public List<ContentPackItemDB> GetAllLocalContentPackItems (int forContentPackID, bool iconOnly)
		{
			lock (this.cacheLock) {
				List<ContentPackItemDB> allContentPackItms = dbm.GetAllContentPackItems (forContentPackID);
				List<ContentPackItemDB> toReturn = new List<ContentPackItemDB> ();

				foreach (ContentPackItemDB eachContentPackItem in allContentPackItms) {
					if (iconOnly) {
						if (this.CheckContentPackItemIconExist (eachContentPackItem)) {
							eachContentPackItem.ContentPackItemIcon = GetBufferFromPropertyFile (eachContentPackItem.ContentPackItemIconFile);

							if (!string.IsNullOrEmpty (eachContentPackItem.ContentPackDataFile) &&
								this.CheckContentPackItemDataExist (eachContentPackItem)) {
								eachContentPackItem.ContentPackData = GetBufferFromPropertyFile (eachContentPackItem.ContentPackDataFile);
							}//end if

							toReturn.Add (eachContentPackItem);
						}//end if

					} else {

						if (this.CheckContentPackItemIconExist (eachContentPackItem) &&
							this.CheckContentPackItemDataExist (eachContentPackItem)) {
							eachContentPackItem.ContentPackItemIcon = GetBufferFromPropertyFile (eachContentPackItem.ContentPackItemIconFile);
							eachContentPackItem.ContentPackData = GetBufferFromPropertyFile (eachContentPackItem.ContentPackDataFile);
							toReturn.Add (eachContentPackItem);
						}//end if
					}//end if else
				}//end forea
				return toReturn;
			}//end loc
		}//end List<ContentPackItemDB> GetLocalContentPackItems

		public List<ContentPackItemDB> GetLocalContentPackItems (List<int> contentPackItemIDs)
		{
			lock (this.cacheLock) {
				List<ContentPackItemDB> contentPackItems =
                    dbm.GetContentPackItems (contentPackItemIDs);

				List<ContentPackItemDB> toReturn = new List<ContentPackItemDB> ();
				foreach (ContentPackItemDB eachItem in contentPackItems) {
					if (this.CheckContentPackItemDataExist (eachItem)) {
						eachItem.ContentPackItemIcon = GetBufferFromPropertyFile (eachItem.ContentPackItemIconFile);
						eachItem.ContentPackData = GetBufferFromPropertyFile (eachItem.ContentPackDataFile);

						toReturn.Add (eachItem);
					}//end if
				}//end foreach
				return toReturn;
			}//end lock
		}//end List<ContentPackItemDB> GetLocalContentPackItems

		public List<PollingStepDB> GetAllLocalPollingSteps ()
		{
			lock (this.cacheLock) {
				List<PollingStepDB> allPollingSteps =
                    dbm.GetAllPollingSteps ();
				List<PollingStepDB> toReturn = new List<PollingStepDB> ();

				foreach (PollingStepDB eachPollingStep in allPollingSteps) {
					if (string.IsNullOrEmpty (eachPollingStep.PollingAnswer1)) {
						if (this.CheckPhotoPollStepFilesExist (eachPollingStep)) {
							if (!string.IsNullOrEmpty (eachPollingStep.PollingData1File))
								eachPollingStep.PollingData1 = GetBufferFromPropertyFile (eachPollingStep.PollingData1File);

							if (!string.IsNullOrEmpty (eachPollingStep.PollingData2File))
								eachPollingStep.PollingData2 = GetBufferFromPropertyFile (eachPollingStep.PollingData2File);

							if (!string.IsNullOrEmpty (eachPollingStep.PollingData3File))
								eachPollingStep.PollingData3 = GetBufferFromPropertyFile (eachPollingStep.PollingData3File);

							if (!string.IsNullOrEmpty (eachPollingStep.PollingData4File))
								eachPollingStep.PollingData4 = GetBufferFromPropertyFile (eachPollingStep.PollingData4File);
						} else
							continue;
					}//end if
					toReturn.Add (eachPollingStep);
				}//end foreach
				return toReturn;
			}//end lock
		}//end List<PollingStepDB> GetLocalPollingSteps

		public List<PollingStepDB> GetLocalPollingStepsForMessage (string messageGuid)
		{
			lock (this.cacheLock) {
				List<PollingStepDB> pollingSteps =
                    dbm.GetPollingSteps (new List<string> () { messageGuid });

				List<PollingStepDB> toReturn = new List<PollingStepDB> ();

				foreach (PollingStepDB eachItem in pollingSteps) {
					if (string.IsNullOrEmpty (eachItem.PollingAnswer1)) {
						if (this.CheckPhotoPollStepFilesExist (eachItem)) {
							if (!string.IsNullOrEmpty (eachItem.PollingData1File))
								eachItem.PollingData1 = GetBufferFromPropertyFile (eachItem.PollingData1File);

							if (!string.IsNullOrEmpty (eachItem.PollingData2File))
								eachItem.PollingData2 = GetBufferFromPropertyFile (eachItem.PollingData2File);

							if (!string.IsNullOrEmpty (eachItem.PollingData3File))
								eachItem.PollingData3 = GetBufferFromPropertyFile (eachItem.PollingData3File);

							if (!string.IsNullOrEmpty (eachItem.PollingData4File))
								eachItem.PollingData4 = GetBufferFromPropertyFile (eachItem.PollingData4File);
						} else
							continue;
					}//end if
					toReturn.Add (eachItem);
				}//end foreach
				return toReturn;
			}//end lock
		}//end List<PollingStepDB> GetLocalPollingSteps

		private byte[] GetBufferFromPropertyFile (string filename)
		{
			string dataFile = System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ContentDirectory, filename);
			if (!File.Exists (dataFile)) {
#if DEBUG
				System.Diagnostics.Debug.WriteLine ("File {0} doesn't exist", dataFile);
#endif
				return new byte[0];
			}
			bool rv = false;
			byte[] dataBuffer = null;
			dataBuffer = File.ReadAllBytes (dataFile);
			rv = dataBuffer == null ? true : false;
			return rv == true ? new byte[0] : dataBuffer;
		}

		public Dictionary<Guid, Dictionary<int, string>> GetLocalVoiceFiles (List<Pair<Guid, List<int>>> voiceStepCriteria)
		{
			lock (this.cacheLock) {
				Dictionary<Guid, Dictionary<int, string>> toReturn = new Dictionary<Guid, Dictionary<int, string>> ();

				foreach (Pair<Guid, List<int>> eachItem in voiceStepCriteria) {
					Dictionary<int, string> eachMessageVoiceFiles = new Dictionary<int, string> ();
					foreach (int eachStep in eachItem.ItemB) {
						if (this.CheckVoiceFileExists (eachItem.ItemA, eachStep))
							eachMessageVoiceFiles [eachStep] = GetVoiceRecordingFilename (eachItem.ItemA, eachStep);
						else
							continue;
					}//end foreach

					if (eachMessageVoiceFiles.Count > 0)
						toReturn [eachItem.ItemA] = eachMessageVoiceFiles;
				}//end foreach
				return toReturn;
			}//end lock
		}//end Dictionary<Guid, List<string>> GetLocalVoiceFiles

		public void SaveContentPack (ContentPackDB pack)
		{
			lock (this.cacheLock) {
				pack.ContentPackAdFile = StringUtils.ConstructContentPackAdFilename (pack.ContentPackID);
				pack.ContentPackIconFile = StringUtils.ConstructContentPackIconFilename (pack.ContentPackID);

				SaveContentPackBuffers (pack);
			}//end lock
		}//end void SaveContentPack

		private void SaveContentPackBuffers (ContentPackDB contentPack)
		{
			if (null != contentPack.ContentPackIcon && contentPack.ContentPackIcon.Length > 0) {
				string iconFile = Path.Combine (this.ContentPath, contentPack.ContentPackIconFile);
				if (File.Exists (iconFile))
					File.Delete (iconFile);

				byte[] iconData = contentPack.ContentPackIcon;
				try {
					File.WriteAllBytes (iconFile, iconData);
				} catch (IOException ex) {
					#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Error saving content pack icon file! {0}", ex.ToString ());
					#endif
				}
			}

			if (null != contentPack.ContentPackAd && contentPack.ContentPackAd.Length > 0) {
				string adFile = System.IO.Path.Combine (this.ContentPath, contentPack.ContentPackAdFile);

				if (System.IO.File.Exists (adFile))
					System.IO.File.Delete (adFile);

				byte[] adData = contentPack.ContentPackAd;
				try {
					File.WriteAllBytes (adFile, adData);
				} catch (IOException e) {
					#if DEBUG
					System.Diagnostics.Debug.WriteLine ("Error saving content ad file! {0}", e.ToString ());
					#endif
				}
			}
		}

		private string GetVoiceRecordingFilename (Guid msgID, int stepNumber)
		{
			return System.IO.Path.Combine (wowZapp.LaffOutOut.Singleton.ContentDirectory, string.Format (LOLConstants.VoiceRecordingFormat, msgID.ToString (), stepNumber));
		}

		public void SavePhotoPollBuffers (PollingStepDB pollStepToSave)
		{
			lock (this.cacheLock) {
				// Poll is photo poll
				if (null != pollStepToSave.PollingData1 && pollStepToSave.PollingData1.Length > 0)
					pollStepToSave.PollingData1File = StringUtils.ConstructPollingStepDataFile (1, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);

				if (null != pollStepToSave.PollingData2 && pollStepToSave.PollingData2.Length > 0)
					pollStepToSave.PollingData2File = StringUtils.ConstructPollingStepDataFile (2, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);

				if (null != pollStepToSave.PollingData3 && pollStepToSave.PollingData3.Length > 0)
					pollStepToSave.PollingData3File = StringUtils.ConstructPollingStepDataFile (3, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);

				if (null != pollStepToSave.PollingData4 && pollStepToSave.PollingData4.Length > 0)
					pollStepToSave.PollingData4File = StringUtils.ConstructPollingStepDataFile (4, pollStepToSave.MessageGuid, pollStepToSave.StepNumber);

				// Save buffers to file system
				SavePollingStepDataBuffers (pollStepToSave);
			}//end lock
		}//end void SavePhotoPollBuffer

		private void SavePollingStepDataBuffers (PollingStepDB pollingStep)
		{
			for (int i = 1; i <= 4; i++) {
				string dataFile = string.Empty;
				byte[] buffer = null;

				switch (i) {
				case 1:
					if (!string.IsNullOrEmpty (pollingStep.PollingData1File)) {
						dataFile = System.IO.Path.Combine (ContentPath, pollingStep.PollingData1File);
						buffer = pollingStep.PollingData1;
					}
					break;
				case 2:
					if (!string.IsNullOrEmpty (pollingStep.PollingData2File)) {
						dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData2File);
						buffer = pollingStep.PollingData2;
					}
					break;
				case 3:
					if (!string.IsNullOrEmpty (pollingStep.PollingData3File)) {
						dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData3File);
						buffer = pollingStep.PollingData3;
					}
					break;
				case 4:
					if (!string.IsNullOrEmpty (pollingStep.PollingData4File)) {
						dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData4File);
						buffer = pollingStep.PollingData4;
					}
					break;
				}//end switch

				if (null != buffer && buffer.Length > 0) {
					try {
						File.WriteAllBytes (dataFile, buffer);
					} catch (IOException e) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Unable to save polling step");
						#endif
					}
				}
			}
		}
	}
}


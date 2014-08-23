using System;
using System.IO;

using Android.Graphics;
using Android.OS;

using LOLApp_Common;

namespace wowZapp
{
	public class FSManager
	{
		public FSManager ()
		{
			this.SetupPaths ();
		}

		private string pContentPath;

		public string ContentPath {
			get {    
				if (string.IsNullOrEmpty (pContentPath)) {
					string cache = Android.OS.Environment.GetExternalStoragePublicDirectory (Android.OS.Environment.DirectoryDownloads).ToString ();
					pContentPath = System.IO.Path.Combine (cache, "Content");
				}
				return pContentPath;
			}
		}

		private void SetupPaths ()
		{
			if (!Directory.Exists (ContentPath)) {
				#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Will create directory: {0}", ContentPath);
				#endif
				try {
					Directory.CreateDirectory (ContentPath);
				} catch (UnauthorizedAccessException ex) {
					#if DEBUG
					System.Diagnostics.Debug.WriteLine ("UnauthorizedAccessException thrown : {0}", ex.ToString ());
					#endif
				} catch (IOException ex) {
					#if DEBUG
					System.Diagnostics.Debug.WriteLine ("IOException thrown : {0}", ex.ToString ());
					#endif
				}
			}
		}

		public void SaveContentPackBuffers (ContentPackDB contentPack)
		{
			FileStream output = null;
			if (null != contentPack.ContentPackIcon && contentPack.ContentPackIcon.Length > 0) {
				string iconFile = System.IO.Path.Combine (this.ContentPath, contentPack.ContentPackIconFile);
				if (System.IO.File.Exists (iconFile))
					System.IO.File.Delete (iconFile);

				using (Bitmap iconData = BitmapFactory.DecodeByteArray(contentPack.ContentPackIcon, 0, contentPack.ContentPackIcon.Length)) {
					output = new FileStream (iconFile, FileMode.CreateNew);
					try {
						iconData.Compress (Bitmap.CompressFormat.Png, 100, output);
					} catch (System.IO.IOException ex) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Error saving content pack icon file! {0}", ex.ToString ());
						#endif
					} finally {
						output.Close ();
					}
				}
			}

			if (null != contentPack.ContentPackAd && contentPack.ContentPackAd.Length > 0) {
				string adFile = System.IO.Path.Combine (this.ContentPath, contentPack.ContentPackAdFile);

				if (System.IO.File.Exists (adFile))
					System.IO.File.Delete (adFile);

				using (Bitmap adData = BitmapFactory.DecodeByteArray(contentPack.ContentPackAd,0,contentPack.ContentPackAd.Length)) {
					output = new FileStream (adFile, FileMode.CreateNew);
					try {
						adData.Compress (Bitmap.CompressFormat.Png, 100, output);
					} catch (System.IO.IOException ex) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Error saving content ad file! {0}", ex.ToString ());
						#endif
					} finally {
						output.Close ();
					}
				}
			}
		}

		public void SaveContentPackItemBuffers (ContentPackItemDB contentPackItem)
		{
			FileStream output = null;
			if (null != contentPackItem.ContentPackItemIcon && contentPackItem.ContentPackItemIcon.Length > 0) {
				string iconFile = System.IO.Path.Combine (this.ContentPath, contentPackItem.ContentPackItemIconFile);
				if (System.IO.File.Exists (iconFile))
					System.IO.File.Delete (iconFile);

				using (Bitmap iconData = BitmapFactory.DecodeByteArray(contentPackItem.ContentPackItemIcon,0,contentPackItem.ContentPackItemIcon.Length)) {
					output = new FileStream (iconFile, FileMode.CreateNew);
					try {
						iconData.Compress (Bitmap.CompressFormat.Png, 100, output);
					} catch (System.IO.IOException ex) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Error saving content pack icon file! {0}", ex.ToString ());
						#endif
					} finally {
						output.Close ();
					}
				}
			}
            
			if (null != contentPackItem.ContentPackData && contentPackItem.ContentPackData.Length > 0) {
				string dataFile = System.IO.Path.Combine (this.ContentPath, contentPackItem.ContentPackDataFile);
				if (System.IO.File.Exists (dataFile))
					System.IO.File.Delete (dataFile);

				using (Bitmap data = BitmapFactory.DecodeByteArray(contentPackItem.ContentPackData,0,contentPackItem.ContentPackData.Length)) {
					output = new FileStream (dataFile, FileMode.CreateNew);
					try {
						data.Compress (Bitmap.CompressFormat.Png, 100, output);
					} catch (System.IO.IOException ex) {
						#if DEBUG
						System.Diagnostics.Debug.WriteLine ("Error saving content pack item data file! {0}", ex.ToString ());
						#endif
					} finally {
						output.Close ();
					}
				}
			}
		}

		public byte[] GetBufferFromPropertyFile (string filename)
		{
			byte[] dataBuffer = File.ReadAllBytes (System.IO.Path.Combine (this.ContentPath, filename));
			if (null == dataBuffer)
				return new byte[0];
			else
				return dataBuffer;
			/*else
            {
                byte[] buffer = dataBuffer.ToArray();
                dataBuffer.Dispose();
                return buffer;
            }*/
		}

		public void SavePollingStepDataBuffers (PollingStepDB pollingStep)
		{

			for (int i = 1; i <= 4; i++) {
				string dataFile = string.Empty;
				byte[] buffer = null;

				switch (i) {
				case 1:
					if (!string.IsNullOrEmpty (pollingStep.PollingData1File)) {
						dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData1File);
						buffer = pollingStep.PollingData1;
					}//end if
					break;

				case 2:
					if (!string.IsNullOrEmpty (pollingStep.PollingData2File)) {
						dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData2File);
						buffer = pollingStep.PollingData2;
					}//end if
					break;

				case 3:
					if (!string.IsNullOrEmpty (pollingStep.PollingData3File)) {
						dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData3File);
						buffer = pollingStep.PollingData3;
					}//end if
					break;

				case 4:
					if (!string.IsNullOrEmpty (pollingStep.PollingData4File)) {
						dataFile = System.IO.Path.Combine (this.ContentPath, pollingStep.PollingData4File);
						buffer = pollingStep.PollingData4;
					}//end if
					break;
				}//end switch
			}//end for
		}//end void SavePollingStepDataBuffers


		public byte[] GetBufferFromFile (string filename)
		{
			string fileIn = System.IO.Path.Combine (this.ContentPath, filename);
			byte[] buffer = null;
			if (!System.IO.File.Exists (fileIn))
				return new byte[0];
			else
				buffer = System.IO.File.ReadAllBytes (filename);
			return buffer;
		}

		public string SaveVoiceRecordingFile (byte[] voiceBuffer, Guid msgID, int stepNumber)
		{
			string dataFile = System.IO.Path.Combine (this.ContentPath, string.Format (LOLConstants.VoiceRecordingFormat, 
                msgID.ToString (), stepNumber));

			try {
				System.IO.File.WriteAllBytes (dataFile, voiceBuffer);
			} catch (System.IO.IOException ex) {
				#if DEBUG
				System.Diagnostics.Debug.WriteLine ("Error saving voice recording file! {0}", ex.ToString ());
				#endif
			}
			return dataFile;
		}

		public string GetVoiceRecordingFilename (Guid msgID, int stepNumber)
		{
			return System.IO.Path.Combine (this.ContentPath, string.Format (LOLConstants.VoiceRecordingFormat, msgID.ToString (), 
                stepNumber));
		}

		public void CleanUpFiles ()
		{
			string[] savedFiles = Directory.GetFiles (this.ContentPath);
			foreach (string eachFile in savedFiles)
				System.IO.File.Delete (eachFile);
		}
	}
}
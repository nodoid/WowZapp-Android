// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System.IO;
using System.IO.Compression;

namespace LOLApp_Common
{
	public class Compression
	{

		#region Static members

		/// <summary>
		/// Decompresses a byte array of compressed data
		/// </summary>
		/// <param name="compressedArray">The array of bytes to decompress</param>
		/// <returns>A decompressed byte array</returns>
		public static byte[] DecompressByteArray(byte[] compressedArray)
		{

			byte[] decompressed = null;
			using (MemoryStream ms = new MemoryStream())
			{

				ms.Write(compressedArray, 0, compressedArray.Length);
				ms.Position = 0;

				int totalBytes = 0;
				using (GZipStream gZip = new GZipStream(ms, CompressionMode.Decompress, true))
				{

					//
					//determine the size of the byte array, uncompressed
					//
					byte[] segmentBuffer = new byte[50];

					while (true)
					{

						int byteCount = gZip.Read(segmentBuffer, 0, segmentBuffer.Length);

						if (byteCount == 0)
						{

							break;

						}//end if

						totalBytes += byteCount;

					}//end while

					ms.Position = 0;

				}//end using

				decompressed = new byte[totalBytes];
				using (GZipStream gZip = new GZipStream(ms, CompressionMode.Decompress))
				{

					gZip.Read(decompressed, 0, decompressed.Length);
					gZip.Close();

				}//end using

				ms.Close();

			}//end using

			return decompressed;

		}//end byte[] DecompressByteArray




		/// <summary>
		/// Compresses a byte array of uncompressed data
		/// </summary>
		/// <param name="uncompressedArray">The array of bytes to compress</param>
		/// <returns>An compressed byte array</returns>
		public static byte[] CompressByteArray(byte[] uncompressedArray)
		{

			byte[] compressed = null;
			using (MemoryStream ms = new MemoryStream())
			{

				using (GZipStream gZip = new GZipStream(ms, CompressionMode.Compress, true))
				{

					gZip.Write(uncompressedArray, 0, uncompressedArray.Length);
					gZip.Close();

				}//end using

				ms.Position = 0;

				compressed = new byte[ms.Length];
				ms.Read(compressed, 0, compressed.Length);
				ms.Close();

			}//end using

			return compressed;

		}//end byte[] CompressByteArray

		#endregion Static members
	}
}


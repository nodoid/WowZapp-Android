// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LOLApp_Common
{
	public class BinaryUtils
	{
		/// <summary>
		/// Serializes an object
		/// </summary>
		/// <param name="data">The object to serialize</param>
		/// <returns>The specified object in a byte array</returns>
		public static byte[] SerializeObject (object data)
		{
			
			byte[] toReturn = null;
			using (MemoryStream ms = new MemoryStream()) {
				
				BinaryFormatter bf = new BinaryFormatter ();
				
				bf.Serialize (ms, data);
				
				ms.Position = 0;
				
				toReturn = new byte[ms.Length];
				
				ms.Read (toReturn, 0, toReturn.Length);
				ms.Close ();
				
			}//end using
			
			return toReturn;
			
		}//end byte[] Ser?alizeBuffer




		/// <summary>
		/// Deserialiazes an object
		/// </summary>
		/// <param name="buffer">The serialized object</param>
		/// <returns>The object ready for use</returns>
		public static object DeserializeObject (byte[] buffer)
		{
			
			object toReturn = null;
			using (MemoryStream ms = new MemoryStream()) {
				
				BinaryFormatter bf = new BinaryFormatter ();
				
				ms.Write (buffer, 0, buffer.Length);
				ms.Position = 0;
				
				toReturn = bf.Deserialize (ms);
				
				ms.Close ();
				
			}//end using
			
			return toReturn;
			
		}//end object DeserialiazeObject
	}
}


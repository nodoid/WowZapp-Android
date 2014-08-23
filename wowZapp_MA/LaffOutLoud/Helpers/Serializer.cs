// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System.Xml.Serialization;
using System.IO;

namespace wowZapp
{
	public class Serializer
	{


		public static void XmlSerializeObject<T>(T obj, string filePath)
		{

			using (StreamWriter sw = new StreamWriter(filePath))
			{

				XmlSerializer xmlSer = new XmlSerializer(typeof(T));
				xmlSer.Serialize(sw, obj);

			}//end using sw

		}//end static void SerializeObject



		public static T XmlDeserializeObject<T>(string filePath)
		{

			using (StreamReader sr = new StreamReader(filePath))
			{

				XmlSerializer xmlSer = new XmlSerializer(typeof(T));
				return (T)xmlSer.Deserialize(sr);

			}//end using sr

		}//end static T XmlDeserializeObject<T>
	}
}


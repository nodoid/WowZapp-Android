using System;
using System.Text;
using System.Net;
using System.IO;

namespace LOLApp_Common
{
	public interface IHttpHelper
	{
		string Post(string url, string postdata);
		string Get(string url);
	}




	public class HttpHelper : IHttpHelper
	{

		public HttpHelper()
		{

		}//end HttpHelper



		public string Post(string url, string postdata)
		{

			try
			{

				byte[] postDataBuffer = Encoding.ASCII.GetBytes(postdata);
				HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = postDataBuffer.Length;

				using (Stream s = request.GetRequestStream())
				{
					s.Write(postDataBuffer, 0, postDataBuffer.Length);
				}//end using

				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
				{
					return sr.ReadToEnd();
				}//end using sr

			} catch (Exception ex)
			{

				throw ex;

			}//end try catch

		}//end string Post




		public string Get(string url)
		{

			try
			{

				HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
				request.Method = "GET";
	         
				using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
				{
					return sr.ReadToEnd();
				}//end using sr

			} catch (Exception ex)
			{
				throw ex;
			}//end try catch

		}//end string Get

	}//end class HttpHelper


}
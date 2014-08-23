// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;


namespace LOLApp_Common
{
	public class CertValidator
	{
		public static bool Validator (object sender, X509Certificate certificate, X509Chain chain, 
                                      SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}


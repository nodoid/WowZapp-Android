// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Net;

namespace WZCommon
{
	public class RequestState<T>
	{
		
		public RequestState(HttpWebRequest request, T state)
		{
			this.Request = request;
			this.State = state;
		}
		
		
		
		
		public HttpWebRequest Request
		{
			
			get;
			private set;
			
		}//end public HttpWebRequest Request
		
		
		
		
		public T State
		{
			
			get;
			private set;
			
		}//end public T State
		
	}
}


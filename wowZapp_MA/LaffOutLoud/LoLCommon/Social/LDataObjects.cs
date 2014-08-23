// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System.Net;


namespace LOLApp_Common
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




	/*public class Pair<TItemA, TItemB>
	{

		public Pair(TItemA itemA, TItemB itemB)
		{

			this.ItemA = itemA;
			this.ItemB = itemB;

		}//end ctor



		public TItemA ItemA
		{
			get;
			set;
		}//end TItemA ItemA



		public TItemB ItemB
		{
			get;
			set;
		}//end TItemB ItemB

	}*/

}


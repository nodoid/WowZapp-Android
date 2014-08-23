using System;

namespace LOLApp_Common
{
	public class NewContact
	{
			
		public Guid AccountID { get; set; }
			
		public string FullName { get; set; }
			
		public string FirstName { get; set; }
			
		public string LastName { get; set; }
			
		public bool Blocked { get; set; }
			
		public DateTime LastMessageDate { get; set; }
			
		public Android.Graphics.Bitmap ProfilePicture { get; set; }
			
		public Guid ContactID { get; set; }
			
		public string OwnerAccountID { get; set; }
			
	}
}


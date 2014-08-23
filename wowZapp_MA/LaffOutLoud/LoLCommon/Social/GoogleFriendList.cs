using System;
using System.Collections.Generic;
using System.Linq;
using LOLAccountManagement;
using System.Xml.Linq;

namespace LOLApp_Common
{
	public class FriendList
	{
		//private string p;
		//private JsonValue NextToken;

		public FriendList (string access_token, int count = 20)
		{
			NextStart = count;
			this.CountPerRequest = count;
			//NextToken = null;
			this.AccessToken = access_token;
			string requestUrl = string.Format ("https://www.google.com/m8/feeds/contacts/default/thin?alt=json&access_token={0}&start-index={1}&max-results={2}&v=3.0",
                              AccessToken, 1, CountPerRequest);
		}
   
       
		public int NextStart { get; set; }
 

		public string AccessToken { get; set; }
		public int CountPerRequest { get; set; }
		static string atomNS = "http://www.w3.org/2005/Atom";
		static string googleNS = "http://schemas.google.com/g/2005";
		static string googleContactNS = "http://schemas.google.com/contact/2008";

		public static IEnumerable<Contact> ParseFriendsResponseGoogle (string responseStr)
		{

			List<Contact> toReturn = new List<Contact> ();

			XDocument xDoc = XDocument.Parse (responseStr);

			XElement feedElement = xDoc.Element (XName.Get ("feed", atomNS));


			var entryElements = feedElement.Elements (XName.Get ("entry", atomNS));
			XElement node = entryElements.First ();

			while (node != null) {
				Contact contactItem = GoogleXElementToContact (node);
				yield return contactItem;
				node = (XElement)node.NextNode;
			}

		}

		private static Contact GoogleXElementToContact (XElement eachElement)
		{
			string fullName = string.Empty;
			string lastName = string.Empty;
			string firstName = string.Empty;
			string email = string.Empty;
			string googleProfile = string.Empty;
			string profilePicture = string.Empty;

			XElement nameElement = eachElement.Element (XName.Get ("name", googleNS));
			if (null != nameElement) {
				XElement fullNameElement = nameElement.Element (XName.Get ("fullName", googleNS));
				if (null != fullNameElement) {
					fullName = fullNameElement.Value;
				}//end if

				XElement familyNameElement = nameElement.Element (XName.Get ("familyName", googleNS));
				if (null != familyNameElement) {
					lastName = familyNameElement.Value;
				}//end if

				if (!string.IsNullOrEmpty (fullName) &&
					!string.IsNullOrEmpty (lastName)) {
					firstName = fullName.Replace (lastName, "").Trim ();
				}//end if
			}//end if

			XElement emailElement = eachElement.Element (XName.Get ("email", googleNS));
			if (null != emailElement) {
				XAttribute emailAddressAttr = emailElement.Attribute (XName.Get ("address"));
				if (null != emailAddressAttr) {
					email = emailAddressAttr.Value;
				}//end if

			}//end if

			XElement googleProfileElement = eachElement.Element (XName.Get ("website", googleContactNS));
			googleProfile = googleProfileElement != null ? googleProfileElement.Attribute ("href").Value : string.Empty;

			XElement photoLinkElement = eachElement.Elements (XName.Get ("link", atomNS))
                .Where (s => s.Attribute ("rel").Value.Contains ("photo"))
                    .Where (s => s.Attribute (XName.Get ("etag", googleNS)) != null)
                    .FirstOrDefault ();

			if (photoLinkElement != null) {
				profilePicture = photoLinkElement.Attribute ("href").Value;
			}
            

			User userObj = new User ();
			userObj.DateOfBirth = new DateTime (1900, 1, 1);
			userObj.EmailAddress = email;
			userObj.FirstName = firstName;
			userObj.LastName = lastName;
			userObj.PictureURL = profilePicture;

			Contact contactItem = new Contact ();
			contactItem.ContactOAuths = new Contact.ContactOAuth[] {

					new Contact.ContactOAuth () {
						OAuthID = !string.IsNullOrEmpty(googleProfile) ?
							googleProfile.Substring(googleProfile.LastIndexOf("/") + 1) :
								string.Empty,
						OAuthType = AccountOAuth.OAuthTypes.Google
					}
				}.ToList ();
			contactItem.ContactUser = userObj;
			return contactItem;
		}



	}
}
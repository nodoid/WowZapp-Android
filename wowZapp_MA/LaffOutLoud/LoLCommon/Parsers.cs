// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using LOLAccountManagement;
using System.Json;
using System.Xml.Linq;
using System.Linq;
using WZCommon;

namespace LOLApp_Common
{
    public class Parsers
    {

        public static List<Contact> ParseFriendsResponseFacebook(string responseStr)
        {

            List<Contact> toReturn = new List<Contact>();

            if (!string.IsNullOrEmpty(responseStr))
            {

                JsonValue responseJson = JsonValue.Parse(responseStr);
                if (responseJson.ContainsKey("data"))
                {
                    JsonArray dataJsonArr = (JsonArray)responseJson ["data"];
	
                    if (dataJsonArr.Count > 0)
                    {
	
                        for (int i = 0; i < dataJsonArr.Count; i++)
                        {
                            string firstName = dataJsonArr [i].ContainsKey("first_name") ? (string)dataJsonArr [i] ["first_name"] : string.Empty;
                            string lastName = dataJsonArr [i].ContainsKey("last_name") ? (string)dataJsonArr [i] ["last_name"] : string.Empty;
                            string id = (string)dataJsonArr [i] ["id"];
                            DateTime dateOfBirth = new DateTime(1900, 1, 1);
	
                            User userObj = new User();
                            if (dataJsonArr [i].ContainsKey("birthday"))
                            {
                                try
                                {
                                    string dateStr = (string)dataJsonArr ["birthday"];
                                    DateTime.TryParse(dateStr, out dateOfBirth);
                                } catch
                                {
                                    #if(DEBUG)
                                    Console.WriteLine("Error getting birth date for: {0} {1}", firstName, lastName);
                                    #endif
                                }//end try catch
                            }//end if
                            userObj.FirstName = firstName;
                            userObj.LastName = lastName;
                            userObj.PictureURL = id;
                            userObj.DateOfBirth = dateOfBirth;
	
                            Contact contactItem = new Contact();
                            contactItem.ContactOAuths = new Contact.ContactOAuth[] {
	
								new Contact.ContactOAuth() { OAuthID = id, OAuthType = AccountOAuth.OAuthTypes.FaceBook }
							}.ToList();
                            contactItem.ContactUser = userObj;
	
                            toReturn.Add(contactItem);
		
                        }//end for
	
                    }//end if
	
                }//end if

            }//end if

            return toReturn;

        }//end static List<Contact> ParseFriendsResponseFacebook





        public static List<Contact> ParseFriendsResponseGoogle(string responseStr, out int totalContactCount)
        {

            List<Contact> toReturn = new List<Contact>();

            XDocument xDoc = XDocument.Parse(responseStr);
            string openSearchNS = "http://a9.com/-/spec/opensearch/1.1/";
            string atomNS = "http://www.w3.org/2005/Atom";
            string googleNS = "http://schemas.google.com/g/2005";
            string googleContactNS = "http://schemas.google.com/contact/2008";
            XElement feedElement = xDoc.Element(XName.Get("feed", atomNS));

            totalContactCount = Convert.ToInt32(feedElement.Element(XName.Get("totalResults", openSearchNS)).Value);
			
            var entryElements = feedElement.Elements(XName.Get("entry", atomNS));
			
            foreach (XElement eachElement in entryElements)
            {

                string fullName = string.Empty;
                string lastName = string.Empty;
                string firstName = string.Empty;
                string email = string.Empty;
                string googleProfile = string.Empty;
                string profilePicture = string.Empty;

                XElement googleProfileElement = eachElement.Element(XName.Get("website", googleContactNS));
                googleProfile = googleProfileElement != null ? googleProfileElement.Attribute("href").Value : string.Empty;

                if (null == googleProfileElement)
                {
                    continue;
                }//end if

                XElement nameElement = eachElement.Element(XName.Get("name", googleNS));

                if (null != nameElement)
                {
                    XElement fullNameElement = nameElement.Element(XName.Get("fullName", googleNS));
                    if (null != fullNameElement)
                    {
                        fullName = fullNameElement.Value;
                    }//end if

                    XElement familyNameElement = nameElement.Element(XName.Get("familyName", googleNS));
                    if (null != familyNameElement)
                    {
                        lastName = familyNameElement.Value;
                    }//end if

                    if (!string.IsNullOrEmpty(fullName) &&
                        !string.IsNullOrEmpty(lastName))
                    {
                        firstName = fullName.Replace(lastName, "").Trim();
                    }//end if
                }//end if

                XElement emailElement = eachElement.Element(XName.Get("email", googleNS));
                if (null != emailElement)
                {
                    XAttribute emailAddressAttr = emailElement.Attribute(XName.Get("address"));
                    if (null != emailAddressAttr)
                    {
                        email = emailAddressAttr.Value;
                    }//end if

                }//end if

                XElement photoLinkElement = eachElement.Elements(XName.Get("link", atomNS))
					.Where(s => s.Attribute("rel").Value.Contains("photo"))
						.Where(s => s.Attribute(XName.Get("etag", googleNS)) != null)
						.FirstOrDefault();

                if (photoLinkElement != null)
                {
                    profilePicture = photoLinkElement.Attribute("href").Value;
                }//end if

                //NOTE: CAUTION! Both firstName and lastName can be empty.
                //If that is the case, assign email address as firstName, for display only (so, not in this method)!
                //Do NOT assign email address as first name in the user object! <- Need to ask Andrew if we can do that.
                User userObj = new User();
                userObj.DateOfBirth = new DateTime(1900, 1, 1);
                userObj.EmailAddress = email;
                userObj.FirstName = firstName;
                userObj.LastName = lastName;
                userObj.PictureURL = profilePicture;

                Contact contactItem = new Contact();
                contactItem.ContactOAuths = new Contact.ContactOAuth[] {

					new Contact.ContactOAuth() {
						OAuthID = !string.IsNullOrEmpty(googleProfile) ?
							googleProfile.Substring(googleProfile.LastIndexOf("/") + 1) :
								string.Empty,
						OAuthType = AccountOAuth.OAuthTypes.Google
					}
				}.ToList();
                contactItem.ContactUser = userObj;

                toReturn.Add(contactItem);

            }//end foreach

            return toReturn;

        }//end static List<Contact> ParseFriendsResponse



        /*public static List<VideoAlbumInfo> ParseYouTubeVideosResponse(string responseStr, out int videoCount)
		{
			
			List<VideoAlbumInfo> toReturn = new List<VideoAlbumInfo>();
			
			if (!string.IsNullOrEmpty(responseStr))
			{

				XDocument xDoc = XDocument.Parse (responseStr);
				string openSearchNS = "http://a9.com/-/spec/opensearchrss/1.0/";
				string atomNS = "http://www.w3.org/2005/Atom";
				string mediaNS = "http://search.yahoo.com/mrss/";
				string ytNS = "http://gdata.youtube.com/schemas/2007";
				XElement feedElement = xDoc.Element (XName.Get ("feed", atomNS));

				videoCount = Convert.ToInt32(feedElement.Element(XName.Get("totalResults", openSearchNS)).Value);

				var entryElements = feedElement.Elements(XName.Get("entry", atomNS));

				foreach (XElement eachElement in entryElements)
				{

					string id = string.Empty;

					XElement idElement = eachElement.Element(XName.Get("id", atomNS));
					if (null != idElement)
					{
						id = idElement.Value;
					}//end if

					DateTime publishedDate = DateTime.MinValue;
					XElement publishedElement = eachElement.Element(XName.Get("published", atomNS));
					if (null != publishedElement)
					{
						DateTime.TryParse(publishedElement.Value, out publishedDate);
					}//end if

					string title = string.Empty;

					XElement titleElement = eachElement.Element(XName.Get("title", atomNS));
					if (null != titleElement)
					{
						title = titleElement.Value;
					}//end if

					string sourceUrl = string.Empty;
					string thumbUrl = string.Empty;

					XElement mediaGroupElement = eachElement.Element(XName.Get("group", mediaNS));

					XElement contentElement = mediaGroupElement.Element(XName.Get("content", mediaNS));
					sourceUrl = contentElement.Attribute("url").Value;

					XElement thumbElement = mediaGroupElement.Element(XName.Get("thumbnail", mediaNS));
					if (null != thumbElement)
					{
						thumbUrl = thumbElement.Attribute("url").Value;
					}//end if

					double duration = 0;
					XElement durationElement = eachElement.Element(XName.Get("duration", ytNS));
					if (null != durationElement)
					{
						Double.TryParse(durationElement.Attribute("seconds").Value, out duration);
					}//end if

					toReturn.Add(new VideoAlbumInfo(id, title, thumbUrl, sourceUrl, publishedDate == DateTime.MinValue ? (DateTime?)null : publishedDate, VideoSource.YouTube) {

						Duration = duration

					});

				}//end foreach
				
			} else
			{

				videoCount = 0;

			}//end if else
			
			return toReturn;
			
		}//end static List<VideoAlbumInfo>
*/




        public static List<Contact> ParseFriendsResponseLinkedIn(string responseStr, out int totalContactCount)
        {

            List<Contact> toReturn = new List<Contact>();

            XDocument xDoc = XDocument.Parse(responseStr);
            XElement connectionsElement = xDoc.Element(XName.Get("connections"));
            totalContactCount = Int32.Parse(connectionsElement.Attribute(XName.Get("total")).Value);

            var personElements = connectionsElement.Elements(XName.Get("person"));
            foreach (XElement eachElement in personElements)
            {

                string id = string.Empty;
                string firstName = string.Empty;
                string lastName = string.Empty;
                string profilePictureUrl = string.Empty;
                DateTime dateOfBirth = new DateTime(1900, 1, 1);

                XElement idElement = eachElement.Element("id");
                XElement firstNameElement = eachElement.Element("first-name");
                XElement lastNameElement = eachElement.Element("last-name");
                XElement profilePictureElement = eachElement.Element("picture-url");
                XElement dateOfBirthElement = eachElement.Element("date-of-birth");

                if (null != idElement)
                {
                    id = idElement.Value;
                }//end if

                if (null != firstNameElement)
                {
                    firstName = firstNameElement.Value;
                }//end if

                if (null != lastNameElement)
                {
                    lastName = lastNameElement.Value;
                }//end if

                if (null != profilePictureElement)
                {
                    profilePictureUrl = profilePictureElement.Value;
                }//end if

                if (null != dateOfBirthElement)
                {
                    DateTime.TryParse(dateOfBirthElement.Value, out dateOfBirth);
                }//end if

                User userObj = new User();
                userObj.DateOfBirth = dateOfBirth;
                userObj.FirstName = firstName;
                userObj.LastName = lastName;
                userObj.PictureURL = profilePictureUrl;

                Contact contactItem = new Contact();
                contactItem.ContactOAuths = new Contact.ContactOAuth[] {

					new Contact.ContactOAuth() {
						OAuthID = id,
						OAuthType = AccountOAuth.OAuthTypes.LinkedIn
					}
				}.ToList();
                contactItem.ContactUser = userObj;

                toReturn.Add(contactItem);

            }//end foreach

            return toReturn;

        }//end static List<Contact> ParseFriendsResponse




        public static List<PhotoAlbumInfo> ParseFacebookPhotoAlbumsResponse(string responseStr)
        {

            List<PhotoAlbumInfo> toReturn = new List<PhotoAlbumInfo>();

            if (!string.IsNullOrEmpty(responseStr))
            {

                JsonValue responseJson = JsonValue.Parse(responseStr);
                if (responseJson.ContainsKey("data"))
                {

                    JsonArray albumArr = (JsonArray)responseJson ["data"];
                    if (albumArr.Count > 0)
                    {

                        for (int i = 0; i < albumArr.Count; i++)
                        {

                            string id = albumArr [i].ContainsKey("id") ? (string)albumArr [i] ["id"] : string.Empty;
                            string name = albumArr [i].ContainsKey("name") ? (string)albumArr [i] ["name"] : string.Empty;

                            int itemCount = 0;
                            if (albumArr [i].ContainsKey("count"))
                            {

                                try
                                {

                                    itemCount = (int)albumArr [i] ["count"];

                                } catch (Exception ex)
                                {
#if(DEBUG)
                                    Console.WriteLine("Could not get item count for album: {0} Message: {1}--{2}", name, ex.Message, ex.StackTrace);
#endif
                                }//end try catch

                            }//end if

                            PhotoAlbumInfo albumInfoItem = new PhotoAlbumInfo(id, name, itemCount);
                            toReturn.Add(albumInfoItem);

                        }//end for

                    }//end if
                }//end if
            }//end if

            return toReturn;

        }//end static List<PhotoAlbumInfo> ParsePhotoAlbumsResponse



        public static List<VideoAlbumInfo> ParseFacebookVideoAlbumsResponse(string responseStr)
        {

            List<VideoAlbumInfo> toReturn = new List<VideoAlbumInfo>();

            if (!string.IsNullOrEmpty(responseStr))
            {

                JsonValue responseJson = JsonValue.Parse(responseStr);
                if (responseJson.ContainsKey("data"))
                {

                    JsonArray albumArr = (JsonArray)responseJson ["data"];
                    if (albumArr.Count > 0)
                    {

                        for (int i = 0; i < albumArr.Count; i++)
                        {

                            string id = albumArr [i].ContainsKey("id") ? (string)albumArr [i] ["id"] : string.Empty;
                            string description = albumArr [i].ContainsKey("description") ? (string)albumArr [i] ["description"] : string.Empty;
                            string pictureUrl = albumArr [i].ContainsKey("picture") ? (string)albumArr [i] ["picture"] : string.Empty;
                            string sourceUrl = albumArr [i].ContainsKey("source") ? (string)albumArr [i] ["source"] : string.Empty;
                            DateTime createdDate = default(DateTime);
                            DateTime? createdDateVal = null;

                            if (albumArr [i].ContainsKey("created_time"))
                            {

                                if (DateTime.TryParse((string)albumArr [i] ["created_time"], out createdDate))
                                {
                                    createdDateVal = createdDate;
                                }//end if

                            }//end try catch

                            VideoAlbumInfo vidInfo = new VideoAlbumInfo(id, description, pictureUrl, sourceUrl, createdDateVal);
                            toReturn.Add(vidInfo);

                        }//end for

                    }//end if

                }//end if

            }//end if

            return toReturn;

        }//end List<VideoAlbumIndo> ParseFacebookVideoAlbumsResponse












        public static List<PhotoInfo> ParseFacebookPhotosResponse(string responseStr)
        {

            List<PhotoInfo> toReturn = new List<PhotoInfo>();

            if (!string.IsNullOrEmpty(responseStr))
            {

                JsonValue responseJson = JsonValue.Parse(responseStr);
                if (responseJson.ContainsKey("data"))
                {

                    JsonArray photoArr = (JsonArray)responseJson ["data"];
                    if (photoArr.Count > 0)
                    {

                        for (int i = 0; i < photoArr.Count; i++)
                        {

                            string id = photoArr [i].ContainsKey("id") ? (string)photoArr [i] ["id"] : string.Empty;
                            string smallUrl = photoArr [i].ContainsKey("picture") ? (string)photoArr [i] ["picture"] : string.Empty;
                            string largeUrl = photoArr [i].ContainsKey("source") ? (string)photoArr [i] ["source"] : string.Empty;

                            PhotoInfo photoInfoItem = new PhotoInfo(id, string.Empty, smallUrl, largeUrl);
                            toReturn.Add(photoInfoItem);

                        }//end for
                    }//end if
                }//end if
            }//end if

            return toReturn;

        }//end static List<PhotoInfo> ParseFacebookPhotosResponse
    }
}


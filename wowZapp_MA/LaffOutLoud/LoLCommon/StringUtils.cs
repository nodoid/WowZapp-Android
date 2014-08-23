// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Text.RegularExpressions;
using LOLAccountManagement;
using LOLMessageDelivery;
using System.Xml;
using System.Web;
using System.Text;
using System.Collections.Generic;
using WZCommon;
using System.Drawing;
using LOLMessageDelivery.Classes.LOLAnimation;


namespace LOLApp_Common
{
    public class StringUtils
    {
        public static bool IsEmailAddress(string emailAddressStr)
        {

            Match emailMatch = Regex.Match(emailAddressStr, LOLConstants.RegexEmailMatch);
            return emailMatch.Success;

        }//end static bool IsEmailAddress



        public static string CreateErrorMessageFromGeneralErrors(List<LOLAccountManagement.GeneralError> errors)
        {

            string toReturn = string.Empty;
            foreach (LOLAccountManagement.GeneralError eachError in errors)
            {
                toReturn += string.Format("{0}--{1}--{2}", eachError.ErrorTitle, eachError.ErrorDescription, eachError.ErrorNumber);
            }//end foreach

            return toReturn;

        }//end static string CreateErrorMessageFromGeneralErrors



        public static string CreateErrorMessageFromMessageGeneralErrors(List<LOLMessageDelivery.GeneralError> msgErrors)
        {

            string toReturn = string.Empty;
            foreach (LOLMessageDelivery.GeneralError eachError in msgErrors)
            {
                toReturn += string.Format("{0}--{1}--{2}", eachError.ErrorTitle, eachError.ErrorDescription, eachError.ErrorNumber);
            }//end foreach

            return toReturn;

        }//end static string CreateErrorMessageFromMessageGeneralErrors




        public static string CreateLinkedInXMLMessage(string title, string lolAppUrl, User owner, string userProfileUrl)
        {

            // This is for creating a Share
//			string xml = 
//				"<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
//				"<share>" +
//				"<content>" +
//					string.Format("<title>{0}</title>", title) +
//					string.Format("<submitted-url>{0}</submitted-url>", lolAppUrl) +
//				"</content>" +
//				"<visibility>" +
//					"<code>anyone</code>" +
//				"</visibility>" +
//				"</share>";

            // This is for creating an activity
            string body = 
				HttpUtility.HtmlEncode(string.Format("<a href=\"{0}\">{1}</a> {2} <a href=\"{3}\">WowZapp</a>.",
				              userProfileUrl, owner.FirstName + " " + owner.LastName, title, lolAppUrl));

            string xml = 
				"<activity locale=\"en_US\">" +
                "<content-type>linkedin-html</content-type>" +
                string.Format("<body>{0}</body>", body) +
                "</activity>";


            return xml;

        }//end static string CreateLinkedInXMLMessage




        public static ConnectServiceErrors GetErrorForErrorNumberStr(string errorNumberStr)
        {

            int errorNo = -1;
            if (Int32.TryParse(errorNumberStr, out errorNo))
            {
                if (Enum.IsDefined(typeof(ConnectServiceErrors), errorNo))
                {
                    return (ConnectServiceErrors)errorNo;
                } else
                {
                    return ConnectServiceErrors.None;
                }//end if else
            } else
            {
                return ConnectServiceErrors.Unknown;
            }//end if else

        }//end static ConnectServiceErrors GetErrorForErrorNumberStr



        public static string CreateLocalizedDateTimeStr(DateTime dateTime)
        {

            return dateTime.ToLocalTime().ToString("MM/dd/yy H:mm:ss");

        }//end static string CreateLocalizedDateTimeStr



        public static string ConstructContentPackIconFilename(int contentPackID)
        {

            return string.Format(LOLConstants.ContentPackIconFormat, contentPackID);

        }//end string ConstructIconFilename



        public static string ConstructContentPackAdFilename(int contentPackID)
        {

            return string.Format(LOLConstants.ContentPackAdFormat, contentPackID);

        }//end string ConstructAdFilename



        public static string ConstructContentPackItemIconFilename(int contentPackItemID)
        {

            return string.Format(LOLConstants.ContentPackItemIconFormat, contentPackItemID);

        }//end string ConstructContentPackItemIconFilename



        public static string ConstructContentPackItemDataFile(int contentPackItemID)
        {

            return string.Format(LOLConstants.ContentPackItemDataFormat, contentPackItemID);

        }//end string ConstructContentPackItemDataFile



        public static string ConstructPollingStepDataFile(int dataNumber, string messageGuid, int stepNumber)
        {

            return string.Format(LOLConstants.PollingStepDataFormat, dataNumber, messageGuid, stepNumber);

        }//end static string ConstructPollingStepDataFile




        public static string ConstructAnimationObjectDataFile(string animationGuid, string messageGuid, int stepNumber)
        {

            return string.Format(LOLConstants.AnimationObjectDataFormat, animationGuid, messageGuid, stepNumber);

        }//end static string ConstructAnimationObjectDataFile



        /// <summary>
        /// Creates and returns the text parameters for the given callout text area.
        /// </summary>
        /// <returns>
        /// A Pair<UIFont, SizeF> containing the font and text size.
        /// </returns>
        /// <param name='textRect'>
        /// The current text area of the callout.
        /// </param>
        /// <param name='text'>
        /// The text for which to calculate and create the parameters.
        /// </param>
        /*public static Pair<UIFont, SizeF> GetTextParamsForCallout(RectangleF textRect, string text)
        {
			
            Pair<UIFont, SizeF> toReturn = new Pair<UIFont, SizeF>(UIFont.FromName("Arial", 12f), SizeF.Empty);
			
            using (NSString textStr = new NSString(text))
            {
				
                SizeF constrainSize = new SizeF(textRect.Width, 9999);
                toReturn.ItemB = textStr.StringSize(toReturn.ItemA, constrainSize, UILineBreakMode.WordWrap);
                float minHeight = textRect.Height - 8f;
                float minWidth = textRect.Width - 8f;
				
                if (toReturn.ItemB.Width > minWidth)
                {
					
                    do
                    {
						
                        toReturn.ItemA = UIFont.FromName("Arial", toReturn.ItemA.PointSize - 1f);
                        toReturn.ItemB = textStr.StringSize(toReturn.ItemA, constrainSize, UILineBreakMode.WordWrap);
						
                    } while (toReturn.ItemB.Width > minWidth);
					
                } else if (toReturn.ItemB.Width < minWidth)
                {
					
                    do
                    {
						
                        toReturn.ItemA = UIFont.FromName("Arial", toReturn.ItemA.PointSize + 1f);
                        toReturn.ItemB = textStr.StringSize(toReturn.ItemA, constrainSize, UILineBreakMode.WordWrap);
						
                    } while (toReturn.ItemB.Width < minWidth);
					
                }//end if else
				
                if (toReturn.ItemB.Height > minHeight - 2f)
                {
					
                    do
                    {
						
                        toReturn.ItemA = UIFont.FromName("Arial", toReturn.ItemA.PointSize - 0.1f);
                        toReturn.ItemB = textStr.StringSize(toReturn.ItemA, constrainSize, UILineBreakMode.WordWrap);
						
                    } while (toReturn.ItemB.Height > minHeight - 2f);
					
                } else if (toReturn.ItemB.Height < minHeight + 2f)
                {
					
                    do
                    {
						
                        toReturn.ItemA = UIFont.FromName("Arial", toReturn.ItemA.PointSize + 0.1f);
                        toReturn.ItemB = textStr.StringSize(toReturn.ItemA, constrainSize, UILineBreakMode.WordWrap);
						
                    } while (toReturn.ItemB.Height < minHeight + 2f);
					
                }//end if else
				
            }//end using textStr
			
            return toReturn;
			
        }//end Pair<UIFont, SizeF> GetTextParamsForCallout






        public static SizeF AdjustTextToSize(string text, SizeF box, float edgeInset, ref UIFont forFont)
        {
			
            SizeF constrainSize = new SizeF(box.Width - edgeInset, 9999);
            SizeF toReturn = SizeF.Empty;
            UIFont sampleFont = forFont;
			
            using (NSString nsText = new NSString(text))
            {
				
                SizeF textSize = nsText.StringSize(forFont, constrainSize, UILineBreakMode.WordWrap);
				
                if (textSize.Height > box.Height - edgeInset)
                {
					
                    do
                    {
						
                        float pointSize = sampleFont.PointSize;
                        pointSize -= 0.5f;
                        sampleFont = UIFont.FromName(sampleFont.Name, pointSize);
                        textSize = nsText.StringSize(sampleFont, constrainSize, UILineBreakMode.WordWrap);
						
                    } while (textSize.Height > box.Height - edgeInset);
					
                    toReturn = textSize;
					
                } else if (textSize.Height < box.Height - edgeInset)
                {
					
                    do
                    {
						
                        float pointSize = sampleFont.PointSize;
                        pointSize += 0.5f;
                        sampleFont = UIFont.FromName(sampleFont.Name, pointSize);
                        textSize = nsText.StringSize(sampleFont, constrainSize, UILineBreakMode.WordWrap);
						
                    } while (textSize.Height < box.Height - edgeInset);
					
                    toReturn = textSize;
					
                } else
                {
					
                    toReturn = textSize;
					
                }//end if else
				
            }//end using nsText
			
            forFont = sampleFont;
            return toReturn;
			
        }//end static SizeF GetTextSize




        public static TransitionDescription CreateTransitionDescriptionForEffect(AnimationTypesTransitionEffectType efType)
        {
			
            TransitionDescription toReturn = null;
			
            switch (efType)
            {
				
                case AnimationTypesTransitionEffectType.Move:
				
                    toReturn = 
					new TransitionDescription(
						AppDelegate.Self.LB.LocalizedString("Transition.Move", string.Empty, "Localizable"),
						AppDelegate.Self.LB.LocalizedString("Transition.Move.Description", string.Empty, "Localizable"),
						UIImage.FromBundle(ImgConstants.TransitionMove),
						efType);
				
                    break;
				
                case AnimationTypesTransitionEffectType.Scale:
				
                    toReturn = 
					new TransitionDescription(
						AppDelegate.Self.LB.LocalizedString("Transition.Scale", string.Empty, "Localizable"),
						AppDelegate.Self.LB.LocalizedString("Transition.Scale.Description", string.Empty, "Localizable"),
						UIImage.FromBundle(ImgConstants.TransitionScale),
						efType);
				
                    break;
				
                case AnimationTypesTransitionEffectType.Rotate:
				
                    toReturn = 
					new TransitionDescription(
						AppDelegate.Self.LB.LocalizedString("Transition.Rotation", string.Empty, "Localizable"),
						AppDelegate.Self.LB.LocalizedString("Transition.Rotation.Description", string.Empty, "Localizable"),
						UIImage.FromBundle(ImgConstants.TransitionRotate),
						efType);
				
                    break;
				
                case AnimationTypesTransitionEffectType.FadeIn:
				
                    toReturn = 
					new TransitionDescription(
						AppDelegate.Self.LB.LocalizedString("Transition.FadeIn", string.Empty, "Localizable"),
						AppDelegate.Self.LB.LocalizedString("Transition.FadeIn.Description", string.Empty, "Localizable"),
						UIImage.FromBundle(ImgConstants.TransitionFadeIn),
						efType);
				
                    break;
				
                case AnimationTypesTransitionEffectType.FadeOut:
				
                    toReturn = 
					new TransitionDescription(
						AppDelegate.Self.LB.LocalizedString("Transition.FadeOut", string.Empty, "Localizable"),
						AppDelegate.Self.LB.LocalizedString("Transition.FadeOut.Description", string.Empty, "Localizable"),
						UIImage.FromBundle(ImgConstants.TransitionFadeOut),
						efType);
				
                    break;
				
                default:
				
                    throw new InvalidOperationException(string.Format("Don't know what to do with transition effect: {0}", efType));
				
            }//end switch
			
			
            return toReturn;
			
        }//end static TransitionDescription CreateTransitionDescriptionForEffect
*/
    }
}


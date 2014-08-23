using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using LOLApp_Common;
using LOLAccountManagement;
using Android.Util;
using Android.Graphics;

using WZCommon;

namespace wowZapp.Contacts
{
    public static class ContactsUtil
    {
        public static List<ContactDB> contacts
        {
            get;
            set;
        }
		
        public static List<string> contactFilenames
        {
            get;
            set;
        }
    }

    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class GetConnectedActivity : Activity, IDisposable
    {
        private AccountOAuth.OAuthTypes networkType;
        private List<Contact> contacts;
        private List<Contact> selectedContacts;
        private List<Contact> searchResults;
        private int totalFriendCount;
        private bool isFind, gl, gg, gf;
        private Context context;
        private DBManager db;
        public ISocialProviderManager Provider { get; private set; }
        private const int FACEBOOK_BACK = 1, GOOGLE_BACK = 2, LINKEDIN_BACK = 3;
        private ProgressDialog dialog;
        private ScrollView scroller;
        private EditText search;
        public LinearLayout listContainer;

        //DON'T TOUCH these, they are not strings for display, just attributes I use to detect checks on ImageView
        private const string CHECKBOX_CHECKED = "Selected", CHECKBOX_UNCHECKED = "Unselected";
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.GetConnectedContacts);

            db = wowZapp.LaffOutOut.Singleton.dbm;

            isFind = base.Intent.GetBooleanExtra("isfind", true);
            selectedContacts = new List<Contact>();
            searchResults = new List<Contact>();
            contacts = new List<Contact>();

            if (ContactsUtil.contacts == null)
                ContactsUtil.contacts = new List<ContactDB>();

            search = FindViewById<EditText>(Resource.Id.editSearch);
            Button cont = FindViewById<Button>(Resource.Id.btnGetConnectedContinue);
            scroller = FindViewById<ScrollView>(Resource.Id.scrollViewContainer);
            listContainer = FindViewById<LinearLayout>(Resource.Id.linearListContainer);

            context = scroller.Context;

            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewUserHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
			
            Header.headertext = Application.Context.Resources.GetString(Resource.String.getconnectedTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            int netType = base.Intent.GetIntExtra("type", 0);
            switch (netType)
            {
                case 1:
                    this.networkType = AccountOAuth.OAuthTypes.Google;
                    if (string.IsNullOrEmpty(AndroidData.GooglePlusAccessToken))
                    {
                        this.Provider = new LGooglePlusManager(LOLConstants.GooglePlusConsumerKey, LOLConstants.GooglePlusConsumerSecret, new HttpHelper());
                        gg = false;
                    } else
                    {
                        this.Provider = new LGooglePlusManager(LOLConstants.GooglePlusConsumerKey, LOLConstants.GooglePlusConsumerSecret, AndroidData.GooglePlusAccessToken, AndroidData.GooglePlusRefreshToken, 
                            AndroidData.GoogleAccessTokenExpiration, new HttpHelper());
                        gg = true;
                    }
                    break;

                case 2:
                    this.networkType = AccountOAuth.OAuthTypes.LinkedIn;
                    if (string.IsNullOrEmpty(AndroidData.LinkedInAccessToken))
                    {
                        this.Provider = new LLinkedInManager(LOLConstants.LinkedInConsumerKey, LOLConstants.LinkedInConsumerSecret, string.Empty, string.Empty);
                        gl = false;
                    } else
                    {
                        this.Provider = new LLinkedInManager(LOLConstants.LinkedInConsumerKey, LOLConstants.LinkedInConsumerSecret, AndroidData.LinkedInAccessToken, AndroidData.LinkedInAccessTokenSecret);
                        gl = true;
                    }
                    break;

                case 3:
                    this.networkType = AccountOAuth.OAuthTypes.FaceBook;
                    if (string.IsNullOrEmpty(AndroidData.FacebookAccessToken))
                    {
                        gf = false;
                        this.Provider = new LFacebookManager(LOLConstants.FacebookAPIKey, LOLConstants.FacebookAppSecret);
                    } else
                    {
                        this.Provider = new LFacebookManager(LOLConstants.FacebookAPIKey, LOLConstants.FacebookAppSecret, AndroidData.FacebookAccessToken, AndroidData.FacebookAccessTokenExpiration);
                        gf = true;
                    }
                    break;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() => SetDataSources());

            cont.Click += (object s, EventArgs e) => {
                cont_Click(); };
            search.Clickable = true;
            search.TextChanged += delegate
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("Text changed - now is {0}", search.Text);
                #endif
                FilterContacts();
            };
        }

        private void SetDataSources()
        {
            this.contacts = new List<Contact>();
            ThreadPool.QueueUserWorkItem(delegate
            {
                this.DownloadContacts();
            });
        }

        private void DownloadContacts()
        {
            try
            {
                switch (this.networkType)
                {
                    case AccountOAuth.OAuthTypes.FaceBook:
                        if (gf == false)
                        {
                            Intent auth = new Intent(this, typeof(GetConnectedAuthActivity));
                            auth.PutExtra("type", FACEBOOK_BACK);
                            StartActivityForResult(auth, FACEBOOK_BACK);
                        } else
                            GetFacebookFriends();
                        break;
                    case AccountOAuth.OAuthTypes.Google:
                        if (gg == false)
                        {
                            Intent auth = new Intent(this, typeof(GetConnectedAuthActivity));
                            auth.PutExtra("type", GOOGLE_BACK);
                            StartActivityForResult(auth, GOOGLE_BACK);
                        } else
                            GetGooglePlusFriends();
                        break;
                    case AccountOAuth.OAuthTypes.LinkedIn:
                        if (gl == false)
                        {
                            Intent auth = new Intent(this, typeof(GetConnectedAuthActivity));
                            auth.PutExtra("type", LINKEDIN_BACK);
                            StartActivityForResult(auth, LINKEDIN_BACK);
                        } else
                            GetLinkedInFriends();
                        break;
                }
            } catch (Exception ex)
            {
                RunOnUiThread(delegate
                {
                    string m = string.Format("{0} {1}",
                    string.Format(Application.Context.GetString(Resource.String.errorDownloadingFriendsFormat),
                                  this.Provider.ProviderType), ex.Message);
                    GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), m);
                });
            }
        }

        private void GetFacebookFriends()
        {
            LFacebookManager fMan = (LFacebookManager)this.Provider;
            RunOnUiThread(() => dialog = ProgressDialog.Show(context, Application.Context.GetString(Resource.String.contactsGetFacebook),
                                                                      Application.Context.GetString(Resource.String.contactsFinding), true));

            if (DateTime.Now.CompareTo(AndroidData.FacebookAccessTokenExpiration) > -1)
            {
                try
                {
                    if (fMan.RefreshAccessToken())
                    {
                        AndroidData.FacebookAccessToken = fMan.AccessToken;
                        AndroidData.FacebookAccessTokenExpiration = fMan.AccessTokenExpirationTime.Value;
                    }
                } catch (Exception ex)
                {
                    string m = string.Format("{0} {1}",
                                      string.Format(Application.Context.GetString(Resource.String.errorRefreshingAccessTokenFormat),
                                      this.Provider.ProviderType),
                                      ex.Message);
                    RunOnUiThread(delegate
                    {
                        if (dialog != null)
                            dialog.Dismiss();
                        GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), m);
                    });
                    return;
                }
            }

            string responseStr = fMan.GetAllUserFriends();
            if (string.IsNullOrEmpty(responseStr))
                noFriends();
            this.contacts = Parsers.ParseFriendsResponseFacebook(responseStr);
            contacts = sortList(contacts);
            propogateListView(contacts);
        }

        private void GetGooglePlusFriends()
        {
            LGooglePlusManager gMan = (LGooglePlusManager)this.Provider;
            RunOnUiThread(delegate
            {
                dialog = ProgressDialog.Show(context, Application.Context.GetString(Resource.String.contactsGetGoogle), Application.Context.GetString(Resource.String.contactsFinding), true);
            });

            if (DateTime.Now.CompareTo(AndroidData.GoogleAccessTokenExpiration) > -1)
            {
                try
                {
                    if (gMan.RefreshAccessToken())
                    {
                        AndroidData.GooglePlusAccessToken = gMan.AccessToken;
                        AndroidData.GoogleAccessTokenExpiration = gMan.AccessTokenExpirationTime.Value;
                    }
                } catch (Exception ex)
                {
                    string m = string.Format("{0} {1}",
                                      string.Format(Application.Context.GetString(Resource.String.errorRefreshingAccessTokenFormat),
                                      this.Provider.ProviderType),
                                      ex.Message);
                    RunOnUiThread(delegate
                    {
                        if (dialog != null)
                            dialog.Dismiss();
                        GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), m);
                    });
                    return;
                }
            }

            if (string.IsNullOrEmpty(gMan.AccessToken))
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine("AccessToken is null");
                #endif
                return;
            }

            string responseXml = gMan.GetAllUserFriends();
            if (string.IsNullOrEmpty(responseXml))
                noFriends();
            this.contacts = Parsers.ParseFriendsResponseGoogle(responseXml, out this.totalFriendCount);
            contacts = sortList(contacts);
            propogateListView(contacts);
        }

        private void GetLinkedInFriends()
        {
            LLinkedInManager lMan = (LLinkedInManager)this.Provider;

            RunOnUiThread(delegate
            {
                dialog = ProgressDialog.Show(context, Application.Context.GetString(Resource.String.contactsLinkedIn), Application.Context.GetString(Resource.String.contactsFinding), true);
            });

            string responseXml = lMan.GetAllUserFriends();
            if (string.IsNullOrEmpty(responseXml))
                noFriends();
            this.contacts = Parsers.ParseFriendsResponseLinkedIn(responseXml, out this.totalFriendCount);
            contacts = sortList(contacts);
            propogateListView(contacts);
        }
		
        private void noFriends()
        {
            RunOnUiThread(delegate
            {
                //Alert (context, Application.Context.GetString (Resource.String.errorNoContactsTitle), Application.Context.GetString (Resource.String.errorNoContacts));
                Finish();
            });
        }
        
        private void propogateListView(List<Contact> contacts)
        {
            RunOnUiThread(delegate
            {
                if (contacts.Count != 0)
                {
                    for (int n = 0; n < contacts.Count; ++n)
                    {
                        LinearLayout layout = new LinearLayout(context);
                        layout.Orientation = Android.Widget.Orientation.Horizontal;
                        layout.SetGravity(GravityFlags.Center);
                        layout.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), (int)ImageHelper.convertDpToPixel(10f, context));

                        ImageView profilepic = new ImageView(context);
                        profilepic.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(40f, context), (int)ImageHelper.convertDpToPixel(40f, context));
                        profilepic.Tag = new Java.Lang.String("profilepic_" + n.ToString());
                        layout.AddView(profilepic);

                        TextView text = new TextView(context);
                        text.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(235f, context), (int)ImageHelper.convertDpToPixel(40f, context));
                        text.SetPadding((int)ImageHelper.convertDpToPixel(10f, context), 0, (int)ImageHelper.convertDpToPixel(10f, context), 0);
                        text.Gravity = GravityFlags.CenterVertical;
                        text.TextSize = 16f;
                        text.SetTextColor(Android.Graphics.Color.White);
                        if (contacts [n].ContactUser.FirstName != "" || contacts [n].ContactUser.LastName != "")
                        {
                            text.Text = contacts [n].ContactUser.FirstName + " " + contacts [n].ContactUser.LastName;
                        } else
                        {
                            text.Text = contacts [n].ContactUser.EmailAddress;
                        }
                        layout.AddView(text);

                        ImageView checkbox = new ImageView(context);
                        checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.checkbox));
                        checkbox.LayoutParameters = new ViewGroup.LayoutParams((int)ImageHelper.convertDpToPixel(25f, context), (int)ImageHelper.convertDpToPixel(25f, context));
                        checkbox.ContentDescription = CHECKBOX_UNCHECKED;

                        int contactId = new int();
                        contactId = n;
                        layout.Clickable = true;
                        layout.Click += (object s, EventArgs e) => {
                            handleContactClick(checkbox, contactId); };
                        layout.AddView(checkbox);

                        this.listContainer.AddView(layout);
                    }
                }

                if (dialog != null)
                    dialog.Dismiss();
            });

            for (int n = 0; n < contacts.Count; ++n)
            {
                loadProfilePicture(n);
            }
        }

        private void loadProfilePicture(int contactId)
        {
            float size = ImageHelper.convertDpToPixel(56f, context);
            byte[] imgdata = Provider.GetUserImage(contacts [contactId].ContactUser.PictureURL);
            using (Bitmap image = ImageHelper.CreateUserProfileImageForDisplay(imgdata, (int)size, (int)size, this.Resources))
            {
                RunOnUiThread(delegate
                {
                    ImageView pic = (ImageView)listContainer.FindViewWithTag(new Java.Lang.String("profilepic_" + contactId.ToString()));
                    pic.SetImageBitmap(image);
                });
            }
        }

        private void handleContactClick(ImageView checkbox, int contactId)
        {
            if (checkbox.ContentDescription == CHECKBOX_UNCHECKED)
            {
                checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.@checkedbox));
                checkbox.ContentDescription = CHECKBOX_CHECKED;
                selectedContacts.Add(contacts [contactId]);
            } else
            {
                checkbox.SetImageDrawable(Application.Context.Resources.GetDrawable(Resource.Drawable.checkbox));
                checkbox.ContentDescription = CHECKBOX_UNCHECKED;
                selectedContacts.Remove(contacts [contactId]);
            }
        }

        private List<Contact> sortList(List<Contact> source)
        {
            source = source.Distinct().ToList();
            var c = source.OrderBy(person => person.ContactUser.LastName).ThenBy(person => person.ContactUser.FirstName);
            List<Contact> tmpList = new List<Contact>();
            foreach (Contact cc in c)
                tmpList.Add(cc);
            contacts = tmpList;
            return contacts;
        }
        
        private void cont_Click()
        {
            if (selectedContacts.Count > 0)
            {
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.ContactsSearchListCompleted += Service_ContactsSearchListCompleted;

                List<LOLConnectSearchCriteria> searchCriteria = new List<LOLConnectSearchCriteria>();

                this.selectedContacts.ForEach(sc =>
                {
                    Contact.ContactOAuth scAuth = default(Contact.ContactOAuth);
                    for (int i = 0; i < sc.ContactOAuths.Count; i++)
                    {
                        if (sc.ContactOAuths [i].OAuthType == this.networkType)
                        {
                            scAuth = sc.ContactOAuths [i];
                            break;
                        }//end if
                    }//end for

                    searchCriteria.Add(new LOLConnectSearchCriteria()
                    {
                        FirstName = sc.ContactUser.FirstName ?? string.Empty,
                        LastName = sc.ContactUser.LastName ?? string.Empty,
                        EmailAddress = sc.ContactUser.EmailAddress ?? string.Empty,
                        OAuthID = scAuth.OAuthID,
                        OAuthType = scAuth.OAuthType
                    });
                });

                service.ContactsSearchListAsync(searchCriteria, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
            } else
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError),
                                   Application.Context.GetString(Resource.String.contactsNoneSelected));
                });
            }
        }

        private void Service_ContactsSearchListCompleted(object sender, ContactsSearchListCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.ContactsSearchListCompleted -= Service_ContactsSearchListCompleted;

            RunOnUiThread(() => Toast.MakeText(context, Resource.String.commonOneSec, ToastLength.Short).Show());

            if (null == e.Error)
            {
                LOLConnectSearchResult[] results = e.Result.ToArray();

                foreach (LOLConnectSearchResult eachItem in results)
                {
                    Contact foundContact = null;
                    foreach (Contact eachContact in this.selectedContacts)
                    {
                        foreach (Contact.ContactOAuth eachOAuth in eachContact.ContactOAuths)
                        {
                            if (eachOAuth.OAuthType == this.networkType &&
                                eachOAuth.OAuthID == eachItem.SearchedCriteria.OAuthID)
                            {
                                foundContact = eachContact;
                            }//end if
                        }//end foreach
                    }

                    foundContact.ContactUser = eachItem.ContactUser;
                    foundContact.Blocked = false;
                    foundContact.ContactAccountID = eachItem.ContactUser.AccountID;
                    foundContact.OwnerAccountID = AndroidData.CurrentUser.AccountID;
                    this.searchResults.Add(foundContact);
                }//end foreach

                // Search through selected contacts to find friends
                // that are not included in the search results.
                List<Contact> notAppOwners = new List<Contact>();
                foreach (Contact eachContact in this.selectedContacts)
                {
                    Contact.ContactOAuth selectedContactAuth = default(Contact.ContactOAuth);
                    for (int i = 0; i < eachContact.ContactOAuths.Count; i++)
                    {
                        if (eachContact.ContactOAuths [i].OAuthType == this.networkType)
                        {
                            selectedContactAuth = eachContact.ContactOAuths [i];
                            break;
                        }//end if
                    }//end for


                    bool existsInResults = false;
                    foreach (LOLConnectSearchResult eachResult in results)
                    {
                        if (eachResult.SearchedCriteria.OAuthID == selectedContactAuth.OAuthID)
                        {
                            existsInResults = true;
                        }//end if
                    }//end foreach

                    if (!existsInResults)
                    {
                        notAppOwners.Add(eachContact);
                    }//end if
                }//end foreach

                this.SendInvites(notAppOwners);
                this.SaveFoundContacts();
            } else
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), string.Format("{0} {1}",
                                  Application.Context.GetString(Resource.String.errorSearchingContactsFailed), e.Error.Message));
                });
            }//end if else
        }

        private void SendInvites(List<Contact> toContactsList)
        {
            int count = 0;
            RunOnUiThread(delegate
            {
                dialog = new ProgressDialog(context);
                dialog.SetMessage(Application.Context.GetString(Resource.String.invitePBMessage));
                dialog.SetTitle(Application.Context.GetString(Resource.String.invitePBTitle));
                dialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                dialog.Max = toContactsList.Count;
                dialog.Progress = 0;
                dialog.Show();
            });
            switch (this.networkType)
            {

                case AccountOAuth.OAuthTypes.FaceBook:

                    foreach (Contact eachContact in toContactsList)
                    {
                        try
                        {
                            string oauthID = string.Empty;
                            for (int i = 0; i < eachContact.ContactOAuths.Count; i++)
                            {
                                if (eachContact.ContactOAuths [i].OAuthType == AccountOAuth.OAuthTypes.FaceBook)
                                {
                                    oauthID = eachContact.ContactOAuths [i].OAuthID;
                                    break;
                                }
                            }
#if(DEBUG)
                            this.Provider.PostToFeed("Testing, nevermind. http://www.example.com", oauthID);
#else
						this.Provider.PostToFeed (string.Format (
							Application.Context.GetString (Resource.String.inviteFacebookPostMessageFormat),
							LOLConstants.LinkLOLAppWebsiteUrl), oauthID);
#endif
                            RunOnUiThread(delegate
                            {
                                dialog.Progress = (int)(++count / toContactsList.Count);
                            });
                        } catch (Exception ex)
                        {
#if(DEBUG)
                            System.Diagnostics.Debug.WriteLine("Exception inviting user: {0} {1}\n{2}--{3}",
                                              eachContact.ContactUser.FirstName,
                                              eachContact.ContactUser.LastName,
                                              ex.Message,
                                              ex.StackTrace);
#endif
                        }
                    }
                    break;

                case AccountOAuth.OAuthTypes.Google:
                case AccountOAuth.OAuthTypes.YouTube:
                    if (toContactsList.Count > 0)
                    {
                        List<LOLConnectInviteEmail> emailInvites = new List<LOLConnectInviteEmail>();
                        emailInvites =
                            toContactsList
                                .Select(s =>
                        {
                            return new LOLConnectInviteEmail()
                                    {
                                        ContactName = string.Format("{0} {1}", s.ContactUser.FirstName, s.ContactUser.LastName),
                                        EmailAddress = s.ContactUser.EmailAddress,
                                        OAuthType = this.networkType
                                    };
                        })
                                .ToList();

                        //RunOnUiThread(delegate { dialog.Progress = (int)(++count / toContactsList.Count); });
                        RunOnUiThread(() => Toast.MakeText(context, Application.Context.GetString(Resource.String.commonSendInvite), ToastLength.Short).Show());
                        LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                        service.ContactsSendInviteEmailCompleted += Service_ContactsSendInviteEmailCompleted;
                        service.ContactsSendInviteEmailAsync(emailInvites);
                    }//end if
                    break;

                case AccountOAuth.OAuthTypes.LinkedIn:
                    if (toContactsList.Count > 0)
                    {
                        try
                        {
                            LLinkedInManager lMan = (LLinkedInManager)this.Provider;
                            lMan.PostToFeed(
                                StringUtils.CreateLinkedInXMLMessage(Application.Context.GetString(Resource.String.inviteLinkedInPostMessage),
                                                                 LOLConstants.LinkLOLAppWebsiteUrl,
                                                                 AndroidData.CurrentUser, lMan.GetUserProfileUrl()), string.Empty);

                            RunOnUiThread(delegate
                            {
                                dialog.Progress = (int)(++count / toContactsList.Count);
                            });

                        } catch (Exception ex)
                        {
#if(DEBUG)
                            System.Diagnostics.Debug.WriteLine("Exception creating LinkedIn activity: {0}--{1}",
                                              ex.Message,
                                              ex.StackTrace);
#endif
                        }
                    }
                    break;
            }
            RunOnUiThread(delegate
            {
                if (dialog != null)
                    dialog.Dismiss();
            });
        }

        private void Service_ContactsSendInviteEmailCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.ContactsSendInviteEmailCompleted -= Service_ContactsSendInviteEmailCompleted;

            if (null == e.Error)
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Email invites sent!");
#endif				
                RunOnUiThread(() => Toast.MakeText(context, Application.Context.GetString(Resource.String.commonSendInviteSent), ToastLength.Short).Show());

            } else
            {
                string m = string.Format("{0} {1}", Application.Context.GetString(Resource.String.errorSendingInvitations),
                        e.Error.Message);
                RunOnUiThread(() => GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), m)); 
            }
        }

        private void SaveFoundContacts()
        {
            RunOnUiThread(() =>
            {
                Toast.MakeText(context, Resource.String.chooseFriendsAddingSelectedContacts, ToastLength.Short);
            });
            AccountOAuth accountOAuth = new AccountOAuth();

            try
            {
                this.Provider.GetUserInfoObject(accountOAuth);
            } catch (Exception ex)
            {

#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Error retrieving user info object.");
#endif
                string m = string.Format("{0} {1}",
                        Application.Context.GetString(Resource.String.errorSavingContacts),
                        ex.Message);
                RunOnUiThread(() => GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), m));
                return;
            }

            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            service.AccountOAuthCreateCompleted += this.Service_AccountOAuthCreateCompleted;
            service.AccountOAuthCreateAsync(AndroidData.CurrentUser.AccountID,
                                            accountOAuth.OAuthType,
                                            accountOAuth.OAuthID,
                                            accountOAuth.OAuthToken,
                                            new Guid(AndroidData.ServiceAuthToken));
        }

        private void Service_AccountOAuthCreateCompleted(object sender, AccountOAuthCreateCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.AccountOAuthCreateCompleted -= Service_AccountOAuthCreateCompleted;

            if (null == e.Error)
            {
                AccountOAuth result = e.Result;
                if (result.Errors.Count > 0)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), 
                                      StringUtils.CreateErrorMessageFromGeneralErrors(result.Errors).ToString());
                    });
                } else
                {
                    service.ContactsSaveListCompleted += Service_ContactsSaveListCompleted;
                    service.ContactsSaveListAsync(this.searchResults, new Guid(AndroidData.ServiceAuthToken));
                }//end if else
            } else
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), string.Format("{0} {1}",
                                   Application.Context.GetString(Resource.String.errorSavingContacts), e.Error.Message));
                });

            }//end if else
        }

        private void Service_ContactsSaveListCompleted(object sender, ContactsSaveListCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.ContactsSaveListCompleted -= Service_ContactsSaveListCompleted;

            RunOnUiThread(() => Toast.MakeText(context, Resource.String.contactsPDTitle, ToastLength.Short));

            if (null == e.Error)
            {
                Contact[] result = e.Result.ToArray();
                List<Contact> contactList = new List<Contact>();
                foreach (Contact eachContact in result)
                {
                    if (eachContact.Errors.Count > 0)
                    {
                        #if DEBUG
                        System.Diagnostics.Debug.WriteLine("Error saving contact: {0}", StringUtils.CreateErrorMessageFromGeneralErrors(eachContact.Errors).ToString());
                        #endif
                    } else
                    {
                        contactList.Add(eachContact);
                    }//end if else
                }

                // Save the contacts to the local database
                foreach (Contact user in result)
                {
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ImageDirectory, user.ContactUser.AccountID.ToString()), 
					                              user.ContactUser.Picture);
                    Contacts.ContactsUtil.contactFilenames.Add(user.ToString());
                }
                db.InserOrUpdateContacts(contactList);
                RunOnUiThread(delegate
                {
                    SetResult(Result.Ok);
                    Finish();
                });
            } else
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError), string.Format("{0} {1}",
                                   Application.Context.GetString(Resource.String.errorSavingContacts), e.Error.Message));
                });

            }//end if else
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
           
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {

                case FACEBOOK_BACK:

                    if (resultCode == Result.Ok)
                    {
                        this.Provider = new LFacebookManager(LOLConstants.FacebookAPIKey, 
					                                     LOLConstants.FacebookAppSecret, 
					                                     AndroidData.FacebookAccessToken, 
					                                     AndroidData.FacebookAccessTokenExpiration);

                        this.GetFacebookFriends();
                    }//end if
                    break;

                case GOOGLE_BACK:

                    if (resultCode == Result.Ok)
                    {
                        this.Provider = new LGooglePlusManager(LOLConstants.GooglePlusConsumerKey, 
					                                       LOLConstants.GooglePlusConsumerSecret, 
					                                       AndroidData.GooglePlusAccessToken, 
					                                       AndroidData.GooglePlusRefreshToken, 
					                                       AndroidData.GoogleAccessTokenExpiration, new HttpHelper());

                        this.GetGooglePlusFriends();
                    }//end if

                    break;

                case LINKEDIN_BACK:

                    if (resultCode == Result.Ok)
                    {
                        this.Provider = new LLinkedInManager(LOLConstants.LinkedInConsumerKey, 
					                                     LOLConstants.LinkedInConsumerSecret, 
					                                     AndroidData.LinkedInAccessToken, 
					                                     AndroidData.LinkedInAccessTokenSecret);
                        this.GetLinkedInFriends();

                    }//end if

                    break;

            }//end switch
        }

        private List<Contact> GetContactsPortion(int startIndex, int count)
        {
            List<Contact> toReturn = new List<Contact>(count);
            string responseStr = string.Empty;

            switch (this.networkType)
            {
                case AccountOAuth.OAuthTypes.FaceBook:

                    responseStr = this.Provider.GetUserFriends(startIndex, count);
                    toReturn = Parsers.ParseFriendsResponseFacebook(responseStr);

                    break;

                case AccountOAuth.OAuthTypes.Google:
                case AccountOAuth.OAuthTypes.YouTube:

                    responseStr = this.Provider.GetUserFriends(startIndex, count);
                    toReturn = Parsers.ParseFriendsResponseGoogle(responseStr, out this.totalFriendCount);

                    break;

                case AccountOAuth.OAuthTypes.LinkedIn:

                    responseStr = this.Provider.GetUserFriends(startIndex, count);
                    toReturn = Parsers.ParseFriendsResponseLinkedIn(responseStr, out this.totalFriendCount);

                    break;
            }
            return toReturn;
        }

        private void FilterContacts()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("search.Text = {0}", search.Text);
#endif
            if (search.Text.Trim() == "")
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Text trim = null string");
#endif
                for (int i = 1; i < listContainer.ChildCount; i++)
                {
                    listContainer.GetChildAt(i).Visibility = ViewStates.Visible;
                }
            } else
            {
                for (int i = 1; i < listContainer.ChildCount; i++)
                {
                    ViewStates visibility = ViewStates.Gone;
                    LinearLayout temp = (LinearLayout)listContainer.GetChildAt(i);
                    TextView text = (TextView)temp.GetChildAt(1);

                    string[] lastNameArr = text.Text.Split(' ');
                    string lastNameStr = "";
                    if (lastNameArr.Length > 1)
                        lastNameStr = lastNameArr [lastNameArr.Length - 1];
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("lastNameArr[0] = {0}, lastNameArr[1] = {1}, lastNameStr = {2}", lastNameArr [0], lastNameArr [1], lastNameStr);
#endif
                    if (text.Text.ToLower().StartsWith(search.Text.Trim().ToLower()) ||
                        lastNameStr.ToLower().StartsWith(search.Text.Trim().ToLower()))
                    {
                        visibility = ViewStates.Visible;
                    }
                    temp.Visibility = visibility;
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}
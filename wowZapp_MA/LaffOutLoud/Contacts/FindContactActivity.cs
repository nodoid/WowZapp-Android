using System;

using LOLApp_Common;
using LOLAccountManagement;
using System.Collections.Generic;

using Android.Widget;
using Android.Views;
using Android.App;
using Android.Content;
using Android.OS;

using WZCommon;

namespace wowZapp.Contacts
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.Normal", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class FindContactActivity : Activity, IDisposable
    {
        private DBManager db;

        private Context context;
        private const int SEARCH = 0;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            db = wowZapp.LaffOutOut.Singleton.dbm;
            SetContentView(Resource.Layout.FindContactWCF);
            TextView header = FindViewById<TextView>(Resource.Id.txtFirstScreenHeader);
            ImageView btns = FindViewById<ImageView>(Resource.Id.imgNewLoginHeader);
            RelativeLayout relLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            ImageHelper.setupTopPanel(btns, header, relLayout, header.Context);
			
            context = header.Context;
            Header.headertext = Application.Context.Resources.GetString(Resource.String.findContactTitle);
            Header.fontsize = 36f;
            ImageHelper.fontSizeInfo(header.Context);
            header.SetTextSize(Android.Util.ComplexUnitType.Dip, Header.fontsize);
            header.Text = Header.headertext;

            int dist = 0;

            Button search = FindViewById<Button>(Resource.Id.btnSearch);
            Button internet = FindViewById<Button>(Resource.Id.btnOnline);
            ImageButton btnBack = FindViewById<ImageButton>(Resource.Id.btnBack);
            EditText first = FindViewById<EditText>(Resource.Id.editFirstName);
            EditText last = FindViewById<EditText>(Resource.Id.editLastName);
            EditText screenname = FindViewById<EditText>(Resource.Id.editScreenName);
            EditText email = FindViewById<EditText>(Resource.Id.editEmailAddress);
            Spinner spinDist = FindViewById<Spinner>(Resource.Id.spinDistance);
			
            string[] distances = new string[]
            {
                "0",
                "1",
                "2",
                "5",
                "10",
                "25",
                "50",
                "100"
            };
            ArrayAdapter adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, distances);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinDist.Adapter = adapter;

            spinDist.Click += delegate(object sender, EventArgs e)
            {
                Spinner t = (Spinner)sender;
                dist = Convert.ToInt32(distances [t.SelectedItemPosition]);
            };

            search.Click += delegate
            {
                StartContactSearch(email.Text, first.Text, last.Text, screenname.Text, dist);
            };

            internet.Click += delegate
            {
                StartActivity(typeof(GetConnectedInsideActivity));
                Finish();
            };
            btnBack.Click += delegate
            {
                SetResult(Result.Ok);
                Finish();
            };
        }

        private void StartContactSearch(string email, string first, string last, string screen, int distance)
        {
            if (ValidateFields(email, first, last) == true)
            {
                LOLConnectSearchCriteria criteria = new LOLConnectSearchCriteria();
                criteria.EmailAddress = string.IsNullOrEmpty(email) ?
                    string.Empty : email;
                criteria.FirstName = string.IsNullOrEmpty(first) ?
                    string.Empty : first;
                criteria.LastName = string.IsNullOrEmpty(last) ?
                    string.Empty : last;
                criteria.UserName = string.IsNullOrEmpty(screen) ? 
                    string.Empty : screen;
 
                LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
                service.ContactsSearchCompleted += Service_ContactsSearchCompleted;
                service.ContactsSearchAsync(criteria, AndroidData.CurrentUser.AccountID, new Guid(AndroidData.ServiceAuthToken));
            }
        }

        private bool ValidateFields(string email, string first, string last)
        {
            if (!string.IsNullOrEmpty(email))
            {
                if (StringUtils.IsEmailAddress(email))
                {
                    return true;
                } else
                {
                    RunOnUiThread(() => Toast.MakeText(context, Resource.String.errorEmailInvalid, ToastLength.Short).Show());
                    return false;
                }
            }

            if (first.Length < 2 && last.Length < 2)
            {
                RunOnUiThread(() => Toast.MakeText(context, Resource.String.errorFirstLastTooShort, ToastLength.Short).Show());
                return false;
            }
            return true;
        }
		
        private void Service_ContactsSearchCompleted(object sender, ContactsSearchCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.ContactsSearchCompleted -= Service_ContactsSearchCompleted;

            if (null == e.Error)
            {
                LOLConnectSearchResult[] result = e.Result.ToArray();
                List<ContactDB> resultContacts = new List<ContactDB>();

                foreach (LOLConnectSearchResult eachItem in result)
                {
                    if (eachItem.ContactUser.AccountID.Equals(AndroidData.CurrentUser.AccountID))
                    {
                        continue;
                    }

                    ContactDB contactItem = new ContactDB();
                    contactItem.Blocked = false;
                    contactItem.ContactAccountID = eachItem.ContactUser.AccountID;
                    contactItem.OwnerAccountID = AndroidData.CurrentUser.AccountID;
                    contactItem.ContactUser = eachItem.ContactUser;

                    resultContacts.Add(contactItem);
                }
                RunOnUiThread(delegate
                {
                    if (resultContacts.Count == 0)
                    {
                        RunOnUiThread(delegate
                        {
                            GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError),
                                                                Application.Context.GetString(Resource.String.findContactNoContactsFound));
                        });
                    } else
                    {
                        FindContactResultsUtil.contacts = resultContacts;
                        StartActivityForResult(typeof(FindContactResultsActivity), SEARCH);
                    }
                });
            } else
            {
                RunOnUiThread(delegate
                {
                    GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.commonError),
                                                        Application.Context.GetString(Resource.String.errorSearchingContactsByCriteria));
                });
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Finish();
        }
		
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
    }
}

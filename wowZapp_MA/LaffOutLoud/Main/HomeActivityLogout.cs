using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LOLApp_Common;

namespace wowZapp.Main
{		
    public partial class HomeActivity : Activity
    {
        private void btnLogout_Click(object s, EventArgs e)
        {
            ModalPreviewDialog = new Dialog(this, Resource.Style.lightbox_dialog);
            ModalPreviewDialog.SetContentView(Resource.Layout.ModalLogoutModal1);
			
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnTop)).Click += delegate
            {
                DismissModalPreviewDialog();
                aboutUs();
            };
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnMiddle)).Click += delegate
            {
                DismissModalPreviewDialog();
            };
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnBottom)).Click += delegate
            {
                DismissModalPreviewDialog();
                LogoutStep2();
            };
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnUserProf)).Click += delegate
            {
                DismissModalPreviewDialog();
                Intent i = new Intent(this, typeof(Contacts.AboutMe));
                StartActivity(i);
            };
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnCancel)).Click += delegate
            {
                DismissModalPreviewDialog();
                if (scroller != null)
                    scroller.Start();
                wowZapp.LaffOutOut.Singleton.EnableMessageTimer();
                wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;
            };
			
            ModalPreviewDialog.Show();
        }
		
        private void LogoutStep2()
        {
            ModalPreviewDialog = new Dialog(this, Resource.Style.lightbox_dialog);
            ModalPreviewDialog.SetContentView(Resource.Layout.ModalLogoutModal2);
            if (scroller != null)
                scroller.Stop();
            killed = true;
            wowZapp.LaffOutOut.Singleton.DisableMessageTimer();
            wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnMiddle)).Click += delegate
            {
				
                DismissModalPreviewDialog();
                LogoutWithClear(true);
            };
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnBottom)).Click += delegate
            {
                DismissModalPreviewDialog();
                LogoutWithClear(false);
            };
            ((Button)ModalPreviewDialog.FindViewById(Resource.Id.btnCancel)).Click += delegate
            {
                if (scroller != null)
                    scroller.Start();
                killed = false;
                wowZapp.LaffOutOut.Singleton.EnableMessageTimer();
                wowZapp.LaffOutOut.Singleton.ReceivedMessages += AppDelegate_ReceivedMessages;
                DismissModalPreviewDialog();
            };
			
            ModalPreviewDialog.Show();
        }
	
        private void LogoutWithClear(bool withClear)
        {
            wowZapp.LaffOutOut.Singleton.DisableMessageTimer();
            if (scroller != null)
            {
                scroller.Stop();
            }
            wowZapp.LaffOutOut.Singleton.ReceivedMessages -= AppDelegate_ReceivedMessages;
            LOLConnectClient service = new LOLConnectClient(LOLConstants.DefaultHttpBinding, LOLConstants.LOLConnectEndpoint);
            service.UserLogOutCompleted += Service_UserLogOutCompleted;
            service.UserLogOutAsync(AndroidData.NewDeviceID, AndroidData.CurrentUser.AccountID,
			                         new Guid(AndroidData.ServiceAuthToken), withClear);
        }
		
        private void Service_UserLogOutCompleted(object sender, UserLogOutCompletedEventArgs e)
        {
            LOLConnectClient service = (LOLConnectClient)sender;
            service.UserLogOutCompleted -= Service_UserLogOutCompleted;
            if (null == e.Error)
            {
                if (e.Result.Count > 0)
                {
                    RunOnUiThread(delegate
                    {
                        GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.logoutTitle), StringUtils.CreateErrorMessageFromGeneralErrors(e.Result));
                    });
                } else
                {
                    AndroidData.IsLoggedIn = false;
                    AndroidData.IsAppActive = false;
                    bool withClear = (bool)e.UserState;
                    if (withClear)
                    {
                        foreach (string filename in Contacts.ContactsUtil.contactFilenames)
                        {
                            string path = System.IO.Path.Combine(wowZapp.LaffOutOut.Singleton.ContentDirectory, filename);
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                        Contacts.ContactsUtil.contactFilenames.Clear();
                        dbm.CleanUpDB();
                    }//end if
                    AndroidData.ClearSocialNetworkInfo();
					
                    AndroidData.CurrentUser = null;
                    RunOnUiThread(delegate
                    {
                        Intent i = new Intent(context, typeof(Login.LoginChoiceActivity));
                        i.AddFlags(ActivityFlags.ClearTop);
                        StartActivity(i);
                    });
					
                }//end if else
            } else
            {
#if(DEBUG)
                System.Diagnostics.Debug.WriteLine("Exception in logout user! {0}--{1}", e.Error.Message, e.Error.StackTrace);
#endif
                RunOnUiThread(delegate
                {
                    string m = string.Format("{0} {1}", Application.Context.GetString(Resource.String.alertViewMessageLogoutFailed),
					                          e.Error.Message);
                    GeneralUtils.Alert(context, Application.Context.GetString(Resource.String.logoutTitle), m);
                });
            }//end if else
        }
    }
}


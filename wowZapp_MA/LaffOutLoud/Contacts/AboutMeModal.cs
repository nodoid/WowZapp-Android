using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using LOLAccountManagement;

namespace wowZapp.Contacts
{			
    public partial class AboutMe : Activity
    {
        private const int CAMMY = 1, PICCY = 2;
        private Dialog ModalEdit;
        private ImageView newPic;
        private float[] imageSize;
        private string myMD5 = "";

        private void generateModal(int option)
        {
            ModalEdit = new Dialog(this, Resource.Style.lightbox_dialog);
            ModalEdit.SetContentView(Resource.Layout.ModalEditAboutMe);
            EditText edit = null;
            Button accept = ((Button)ModalEdit.FindViewById(Resource.Id.btnAccept));
            accept.Visibility = ViewStates.Invisible;
			
            ((Button)ModalEdit.FindViewById(Resource.Id.btnCancel)).Click += delegate
            {
                DismissModalEditDialog();
            };
            LayoutInflater factory = LayoutInflater.From(this);
            View myView = null;
            LinearLayout linLay = ((LinearLayout)ModalEdit.FindViewById(Resource.Id.linearLayout5));
            switch (option)
            {
                case 0:
                    myView = factory.Inflate(Resource.Layout.fragNewImage, null);
                    linLay.AddView(myView);
                    ImageView current = ((ImageView)linLay.FindViewById(Resource.Id.imgCurrent));
                    newPic = ((ImageView)linLay.FindViewById(Resource.Id.imgNew));
                    ImageView rotLeft = ((ImageView)linLay.FindViewById(Resource.Id.imgLeft));
                    ImageView rotRight = ((ImageView)linLay.FindViewById(Resource.Id.imgRight));
                    imageSize = new float[2];
                    imageSize [0] = imageSize [1] = ImageHelper.convertDpToPixel(100f, context);
                    if (AndroidData.CurrentUser.Picture.Length > 0)
                    {
                        using (Bitmap bm = BitmapFactory.DecodeByteArray (AndroidData.CurrentUser.Picture, 0, AndroidData.CurrentUser.Picture.Length))
                        {
                            using (MemoryStream ms = new MemoryStream ())
                            {
                                bm.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                                byte[] image = ms.ToArray();
                                displayImage(image, current);
                            }
                        }
                    }
				
                    rotLeft.Click += delegate
                    {
                        if (!string.IsNullOrEmpty(AndroidData.imageFileName))
                            newPic.SetImageBitmap(ImageHelper.rotateImage(true));
                    };
				
                    rotRight.Click += delegate
                    {
                        if (!string.IsNullOrEmpty(AndroidData.imageFileName))
                            newPic.SetImageBitmap(ImageHelper.rotateImage());
                    };
				
                    bool[] camTest = new bool[2];
                    camTest = GeneralUtils.CamRec();
				
                    if (camTest [0] == false || camTest [1] == false)
                    {
                        DismissModalEditDialog();
                    }
				
                    ((Button)linLay.FindViewById(Resource.Id.btnCamera)).Click += delegate(object s, EventArgs e)
                    {
                        Intent i = new Intent(this, typeof(CameraVideo.CameraTakePictureActivity));
                        i.PutExtra("camera", CAMMY);
                        StartActivityForResult(i, CAMMY);
                    };
				
                    ((Button)linLay.FindViewById(Resource.Id.btnStorage)).Click += delegate(object s, EventArgs e)
                    {
                        var imageIntent = new Intent();
                        imageIntent.SetType("image/*");
                        imageIntent.SetAction(Intent.ActionGetContent);
                        StartActivityForResult(Intent.CreateChooser(imageIntent, "Choose Image"), PICCY);
                    };
				
                    accept.Visibility = ViewStates.Visible;
                    if (accept.Visibility == ViewStates.Visible)
                    {
                        accept.Click += delegate
                        {
                            DismissModalEditDialog();
                            if (myMD5 != MD5Check)
                                imageChanged = true;
                        };
                    }
                    break;
                case 1:
                    myView = factory.Inflate(Resource.Layout.fragNewName, null);
                    linLay.AddView(myView);
                    ((TextView)linLay.FindViewById(Resource.Id.textCurrentName)).Text = fullName.Text;
                    EditText first = ((EditText)linLay.FindViewById(Resource.Id.editFirst));
                    EditText last = ((EditText)linLay.FindViewById(Resource.Id.editLast));
                    first.Text = AndroidData.CurrentUser.FirstName;
                    first.TextChanged += delegate
                    {
                        if (first.Text != AndroidData.CurrentUser.FirstName && first.Text.Length > 0 && last.Text.Length > 0)
                        {
                            accept.Visibility = ViewStates.Visible;
                            accept.Click += delegate
                            {
                                DismissModalEditDialog();
                                fullName.Text = first.Text + " " + last.Text;
                            };
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    last.TextChanged += delegate
                    {
                        if (last.Text != AndroidData.CurrentUser.LastName && first.Text.Length > 0 && last.Text.Length > 0)
                        {
                            accept.Visibility = ViewStates.Visible;
                            accept.Click += delegate
                            {
                                DismissModalEditDialog();
                                fullName.Text = first.Text + " " + last.Text;
                            };
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    break;
                case 2:
                    myView = factory.Inflate(Resource.Layout.fragNewScreenName, null);
                    linLay.AddView(myView);
                    ((TextView)linLay.FindViewById(Resource.Id.textScreen)).Text = screenName.Text;
                    edit = ((EditText)linLay.FindViewById(Resource.Id.editScreen));
                    edit.TextChanged += delegate
                    {
                        if (edit.Text != AndroidData.CurrentUser.UserName && edit.Text.Length > 0)
                        {
                            accept.Visibility = ViewStates.Visible;
                            accept.Click += delegate
                            {
                                DismissModalEditDialog();
                                screenName.Text = edit.Text;
                            };
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    break;
                case 3:
                    myView = factory.Inflate(Resource.Layout.fragNewEmail, null);
                    linLay.AddView(myView);
                    ((TextView)linLay.FindViewById(Resource.Id.txtEmail)).Text = emailAddress.Text;
                    edit = ((EditText)linLay.FindViewById(Resource.Id.editEmail));
                    edit.TextChanged += delegate
                    {
                        if (edit.Text != AndroidData.CurrentUser.EmailAddress && edit.Text.Length > 0)
                        {
                            accept.Visibility = ViewStates.Visible;
                            accept.Click += delegate
                            {
                                DismissModalEditDialog();
                                emailAddress.Text = edit.Text;
                            };
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    break;
                case 4:
                    myView = factory.Inflate(Resource.Layout.fragNewGender, null);
                    linLay.AddView(myView);
                    int gen = -1;
                    ((TextView)linLay.FindViewById(Resource.Id.textView5)).Text = gender.Text;
                    ((ImageView)linLay.FindViewById(Resource.Id.imgMale)).Click += delegate
                    {
                        if (AndroidData.CurrentUser.UserGender != User.Gender.Male)
                        {
                            accept.Visibility = ViewStates.Visible;
                            gen = 1;
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    ((ImageView)linLay.FindViewById(Resource.Id.imgFemale)).Click += delegate
                    {
                        if (AndroidData.CurrentUser.UserGender != User.Gender.Female)
                        {
                            accept.Visibility = ViewStates.Visible;
                            gen = 2;
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    ((ImageView)linLay.FindViewById(Resource.Id.imgAlien)).Click += delegate
                    {
                        if (AndroidData.CurrentUser.UserGender != User.Gender.Alien)
                        {
                            accept.Visibility = ViewStates.Visible;
                            gen = 3;
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    ((ImageView)linLay.FindViewById(Resource.Id.imgMonster)).Click += delegate
                    {
                        if (AndroidData.CurrentUser.UserGender != User.Gender.Monster)
                        {
                            accept.Visibility = ViewStates.Visible;
                            gen = 4;
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    ((ImageView)linLay.FindViewById(Resource.Id.imgUnknown)).Click += delegate
                    {
                        if (AndroidData.CurrentUser.UserGender != User.Gender.Unknown)
                        {
                            accept.Visibility = ViewStates.Visible;
                            gen = 0;
                        } else
                            accept.Visibility = ViewStates.Invisible;
                    };
                    if (accept.Visibility == ViewStates.Visible)
                    {
                        accept.Click += delegate
                        {
                            DismissModalEditDialog();
                            gender.Text = genders [gen];
                        };
                    }
                    break;
                case 5:
                    myView = factory.Inflate(Resource.Layout.fragNewPassword, null);
                    linLay.AddView(myView);
                    EditText oldPass = ((EditText)linLay.FindViewById(Resource.Id.editOldPass));
                    EditText newPass = ((EditText)linLay.FindViewById(Resource.Id.editNewPass));
                    if (oldPass.Text.Length == newPass.Text.Length && oldPass.Text == newPass.Text)
                    {
                        accept.Visibility = ViewStates.Visible;
                        accept.Click += delegate
                        {
                            DismissModalEditDialog();
                            password.Text = newPass.Text;
                        };
                    } else
                        accept.Visibility = ViewStates.Invisible;
                    break;
                case 6:
                    myView = factory.Inflate(Resource.Layout.fragNewDOB, null);
                    linLay.AddView(myView);
                    ((TextView)linLay.FindViewById(Resource.Id.textCurrentBD)).Text = birthday.Text;
                    Spinner day = ((Spinner)linLay.FindViewById(Resource.Id.spinDate));
                    Spinner month = ((Spinner)linLay.FindViewById(Resource.Id.spinMonth));
                    Spinner year = ((Spinner)linLay.FindViewById(Resource.Id.spinYear));
                    int y = DateTime.Now.Year;
                    int m = DateTime.Now.Month;
                    int d = DateTime.Now.Day;
                    var adapterM = ArrayAdapter.CreateFromResource(this, Resource.Array.createMonths, Android.Resource.Layout.SimpleSpinnerItem);
                    adapterM.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    month.Adapter = adapterM;
                    month.SetSelection(m - 1);
                    month.ItemSelected += (sender, e) => {
                        string mm = (string)month.GetItemAtPosition(e.Position);
                        m = Convert.ToInt32(mm);
                    };

                    List<string> dates = new List<string>();
                    List<string> years = new List<string>();
                    for (int n = 1; n < 32; ++n)
                        dates.Add(n.ToString());
                    for (int n = 0; n < 75; ++n)
                        years.Add((y - 75 + n).ToString());
				
                    var yearsAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, years);
                    yearsAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    year.Adapter = yearsAdapter;
                    year.SetSelection(54);
                    y -= 21;
				
                    var daysAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, dates);
                    daysAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    day.Adapter = daysAdapter;
                    day.SetSelection(d - 1);
				
                    day.ItemSelected += (sender, e) => {
                        string dd = (string)day.GetItemAtPosition(e.Position);
                        d = Convert.ToInt32(dd);
                    };
                    year.ItemSelected += (sender, e) => {
                        string yy = (string)year.GetItemAtPosition(e.Position);
                        y = Convert.ToInt32(yy);
                    };
                    string date = y.ToString() + "/" + m.ToString() + "/" + d.ToString(); 
                    if (date != AndroidData.CurrentUser.DateOfBirth.ToShortDateString())
                    {
                        accept.Visibility = ViewStates.Visible;
                        accept.Click += delegate
                        {
                            DismissModalEditDialog();
                            birthday.Text = date;
                        };
                    } else
                        accept.Visibility = ViewStates.Invisible;
                    break;
                case 7:
                    myView = factory.Inflate(Resource.Layout.fragNewDescription, null);
                    linLay.AddView(myView);
                    ((TextView)linLay.FindViewById(Resource.Id.textScreen)).Text = profDesc.Text;
                    edit = ((EditText)linLay.FindViewById(Resource.Id.editScreen));
                    if (edit.Text != profDesc.Text && edit.Text.Length > 0)
                    {
                        accept.Visibility = ViewStates.Visible;
                        accept.Click += delegate
                        {
                            DismissModalEditDialog();
                            profDesc.Text = edit.Text;
                        };
                    } else
                        accept.Visibility = ViewStates.Invisible;
                    break;
            }
            ((Button)ModalEdit.FindViewById(Resource.Id.btnCancel)).Click += delegate
            {
                DismissModalEditDialog();
            };
			
            ModalEdit.Show();
        }	
		
        private void DismissModalEditDialog()
        {
            if (ModalEdit != null)
                ModalEdit.Dismiss();
            ModalEdit = null;
        }
		
        private void displayImage(byte[] image, ImageView cPic = null)
        {
            if (image.Length > 0)
            {
                using (Bitmap myBitmap = ImageHelper.CreateUserProfileImageForDisplay(image, (int)imageSize[0], (int)imageSize[1], this.Resources))
                {
                    if (cPic != null)
                        RunOnUiThread(() => cPic.SetImageBitmap(myBitmap));
                    else
                        RunOnUiThread(() => newPic.SetImageBitmap(myBitmap));
                }
            }
        }
		
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case CAMMY:
                    if (resultCode == Result.Ok)
                    {
                        string filename = data.GetStringExtra("filename");
                        if (!string.IsNullOrEmpty(filename))
                        {
                            using (Bitmap bmp = BitmapFactory.DecodeFile(filename))
                            {
                                if (bmp != null)
                                {
                                    using (MemoryStream ms = new MemoryStream ())
                                    {
                                        bmp.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
                                        byte[] image = ms.ToArray();
                                        myMD5 = generateMD5(image);
                                        displayImage(image);
                                    }
                                }
                            }
                            AndroidData.imageFileName = filename;
                        }
                    }
                    break;
                case PICCY:
                    if (resultCode == Result.Ok)
                    {
                        string filename = getRealPathFromUri(data.Data);
                        using (Android.Graphics.Drawables.Drawable draw = Android.Graphics.Drawables.Drawable.CreateFromPath(filename))
                        {
                            newPic.SetImageDrawable(draw);
                        }
                        AndroidData.imageFileName = filename;
                        myMD5 = generateMD5(filename);
                    }
                    break;
            }
        }

        private string getRealPathFromUri(Android.Net.Uri contentUri)
        {
            string[] proj = { Android.Provider.MediaStore.Images.ImageColumns.Data };
            Android.Database.ICursor cursor = this.ManagedQuery(contentUri, proj, null, null, null);
            int column_index = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.ImageColumns.Data);
            cursor.MoveToFirst();
            return cursor.GetString(column_index);
        }
    }
}


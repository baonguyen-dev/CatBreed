using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using CatBreed.ApiClient;
using CatBreed.ServiceLocators.DI;
using Android.Widget;
using System.Threading.Tasks;
using System.Collections.Generic;
using CatBreed.ServiceLocators.Services;
using AndroidX.Core.Content;
using Plugin.Permissions;
using AndroidX.RecyclerView.Widget;
using System.Runtime.InteropServices;
using CatBreed.Droid.Adapters;
using System.Linq;
using Bumptech.Glide;
using Android.Content;
using CatBreed.Droid.Activities;
using Android.Net;

namespace CatBreed.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        
        private int PERMISSION_ALL = 1;
        private string[] PERMISSIONS = {
            Android.Manifest.Permission.WriteExternalStorage,
            Android.Manifest.Permission.ReadExternalStorage
        };

        private bool CheckPermission(string[] permissions)
        {
            foreach (var item in permissions)
            {
                Android.Content.PM.Permission result = ContextCompat.CheckSelfPermission(this, item);
                if (result != Android.Content.PM.Permission.Granted) return false;
            }

            return true;
        }

        private void RequestPermission(string[] permission, int code)
        {
            AndroidX.Core.App.ActivityCompat.RequestPermissions(this, permission, code);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (!CheckPermission(PERMISSIONS))
                {
                    RequestPermission(PERMISSIONS, PERMISSION_ALL);
                }
                else
                {
                    StartHomeAcitivity();
                }
            }

            //StartHomeAcitivity();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == PERMISSION_ALL)
            {
                if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                {
                    StartHomeAcitivity();
                }
                else
                {
                    Toast.MakeText(this, "Application cannot get permission", ToastLength.Long).Show();
                    Finish();
                }
            }
        }

        private void StartHomeAcitivity()
        {
            Intent intent = new Intent();

            intent.SetClass(this, typeof(HomeActivity));

            StartActivity(intent);
        }
    }
}

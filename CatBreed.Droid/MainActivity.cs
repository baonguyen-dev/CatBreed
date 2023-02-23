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

namespace CatBreed.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ICatBreedClient _catBreedClient => ServiceLocator.Instance.Get<ICatBreedClient>();
        private ImageView _ivCatBreed;
        private IList<CatBreedModel> _model;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _ivCatBreed = FindViewById<ImageView>(Resource.Id.iv_cat_breed);

        }

        protected override async void OnResume()
        {
            base.OnResume();

            
            _model = await _catBreedClient.GetCatBreed();

            var data = _model[0].Url;

            RunOnUiThread(() =>
            {
                //Toast.MakeText(this, _model.Url, ToastLength.Long).Show();
            });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        }
    }
}

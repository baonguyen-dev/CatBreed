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

namespace CatBreed.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ICatBreedClient _catBreedClient => ServiceLocator.Instance.Get<ICatBreedClient>();
        private IFileService _fileService => ServiceLocator.Instance.Get<IFileService>();
        private ImageView _ivCatBreed;
        private TextView _tvCount;
        private Button _btnCount;
        private List<CatBreedModel> _model;
        private RecyclerView _rcvImage;
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
            }

            _ivCatBreed = FindViewById<ImageView>(Resource.Id.iv_image);

            _rcvImage = FindViewById<RecyclerView>(Resource.Id.rcv_image);

            _tvCount = FindViewById<TextView>(Resource.Id.tv_count);

            _btnCount = FindViewById<Button>(Resource.Id.btn_count);

            _btnCount.Click += _btnCount_Click; // set the Ada

            Task.Factory.StartNew(async () =>
            {
                _model = await _catBreedClient.GetCatBreed() as List<CatBreedModel>;

                //foreach(var cat in _model)
                //{
                //    //var data = _model[0].Url;

                //    var path = _fileService.DownloadImage(cat.Id, cat.Url);

                RunOnUiThread(() =>
                {
                    StaggeredGridLayoutManager staggeredGridLayoutManager = new StaggeredGridLayoutManager(1, LinearLayoutManager.Vertical);

                    _rcvImage.SetLayoutManager(staggeredGridLayoutManager);

                    ListViewAdapter customAdapter = new ListViewAdapter(this, _model);

                    _rcvImage.SetAdapter(customAdapter);

                    var scrollListener = new ListViewScrollListenner(async () =>
                    {
                        var count = customAdapter.ItemCount;

                        var model = await _catBreedClient.GetCatBreed();

                        _model.AddRange(model);

                        customAdapter.NotifyItemInserted(count - 1);
                    });

                    _rcvImage.AddOnScrollListener(scrollListener);
                    //Toast.MakeText(this, path, ToastLength.Long).Show();
                    //customAdapter.NotifyDataSetChanged();
                });
                //    // set LayoutManager to RecyclerView
                //    //  call the constructor of CustomAdapter to send the reference and data to Adapter
                //}pter to RecyclerView


            });

        }

        private void _btnCount_Click(object sender, EventArgs e)
        {
            var count = new Random().Next();
            _tvCount.Text = count.ToString();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == PERMISSION_ALL)
            {
                if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                {
                }
                else
                {
                    Toast.MakeText(this, "Application cannot get permission", ToastLength.Long).Show();
                    Finish();
                }
            }
        }
    }
}

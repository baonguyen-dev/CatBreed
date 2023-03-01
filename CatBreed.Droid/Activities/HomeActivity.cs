
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Service.QuickSettings;
using Android.Views;
using Android.Widget;
using AndroidX.Lifecycle;
using AndroidX.RecyclerView.Widget;
using CatBreed.ApiClient;
using CatBreed.ApiClient.Models;
using CatBreed.ApiClient.ViewModels;
using CatBreed.ApiLocators.Models;
using CatBreed.Droid.Adapters;
using CatBreed.Droid.BroadcastService;
using CatBreed.Entities;
using CatBreed.Repositories;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using Java.Util;
using static System.Net.Mime.MediaTypeNames;
using static Android.App.DownloadManager;
using static Android.Content.ClipData;
using static Android.Graphics.ColorSpace;
using static AndroidX.RecyclerView.Widget.RecyclerView;

namespace CatBreed.Droid.Activities
{
    [Activity(Label = "HomeActivity")]
    public class HomeActivity : BaseActivity, SearchView.IOnQueryTextListener
    {
        private IDeviceService _deviceService => ServiceLocator.Instance.Get<IDeviceService>();

        private const int _size = 15;
        private List<CatBreedModel> _catBreedViewModels;
        private RecyclerView _rcvImage;
        private SearchView _svSearch;
        private ListViewAdapter _listViewAdapter;
        private string _queryBreed;
        private ProgressBar _pbWaiting;
        private NetworkChangeReceiver _networkChangeReceiver;
        private Button _btnReset;
        ListView _list;
        SearchListAdapter _adapter;
        private List<CatTypeModel> _catTypesViewModel;
        private CatBreedViewModel _viewModel;

        protected override void OnCreate(Bundle savedInstanceState)
        {   
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.home_activity);

            _list = FindViewById<ListView>(Resource.Id.listview);

            _svSearch = FindViewById<SearchView>(Resource.Id.sv_search);

            _rcvImage = FindViewById<RecyclerView>(Resource.Id.rcv_image);

            _pbWaiting = FindViewById<ProgressBar>(Resource.Id.progressBar_cyclic);

            _btnReset = FindViewById<Button>(Resource.Id.btn_clear);

            _svSearch.SetOnQueryTextListener(this);

            _viewModel = new CatBreedViewModel();

            _catBreedViewModels = new List<CatBreedModel>();

            _catTypesViewModel = new List<CatTypeModel>();

            Task.Factory.StartNew(async () =>
            {
                _catTypesViewModel.AddRange(await _viewModel.GetAllCatType());

                _adapter.SetListItem(_catTypesViewModel);
            });

            _adapter = new SearchListAdapter(this, _catTypesViewModel, (model) =>
            {
                ShowProgressBar(true);

                if (_deviceService.IsDeviceOnline())
                {
                    _queryBreed = model.Type;

                    _list.Visibility = ViewStates.Invisible;

                    _svSearch.ClearFocus();

                    QueryBreedData();
                }
            });

            _list.Adapter = _adapter;

            _btnReset.Click += (sender, args) =>
            {
                ShowProgressBar(true);

                if (_deviceService.IsDeviceOnline())
                {
                    _svSearch.SetQuery("", false);

                    _svSearch.ClearFocus();

                    _queryBreed = "";

                    QueryBreedData();
                }
                else
                {
                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(_viewModel.QueryAllImageFromDatabase());

                    _listViewAdapter.NotifyDataSetChanged();

                    ShowProgressBar(false);
                }
            };

            _networkChangeReceiver = new NetworkChangeReceiver((isOnline) =>
            {
                ShowProgressBar(true);

                _catBreedViewModels.Clear();

                Task.Factory.StartNew(async () =>
                {
                    if (!isOnline)
                    {
                        _catBreedViewModels.AddRange(_viewModel.QueryAllImageFromDatabase());
                    }
                    else
                    {
                        _catBreedViewModels.AddRange(await _viewModel.QueryCatBreed(_size));
                    }

                    RunOnUiThread(() =>
                    {
                        _listViewAdapter.NotifyDataSetChanged();

                        ShowProgressBar(false);
                    });
                });
            });

            RegisterReceiver(_networkChangeReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));

            StaggeredGridLayoutManager staggeredGridLayoutManager = new StaggeredGridLayoutManager(1, LinearLayoutManager.Vertical);

            _rcvImage.SetLayoutManager(staggeredGridLayoutManager);

            _listViewAdapter = new ListViewAdapter(this, _catBreedViewModels, (data) =>
            {
                if (!string.IsNullOrEmpty(data.Id))
                {
                    ShowProgressBar(true);

                    _list.Visibility = ViewStates.Invisible;

                    _svSearch.ClearFocus();

                    _queryBreed = data.Id;

                    QueryBreedData();
                }
            }, (data) =>
            {
                ShowProgressBar(true);

                Task.Factory.StartNew(() =>
                {
                    _viewModel.DownloadAndSaveImage(data, (status) =>
                    {
                        RunOnUiThread(() =>
                        {
                            switch (status)
                            {
                                case SaveStatus.EXIST:
                                    new AlertDialog.Builder(this)
                                                    .SetTitle("Error")
                                                    .SetMessage("Data already exists!")
                                                    .SetPositiveButton(text: "OK", handler: null)
                                                    .Show();
                                    break;
                                case SaveStatus.SUCCESS:
                                    new AlertDialog.Builder(this)
                                                        .SetTitle("Notify")
                                                        .SetMessage("Data saved")
                                                        .SetPositiveButton(text: "OK", handler: null)
                                                        .Show();
                                    break;
                            }
                        });
                    });

                    ShowProgressBar(false);
                });

            });

            _rcvImage.SetAdapter(_listViewAdapter);

            var scrollListener = new ListViewScrollListenner(async () =>
            {
                if (_deviceService.IsDeviceOnline())
                {
                    ShowProgressBar(true);

                    try
                    {
                        if (!string.IsNullOrEmpty(_queryBreed))
                        {
                            _catBreedViewModels.AddRange(await _viewModel.QueryCatImageType(_queryBreed, _listViewAdapter.ItemCount + _size));
                        }
                        else
                        {
                            var catBreedModes = await _viewModel.QueryCatBreed(_listViewAdapter.ItemCount + _size);

                            foreach(var item in catBreedModes)
                            {
                                _catBreedViewModels.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        RunOnUiThread(() =>
                        {
                            ShowProgressBar(false);

                            AlertDialog alert = new AlertDialog.Builder(this)
                                                    .SetTitle("Error")
                                                    .SetMessage("To many request!")
                                                    .SetPositiveButton(text: "OK", handler: null)
                                                    .Show();
                        });
                    }

                    if (_listViewAdapter.ItemCount > _size)
                    {
                        _listViewAdapter.NotifyItemInserted(_listViewAdapter.ItemCount - 1);
                    }

                    ShowProgressBar(false);
                }
            });

            _rcvImage.AddOnScrollListener(scrollListener);
        }

        public bool OnQueryTextChange(string newText)
        {
            if (_deviceService.IsDeviceOnline())
            {
                if (!string.IsNullOrEmpty(newText))
                {
                    _adapter.Filter(newText);

                    _list.BringToFront();

                    _list.Visibility = ViewStates.Visible;
                }
                else
                {
                    _list.Visibility = ViewStates.Invisible;
                }
            }
            else
            {
                _catBreedViewModels.Clear();

                _catBreedViewModels.AddRange(_viewModel.QueryCatBreedFromDatabase(newText));

                _listViewAdapter.NotifyDataSetChanged();

                ShowProgressBar(false);
            }
            return false;
        }

        public bool OnQueryTextSubmit(string query)
        {
            return false;
        }

        public void ShowProgressBar(bool isShow)
        {
            RunOnUiThread(() =>
            {
                if (isShow)
                {
                    Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);
                }
                else
                {
                    Window.ClearFlags(WindowManagerFlags.NotTouchable);
                }
                _pbWaiting.BringToFront();
                _pbWaiting.Visibility = isShow ? ViewStates.Visible : ViewStates.Gone;
            });
        }

        private void QueryBreedData()
        {
            Task.Factory.StartNew(async () =>
            {
                if (!string.IsNullOrEmpty(_queryBreed))
                {
                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(await _viewModel.QueryCatImageType(_queryBreed));
                }
                else
                {
                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(await _viewModel.QueryCatBreed(_size));
                }

                RunOnUiThread(() =>
                {
                    _listViewAdapter.NotifyDataSetChanged();
                });

                ShowProgressBar(false);
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            try
            {
                UnregisterReceiver(_networkChangeReceiver);
            }
            catch (Exception ex)
            {
            }
        }
    }
}


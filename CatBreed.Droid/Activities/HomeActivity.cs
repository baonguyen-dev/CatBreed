
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
using AndroidX.RecyclerView.Widget;
using CatBreed.ApiClient;
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
using static Android.Content.ClipData;
using static Android.Graphics.ColorSpace;
using static AndroidX.RecyclerView.Widget.RecyclerView;

namespace CatBreed.Droid.Activities
{
    [Activity(Label = "HomeActivity")]
    public class HomeActivity : BaseActivity, SearchView.IOnQueryTextListener
    {
        private ICatBreedClient _catBreedClient => ServiceLocator.Instance.Get<ICatBreedClient>();
        private IFileService _fileService => ServiceLocator.Instance.Get<IFileService>();
        private IDeviceService _deviceService => ServiceLocator.Instance.Get<IDeviceService>();

        private List<CatBreedViewModel> _catBreedViewModels;
        private RecyclerView _rcvImage;
        private Android.Widget.SearchView _svSearch;
        private ListViewAdapter _listViewAdapter;
        private string _queryBreed;
        private ProgressBar _pbWaiting;
        private CatBreedRepository _catBreedRepository;
        private List<CatEntity> _catEntities;
        private NetworkChangeReceiver _networkChangeReceiver;
        private Button _btnReset;
        ListView _list;
        SearchListAdapter _adapter;
        private List<CatTypeEntity> _typeEntities;
        private CatTypeRepository _catTypeRepository;
        private List<CatTypeViewModel> _catTypesViewModel;

        protected override void OnCreate(Bundle savedInstanceState)
        {   
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.home_activity);

            _catTypeRepository = new CatTypeRepository();

            _typeEntities = _catTypeRepository.LoadAll().ToList();

            _list = FindViewById<ListView>(Resource.Id.listview);

            _svSearch = FindViewById<Android.Widget.SearchView>(Resource.Id.sv_search);

            _rcvImage = FindViewById<RecyclerView>(Resource.Id.rcv_image);

            _pbWaiting = FindViewById<ProgressBar>(Resource.Id.progressBar_cyclic);

            _btnReset = FindViewById<Button>(Resource.Id.btn_clear);

            if (_typeEntities == null || _typeEntities.Count == 0)
            {
                ShowProgressBar(true);

                Task.Factory.StartNew(async () =>
                {
                    var allBread = await _catBreedClient.GetCatBreed() as List<CatBreedModel>;

                    foreach (var item in allBread)
                    {
                        _typeEntities.Add(new CatTypeEntity()
                        {
                            Name = item.Name,
                            Type = item.Id
                        });
                    }

                    _catTypeRepository.InsertAll(_typeEntities);

                    ShowProgressBar(false);
                });
            }

            _catTypesViewModel = new List<CatTypeViewModel>();

            _catTypesViewModel.AddRange(_typeEntities.ToCatBreedViewModels());

            _adapter = new SearchListAdapter(this, _catTypesViewModel, (model) =>
            {
                ShowProgressBar(true);

                if (_deviceService.IsDeviceOnline())
                {
                    QueryBreedData();
                }
                else
                {
                    var tempCatEntities = _catEntities.Where(p => p.Name.ToLower().Contains(_queryBreed.ToLower())).ToList();

                    var tempCatBreedViewModels = tempCatEntities.ToCatBreedViewModels();

                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(tempCatBreedViewModels);

                    _listViewAdapter.NotifyDataSetChanged();

                    ShowProgressBar(false);
                }
            });

            _list.Adapter = _adapter;

            _btnReset.Click += (sender, args) =>
            {
                ShowProgressBar(true);

                _svSearch.SetQuery("", false);

                _svSearch.ClearFocus();

                _queryBreed = "";

                QueryBreedData();
            };

            _networkChangeReceiver = new NetworkChangeReceiver((isOnline) =>
            {
                ShowProgressBar(true);

                _catBreedViewModels.Clear();

                Task.Factory.StartNew(async () =>
                {
                    if (!isOnline)
                    {
                        _catEntities = _catBreedRepository.LoadAll().ToList();

                        var tempCatBreedViewModels = _catEntities.ToCatBreedViewModels();

                        _catBreedViewModels.AddRange(tempCatBreedViewModels);
                    }
                    else
                    {
                        var tempCatBreedModels = await _catBreedClient.GetCatBreed(10) as List<CatBreedModel>;

                        foreach (var item in tempCatBreedModels)
                        {
                            var referenceImage = await _catBreedClient.GetReferenceImage(item.ReferenceImageId);

                            _catBreedViewModels.Add(item.ToCatBreedViewModel(referenceImage));
                        }
                    }

                    RunOnUiThread(() =>
                    {
                        _listViewAdapter.NotifyDataSetChanged();

                        ShowProgressBar(false);
                    });
                });
            });

            RegisterReceiver(_networkChangeReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));

            _svSearch.SetOnQueryTextListener(this);

            _catBreedViewModels = new List<CatBreedViewModel>();

            _catBreedRepository = new CatBreedRepository();

            StaggeredGridLayoutManager staggeredGridLayoutManager = new StaggeredGridLayoutManager(1, LinearLayoutManager.Vertical);

            _rcvImage.SetLayoutManager(staggeredGridLayoutManager);

            _listViewAdapter = new ListViewAdapter(this, _catBreedViewModels, (data) =>
            {
                if (!string.IsNullOrEmpty(data.Id))
                {
                    ShowProgressBar(true);

                    _queryBreed = data.Id;

                    QueryBreedData();
                }
            }, (data) =>
            {
                ShowProgressBar(true);

                Task.Factory.StartNew(() =>
                {
                    _catEntities = _catBreedRepository.LoadAll().ToList();

                    var catEntity = _catEntities.FirstOrDefault(p => p.Name == data.Name);

                    var isExist = catEntity != null;

                    if (!isExist)
                    {
                        var path = _fileService.DownloadImage(data.Name, data.Url);

                        _catBreedRepository.InsertOrReplace(new CatEntity()
                        {
                            Name = data.Name,
                            Height = data.Height,
                            Width = data.Width,
                            Url = path
                        });

                        RunOnUiThread(() =>
                        {
                            AlertDialog alert = new AlertDialog.Builder(this)
                                                    .SetTitle("Notify")
                                                    .SetMessage("Data saved")
                                                    .SetPositiveButton(text: "OK", handler: null)
                                                    .Show();
                        });
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            AlertDialog alert = new AlertDialog.Builder(this)
                                                    .SetTitle("Error")
                                                    .SetMessage("Data already exists!")
                                                    .SetPositiveButton(text: "OK", handler: null)
                                                    .Show();
                        });
                    }

                    ShowProgressBar(false);
                });

            });

            _rcvImage.SetAdapter(_listViewAdapter);

            var scrollListener = new ListViewScrollListenner(async () =>
            {
                ShowProgressBar(true);

                if (_deviceService.IsDeviceOnline())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(_queryBreed))
                        {
                            var tempReferenceModels = await _catBreedClient.GetCatBreedIds(_queryBreed, _listViewAdapter.ItemCount + 10) as List<ReferenceImage>;

                            if (tempReferenceModels.Count > 10)
                            {
                                tempReferenceModels = tempReferenceModels.GetRange(tempReferenceModels.Count - 10, 10);
                            }

                            _catBreedViewModels.AddRange(tempReferenceModels.ToCatBreedViewModels(QueryType.SAMPLE));
                        }
                        else
                        {
                            var tempCatBreedModels = await _catBreedClient.GetCatBreed(_listViewAdapter.ItemCount + 10) as List<CatBreedModel>;

                            if (tempCatBreedModels.Count > 10)
                            {
                                tempCatBreedModels = tempCatBreedModels.GetRange(tempCatBreedModels.Count - 10, 10);
                            }

                            foreach (var item in tempCatBreedModels)
                            {
                                var referenceImage = await _catBreedClient.GetReferenceImage(item.ReferenceImageId);

                                _catBreedViewModels.Add(item.ToCatBreedViewModel(referenceImage));
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

                    if (_listViewAdapter.ItemCount > 10)
                    {
                        _listViewAdapter.NotifyItemInserted(_listViewAdapter.ItemCount - 1);
                    }
                }

                ShowProgressBar(false);
            });

            _rcvImage.AddOnScrollListener(scrollListener);
        }

        public bool OnQueryTextChange(string newText)
        {
            return false;
        }

        public bool OnQueryTextSubmit(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                _adapter.Filter(query);

                _list.BringToFront();

                _list.Visibility = ViewStates.Visible;
            }
            else
            {
                _list.Visibility = ViewStates.Invisible;
            }

            _queryBreed = query;

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
                    try
                    {
                        var referenceModels = await _catBreedClient.GetCatBreedIds(_queryBreed) as List<ReferenceImage>;

                        _catBreedViewModels.Clear();

                        _catBreedViewModels.AddRange(referenceModels.ToCatBreedViewModels(QueryType.SAMPLE));
                    }
                    catch (Exception sie)
                    {
                        RunOnUiThread(() =>
                        {
                            AlertDialog alert = new AlertDialog.Builder(this)
                                                    .SetTitle("Error")
                                                    .SetMessage("Breed not found, please try another type\nFor example: beng (first 4 digits)")
                                                    .SetPositiveButton(text: "OK", handler: null)
                                                    .Show();
                        });
                    }
                }
                else
                {
                    var tempCatBreedModels = await _catBreedClient.GetCatBreed() as List<CatBreedModel>;

                    _catBreedViewModels.Clear();

                    foreach (var item in tempCatBreedModels)
                    {
                        var referenceImage = await _catBreedClient.GetReferenceImage(item.ReferenceImageId);

                        _catBreedViewModels.Add(item.ToCatBreedViewModel(referenceImage, QueryType.BREED));
                    }
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


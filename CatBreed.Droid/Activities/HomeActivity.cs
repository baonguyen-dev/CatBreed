
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
using CatBreed.Entities;
using CatBreed.Repositories;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using static Android.Content.ClipData;

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
        private List<CatBreedModel> _catBreedModels;
        private ProgressBar _pbWaiting;
        private CatBreedRepository _catBreedRepository;
        private List<CatEntity> _catEntities;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.home_activity);

            _svSearch = FindViewById<Android.Widget.SearchView>(Resource.Id.sv_search);

            _rcvImage = FindViewById<RecyclerView>(Resource.Id.rcv_image);

            _pbWaiting = FindViewById<ProgressBar>(Resource.Id.progressBar_cyclic);

            ShowProgressBar(true);

            _svSearch.SetOnQueryTextListener(this);

            _catBreedViewModels = new List<CatBreedViewModel>();

            _catBreedRepository = new CatBreedRepository();

            Task.Factory.StartNew(async () =>
            {
                if (!_deviceService.IsDeviceOnline())
                {
                    _catEntities = _catBreedRepository.LoadAll().ToList();

                    _catBreedViewModels = _catEntities.ToCatBreedViewModels();
                }
                else
                {
                    var tempCatBreedModels = await _catBreedClient.GetCatBreed(limit: 10) as List<CatBreedModel>;

                    foreach (var item in tempCatBreedModels)
                    {
                        var referenceImage = await _catBreedClient.GetReferenceImage(item.ReferenceImageId);

                        _catBreedViewModels.Add(item.ToCatBreedViewModel(referenceImage));
                    }
                }

                RunOnUiThread(() =>
                {
                    ShowProgressBar(false);

                    StaggeredGridLayoutManager staggeredGridLayoutManager = new StaggeredGridLayoutManager(1, LinearLayoutManager.Vertical);

                    _rcvImage.SetLayoutManager(staggeredGridLayoutManager);

                    _listViewAdapter = new ListViewAdapter(this, _catBreedViewModels, (data) =>
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
                    }, _deviceService.IsDeviceOnline());

                    _rcvImage.SetAdapter(_listViewAdapter);

                    var scrollListener = new ListViewScrollListenner(async () =>
                    {
                        if (_deviceService.IsDeviceOnline())
                        {
                            ShowProgressBar(true);

                            var count = _listViewAdapter.ItemCount;

                            if (!string.IsNullOrEmpty(_queryBreed))
                            {
                                var tempReferenceModels = await _catBreedClient.GetCatBreedIds(_queryBreed, count + 10) as List<ReferenceImage>;

                                tempReferenceModels = tempReferenceModels.GetRange(tempReferenceModels.Count - 10, 10);

                                _catBreedViewModels.AddRange(tempReferenceModels.ToCatBreedViewModels());
                            }
                            else
                            {
                                var tempCatBreedModels = await _catBreedClient.GetCatBreed(count + 10) as List<CatBreedModel>;

                                tempCatBreedModels = tempCatBreedModels.GetRange(tempCatBreedModels.Count - 10, 10);

                                foreach (var item in tempCatBreedModels)
                                {
                                    var referenceImage = await _catBreedClient.GetReferenceImage(item.ReferenceImageId);

                                    _catBreedViewModels.Add(item.ToCatBreedViewModel(referenceImage));
                                }
                            }

                            _listViewAdapter.NotifyItemInserted(count - 1);

                            ShowProgressBar(false);
                        }
                    });

                    _rcvImage.AddOnScrollListener(scrollListener);
                });


            });
        }

        public bool OnQueryTextChange(string newText)
        {
            if (string.IsNullOrEmpty(newText))
            {
                OnQueryTextSubmit(newText);
            }

            return false;
        }

        public bool OnQueryTextSubmit(string query)
        {
            _queryBreed = query;

            ShowProgressBar(true);

            if (_deviceService.IsDeviceOnline())
            {
                Task.Factory.StartNew(async () =>
                {
                    if (!string.IsNullOrEmpty(_queryBreed))
                    {
                        try
                        {
                            var referenceModels = await _catBreedClient.GetCatBreedIds(_queryBreed) as List<ReferenceImage>;

                            _catBreedViewModels.Clear();

                            _catBreedViewModels.AddRange(referenceModels.ToCatBreedViewModels());
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

                            _catBreedViewModels.Add(item.ToCatBreedViewModel(referenceImage));
                        }
                    }

                    RunOnUiThread(() =>
                    {
                        _listViewAdapter.NotifyDataSetChanged();

                        ShowProgressBar(false);
                    });
                });
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
    }
}


using CatBreed.ApiClient;
using CatBreed.ApiClient.ViewModels;
using CatBreed.Entities;
using CatBreed.iOS.Controllers;
using CatBreed.iOS.ListViews.Cells.CatImageTableCell;
using CatBreed.iOS.ListViews.DataSources;
using CatBreed.iOS.Services;
using CatBreed.Repositories;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemConfiguration;
using UIKit;

namespace CatBreed.iOS
{
    public partial class ViewController : BaseController, IUISearchBarDelegate
    {
        private ICatBreedClient _catBreedClient => ServiceLocator.Instance.Get<ICatBreedClient>();
        private IFileService _fileService => ServiceLocator.Instance.Get<IFileService>();
        private IDeviceService _deviceSerivce => ServiceLocator.Instance.Get<IDeviceService>();

        private List<CatBreedViewModel> _catBreedViewModels;
        private string _queryBreed;
        private List<CatBreedModel> _catBreedModels;
        private List<CatEntity> _catEntities;
        private CatBreedRepository _catBreedRepository;
        private ReachabilityService _internetReachability;

        private ReachabilityService _hostReachability;
        private bool _isOnline;

        public ViewController() : base("ViewController", null)
        {
        }

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            const string remoteHostName = "www.google.com";

            _catBreedViewModels = new List<CatBreedViewModel>();

            _catBreedRepository = new CatBreedRepository();

            searchBar.Delegate = this;

            searchBar.ReturnKeyType = UIReturnKeyType.Search;

            activityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Large;

            activityIndicator.StartAnimating();

            tableView.RegisterNibForCellReuse(CatImageViewCell.Nib, "CatImageViewCell");

            var catImageSource = new CatImageViewSource(_catBreedViewModels, (data) =>
            {
                ShowProgressBar(true);

                if (!string.IsNullOrEmpty(data.Id))
                {
                    //_svSearch.SetQuery(data.Id, true);
                    _queryBreed = data.Id;
                }

                QueryBreedData();
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

                        InvokeOnMainThread(() =>
                        {
                            UIAlertController alert = UIAlertController.Create("Notify", "Data saved!", UIAlertControllerStyle.Alert);

                            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));

                            this.PresentViewController(alert, true, null);
                        });
                    }
                    else
                    {
                        InvokeOnMainThread(() =>
                        {
                            UIAlertController alert = UIAlertController.Create("Error", "Data already exists!", UIAlertControllerStyle.Alert);

                            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));

                            this.PresentViewController(alert, true, null);
                        });
                    }

                    ShowProgressBar(false);
                });
            });

            catImageSource.SetOnScrolledToEnd(async () =>
            {
                if (_deviceSerivce.IsDeviceOnline())
                {
                    ShowProgressBar(true);

                    try
                    {
                        if (!string.IsNullOrEmpty(_queryBreed))
                        {
                            var tempReferenceModels = await _catBreedClient.GetCatBreedIds(_queryBreed,  10) as List<ReferenceImage>;

                            if (tempReferenceModels.Count > 10)
                            {
                                tempReferenceModels = tempReferenceModels.GetRange(tempReferenceModels.Count - 10, 10);
                            }

                            _catBreedViewModels.AddRange(tempReferenceModels.ToCatBreedViewModels(QueryType.SAMPLE));

                            ShowProgressBar(false);
                        }
                        else
                        {
                            var tempCatBreedModels = await _catBreedClient.GetCatBreed(_catBreedViewModels.Count + 10) as List<CatBreedModel>;

                            if (tempCatBreedModels.Count > 10)
                            {
                                tempCatBreedModels = tempCatBreedModels.GetRange(tempCatBreedModels.Count - 10, 10);
                            }

                            foreach (var item in tempCatBreedModels)
                            {
                                var referenceImage = await _catBreedClient.GetReferenceImage(item.ReferenceImageId);

                                _catBreedViewModels.Add(item.ToCatBreedViewModel(referenceImage));
                            }

                            ShowProgressBar(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        InvokeOnMainThread(() =>
                        {
                            ShowProgressBar(false);

                            UIAlertController alert = UIAlertController.Create("Error", "Too many request!", UIAlertControllerStyle.Alert);

                            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));

                            this.PresentViewController(alert, true, null);
                        });
                    }

                    tableView.ReloadData();
                }
            });

            tableView.Source = catImageSource;
            tableView.AllowsSelection = false;
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RowHeight = 250f;
            tableView.EstimatedRowHeight = 50f;

            _hostReachability = ReachabilityService.ReachabilityWithHostName(remoteHostName);

            _hostReachability.StartNotifier();

            _internetReachability = ReachabilityService.ReachabilityForInternetConnection();

            _internetReachability.StartNotifier();

            UpdateInterfaceWithReachability(_deviceSerivce.IsDeviceOnline());

            resetButton.UserInteractionEnabled = true;

            resetButton.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _queryBreed = "";

                searchBar.Text = _queryBreed;

                SearchButtonClicked(searchBar);
            }));
        }

        [Export("searchBarSearchButtonClicked:")]
        public void SearchButtonClicked(UISearchBar searchBar)
        {
            _queryBreed = searchBar.Text;

            ShowProgressBar(true);

            if (_deviceSerivce.IsDeviceOnline())
            {
                QueryBreedData();
            }
            else
            {
                var tempCatEntities = _catEntities.Where(p => p.Name.ToLower().Contains(_queryBreed.ToLower())).ToList();

                var tempCatBreedViewModels = tempCatEntities.ToCatBreedViewModels();

                _catBreedViewModels.Clear();

                _catBreedViewModels.AddRange(tempCatBreedViewModels);

                tableView.ReloadData();

                ShowProgressBar(false);
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Observe the kNetworkReachabilityChangedNotification. When that notification is posted, the method reachabilityChanged will be called.
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString(ReachabilityService.ReachabilityChangedNotification), OnReachabilityChanged);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, ReachabilityService.ReachabilityChangedNotification);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void OnReachabilityChanged(NSNotification notification)
        {
            var reachability = notification.Object as ReachabilityService;

            UpdateInterfaceWithReachability(reachability == _hostReachability || reachability == _internetReachability);
        }

        private void UpdateInterfaceWithReachability(bool isOnline)
        {
            _isOnline = isOnline;
            
            ShowProgressBar(true);

            _catBreedViewModels.Clear();

            Task.Factory.StartNew(async () =>
            {
                if (!_isOnline)
                {
                    _catEntities = _catBreedRepository.LoadAll().ToList();

                    var tempCatBreedViewModels = _catEntities.ToCatBreedViewModels();

                    _catBreedViewModels.AddRange(tempCatBreedViewModels);
                }
                else
                {
                    var tempCatBreedModels = await _catBreedClient.GetCatBreed() as List<CatBreedModel>;

                    foreach (var item in tempCatBreedModels)
                    {
                        var referenceImage = await _catBreedClient.GetReferenceImage(item.ReferenceImageId);

                        _catBreedViewModels.Add(item.ToCatBreedViewModel(referenceImage));
                    }
                }

                InvokeOnMainThread(() =>
                {
                    tableView.ReloadData();

                    ShowProgressBar(false);
                });
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
                        InvokeOnMainThread(() =>
                        {
                            UIAlertController alert = UIAlertController.Create("Error", "Breed not found, please try another type\nFor example: beng (first 4 digits)", UIAlertControllerStyle.Alert);

                            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));

                            this.PresentViewController(alert, true, null);
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

                InvokeOnMainThread(() =>
                {
                    tableView.ReloadData();
                });

                ShowProgressBar(false);
            });
        }

        private void ShowProgressBar(bool isShow)
        {
            InvokeOnMainThread(() =>
            {
                if (isShow)
                {
                    UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
                }
                else
                {
                    UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                }

                this.View.BringSubviewToFront(activityIndicator);

                activityIndicator.Hidden = !isShow;

                View.BringSubviewToFront(activityIndicator);
            });
        }
    }
}

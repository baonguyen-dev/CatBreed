using CatBreed.ApiClient;
using CatBreed.ApiClient.Models;
using CatBreed.ApiClient.ViewModels;
using CatBreed.Entities;
using CatBreed.iOS.Controllers;
using CatBreed.iOS.ListViews.Cells.CatImageTableCell;
using CatBreed.iOS.ListViews.Cells.SearchTableView;
using CatBreed.iOS.ListViews.DataSources;
using CatBreed.iOS.Services;
using CatBreed.Repositories;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using Foundation;
using System;
using System.Collections;
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
        private IDeviceService _deviceService => ServiceLocator.Instance.Get<IDeviceService>();

        private const int _size = 15;
        private const string _remoteHostName = "www.google.com";

        private List<ApiClient.Models.CatBreedModel> _catBreedViewModels;
        private string _queryBreed;
        private ReachabilityService _internetReachability;
        private ReachabilityService _hostReachability;
        private CatBreedViewModel _viewModel;
        private List<CatTypeModel> _catTypesViewModel;
        private SearchViewSource _searchViewSource;

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

            _viewModel = new CatBreedViewModel(_catBreedClient, _fileService);

            _catBreedViewModels = new List<ApiClient.Models.CatBreedModel>();

            searchBar.Delegate = this;

            searchBar.ReturnKeyType = UIReturnKeyType.Search;

            activityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Large;

            activityIndicator.StartAnimating();

            _catTypesViewModel = new List<CatTypeModel>();

            searchTableView.Hidden = true;

            searchTableView.RegisterNibForCellReuse(SearchTableViewCell.Nib, "SearchTableViewCell");

            _searchViewSource = new SearchViewSource(searchTableView, _catTypesViewModel, (model) =>
            {
                ShowProgressBar(true);

                if (_deviceService.IsDeviceOnline())
                {
                    _queryBreed = model.Type;

                    searchTableView.Hidden = true;

                    searchBar.ResignFirstResponder();

                    QueryBreedData();
                }
            });


            Task.Factory.StartNew(async () =>
            {
                _catTypesViewModel.AddRange(await _viewModel.GetAllCatType());

                _searchViewSource.SetListItem(_catTypesViewModel);
            });

            searchTableView.Source = _searchViewSource;

            searchTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;

            searchTableView.RowHeight = 30f;

            tableView.RegisterNibForCellReuse(CatImageViewCell.Nib, "CatImageViewCell");

            var catImageSource = new CatImageViewSource(_catBreedViewModels, (data) =>
            {
                if (!string.IsNullOrEmpty(data.Id))
                {
                    ShowProgressBar(true);

                    searchTableView.Hidden = true;

                    searchBar.ResignFirstResponder();

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
                        InvokeOnMainThread(() =>
                        {
                            var title = "";
                            var message = "";

                            switch (status)
                            {
                                case SaveStatus.EXIST:

                                    title = "Error";

                                    message = "Data already exists!";

                                    break;
                                case SaveStatus.SUCCESS:

                                    title = "Notify";

                                    message = "Data saved!";

                                    break;
                            }

                            UIAlertController alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

                            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));

                            this.PresentViewController(alert, true, null);
                        });
                    });

                    ShowProgressBar(false);
                });
            });

            catImageSource.SetOnScrolledToEnd(async () =>
            {
                if (_deviceService.IsDeviceOnline())
                {
                    ShowProgressBar(true);

                    try
                    {
                        if (!string.IsNullOrEmpty(_queryBreed))
                        {
                            _catBreedViewModels.AddRange(await _viewModel.QueryCatImageType(_queryBreed, (int)tableView.NumberOfRowsInSection(0) + _size));
                        }
                        else
                        {
                            var catBreedModes = await _viewModel.QueryCatBreed((int)tableView.NumberOfRowsInSection(0) + _size);

                            foreach (var item in catBreedModes)
                            {
                                _catBreedViewModels.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        InvokeOnMainThread(() =>
                        {
                            ShowProgressBar(false);

                            UIAlertController alert = UIAlertController.Create("Error", "To many request!", UIAlertControllerStyle.Alert);

                            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));

                            this.PresentViewController(alert, true, null);
                        });
                    }

                    tableView.ReloadData();

                    ShowProgressBar(false);
                }
            });

            tableView.Source = catImageSource;
            tableView.AllowsSelection = false;
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 50f;

            _hostReachability = ReachabilityService.ReachabilityWithHostName(_remoteHostName);

            _hostReachability.StartNotifier();

            _internetReachability = ReachabilityService.ReachabilityForInternetConnection();

            _internetReachability.StartNotifier();

            UpdateInterfaceWithReachability(_deviceService.IsDeviceOnline());

            resetButton.UserInteractionEnabled = true;

            resetButton.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                ShowProgressBar(true);

                if (_deviceService.IsDeviceOnline())
                {
                    searchBar.Text = "";

                    searchBar.ResignFirstResponder();

                    _queryBreed = "";

                    QueryBreedData();
                }
                else
                {
                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(_viewModel.QueryAllImageFromDatabase());

                    tableView.ReloadData();

                    ShowProgressBar(false);
                }
            }));
        }

        [Export("searchBar:textDidChange:")]
        public void TextChanged(UISearchBar searchBar, string searchText)
        {
            if (_deviceService.IsDeviceOnline())
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    _searchViewSource.Filter(searchText);
           
                    this.View.BringSubviewToFront(searchTableView);

                    searchTableView.Hidden = false;
                }
                else
                {
                    searchTableView.Hidden = true;
                }
            }
            else
            {
                _catBreedViewModels.Clear();
                
                _catBreedViewModels.AddRange(_viewModel.QueryCatBreedFromDatabase(searchText));

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
                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(await _viewModel.QueryCatImageType(_queryBreed));
                }
                else
                {
                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(await _viewModel.QueryCatBreed(_size));
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

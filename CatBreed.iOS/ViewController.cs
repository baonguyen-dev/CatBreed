using CatBreed.ApiClient;
using CatBreed.ApiClient.ViewModels;
using CatBreed.Entities;
using CatBreed.iOS.Controllers;
using CatBreed.iOS.ListViews.Cells.CatImageTableCell;
using CatBreed.iOS.ListViews.DataSources;
using CatBreed.Repositories;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace CatBreed.iOS
{
    public partial class ViewController : BaseController, IUISearchBarDelegate
    {
        private ICatBreedClient _catBreedClient => ServiceLocator.Instance.Get<ICatBreedClient>();
        private IFileService _fileService => ServiceLocator.Instance.Get<IFileService>();

        private List<CatBreedViewModel> _catBreedViewModels;
        private string _queryBreed;
        private List<CatBreedModel> _catBreedModels;
        private List<CatEntity> _catEntities;
        private CatBreedRepository _catBreedRepository;

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

            searchBar.Delegate = this;

            _catBreedViewModels = new List<CatBreedViewModel>();

            _catBreedRepository = new CatBreedRepository();

            activityIndicator.StartAnimating();

            tableView.RegisterNibForCellReuse(CatImageViewCell.Nib, "CatImageViewCell");

            var catImageSource = new CatImageViewSource(_catBreedViewModels, (data) =>
            {
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
                ShowProgressBar(true);

                if (true)
                {
                    if (!string.IsNullOrEmpty(_queryBreed))
                    {
                        var tempReferenceModels = await _catBreedClient.GetCatBreedIds(_queryBreed,  10) as List<ReferenceImage>;

                        if (tempReferenceModels.Count > 10)
                        {
                            tempReferenceModels = tempReferenceModels.GetRange(tempReferenceModels.Count - 10, 10);
                        }

                        _catBreedViewModels.AddRange(tempReferenceModels.ToCatBreedViewModels(QueryType.SAMPLE));
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
                    }

                    tableView.ReloadData();
                }

                ShowProgressBar(false);
            });

            tableView.Source = catImageSource;
            tableView.AllowsSelection = false;
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RowHeight = 250f;
            tableView.EstimatedRowHeight = 50f;

            ShowProgressBar(true);

            Task.Factory.StartNew(async () =>
            {
                if (true)
                {
                    _catEntities = _catBreedRepository.LoadAll().ToList();

                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(_catEntities.ToCatBreedViewModels());
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

        [Export("searchBarTextDidEndEditing:")]
        public void OnEditingStopped(UISearchBar searchBar)
        {
            _queryBreed = searchBar.Text;

            ShowProgressBar(true);

            if (false)
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

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
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

                activityIndicator.Hidden = !isShow;

                View.BringSubviewToFront(activityIndicator);
            });
        }
    }
}

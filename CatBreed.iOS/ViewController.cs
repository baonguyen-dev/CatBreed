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

            tableView.RegisterNibForCellReuse(CatImageViewCell.Nib, "CatImageViewCell");

            var catImageSource = new CatImageViewSource(_catBreedViewModels, (data) =>
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

            tableView.Source = catImageSource;
            tableView.AllowsSelection = false;
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 100f;

            Task.Factory.StartNew(async () =>
            {
                if (false)
                {
                    _catEntities = _catBreedRepository.LoadAll().ToList();

                    _catBreedViewModels.Clear();

                    _catBreedViewModels.AddRange(_catEntities.ToCatBreedViewModels());
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

                InvokeOnMainThread(() =>
                {
                    tableView.ReloadData();
                });
            });
        }

        [Export("searchBarTextDidEndEditing:")]
        public void OnEditingStopped(UISearchBar searchBar)
        {
            
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void ShowProgressBar(bool isShow)
        {
            InvokeOnMainThread(() =>
            {
                activityIndicator.Hidden = false;

                View.BringSubviewToFront(activityIndicator);
            });
        }
    }
}

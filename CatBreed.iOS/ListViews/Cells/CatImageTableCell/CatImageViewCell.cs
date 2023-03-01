using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatBreed.ApiClient;
using CatBreed.ApiClient.Models;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using CoreFoundation;
using CoreGraphics;
using FFImageLoading;
using Foundation;
using UIKit;

namespace CatBreed.iOS.ListViews.Cells.CatImageTableCell
{
	public partial class CatImageViewCell : UITableViewCell
	{
        private IFileService _fileService => ServiceLocator.Instance.Get<IFileService>();
        private IDeviceService _deviceSerivce => ServiceLocator.Instance.Get<IDeviceService>();

        public static readonly NSString Key = new NSString ("CatImageViewCell");
		public static readonly UINib Nib;

		private Action<int> _onDownloadClicked;
        private Action<int> _onBreedClicked;
		private int _position;
        private int _width;
        private UITableView _tableView;
        private Dictionary<int, float> _rowHeights;

		static CatImageViewCell ()
		{
			Nib = UINib.FromName ("CatImageViewCell", NSBundle.MainBundle);
		}

		protected CatImageViewCell (IntPtr handle) : base (handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _onBreedClicked?.Invoke(_position);
            }));

            TvDownload.UserInteractionEnabled = true;

            TvDownload.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _onDownloadClicked?.Invoke(_position);
            }));
        }

        public void SetOnBreedClicked(Action<int> onBreedClicked)
        {
            _onBreedClicked = onBreedClicked;
        }

        public void SetOnDownloadClicked(Action<int> onDownloadClicked)
		{
			_onDownloadClicked = onDownloadClicked;
		}

		public void UpdateData(UITableView tableView, int position, CatBreedModel item, Dictionary<int, float> rowHeights)
		{
            _rowHeights = rowHeights;

            _tableView = tableView;

            _position = position;

            TvName.Text = item.Name;

            if (item.QueryType == QueryType.BREED)
            {
                TvDownload.Hidden = true;
            }
            else
            {
                TvDownload.Hidden = false;
            }

            if (!_deviceSerivce.IsDeviceOnline())
            {
                Task.Factory.StartNew(async () =>
                {
                    await ImageService.Instance
                       .LoadFile(_fileService.ReconstructImagePath(item.Url))
                       .WithCache(FFImageLoading.Cache.CacheType.Memory)
                       .AsUIImageAsync().ContinueWith(res =>
                       {
                           this.InvokeOnMainThread(() =>
                           {
                               this.Hidden = false;
                               if (!res.IsFaulted)
                               {
                                   SetImage(res.Result, item.Width, item.Height);
                               }
                           });
                       });
                });
            }
            else
            {
                Task.Factory.StartNew(async () =>
                {
                    await ImageService.Instance
                       .LoadUrl(item.Url)
                       .WithCache(FFImageLoading.Cache.CacheType.Memory)
                       .AsUIImageAsync().ContinueWith(res =>
                       {
                           this.InvokeOnMainThread(() =>
                           {
                               this.Hidden = false;
                               if (!res.IsFaulted)
                               {
                                   SetImage(res.Result, item.Width, item.Height);
                               }
                           });
                       });
                });
            }

        }

        public void SetImage(UIImage image, int width, int height)
        {
            //DispatchQueue.MainQueue.DispatchAsync(() =>
            //{

                //IvImage.Frame = new CGRect(0, 0, width, height);

                IvImage.ContentMode = UIViewContentMode.ScaleAspectFit;

                if (image != null)
                {
                    IvImage.Image = image;
                }

                this.Hidden = false;
            //});
        }
    }
}

using System;
using System.Threading.Tasks;
using CatBreed.ApiClient.ViewModels;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using CoreGraphics;
using FFImageLoading;
using Foundation;
using UIKit;

namespace CatBreed.iOS.ListViews.Cells.CatImageTableCell
{
	public partial class CatImageViewCell : UITableViewCell
	{
        private IFileService _fileService => ServiceLocator.Instance.Get<IFileService>();

        public static readonly NSString Key = new NSString ("CatImageViewCell");
		public static readonly UINib Nib;

		private Action<int> _onDownloadClicked;
        private Action<int> _onBreedClicked;
		private int _position;

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

		public void UpdateData(int position, string name, string url, QueryType queryType)
		{
			_position = position;

			TvName.Text = name;

            if (queryType == QueryType.BREED)
            {
                TvDownload.Hidden = true;
            }
            else
            {
                TvDownload.Hidden = false;
            }

            if (true)
            {
                Task.Factory.StartNew(async () =>
                {
                    await ImageService.Instance
                       .LoadFile(_fileService.ReconstructImagePath(url))
                       .AsUIImageAsync().ContinueWith(res =>
                       {
                           this.InvokeOnMainThread(() =>
                           {
                               this.Hidden = false;
                               if (!res.IsFaulted)
                               {
                                   SetImage(res.Result);
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
                       .LoadUrl(url)
                       .AsUIImageAsync().ContinueWith(res =>
                       {
                           this.InvokeOnMainThread(() =>
                           {
                               this.Hidden = false;
                               if (!res.IsFaulted)
                               {
                                   SetImage(res.Result);
                               }
                           });
                       });
                });
            }

        }

        public void SetImage(UIImage image)
        {
            IvImage.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height);
            IvImage.ContentMode = UIViewContentMode.ScaleAspectFit;

            this.Hidden = false;

            if (image != null)
                IvImage.Image = image;
        }
    }
}

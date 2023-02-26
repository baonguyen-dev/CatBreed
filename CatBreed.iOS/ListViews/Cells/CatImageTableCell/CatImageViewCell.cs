using System;
using System.Threading.Tasks;
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

            TvDownload.UserInteractionEnabled = true;

            TvDownload.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _onDownloadClicked?.Invoke(_position);
            }));
        }

        public void SetOnDownloadClicked(Action<int> onDownloadClicked)
		{
			_onDownloadClicked = onDownloadClicked;
		}

		public void UpdateData(int position, string name, string url, int height)
		{
			_position = position;

			TvName.Text = name;

            if (false)
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
                                   SetImage(res.Result, height);
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
                                   SetImage(res.Result, height);
                               }
                           });
                       });
                });
            }

        }

        public void SetImage(UIImage image, int height)
        {
            IvImage.Frame = new CGRect(0, 0, this.Frame.Width, height);
            IvImage.ContentMode = UIViewContentMode.ScaleAspectFit;

            this.Hidden = false;

            if (image != null)
                IvImage.Image = image;
        }
    }
}

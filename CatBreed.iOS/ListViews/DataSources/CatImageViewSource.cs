using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using CatBreed.ApiClient.ViewModels;
using CatBreed.iOS.ListViews.Cells.CatImageTableCell;
using Foundation;
using UIKit;

namespace CatBreed.iOS.ListViews.DataSources
{
	public class CatImageViewSource : UITableViewSource
    {
        List<CatBreedViewModel> _items;
        Context _context;
        Action<CatBreedViewModel> _onDownloadClicked;
        Action<CatBreedViewModel> _onBreedClicked;
        Action _onScrolledToEnd;
        bool _isOnline;

        public CatImageViewSource(List<CatBreedViewModel> items, Action<CatBreedViewModel> onBreedClicked, Action<CatBreedViewModel> onDownloadClicked, bool isOnline = true)
		{
            _items = items;

            _onDownloadClicked = onDownloadClicked;

            _onBreedClicked = onBreedClicked;

            _isOnline = isOnline;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CatImageViewCell.Key) as CatImageViewCell;

            if (_items[indexPath.Row].QueryType == QueryType.BREED)
            {
                cell.SetOnBreedClicked((position) =>
                {
                    _onBreedClicked?.Invoke(_items[position]);
                });
            }

            cell.SetOnDownloadClicked((position) =>
            {
                _onDownloadClicked?.Invoke(_items[position]);
            });

            cell.UpdateData(indexPath.Row, _items[indexPath.Row].Name, _items[indexPath.Row].Url, _items[indexPath.Row].QueryType);

            return cell;
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            var offsetY = scrollView.ContentOffset.Y;
            var contentHeight = scrollView.ContentSize.Height;
            if (offsetY > contentHeight - scrollView.Frame.Height)
            {
                _onScrolledToEnd.Invoke();
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _items == null ? 0 : _items.Count;
        }

        public void SetOnScrolledToEnd(Action action)
        {
            _onScrolledToEnd = action;
        }
    }
}


using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using CatBreed.ApiClient.Models;
using CatBreed.iOS.ListViews.Cells.CatImageTableCell;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using Foundation;
using UIKit;

namespace CatBreed.iOS.ListViews.DataSources
{
	public class CatImageViewSource : UITableViewSource
    {
        private IDeviceService _deviceSerivce => ServiceLocator.Instance.Get<IDeviceService>();

        List<CatBreedModel> _items;
        Context _context;
        Action<CatBreedModel> _onDownloadClicked;
        Action<CatBreedModel> _onBreedClicked;
        Action _onScrolledToEnd;
        bool _isOnline;
        Dictionary<int, float> _rowHeights;
        int _width;

        public CatImageViewSource(List<CatBreedModel> items, Action<CatBreedModel> onBreedClicked, Action<CatBreedModel> onDownloadClicked, bool isOnline = true)
		{
            _items = items;

            _onDownloadClicked = onDownloadClicked;

            _onBreedClicked = onBreedClicked;

            _isOnline = isOnline;

            _rowHeights = new Dictionary<int, float>();

            _width = _deviceSerivce.GetScreenWidth();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CatImageViewCell.Key) as CatImageViewCell;

            var currentItem = _items[indexPath.Row];

            if (currentItem.QueryType == QueryType.BREED)
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

            var ratio = (double)_width / currentItem.Width;

            _rowHeights[indexPath.Row] = (int)(ratio * currentItem.Height);

            cell.UpdateData(tableView, indexPath.Row, currentItem, _rowHeights);

            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (_rowHeights.ContainsKey(indexPath.Row) && _rowHeights[indexPath.Row] != 0)
            {
                return _rowHeights[indexPath.Row];
            }
            else
            {
                return 300f;
            }
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {

            if (indexPath.Row == _items.Count - 1)
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


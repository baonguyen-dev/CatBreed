using System;
using System.Collections.Generic;
using CatBreed.ApiClient.Models;
using CatBreed.iOS.ListViews.Cells.CatImageTableCell;
using CatBreed.iOS.ListViews.Cells.SearchTableView;
using Foundation;
using UIKit;

namespace CatBreed.iOS.ListViews.DataSources
{
	public class SearchViewSource: UITableViewSource
    {
        private List<CatTypeModel> _arraylist;
        private List<CatTypeModel> _items;
        private Action<CatTypeModel> _onItemClicked;
        private UITableView _tableView;

        public SearchViewSource(UITableView tableView, List<CatTypeModel> items, Action<CatTypeModel> onItemClicked)
        {
            _tableView = tableView;
            _items = items;
            _arraylist = new List<CatTypeModel>();
            _arraylist.AddRange(_items);
            _onItemClicked = onItemClicked;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(SearchTableViewCell.Key) as SearchTableViewCell;

            cell.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _onItemClicked?.Invoke(_items[indexPath.Row]);
            }));

            cell.UpdateData(_items[indexPath.Row]);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _items == null ? 0 : _items.Count;
        }

        public void SetListItem(List<CatTypeModel> items)
        {
            _items = items;
            _arraylist = new List<CatTypeModel>();
            _arraylist.AddRange(_items);
        }

        public void Filter(String charText)
        {
            charText = charText.ToLower();
            _items.Clear();
            if (charText.Length == 0)
            {
                _items.AddRange(_arraylist);
            }
            else
            {
                foreach (var item in _arraylist)
                {
                    if (item.Name.ToLower().Contains(charText))
                    {
                        _items.Add(item);
                    }
                }
            }

            _tableView.ReloadData();
        }
    }
}


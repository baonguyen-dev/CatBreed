using System;
using Android.OS;
using Android.Views;
using System.Collections.Generic;
using Android.Widget;
using Android.Content;
using System.Globalization;
using CatBreed.ApiClient.Models;

namespace CatBreed.Droid.Adapters
{
	public class SearchListAdapter : BaseAdapter
	{
        Context _context;
        LayoutInflater _inflater;
        private List<CatTypeModel> _arraylist;
        private List<CatTypeModel> _items;
        private Action<CatTypeModel> _onItemClicked;

        public SearchListAdapter(Context context, List<CatTypeModel> items, Action<CatTypeModel> onItemClicked)
        {
            _context = context;
            _items = items;
            _arraylist = new List<CatTypeModel>();
            _arraylist.AddRange(_items);
            _inflater = LayoutInflater.From(_context);
            _onItemClicked = onItemClicked;
        }

        public class ViewHolder: Java.Lang.Object
        {
            public TextView Name { get; set; }
            public int Position { get; set; }
        }

        public override int Count => _items != null ? _items.Count : 0;

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View view, ViewGroup parent)
        {
            ViewHolder holder;

            if (view == null)
            {
                holder = new ViewHolder();
                view = _inflater.Inflate(Resource.Layout.search_list_layout, null);
                // Locate the TextViews in listview_item.xml
                holder.Name = (TextView)view.FindViewById(Resource.Id.name);
                view.Tag = holder;
            }
            else
            {
                holder = (ViewHolder)view.Tag;
            }
            // Set the results into TextViews
            holder.Name.Text = _items[position].Name;
            holder.Position = position;

            holder.Name.Click += (sender, args) =>
            {
                _onItemClicked?.Invoke(_items[holder.Position]);
            };

            return view;
        }

        public void SetListItem(List<CatTypeModel> items)
        {
            _items = items;
            _arraylist = new List<CatTypeModel>();
            _arraylist.AddRange(_items);
        }

        // Filter Class
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

            NotifyDataSetChanged();
        }
    }
}


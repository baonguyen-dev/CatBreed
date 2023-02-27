using System;
using Android.OS;
using Android.Views;
using System.Collections.Generic;
using Android.Widget;
using Android.Content;
using System.Globalization;

namespace CatBreed.Droid.Adapters
{
	public class SearchListAdapter : BaseAdapter
	{
        Context mContext;
        LayoutInflater inflater;
        private List<string> animalNamesList = null;
        private List<string> arraylist;

        public SearchListAdapter(Context context, List<string> animalNamesList)
        {
            mContext = context;
            this.animalNamesList = animalNamesList;
            inflater = LayoutInflater.From(mContext);
            this.arraylist = new List<string>();
            this.arraylist.AddRange(animalNamesList);
        }

        public class ViewHolder: Java.Lang.Object
        {
            public TextView Name { get; set; }
        }

        public override int Count => animalNamesList.Count;

        public override Java.Lang.Object GetItem(int position)
        {
            return animalNamesList[position];
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
                view = inflater.Inflate(Resource.Layout.search_list_layout, null);
                // Locate the TextViews in listview_item.xml
                holder.Name = (TextView)view.FindViewById(Resource.Id.name);
                view.Tag = holder;
            }
            else
            {
                holder = (ViewHolder)view.Tag;
            }
            // Set the results into TextViews
            holder.Name.Text = animalNamesList[position];
            return view;
        }

        // Filter Class
        public void Filter(String charText)
        {
            charText = charText.ToLower();
            animalNamesList.Clear();
            if (charText.Length == 0)
            {
                animalNamesList.AddRange(arraylist);
            }
            else
            {
                foreach (var item in arraylist)
                {
                    if (item.ToLower().Contains(charText))
                    {
                        animalNamesList.Add(item);
                    }
                }
            }
            NotifyDataSetChanged();
        }
    }
}


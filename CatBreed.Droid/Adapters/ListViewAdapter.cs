using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using CatBreed.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CatBreed.Droid.Adapters.ListViewAdapter;

namespace CatBreed.Droid.Adapters
{
    public class ListViewScrollListenner : RecyclerView.OnScrollListener
    {
        private Action _onScrolledToEnd;
        public ListViewScrollListenner(Action onScrolledToEnd)
        {
            _onScrolledToEnd = onScrolledToEnd;
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            base.OnScrollStateChanged(recyclerView, newState);

            if (!recyclerView.CanScrollVertically(1))
            {
                _onScrolledToEnd?.Invoke();
            }
        }
    }

    public class ListViewAdapter : RecyclerView.Adapter
    {
        IList<CatBreedModel> _items;
        Context _context;

        public override int ItemCount => _items == null ? 0 : _items.Count;

        public ListViewAdapter(Context context, IList<CatBreedModel> items)
        {
            _context = context;

            _items = items;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = (ViewHolder)holder;

            vh.Tag = position;

            // TODO: check image name in storage

            if (_items[position] != null)
            {
                vh.TvName.Text = _items[position].Id;

                Glide
                    .With(_context)
                    .Load(_items[position].Url)
                    .Apply(new RequestOptions().Override(_items[position].Width, _items[position].Height))
                    .Into(vh.IvImage);
            }
            else
            {
                Glide.With(_context).Clear(vh.IvImage);

                vh.IvImage.SetImageDrawable(null);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var holder = new ViewHolder(LayoutInflater.From(_context).Inflate(Resource.Layout.ListViewLayout, parent, false), _context);

            return holder;
        }

        public class ViewHolder : RecyclerView.ViewHolder
        {
            public TextView TvName { get; set; }
            public ImageView IvImage { get; set; }
            public int Tag { get; set; }
            public ViewHolder(View itemView, Context context) : base(itemView)
            {
                TvName = itemView.FindViewById<TextView>(Resource.Id.tv_name);
                IvImage = itemView.FindViewById<ImageView>(Resource.Id.iv_image);
            }
        }
    }
}
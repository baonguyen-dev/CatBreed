using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using CatBreed.ApiClient;
using CatBreed.ApiClient.ViewModels;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.IO;
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
        private IFileService _fileService => ServiceLocator.Instance.Get<IFileService>();

        List<CatBreedViewModel> _items;
        Context _context;
        Action<CatBreedViewModel> _onDownloadClicked;
        Action<CatBreedViewModel> _onBreedClicked;
        bool _isOnline;

        public override int ItemCount => _items == null ? 0 : _items.Count;

        public ListViewAdapter(Context context, List<CatBreedViewModel> items, Action<CatBreedViewModel> onBreedClicked, Action<CatBreedViewModel> onDownloadClicked, bool isOnline = true)
        {
            _context = context;

            _items = items;

            _onBreedClicked = onBreedClicked;

            _onDownloadClicked = onDownloadClicked;

            _isOnline = isOnline;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = (ViewHolder)holder;

            vh.Tag = position;

            if (_items[position] != null)
            {
                vh.TvDownload.Visibility = _items[position].QueryType == QueryType.SAMPLE ? ViewStates.Visible : ViewStates.Invisible;

                vh.TvName.Text = _items[position].Name;

                if (!_isOnline)
                {
                    _items[position].Url = _fileService.ReconstructImagePath(_items[position].Url);

                    BitmapFactory.Options options = new BitmapFactory.Options();
                    options.InPreferredConfig = Bitmap.Config.Argb8888;
                    Bitmap bitmap = BitmapFactory.DecodeFile(_items[position].Url, options);

                    Glide.With(_context)
                        .Load(bitmap)
                        .Apply(new RequestOptions().Override(_items[position].Width, _items[position].Height))
                        .Into(vh.IvImage);
                }
                else
                {
                    Glide
                        .With(_context)
                        .Load(_items[position].Url)
                        .Apply(new RequestOptions().Override(_items[position].Width, _items[position].Height))
                        .Into(vh.IvImage);
                }
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

            if (_items[holder.Tag].QueryType == QueryType.BREED)
            {
                holder.RltParent.Click += (sender, args) =>
                {
                    _onBreedClicked?.Invoke(_items[holder.Tag]);
                };
            }

            holder.TvDownload.Click += (sender, args) =>
            {
                _onDownloadClicked?.Invoke(_items[holder.Tag]);

                holder.TvDownload.SetTextColor(Color.ParseColor("#6a05cf"));
            };

            return holder;
        }

        public class ViewHolder : RecyclerView.ViewHolder
        {
            public TextView TvName { get; set; }
            public ImageView IvImage { get; set; }
            public TextView TvDownload { get; set; }
            public RelativeLayout RltParent { get; set; }
            public int Tag { get; set; }
            public ViewHolder(View itemView, Context context) : base(itemView)
            {
                RltParent = itemView.FindViewById<RelativeLayout>(Resource.Id.rlv_parent);
                TvName = itemView.FindViewById<TextView>(Resource.Id.tv_name);
                IvImage = itemView.FindViewById<ImageView>(Resource.Id.iv_image);
                TvDownload = itemView.FindViewById<TextView>(Resource.Id.tv_download);
            }
        }
    }
}
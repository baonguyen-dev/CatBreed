using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Util;
using Java.Lang;

namespace CatBreed.Droid.BroadcastService
{
    [BroadcastReceiver()]
    public class NetworkChangeReceiver: BroadcastReceiver
	{
        Action<bool> _onNetworkChanged;

        public NetworkChangeReceiver()
        {

        }

		public NetworkChangeReceiver(Action<bool> onNetworkChanged)
		{
            _onNetworkChanged = onNetworkChanged;
		}

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                if (IsOnline(context))
                {
                    _onNetworkChanged(true);
                    Log.Error("catbreed", "Online Connect Intenet ");
                }
                else
                {
                    _onNetworkChanged(false);
                    Log.Error("catbreed", "Conectivity Failure !!! ");
                }
            }
            catch (NullPointerException e)
            {
            }
        }

        private bool IsOnline(Context context)
        {
            try
            {
                ConnectivityManager cm = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
                NetworkInfo netInfo = cm.ActiveNetworkInfo;
                //should check null because in airplane mode it will be null
                return (netInfo != null && netInfo.IsConnected);
            }
            catch (NullPointerException e)
            {
                return false;
            }
        }
    }
}


using System;
using Android.App;
using Android.Content;
using Android.Net;
using CatBreed.ServiceLocators.Services;

namespace CatBreed.Droid.Services
{
    public class DroidDeviceService : IDeviceService
    {
        public bool IsDeviceOnline()
        {
            ConnectivityManager manager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            NetworkInfo networkInfo = manager.ActiveNetworkInfo;
            bool isAvailable = false;
            if (networkInfo != null && networkInfo.IsConnected)
            {
                // Network is present and connected
                isAvailable = true;
            }
            return isAvailable;
        }
    }
}


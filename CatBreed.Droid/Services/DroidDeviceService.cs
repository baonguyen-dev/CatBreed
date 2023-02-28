using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Runtime;
using Android.Util;
using Android.Views;
using CatBreed.ServiceLocators.Services;

namespace CatBreed.Droid.Services
{
    public class DroidDeviceService : IDeviceService
    {
        Context _context;

        public DroidDeviceService()
        {
            
        }

        public DroidDeviceService(Context context)
        {
            _context = context;
        }

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

        public int GetScreenWidth()
        {
            IWindowManager windowManager = Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

            Display display = windowManager.DefaultDisplay;

            return display.Width;
        }
    }
}


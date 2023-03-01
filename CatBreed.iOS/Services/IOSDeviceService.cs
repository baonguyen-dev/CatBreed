using System;
using CatBreed.ServiceLocators.Services;
using UIKit;

namespace CatBreed.iOS.Services
{
	public class IOSDeviceService : IDeviceService
	{
        public int GetScreenWidth()
        {
            return (int)UIScreen.MainScreen.Bounds.Width;
        }

        public bool IsDeviceOnline()
        {
            ReachabilityService reachability = ReachabilityService.ReachabilityForInternetConnection();

            NetworkStatus networkStatus = reachability.CurrentReachabilityStatus();

            if (networkStatus == NetworkStatus.ReachableViaWiFi || networkStatus == NetworkStatus.ReachableViaWWAN)
            {
                return true;
            }

            return false;
        }
    }
}


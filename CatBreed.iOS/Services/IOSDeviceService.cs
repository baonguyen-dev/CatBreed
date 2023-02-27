using System;
using CatBreed.ServiceLocators.Services;

namespace CatBreed.iOS.Services
{
	public class IOSDeviceService : IDeviceService
	{
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


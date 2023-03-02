using System;
using Android.App;
using Android.Runtime;
using CatBreed.ApiClient;
using CatBreed.Droid.Services;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine.Cache;

namespace CatBreed.Droid
{
    [Application]
    public class MainApplication : Application
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            Glide.Init(this, new GlideBuilder().SetDiskCache(new ExternalPreferredCacheDiskCacheFactory(this)));

            RegisterServices();
        }

        private void RegisterServices()
        {
            ServiceLocator.Instance.Register<ICatBreedClient, CatBreedClient>();
            ServiceLocator.Instance.Register<IFileService, DroidFileService>();
            ServiceLocator.Instance.Register<IDeviceService, DroidDeviceService>();
        }
    }
}

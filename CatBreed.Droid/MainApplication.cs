using System;
using Android.App;
using Android.Runtime;
using CatBreed.ApiClient;
using CatBreed.ServiceLocators.DI;

namespace CatBreed.Droid
{
    [Application]
    public class MainApplication : Application
    {
        const string BASE_URL = "https://api.thecatapi.com/";

        public MainApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            RegisterServices();
        }

        private void RegisterServices()
        {
            ServiceLocator.Instance.Register<ICatBreedClient, CatBreedClient>(new Uri(BASE_URL), string.Empty);
        }
    }
}

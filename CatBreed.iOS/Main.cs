using System;
using CatBreed.ApiClient;
using CatBreed.iOS.Services;
using CatBreed.ServiceLocators.DI;
using CatBreed.ServiceLocators.Services;
using UIKit;

namespace CatBreed.iOS
{
    public class Application
    {
        const string BASE_URL = "https://api.thecatapi.com/";
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.

            RegisterServices();

            UIApplication.Main(args, null, "AppDelegate");
        }

        private static void RegisterServices()
        {
            ServiceLocator.Instance.Register<ICatBreedClient, CatBreedClient>(new Uri(BASE_URL), string.Empty);
            ServiceLocator.Instance.Register<IFileService, IOSFileService>();
        }
    }
}

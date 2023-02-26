using Android.Graphics;
using Android.OS;
using CatBreed.ServiceLocators.Services;
using Plugin.CurrentActivity;
using System;
using System.IO;
using System.Linq;
using System.Net;
using Path = System.IO.Path;
using Environment = Android.OS.Environment;
using CatBreed.Mobile.Services;

namespace CatBreed.Droid.Services
{
    public class DroidFileService : MobileFileService
    {
        public DroidFileService()
        {

        }

        public override string GetSdCardFolder()
        {
            var path = "";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                var activity = CrossCurrentActivity.Current.Activity;

                path = Path.Combine(activity.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath, APPNAME);
            }
            else
            {
                path = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, APPNAME);
            }

            if (!FolderExists(path))
            {
                CreateFolder(path);
            }

            return path;
        }
    }
}
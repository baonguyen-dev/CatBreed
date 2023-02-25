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

namespace CatBreed.Droid.Services
{
    public class DroidFileService : IFileService
    {
        public DroidFileService()
        {

        }

        public void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public string GetFilePath(string folder, string file)
        {
            if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(file))
            {
                return Path.Combine(folder, file);
            }
            else
            {
                return null;
            }
        }

        public string GetFolder(params string[] par)
        {
            var path = "";

            if (par == null || par.Length == 0)
                return path;

            foreach (var item in par)
            {
                path = Path.Combine(path, item);
            }

            if (!FolderExists(path)) Directory.CreateDirectory(path);

            return path;
        }

        public string GetSdCardFolder()
        {
            var appName = "CatBreed";

            var path = "";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                var activity = CrossCurrentActivity.Current.Activity;

                path = Path.Combine(activity.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath, appName);
            }
            else
            {
                path = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, appName);
            }

            if (!FolderExists(path))
            {
                CreateFolder(path);
            }

            return path;
        }

        public string GetSdCardFolder(params string[] par)
        {
            var list = par.ToList();
            list.Insert(0, GetSdCardFolder());

            return GetFolder(list.ToArray());
        }

        public string SaveBitmap(string fileName, Bitmap bitmap, params string[] relativeFolderParts)
        {
            var path = GetSdCardFolder(relativeFolderParts);

            var filePath = GetFilePath(path, fileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
            }

            return filePath;
        }

        public string DownloadImageWithName(string fileName, string url, params string[] relativeFolderParts)
        {
            using (var webClient = new WebClient())
            {
                byte[] imageBytes;

                try
                {
                    imageBytes = webClient.DownloadData(url);

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        using (var imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length))
                        {
                            return SaveBitmap(fileName, imageBitmap, relativeFolderParts);
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception e)
                {
                    return string.Empty;
                }
            }
        }

        public string DownloadImage(string name, string url, params string[] relativeFolderParts)
        {
            return DownloadImageWithName($"{name}.png", url, relativeFolderParts);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CatBreed.ServiceLocators.Services;

namespace CatBreed.Mobile.Services
{
	public abstract class MobileFileService : IFileService
	{
        public MobileFileService()
        {
        }

        public const string APPNAME = "CATBREED";

        public abstract string GetSdCardFolder();

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Stream CreateFile(string path)
        {
            return File.Create(path);
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

        public string GetSdCardFolder(params string[] par)
        {
            var list = par.ToList();
            list.Insert(0, GetSdCardFolder());

            return GetFolder(list.ToArray());
        }

        public string DownloadImage(string name, string url, params string[] relativeFolderParts)
        {
            return DownloadImageWithName($"{name}.png", url, relativeFolderParts);
        }

        private string DownloadImageWithName(string fileName, string url, params string[] relativeFolderParts)
        {
            using (var webClient = new WebClient())
            {
                byte[] imageBytes;

                try
                {
                    imageBytes = webClient.DownloadData(url);

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        return SaveImage(fileName, imageBytes, relativeFolderParts);
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

        private string SaveImage(string fileName, byte[] bytes, params string[] relativeFolderParts)
        {
            var path = GetSdCardFolder(relativeFolderParts);

            var filePath = GetFilePath(path, fileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                int length = bytes.Length;
                stream.Write(bytes, 0, length);
            }

            return fileName;
        }

        public string ReconstructImagePath(string name, params string[] relativeFolderParts)
        {
            var path = GetSdCardFolder(relativeFolderParts);

            var filePath = GetFilePath(path, name);

            return filePath;
        }
    }
}


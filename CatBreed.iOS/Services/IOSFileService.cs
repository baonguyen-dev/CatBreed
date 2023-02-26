using System;
using System.Drawing;
using System.IO;
using System.Net;
using CatBreed.Mobile.Services;

namespace CatBreed.iOS.Services
{
	public class IOSFileService : MobileFileService
	{
        //private string SaveImage(string fileName, byte[] bytes, params string[] relativeFolderParts)
        //{
        //    var path = GetSdCardFolder(relativeFolderParts);

        //    var filePath = GetFilePath(path, fileName);

        //    using (FileStream stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        int length = bytes.Length;
        //        stream.Write(bytes, 0, length);
        //    }

        //    return filePath;
        //}

        //private string DownloadImageWithName(string fileName, string url, params string[] relativeFolderParts)
        //{
        //    using (var webClient = new WebClient())
        //    {
        //        byte[] imageBytes;

        //        try
        //        {
        //            imageBytes = webClient.DownloadData(url);

        //            if (imageBytes != null && imageBytes.Length > 0)
        //            {
        //                return SaveImage(fileName, imageBytes, relativeFolderParts);
        //            }
        //            else
        //            {
        //                return "";
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            return string.Empty;
        //        }
        //    }
        //}

        //public override string DownloadImage(string name, string url, params string[] relativeFolderParts)
        //{
        //    return DownloadImageWithName($"{name}.png", url, relativeFolderParts);
        //}

        public override string GetSdCardFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), APPNAME);
        }
    }
}


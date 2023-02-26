using System;
using System.Collections.Generic;
using System.Text;

namespace CatBreed.ServiceLocators.Services
{
    public interface IFileService
    {
        string GetSdCardFolder();
        string DownloadImage(string name, string url, params string[] relativeFolderParts);
        string ReconstructImagePath(string name, params string[] relativeFolderParts);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CatBreed.ServiceLocators.Services
{
    public interface IFileService
    {
        string DownloadImage(string name, string url, params string[] relativeFolderParts);
    }
}

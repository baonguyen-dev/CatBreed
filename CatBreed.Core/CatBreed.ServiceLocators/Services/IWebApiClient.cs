using System;
namespace CatBreed.ServiceLocators.Services
{
    public interface IWebApiClient
    {
        void Init(Uri uri, string userAgent);
    }
}

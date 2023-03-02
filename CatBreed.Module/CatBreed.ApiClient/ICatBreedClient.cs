using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatBreed.ApiLocators.Models;

namespace CatBreed.ApiClient
{
    public interface ICatBreedClient
    {
        void Init(Uri uri, string userAgent);

        Task<IList<TheCatModel>> GetCatBreed(int limit = 0);

        Task<IList<ReferenceImage>> GetCatBreedIds(string breed = "", int limit = 20);

        Task<ReferenceImage> GetReferenceImage(string id);

        //Task<IList<CatBreedModel>> SearchCatBreed(string breed);
    }
}

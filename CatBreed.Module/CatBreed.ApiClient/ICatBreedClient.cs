using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatBreed.ApiLocators.Models;

namespace CatBreed.ApiClient
{
    public interface ICatBreedClient
    {
        Task<IList<CatBreedModel>> GetCatBreed(int limit = 0);

        Task<IList<ReferenceImage>> GetCatBreedIds(string breed = "", int limit = 10);

        Task<ReferenceImage> GetReferenceImage(string id);

        //Task<IList<CatBreedModel>> SearchCatBreed(string breed);
    }
}

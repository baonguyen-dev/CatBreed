using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatBreed.ApiLocators.Models;

namespace CatBreed.ApiClient
{
    public interface ICatBreedClient
    {
        Task<IList<CatBreedModel>> GetCatBreed();
    }
}

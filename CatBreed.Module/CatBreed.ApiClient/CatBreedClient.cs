using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CatBreed.ApiLocators.Models;
using CatBreed.ApiLocators.Services;

namespace CatBreed.ApiClient
{
    public class CatBreedClient : WebApiClient, ICatBreedClient
    {
        const string URL_END_GET_BREED = "v1/breeds";
        const string URL_END_SEARCH_BREED = "v1/images/{0}";
        const string URL_END_GET_BREED_ID = "v1/images/search";

        string _token = string.Empty;

        public CatBreedClient()
        {

        }

        public CatBreedClient(Uri baseurl, string useragent) : base(baseurl, useragent)
        {
        }

        public async Task<IList<CatBreedModel>> GetCatBreed(int limit = 10)
        {
            var request = new HttpRequesMessageBuilder()
               .SetBaseUrl(BaseUrl.ToString())
               .SetApiEndpoint(string.Format(URL_END_GET_BREED))
               .SetMethod(HttpMethod.Get)
               .AddParameter("limit", limit.ToString())
               .AddHeader("x-api-key", "live_UUHGO2ZVXZYewuo2aFMAKaHi7OWzUYHct8Hf6OprNNXT4xbkSf95hCLFxrNKKPpe")
               .Build();

            return await SendRequest<IList<CatBreedModel>>(request);
        }

        public async Task<IList<ReferenceImage>> GetCatBreedIds(string id, int limit = 10)
        {
            var request = new HttpRequesMessageBuilder()
               .SetBaseUrl(BaseUrl.ToString())
               .SetApiEndpoint(string.Format(URL_END_GET_BREED_ID))
               .SetMethod(HttpMethod.Get)
               .AddParameter("limit", limit.ToString())
               .AddParameter("breed_ids", id)
               .AddHeader("x-api-key", "live_UUHGO2ZVXZYewuo2aFMAKaHi7OWzUYHct8Hf6OprNNXT4xbkSf95hCLFxrNKKPpe")
               .Build();

            return await SendRequest<IList<ReferenceImage>>(request);
        }

        public async Task<ReferenceImage> GetReferenceImage(string id)
        {
            var request = new HttpRequesMessageBuilder()
               .SetBaseUrl(BaseUrl.ToString())
               .SetApiEndpoint(string.Format(URL_END_SEARCH_BREED, id))
               .SetMethod(HttpMethod.Get)
               .AddHeader("x-api-key", "live_UUHGO2ZVXZYewuo2aFMAKaHi7OWzUYHct8Hf6OprNNXT4xbkSf95hCLFxrNKKPpe")
               .Build();

            return await SendRequest<ReferenceImage>(request);
        }
    }
}

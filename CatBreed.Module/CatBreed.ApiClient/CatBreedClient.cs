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
        const string URL_END_GET_BREED = "v1/images/search";

        string _token = string.Empty;

        public CatBreedClient()
        {

        }

        public CatBreedClient(Uri baseurl, string useragent) : base(baseurl, useragent)
        {
        }

        public async Task<IList<CatBreedModel>> GetCatBreed()
        {
            var request = new HttpRequesMessageBuilder()
               .SetBaseUrl(BaseUrl.ToString())
               .SetApiEndpoint(string.Format(URL_END_GET_BREED))
               .SetMethod(HttpMethod.Get)
               .AddParameter("limit", "10")
               .AddHeader("x-api-key", "live_UUHGO2ZVXZYewuo2aFMAKaHi7OWzUYHct8Hf6OprNNXT4xbkSf95hCLFxrNKKPpe")
               .Build();

            return await SendRequest<IList<CatBreedModel>>(request);
        }
    }
}

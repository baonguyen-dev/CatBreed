using System;
using Newtonsoft.Json;

namespace CatBreed.ApiClient
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CatBreedModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        public CatBreedModel()
        {
        }
    }
}

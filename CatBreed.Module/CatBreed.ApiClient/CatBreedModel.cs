﻿using System;
using Newtonsoft.Json;

namespace CatBreed.ApiClient
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CatBreedModel
    {
        [JsonProperty("weight")]
        public Weight Weight { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cfa_url")]
        public string CfaUrl { get; set; }

        [JsonProperty("vetstreet_url")]
        public string VetstreetUrl { get; set; }

        [JsonProperty("vcahospitals_url")]
        public string VcahospitalsUrl { get; set; }

        [JsonProperty("temperament")]
        public string Temperament { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("country_codes")]
        public string CountryCodes { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("life_span")]
        public string LifeSpan { get; set; }

        [JsonProperty("indoor")]
        public int Indoor { get; set; }

        [JsonProperty("lap")]
        public int Lap { get; set; }

        [JsonProperty("alt_names")]
        public string AltNames { get; set; }

        [JsonProperty("adaptability")]
        public int Adaptability { get; set; }

        [JsonProperty("affection_level")]
        public int AffectionLevel { get; set; }

        [JsonProperty("child_friendly")]
        public int ChildFriendly { get; set; }

        [JsonProperty("dog_friendly")]
        public int DogFriendly { get; set; }

        [JsonProperty("energy_level")]
        public int EnergyLevel { get; set; }

        [JsonProperty("grooming")]
        public int Grooming { get; set; }

        [JsonProperty("health_issues")]
        public int HealthIssues { get; set; }

        [JsonProperty("intelligence")]
        public int Intelligence { get; set; }

        [JsonProperty("shedding_level")]
        public int SheddingLevel { get; set; }

        [JsonProperty("social_needs")]
        public int SocialNeeds { get; set; }

        [JsonProperty("stranger_friendly")]
        public int StrangerFriendly { get; set; }

        [JsonProperty("vocalisation")]
        public int Vocalisation { get; set; }

        [JsonProperty("experimental")]
        public int Experimental { get; set; }

        [JsonProperty("hairless")]
        public int Hairless { get; set; }

        [JsonProperty("natural")]
        public int Natural { get; set; }

        [JsonProperty("rare")]
        public int Fare { get; set; }

        [JsonProperty("rex")]
        public int Rex { get; set; }

        [JsonProperty("suppressed_tail")]
        public int SuppressedRail { get; set; }

        [JsonProperty("short_legs")]
        public int ShortLegs { get; set; }

        [JsonProperty("wikipedia_url")]
        public string WikipediaUrl { get; set; }

        [JsonProperty("hypoallergenic")]
        public int Hypoallergenic { get; set; }

        [JsonProperty("reference_image_id")]
        public string ReferenceImageId { get; set; }

        [JsonProperty("cat_friendly")]
        public int? CatFriendly { get; set; }

        [JsonProperty("bidability")]
        public int? Bidability { get; set; }

        public CatBreedModel()
        {
        }
    }

    public class Weight
    {
        [JsonProperty("imperial")]
        public string Imperial { get; set; }

        [JsonProperty("metric")]
        public string Metric { get; set; }
    }

    public class ReferenceImage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}

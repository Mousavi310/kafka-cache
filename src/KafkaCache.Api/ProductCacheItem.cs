using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KafkaCache.Api
{
    public class ProductCacheItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }        
    }
}
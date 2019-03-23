using System;
using Newtonsoft.Json;

namespace KafkaCache.Api
{
    public class ProductCacheItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("id")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("creation_time")]
        public DateTime CreationTime { get; set; }
        
    }
}
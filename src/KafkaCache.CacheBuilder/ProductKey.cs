using Newtonsoft.Json;

namespace KafkaCache.CacheBuilder
{
    public class ProductKey
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
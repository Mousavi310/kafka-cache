using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace KafkaCache.Api
{
    public class CacheWarmer : ICacheWarmer
    {
        private readonly IMemoryCache _cache;
        public CacheWarmer(IMemoryCache cache)
        {
            _cache = cache;
        }
        public void WarmUp()
        {
            var consumerConfig = new ConsumerConfig
            {
                //For warmup, group id must be unique to start reading message from beginning of a topic.
                GroupId = $"{Guid.NewGuid().ToString("N")}-group-id",
                BootstrapServers = "localhost:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            using (var c = new ConsumerBuilder<int, string>(consumerConfig).Build())
            {
                c.Subscribe("products.cache");

                try
                {
                    while (true)
                    {
                        try
                        {
                            //If no records exist continute
                            var cr = c.Consume(TimeSpan.FromMilliseconds(1));

                            if (cr == null)
                            {
                                //we read all messages of topic. Continue.
                                return;
                            }

                            if (cr.Value == null)
                            {
                                if(_cache.TryGetValue(cr.Key, out _))
                                {
                                    _cache.Remove(cr.Key);
                                }
                            }
                            else
                            {
                                var item = JsonConvert.DeserializeObject<ProductCacheItem>(cr.Value);
                                _cache.Set(cr.Key, item);
                            }
                        }
                        catch (Exception e)
                        {
                            //Log e
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Log ex
                    throw;
                }
            }
        }
    }
}
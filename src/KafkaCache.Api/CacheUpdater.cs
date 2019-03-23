using System;
using Confluent.Kafka;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace KafkaCache.Api
{
    public class CacheUpdater : ICacheUpdater
    {
        private readonly IMemoryCache _cache;
        public CacheUpdater(IMemoryCache cache)
        {
            _cache = cache;

        }
        public void Run(string groupId, TimeSpan? timeout)
        {
            var consumerConfig = new ConsumerConfig
            {
                GroupId = groupId,
                // We always read data from the beginning.
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
                            ConsumeResult<int, string> cr;
                            
                            cr = timeout == null ? cr = c.Consume(): cr = c.Consume(timeout.Value);                               

                            
                            if (cr == null)
                            {
                                //we read all messages of topic. Continue.
                                return;
                            }

                            if (cr.Value == null)
                            {
                                if (_cache.TryGetValue(cr.Key, out _))
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
                finally
                {
                    //Allow backgroun updater consume new updates.
                    c.Close();
                }
            }
        }
    }
}
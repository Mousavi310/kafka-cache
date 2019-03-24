using System;
using System.Threading;
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
        public void Run(string groupId, bool returnOnLastOffset)
        {
            var consumerConfig = new ConsumerConfig
            {
                GroupId = groupId,
                BootstrapServers = "localhost:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            using (var c = new ConsumerBuilder<Ignore, string>(consumerConfig)
            .Build())
            {
                c.Subscribe("products.cache");

                try
                {
                    var watermark = c.QueryWatermarkOffsets(new TopicPartition("products.cache", new Partition(0)), TimeSpan.FromMilliseconds(1));

                    if(returnOnLastOffset && watermark.High.Value == 0)
                        return;

                    while (true)
                    {
                        try
                        {
                            ConsumeResult<Ignore, string> cr = c.Consume();
                            
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

                            if(returnOnLastOffset && watermark.High.Value - 1 == cr.Offset.Value)
                                return;
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
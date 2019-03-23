using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace KafkaCache.CacheBuilder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var consumerConfig = new ConsumerConfig
            {
                GroupId = "products-cache-group-id",
                BootstrapServers = "kafka:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
            var cacheTopic = "products-cache";

            using (var c = new ConsumerBuilder<string, string>(consumerConfig).Build())
            {
                c.Subscribe("my-topic");

                try
                {
                    while (true)
                    {
                        using (var p = new ProducerBuilder<int, string>(producerConfig).Build())
                        {
                            try
                            {
                                var cr = c.Consume();
                                var key = JsonConvert.DeserializeObject<ProductKey>(cr.Key);

                                await p.ProduceAsync(cacheTopic, new Message<int, string> { Value = cr.Value, Key = key.Id });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw e;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    c.Close();
                }
            }
        }
    }
}

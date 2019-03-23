using System;

namespace KafkaCache.Api
{
    public interface ICacheUpdater
    {
         void Run(string groupId, TimeSpan? timeout);
    }
}
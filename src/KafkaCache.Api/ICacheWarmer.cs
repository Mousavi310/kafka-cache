using System.Threading.Tasks;

namespace KafkaCache.Api
{
    public interface ICacheWarmer
    {
         void WarmUp();
    }
}
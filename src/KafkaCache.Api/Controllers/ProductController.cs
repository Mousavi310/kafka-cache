using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace KafkaCache.Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public ProductController(IMemoryCache cache)
        {
            _cache = cache;

        }
        [HttpGet("{id}")]
        public ActionResult<ProductCacheItem> Get(int id)
        {
            if(!_cache.TryGetValue(id, out var product))
            {
                return NotFound($"Product with product id {id} is not found");
            }

            return (ProductCacheItem)product;
        }
    }
}
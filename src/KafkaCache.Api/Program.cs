using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KafkaCache.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            var cacheUpdater = host.Services.GetRequiredService<ICacheUpdater>();

            //Choose different group id, because we want to read cache topic from the scratch.
            var groupId = $"products.cache.{Guid.NewGuid().ToString("N")}.group.id";

            //Warmup! It will returns to caller method.
            cacheUpdater.Run(groupId, true);

            //Updater in background
            Task.Run(() =>
            {
                try
                {
                    //It never returns;
                    cacheUpdater.Run(groupId, false);
                }
                catch (Exception ex)
                {
                    //log ex
                    throw;
                }
            });

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}

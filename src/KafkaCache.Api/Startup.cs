using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaCache.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMemoryCache();
            services.AddSingleton<ICacheUpdater, CacheUpdater>();


            //Cache warm up
            var cacheUpdater = services.BuildServiceProvider().GetService<ICacheUpdater>();

            var groupId = $"products.cache.{Guid.NewGuid().ToString("N")}.group.id";

            //Warmup! It will returns to caller method.
            cacheUpdater.Run(groupId, TimeSpan.FromMilliseconds(1));

            //Updater in background
            Task.Run(() =>
            {
                try
                {
                    //It never returns;
                    cacheUpdater.Run(groupId, null);
                }
                catch (Exception ex)
                {
                    //log ex
                    throw;
                }
                

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }







            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

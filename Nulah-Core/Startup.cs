using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using NulahCore.Models;
using NulahCore.Controllers;
using StackExchange.Redis;
using Microsoft.Extensions.FileProviders;
using NulahCore.Filters;

namespace NulahCore {
    public class Startup {
        private IConfigurationRoot _config;
        private AppSetting ApplicationSettings = new AppSetting();

        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.dev.json", optional: true, reloadOnChange: false);
            _config = builder.Build();
            ApplicationSettings.ContentRoot = env.ContentRootPath;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            _config.GetSection("ConnectionStrings").Bind(ApplicationSettings);
            ApplicationSettings.Api_Mailgun = _config["Api:Mailgun"];

            IDatabase redis = RedisStore.RedisCache;
            services.AddScoped(_ => redis);
            services.AddScoped(_ => ApplicationSettings);

            services.AddMvc(Options => {
                Options.RespectBrowserAcceptHeader = true;
            })
                .AddMvcOptions(Options => {
                    Options.Filters.Add(new ActionFilter(redis));
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            /*
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"content", @"images")),
                RequestPath = new PathString("/content/images")
            });
            */

            // Needs to be as close to the start of the pipeline as possible to ensure it can rewrite any errors later on
            // Specifically it's probably going to be the 500 ScreamingExceptions throws on an internal error.
            // See: https://andrewlock.net/re-execute-the-middleware-pipeline-with-the-statuscodepages-middleware-to-create-custom-error-pages/
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseScreamingExceptions();
            loggerFactory.AddConsole();

            if(env.IsDevelopment()) {
                //app.UseDeveloperExceptionPage();
                loggerFactory.AddDebug();
            }

            // Best to be last to make sure all the other magic can happen first before we attempt to return a view.
            app.UseMvc();

            /*
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
            */
        }
    }
}

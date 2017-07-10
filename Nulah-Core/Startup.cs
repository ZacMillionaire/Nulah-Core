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
using NulahCore.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using NulahCore.Models.User;
using System.Security.Claims;
using NulahCore.Controllers.Users;

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
            // configure app settings
            _config.GetSection("ConnectionStrings").Bind(ApplicationSettings);
            ApplicationSettings.Api_Mailgun = _config["Api:Mailgun"];
            ApplicationSettings.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), _config["Logging:Level"]);

            // inject redis and app settings
            IDatabase redis = RedisStore.RedisCache;
            services.AddScoped(_ => redis);
            services.AddScoped(_ => ApplicationSettings);

            services.AddAuthentication(
                options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme
            );

            // configure MVC
            services.AddMvc(Options => {
                Options.RespectBrowserAcceptHeader = true;
            })
            .AddMvcOptions(Options => {
                Options.Filters.Add(new ActionFilter(redis));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDatabase Redis) {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // configure cookies
            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/Login"),
                LogoutPath = new PathString("/Logout"),
                AccessDeniedPath = "/",
                ExpireTimeSpan = new TimeSpan(30, 0, 0, 0),
                SlidingExpiration = true
            });

            app.UseOAuthAuthentication(new OAuthOptions {
                ClientId = _config["Api:GitHub:ClientId"],
                ClientSecret = _config["Api:GitHub:ClientSecret"],
                Scope = { "public_repo" },
                SaveTokens = true,
                AuthenticationScheme = "GitHub",
                AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
                TokenEndpoint = "https://github.com/login/oauth/access_token",
                UserInformationEndpoint = "https://api.github.com/user?access_token=",
                CallbackPath = new PathString("/signin-github"),

                // This looks fucking ugly though, need to find out how to move it to a class
                Events = new OAuthEvents {
                    // https://auth0.com/blog/authenticating-a-user-with-linkedin-in-aspnet-core/
                    // The OnCreatingTicket event is called after the user has been authenticated and the OAuth middleware has
                    // created an auth ticket. We need to manually call the UserInformationEndpoint to retrieve the user's information,
                    // parse the resulting JSON to extract the relevant information, and add the correct claims.
                    OnCreatingTicket = async context => {
                        await UserProfile.RegisterUser(context, Redis, ApplicationSettings);
                    }
                }
            });

            /*
            // commented out until I start doing image uploads
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
            loggerFactory.AddProvider(new ScreamingLoggerProvider(Redis, ApplicationSettings));

            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
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

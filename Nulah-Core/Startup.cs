﻿using System;
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
using Microsoft.AspNetCore.Mvc;

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
            ApplicationSettings.Provider = _config["Provider"];
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            // configure app settings
            _config.GetSection("ConnectionStrings").Bind(ApplicationSettings);

            // Get global administrators list and cast to int[]
            ApplicationSettings.GlobalAdministrators = _config.GetSection("SiteSettings:GlobalAdministrators")
                .AsEnumerable()
                .Where(x => x.Value != null)
                .Select(x => int.Parse(x.Value))
                .ToArray();

            ApplicationSettings.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), _config["Logging:Level"]);

            // inject redis and app settings
            IDatabase redis = RedisStore.RedisCache;
            services.AddScoped(_ => redis);
            services.AddScoped(_ => ApplicationSettings);

            services.AddAuthentication(
                options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme
            );

            // configure MVC

            // Debug ssl fix, all you have to do is enable ssl, ctrl+f5 so it won't go into debug mode, jump into IE to install the cert
            // the-get fucked that's fucking stupid but it works
            // https://stackoverflow.com/questions/40965442/an-error-occurred-attempting-to-determine-the-process-id-of-dotnet-exe-which-is
            // not doing ssl anyway, but for reference when I change my mind later
            services.AddMvc(Options => {
                Options.RespectBrowserAcceptHeader = true;
            })
            .AddMvcOptions(Options => {
                Options.Filters.Add(new ActionFilter(redis));
            });
        }

        public class Provider {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string[] Scope { get; set; }
            public bool SaveTokens { get; set; }
            /// <summary>
            ///     <para>
            /// Used for OAuth Authorization header, GitHub uses token [...], Discord uses Bearer [...]
            ///     </para><para>
            /// This is case sensitive as well. eg token != Token
            ///     </para>
            /// </summary>
            public string AuthorizationHeader { get; set; }
            public string AuthenticationScheme { get; set; }
            public string AuthorizationEndpoint { get; set; }
            public string TokenEndpoint { get; set; }
            public string UserInformationEndpoint { get; set; }
            public string CallbackPath { get; set; }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDatabase Redis) {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            //var options = new RewriteOptions()
            //   .AddRedirectToHttps();

            //app.UseRewriter(options);


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

            var loginProvider = _config.GetSection("OAuthProviders").Get<Provider>();

            //foreach(var provider in loginProviders) { // future skeleton for providing multiple provider logins

            var authOptions = new OAuthOptions {
                ClientId = loginProvider.ClientId,
                ClientSecret = loginProvider.ClientSecret,
                SaveTokens = loginProvider.SaveTokens,
                AuthenticationScheme = loginProvider.AuthenticationScheme,
                AuthorizationEndpoint = loginProvider.AuthorizationEndpoint,
                TokenEndpoint = loginProvider.TokenEndpoint,
                UserInformationEndpoint = loginProvider.UserInformationEndpoint,
                CallbackPath = new PathString(loginProvider.CallbackPath),
                // This looks fucking ugly though, need to find out how to move it to a class
                Events = new OAuthEvents {
                    // https://auth0.com/blog/authenticating-a-user-with-linkedin-in-aspnet-core/
                    // The OnCreatingTicket event is called after the user has been authenticated and the OAuth middleware has
                    // created an auth ticket. We need to manually call the UserInformationEndpoint to retrieve the user's information,
                    // parse the resulting JSON to extract the relevant information, and add the correct claims.
                    OnCreatingTicket = async context => {
                        await UserProfile.RegisterUser(context, loginProvider, Redis, ApplicationSettings);
                    }
                }
            };


            // can't just pass the scope array in because...reasons?
            foreach(var scope in loginProvider.Scope) {
                authOptions.Scope.Add(scope);
            }

            app.UseOAuthAuthentication(authOptions);
            //} // end provider loop for later

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

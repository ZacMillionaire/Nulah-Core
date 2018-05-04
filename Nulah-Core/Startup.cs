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
using Nulah.Users.Models;
using System.Security.Claims;
using NulahCore.Controllers.Users;
using Microsoft.AspNetCore.Mvc;

namespace NulahCore {
    public class Startup {
        private IConfigurationRoot _config { get; set; }
        private AppSetting _ApplicationSettings = new AppSetting();

        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.dev.json", optional: true, reloadOnChange: false);
            _config = builder.Build();
            _config.Bind(_ApplicationSettings);
            _ApplicationSettings.ContentRoot = env.ContentRootPath;
            _ApplicationSettings.Redis = _config.GetSection("ConnectionStrings:Redis").Get<RedisConnection>();
            //_ApplicationSettings.Provider = _config["Provider"];
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            // configure app settings
            // Get global administrators list and cast to int[]
            _ApplicationSettings.GlobalAdministrators = _config.GetSection("SiteSettings:GlobalAdministrators")
                .AsEnumerable()
                .Where(x => x.Value != null)
                .Select(x => int.Parse(x.Value))
                .ToArray();

            _ApplicationSettings.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), _config["Logging:Level"]);

            // inject redis and app settings
            IDatabase Redis = RedisStore.RedisCache;
            services.AddScoped(_ => Redis);
            services.AddScoped(_ => _ApplicationSettings);

            var OAuthService = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => options = new CookieAuthenticationOptions {
                    LoginPath = new PathString("/Login"),
                    LogoutPath = new PathString("/Logout"),
                    AccessDeniedPath = "/",
                    ExpireTimeSpan = new TimeSpan(30, 0, 0, 0),
                    SlidingExpiration = true
                });

            var loginProviders = _ApplicationSettings.OAuthProviders;

            foreach(var loginProvider in loginProviders) {
                OAuthService.AddOAuth(loginProvider.AuthenticationScheme, options => {
                    options.ClientId = loginProvider.ClientId;
                    options.ClientSecret = loginProvider.ClientSecret;
                    options.SaveTokens = loginProvider.SaveTokens;
                    options.AuthorizationEndpoint = loginProvider.AuthorizationEndpoint;
                    options.TokenEndpoint = loginProvider.TokenEndpoint;
                    options.UserInformationEndpoint = loginProvider.UserInformationEndpoint;
                    options.CallbackPath = new PathString(loginProvider.CallbackPath);

                    foreach(var scope in loginProvider.Scope) {
                        options.Scope.Add(scope);
                    }

                    // This looks fucking ugly though, need to find out how to move it to a class
                    options.Events = new OAuthEvents {
                        // https://auth0.com/blog/authenticating-a-user-with-linkedin-in-aspnet-core/
                        // The OnCreatingTicket event is called after the user has been authenticated and the OAuth middleware has
                        // created an auth ticket. We need to manually call the UserInformationEndpoint to retrieve the user's information,
                        // parse the resulting JSON to extract the relevant information, and add the correct claims.
                        OnCreatingTicket = async context => {
                            await UserOAuth.RegisterUser(context, loginProvider, Redis, _ApplicationSettings);
                        },/*
                        // Here until I figure out what magic kestrel needs to actually work with https.
                        // Apparently it's not a thing you should do (which is why I have it proxied behind nginx): https://github.com/aspnet/KestrelHttpServer/issues/1108
                        // but it's still fucking annoying having my redirect_uri's going to http, because https causes a weird handshake bug because asdfklsflkashfdaslkf
                        // I'm a "professional", btw. There's no way you'd actually think that looking at my code though.
                        OnRedirectToAuthorizationEndpoint = context => {
                            var uri = HttpUtility.ParseQueryString(context.RedirectUri);
                            uri["redirect_uri"] = uri["redirect_uri"].Replace("http","https");
                            context.Response.Redirect(uri.ToString());
                            return Task.FromResult(0);
                        },*/
                        OnRemoteFailure = async context => {
                            await UserOAuth.OAuthRemoteFailure(context, loginProvider, Redis, _ApplicationSettings);
                            context.HttpContext.Response.StatusCode = 500;
                        }
                    };
                });
            }

            // configure MVC

            // Debug ssl fix, all you have to do is enable ssl, ctrl+f5 so it won't go into debug mode, jump into IE to install the cert
            // the-get fucked that's fucking stupid but it works
            // https://stackoverflow.com/questions/40965442/an-error-occurred-attempting-to-determine-the-process-id-of-dotnet-exe-which-is
            // not doing ssl anyway, but for reference when I change my mind later
            services.AddMvc(Options => {
                Options.RespectBrowserAcceptHeader = true;
            })
            .AddMvcOptions(Options => {
                Options.Filters.Add(new ActionFilter(Redis));
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
            loggerFactory.AddConsole();
            loggerFactory.AddProvider(new ScreamingLoggerProvider(Redis, _ApplicationSettings));

            app.UseDefaultFiles();
            app.UseStaticFiles();

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




            app.UseAuthentication();

            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddDebug();
            }

            // Best to be last to make sure all the other magic can happen first before we attempt to return a view.
            app.UseMvc();
        }
    }
}

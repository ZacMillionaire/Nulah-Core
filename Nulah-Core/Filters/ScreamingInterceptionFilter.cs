using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Filters {

    public static class ScreamingInterceptionExtention {
        public static IApplicationBuilder UseScreamingExceptions(this IApplicationBuilder Builder) {
            return Builder.UseMiddleware<ScreamingInterceptionFilter>();
        }
    }

    public class ScreamingInterceptionFilter : ExceptionFilterAttribute {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _logger;
        private readonly IDatabase _redis;
        private readonly IApplicationBuilder _app;
        private Stopwatch _timer;

        public ScreamingInterceptionFilter(RequestDelegate Next, ILoggerFactory Logger, IDatabase Redis) {
            _next = Next;
            _logger = Logger;
            _redis = Redis;
        }

        public async Task Invoke(HttpContext Context) {
            try {
                _timer = new Stopwatch();
                _timer.Start();
                await _next.Invoke(Context);
                _timer.Stop();
            } catch(Exception e) {
                // set status code and redirect to error page
                Context.Response.StatusCode = 500;
                //Context.Response.Redirect("/Error/500");
            }
        }
    }
}

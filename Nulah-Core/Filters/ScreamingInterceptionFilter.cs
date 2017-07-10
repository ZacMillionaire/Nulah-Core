using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NulahCore.Extensions.Logging;
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
        private readonly ILoggerFactory _logFactory;
        private readonly IDatabase _redis;
        private readonly ILogger _logger;
        private Stopwatch _timer;

        public ScreamingInterceptionFilter(RequestDelegate Next, ILoggerFactory LogFactory, IDatabase Redis) {
            _next = Next;
            _logFactory = LogFactory;
            _logger = LogFactory.CreateLogger<ScreamingInterceptionFilter>();
            _redis = Redis;
        }

        public async Task Invoke(HttpContext Context) {
            try {

                if(string.Equals(Context.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.OrdinalIgnoreCase)) {
                    Context.Request.Scheme = "https";
                }

                var request = new Navigation {
                    Method = Context.Request.Method,
                    Path = Context.Request.Path,
                    Query = new Query {
                        Parts = Context.Request.Query
                            .Select(x => new QueryFragment {
                                Param = x.Key,
                                Value = x.Value
                            })
                            .ToArray()
                    },
                    Protocol = Context.Request.Protocol,
                    Scheme = Context.Request.Scheme,
                    Referer = Context.Request.Headers["Referer"].Count > 0 ? Context.Request.Headers["Referer"][0] : null,
                    UserAgent = Context.Request.Headers["User-Agent"],
                    IpAddress = Context.Request.Headers["X-Forwarded-For"],
                    RequestTime = DateTime.UtcNow,
                    RawHeaders = Context.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString())
                };

                if(request.Referer != null) {
                    var Ref = new Uri(request.Referer);
                    var Path = Ref.AbsolutePath;
                    var Host = Ref.Authority;
                    var Query = Ref.Query;
                    request.Ref = new Referer {
                        Raw = Ref.OriginalString,
                        Host = Host,
                        Path = Path,
                        Query = Query
                    };
                }

                _timer = new Stopwatch();
                _timer.Start();
                await _next.Invoke(Context);
                _timer.Stop();

                request.ResponseTime = _timer.ElapsedMilliseconds;

                _logger.LogNavigation(request, (int)ScreamingLogLevel.Navigation);

            } catch(Exception e) {
                // set status code and redirect to error page
                Context.Response.StatusCode = 500;
                _logger.LogCritical((int)ScreamingLogLevel.Error_Critical, JsonConvert.SerializeObject(e));
                //Context.Response.Redirect("/Error/500");
            }
        }
    }

    public class Navigation {
        public string Method { get; set; }
        public string Path { get; set; }
        public Query Query { get; set; }
        public string Protocol { get; set; }
        public string Scheme { get; set; }
        public string Referer { get; set; }
        public Referer Ref { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// response in ms
        /// </summary>
        public long ResponseTime { get; set; }
        public Dictionary<string, string> RawHeaders { get; set; }
    }

    public class Referer {
        public string Raw { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
    }

    public class Query {
        public QueryFragment[] Parts { get; set; }

        public override string ToString() {
            return string.Join("&", Parts.Select(x => $"{x.Param}={x.Value}"));
        }
    }

    public class QueryFragment {
        public string Param { get; set; }
        public string Value { get; set; }
    }
}

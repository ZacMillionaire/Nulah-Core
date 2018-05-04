using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NulahCore.Controllers.Users;
using NulahCore.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NulahCore.Filters {
    public class ActionFilter : IActionFilter {

        private readonly IDatabase _redis;

        public ActionFilter(IDatabase Redis) {
            _redis = Redis;
        }
        public void OnActionExecuted(ActionExecutedContext context) {
        }

        /// <summary>
        /// Inject user data from data store before the action has started
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context) {
            // Inject a PublicUser into ViewData
            var user = context.HttpContext.User;
            var ViewData = ( context.Controller as Controller ).ViewData;

            // create a blank user profile
            var UserData = new PublicUser();

            if(user.Identity.IsAuthenticated) {
                // create a PublicUser object with data from redis
                var UserKey = user.Claims.First(x => x.Type == "RedisKey").Value;
                UserData = UserProfile.GetUser(UserKey, _redis);
            }
            ViewData.Add("User", UserData);

        }
    }
}

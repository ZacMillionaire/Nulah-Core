using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NulahCore.Models.User;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Filters {
    public class ActionFilter : IActionFilter {

        private readonly IDatabase _redis;

        public ActionFilter(IDatabase Redis) {
            _redis = Redis;
        }
        public void OnActionExecuted(ActionExecutedContext context) {
            //throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context) {
            // Inject a PublicUser into ViewData
            var user = context.HttpContext.User;
            var ViewData = ( context.Controller as Controller ).ViewData;

            // create a blank user profile
            var UserData = new PublicUser();

            if(user.Identity.IsAuthenticated) {
                UserData.IsLoggedIn = true;
            }
            ViewData.Add("User", UserData);

            //throw new NotImplementedException();
        }
    }
}

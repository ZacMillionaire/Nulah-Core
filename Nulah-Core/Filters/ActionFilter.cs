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
            var a = context.Controller as Controller;
            var b = a.ViewData;
            b.Add("User", new PublicUser());

            //throw new NotImplementedException();
        }
    }
}

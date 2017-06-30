using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NulahCore.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Filters {

    public enum Role {
        IsLoggedIn,
        IsLoggedOut,
        CanComment,
        CanAuthor
    }

    /// <summary>
    /// Is done after action filter
    /// </summary>
    public class UserFilter : ActionFilterAttribute {

        private readonly Role[] _RequestedRoles;
        private readonly string _redirect;

        public UserFilter(Role Require) {
            _RequestedRoles = new[] { Require };
            _redirect = "~/";
        }

        public UserFilter(Role[] Requires) {
            _RequestedRoles = Requires;
            _redirect = "~/";
        }

        public UserFilter(Role Require, string Redirect) {
            _RequestedRoles = new[] { Require };
            _redirect = Redirect;
        }

        public UserFilter(Role[] Requires, string Redirect) {
            _RequestedRoles = Requires;
            _redirect = Redirect;
        }

        [ServiceFilter(typeof(PublicUser))]
        public override void OnActionExecuting(ActionExecutingContext context) {

            // This isn't necessarily a real user, it could be a blank PublicUser that only has IsLoggedIn set to false
            // If the instance IsLoggedIn is true however, we have a legitimate user account
            Controller BaseController = (Controller)context.Controller;
            PublicUser CurrentUserInstance = (PublicUser)BaseController.ViewData["User"];

            if(!_RequestedRoles.All(x => CurrentUserInstance.Roles.Contains(x))) {
                context.Result = new RedirectResult(_redirect);
            }

            /*
            var user = context.HttpContext.User;
            if(user.Identity.IsAuthenticated && RequestedRoles.Contains(Role.IsLoggedOut)) {
                context.Result = new RedirectResult("~/");
            }*/
            /*
            // This isn't necessarily a real user, it could be a blank PublicUser that only has IsLoggedIn set to false
            // If the instance IsLoggedIn is true however, we have a legitimate user account
            Controller BaseController = (Controller)context.Controller;
            PublicUser CurrentUserInstance = (PublicUser)BaseController.ViewData["User"];

            // If the page requires a user to be logged out (Login page, Register page), redirect them to the index
            if(RequestedRoles.Contains(Role.IsLoggedOut) && CurrentUserInstance.IsLoggedIn == true) {
                context.Result = new RedirectResult("~/");
            } else if(CurrentUserInstance.IsLoggedIn == false) {
                // Redirect to login page if user is not logged in (or registered) and set a redirect url as a cookie
                context.Result = new RedirectResult("~/Login");
                // TODO: The cookie part
                //BaseController.ViewData["RedirectUrl"] = BaseController.HttpContext.Request.Path;
            }

    */
            base.OnActionExecuting(context);
        }
    }
}

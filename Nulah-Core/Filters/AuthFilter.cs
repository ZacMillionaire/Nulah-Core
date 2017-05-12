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

    public class UserFilter : ActionFilterAttribute {

        private readonly Role[] RequestedRoles;

        public UserFilter(Role Role) {
            RequestedRoles = new[] { Role };
        }

        public UserFilter(Role[] Roles) {
            RequestedRoles = Roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {

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


            base.OnActionExecuting(context);
        }
    }
}

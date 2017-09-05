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
        CanAuthor,
        GlobalAdministrator
    }

    /// <summary>
    /// Is done after action filter
    /// </summary>
    public class UserFilter : ActionFilterAttribute {

        private readonly Role[] _RequestedRoles;
        private readonly string _redirect;
        private readonly bool _globalAdministratorOverride;

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

        /// <summary>
        /// If the user does not contain 1 of the roles in the Requires array, redirect to Redirect.
        /// </summary>
        /// <param name="Requires"></param>
        /// <param name="Redirect"></param>
        public UserFilter(Role[] Requires, string Redirect) {
            _RequestedRoles = Requires;
            _redirect = Redirect;
        }

        /// <summary>
        ///     <para>
        /// If the user does not contain 1 of the roles in the Requires array, redirect to Redirect.
        ///     </para><para>
        /// If GlobalAdministratorOverride is true, the roles are only checked if the user's role list does not contain the GlobalAdministrator role
        ///     </para>
        /// </summary>
        /// <param name="Requires"></param>
        /// <param name="Redirect"></param>
        public UserFilter(Role[] Requires, string Redirect, bool GlobalAdministratorOverride) {
            _RequestedRoles = Requires;
            _redirect = Redirect;
            _globalAdministratorOverride = GlobalAdministratorOverride;
        }

        [ServiceFilter(typeof(PublicUser))]
        public override void OnActionExecuting(ActionExecutingContext context) {

            // This isn't necessarily a real user, it could be a blank PublicUser that only has IsLoggedIn set to false
            // If the instance IsLoggedIn is true however, we have a legitimate user account
            Controller BaseController = (Controller)context.Controller;
            PublicUser CurrentUserInstance = (PublicUser)BaseController.ViewData["User"];

            var globalAdminCheck = _globalAdministratorOverride && !CurrentUserInstance.Roles.Contains(Role.GlobalAdministrator);

            if(!_globalAdministratorOverride || ( _globalAdministratorOverride && !CurrentUserInstance.Roles.Contains(Role.GlobalAdministrator) )) {
                if(!_RequestedRoles.All(x => CurrentUserInstance.Roles.Contains(x))) {
                    context.Result = new LocalRedirectResult(_redirect);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}

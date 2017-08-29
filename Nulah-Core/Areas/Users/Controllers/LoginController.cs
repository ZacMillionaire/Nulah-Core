using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Filters;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;
using NulahCore.Models;
using NulahCore.Controllers.Users;
using NulahCore.Models.User;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Areas.Users.Controllers {
    [Area("Users")]
    public class LoginController : Controller {

        private readonly IDatabase _redis;
        private readonly AppSetting _settings;

        public LoginController(IDatabase Redis, AppSetting Settings) {
            _redis = Redis;
            _settings = Settings;
        }

        [HttpGet]
        [Route("~/Login")]
        [UserFilter(Role.IsLoggedOut)]
        public async Task<IActionResult> Index(string error, string error_description) {
            if(error == null && error_description == null) {
                var challenge = HttpContext.Authentication.ChallengeAsync("GitHub", properties: new AuthenticationProperties {
                    RedirectUri = "/"
                });
                await challenge;
            }
            return View();
        }

        [Route("~/Logout")]
        [UserFilter(Role.IsLoggedIn)]
        public async Task<IActionResult> Logout() {

            var a = HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await a;
            if(a.IsCompleted) {
                PublicUser CurrentUserInstance = (PublicUser)ViewData["User"];

                UserProfile.UserLogOut(CurrentUserInstance, _redis, _settings);
            }

            // Redirect the user to the home page after signing out
            return Redirect("~/");
        }
    }
}

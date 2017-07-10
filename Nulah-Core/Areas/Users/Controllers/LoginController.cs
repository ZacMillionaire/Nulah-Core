using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Filters;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Areas.Users.Controllers {
    [Area("Users")]
    public class LoginController : Controller {

        [HttpGet]
        [Route("~/Login")]
        [UserFilter(Role.IsLoggedOut)]
        public async Task<IActionResult> Index(string error, string error_description) {
            if(error == null && error_description == null) {
                await HttpContext.Authentication.ChallengeAsync("GitHub", properties: new AuthenticationProperties {
                    RedirectUri = "/"
                });
            }
            return View();
        }

        [Route("~/Logout")]
        [UserFilter(Role.IsLoggedIn)]
        public async Task<IActionResult> Logout() {
            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect the user to the home page after signing out
            return Redirect("~/");
        }
    }
}

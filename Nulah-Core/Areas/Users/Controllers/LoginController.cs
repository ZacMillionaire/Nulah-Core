﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;
using NulahCore.Models;
using NulahCore.Controllers.Users;
using Microsoft.AspNetCore.Authentication;

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

        /*
        // TODO: Reimplement this for selecting a specific provider for multiple login
        /// <summary>
        /// Login provider selection page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/Login")]
        [UserFilter(Role.IsLoggedOut)]
        public IActionResult Index() {
            return View();
        }
        */

        [HttpGet]
        [Route("~/Login/{Provider}")]
        [UserFilter(Role.IsLoggedOut)]
        // TODO: Reimplement this for multiple login
        //[Route("~/Login/{Provider}")]
        //public async Task<IActionResult> LoginWithProvider(string Provider) {
        public async Task<IActionResult> LoginWithProvider(string error, string error_description, string Provider) {
            if(error == null && error_description == null) {
                var challenge = HttpContext.ChallengeAsync(Provider, properties: new AuthenticationProperties {
                    RedirectUri = "/"
                });
                await challenge;
            }
            return View();
        }

        [Route("~/Logout")]
        [UserFilter(Role.IsLoggedIn)]
        public async Task<IActionResult> Logout() {

            var a = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await a;
            if(a.IsCompleted) {
                PublicUser CurrentUserInstance = (PublicUser)ViewData["User"];

                UserProfile.Logout(CurrentUserInstance, _redis, _settings);
            }

            // Redirect the user to the home page after signing out
            return Redirect("~/");
        }
    }
}

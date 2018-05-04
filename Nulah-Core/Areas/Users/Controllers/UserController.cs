using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Controllers;
using NulahCore.Filters;
using NulahCore.Controllers.Users;
using StackExchange.Redis;
using NulahCore.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Areas.Users.Controllers {
    [Area("Users")]
    public class UserController : Controller {

        private readonly IDatabase _redis;
        private readonly AppSetting _settings;

        public UserController(IDatabase Redis, AppSetting Settings) {
            _redis = Redis;
            _settings = Settings;
        }

        [HttpGet]
        [Route("~/Profile")]
        //[UserFilter(Role.IsLoggedIn)]
        public IActionResult SelfProfile() {
            return View();
        }

        [HttpGet]
        [Route("~/Profile/{UserId}")]
        public IActionResult OtherUserProfile(string UserId) {
            // Will be null if no data is found for the UserId
            ViewData["Profile"] = UserProfile.GetUserById(UserId, _redis, _settings);
            return View();
        }

        [HttpGet]
        [Route("~/Profile/Refresh")]
        public async Task<IActionResult> RefreshProfile() {
            PublicUser currentUser = (PublicUser)ViewData["User"];

            await UserProfile.RefreshPublicUserProfile(currentUser.UserId, _settings, _redis);
            return RedirectToAction("SelfProfile");
        }

        /*
        [HttpGet]
        [Route("~/Register")]
        public IActionResult Register() {
            return View();
        }

        [HttpPost]
        [Route("~/Api/Register")]
        public void DoRegister()
        {
            new Mail().SendMail();
        }
        */
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Controllers;
using StackExchange.Redis;
using NulahCore.Controllers.Api;

namespace NulahCore.Areas.Index.Controllers
{
    [Area("Index")]
    public class IndexController : Controller
    {
        private readonly IDatabase _redis;

        public IndexController(IDatabase redis)
        {
            _redis = redis;
        }

        [Route("~/")]
        [HttpGet]
        public IActionResult FrontPage()
        {
            ViewData["RedisStats"] = StatusApi.GetRedisStatus(_redis);
            return View();
        }
    }
}

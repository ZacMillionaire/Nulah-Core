using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Models;
using NulahCore.Controllers;
using Newtonsoft.Json;
using NulahCore.Controllers.Api;
using StackExchange.Redis;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Api.Controllers
{
    [Area("Api")]
    public class StatsApiController : Controller
    {
        private readonly IDatabase _redis;

        public StatsApiController(IDatabase redis)
        {
            _redis = redis;
        }

        [HttpGet]
        [Route("/Api/Stats/Redis")]
        public RedisStatus GetRedisStat()
        {
            return StatusApi.GetRedisStatus(_redis);
        }
    }
}

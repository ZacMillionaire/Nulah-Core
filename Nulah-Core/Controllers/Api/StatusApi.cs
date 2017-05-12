using Newtonsoft.Json;
using NulahCore.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Api
{
    public class StatusApi
    {
        public static RedisStatus GetRedisStatus(IDatabase Redis)
        {
            return RedisStore.Deserialise<RedisStatus>(RedisStore.Connection.GetDatabase(1).ListGetByIndex("Nulah-Redis-Status", 0)); //JsonConvert.DeserializeObject<RedisStatus>(RedisStore.Connection.GetDatabase(1).ListGetByIndex("Nulah-Redis-Status", 0));
        }
    }
}

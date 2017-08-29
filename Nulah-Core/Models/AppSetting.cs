using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Models {
    public class AppSetting {
        public RedisConnection Redis { get; set; }
        public string ContentRoot { get; set; }
        public LogLevel LogLevel { get; set; }
        public int[] GlobalAdministrators { get; set; }
    }

    public class RedisConnection {
        public string EndPoint { get; set; }
        public string ClientName { get; set; }
        public bool AdminMode { get; set; }
        public string Password { get; set; }
        public int Database { get; set; }
        /// <summary>
        /// Ends with a colon
        /// </summary>
        public string BaseKey { get; set; }
    }
}

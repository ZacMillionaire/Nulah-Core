using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Providers.Discord {
    public class DiscordOAuthProfile : IOAuthProvider {
        public string username { get; set; }
        public bool verified { get; set; }
        public bool mfa_enabled { get; set; }
        public string id { get; set; }
        public object avatar { get; set; }
        public string discriminator { get; set; }
        public string email { get; set; }
        public string access_token { get; set; }
    }

    public class DiscordProfile : IProviderProfile {
        public string username { get; set; }
        public bool verified { get; set; }
        public string id { get; set; }
        public object avatar { get; set; }
        public string discriminator { get; set; }
        public string email { get; set; }
    }
}

using System;

namespace Core {
    public class AppSettings {
        public class AppSetting {
            public RedisConnection Redis { get; set; }
            public string ContentRoot { get; set; }
            //public LogLevel LogLevel { get; set; }
            public int[] GlobalAdministrators { get; set; }
            public string Provider { get; set; }
            public Provider[] OAuthProviders { get; set; }
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

        public class Provider {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string[] Scope { get; set; }
            public bool SaveTokens { get; set; }
            public string AuthorizationHeader { get; set; }
            public string AuthenticationScheme { get; set; }
            public string AuthorizationEndpoint { get; set; }
            public string TokenEndpoint { get; set; }
            public string UserInformationEndpoint { get; set; }
            public string CallbackPath { get; set; }
            public string ProviderShort { get; set; }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Providers.GitHub {

    /// <summary>
    /// Deserialization result from OAuth request
    /// </summary>
    public class GitHubOAuthProfile : IOAuthProvider {
#pragma warning disable IDE1006 // Naming Styles
        public string login { get; set; }
        public int id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public string access_token { get; set; }

        /// <summary>
        /// GitHub site admin
        /// </summary>
        [DefaultValue(false)]
        public bool site_admin { get; set; }
        public string name { get; set; }
        public string company { get; set; }
        public string blog { get; set; }
        public string location { get; set; }
        public string email { get; set; }

        [DefaultValue(false)]
        public bool hireable { get; set; }

        public string bio { get; set; }
        public int public_repos { get; set; }
        public int public_gists { get; set; }
        public int followers { get; set; }
        public int following { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    /// <summary>
    /// Public data from first OAuth request
    /// </summary>
    public class GitHubProfile : IProviderProfile {
#pragma warning disable IDE1006 // Naming Styles
        public string GitProfile { get; set; }
        public bool Hireable { get; set; }
        public int ID { get; set; }
        /// <summary>
        /// login name
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Actual name if given.
        /// Will be displayed if not null, otherwise views will use DisplayName
        /// </summary>
        public string PublicName { get; set; }
        public string url_repo { get; set; }
        public string url_gists { get; set; }
        public DateTime GitHubUserSince { get; set; }
        public string GitHubBio { get; set; }
        public string GitHubUserApi { get; set; }
        public int PublicRepoCount { get; set; }
        public int PublicGistCount { get; set; }
        public string EmailAddress { get; set; }
        public string Company { get; set; }
        public string Blog { get; set; }
        public string Gravatar { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}

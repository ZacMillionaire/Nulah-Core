using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Users.Models {
    public class User {
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
    }

}

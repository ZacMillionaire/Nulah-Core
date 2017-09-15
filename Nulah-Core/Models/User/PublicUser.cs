using NulahCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Models.User {
    public class PublicUser {
        public bool IsLoggedIn { get; set; }
        public Role[] Roles { get; set; }
        public string DisplayName { get; set; }
        public string GitHubUrl { get; set; }
        public bool Hireable { get; set; }
        public string Company { get; set; }
        public string Blog { get; set; }
        public DateTime MemberSince { get; set; }
        public DateTime LastUpdated { get; set; }
        public int UserId { get; set; }

        public PublicUser() {
            IsLoggedIn = false;
            Roles = new Role[] { Role.IsLoggedOut };
        }
    }

    public class PublicUser<T> {

    }
}

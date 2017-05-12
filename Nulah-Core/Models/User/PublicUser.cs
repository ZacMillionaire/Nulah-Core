using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Models.User {
    public class PublicUser {
        public bool IsLoggedIn { get; set; }

        public PublicUser() {
            IsLoggedIn = false;
        }
    }
}

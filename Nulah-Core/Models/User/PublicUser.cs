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
        public DateTime MemberSince { get; set; }
        public DateTime LastUpdated { get; set; }
        public int UserId { get; set; }
        public TimeSpan Timezone { get; set; }

        /// <summary>
        ///     <para>
        /// Blank user to represent users who aren't logged in.
        ///     </para>
        /// </summary>
        public PublicUser() {
            IsLoggedIn = false;
            Roles = new Role[] { Role.IsLoggedOut };
        }

        /// <summary>
        ///     <para>
        /// Creates a public user from a User object, set to logged in.
        ///     </para>
        /// </summary>
        /// <param name="UserData"></param>
        public PublicUser(Controllers.Users.Models.User UserData) {
            DisplayName = UserData.Details.DisplayName;
            LastUpdated = UserData.Details.LastUpdatedUTC;
            MemberSince = UserData.Details.RegisteredUTC;
            UserId = UserData.Id;
            Timezone = UserData.Details.TimezoneAdjustment;

            // This is coming from a registration/login, so the user will be logged in
            IsLoggedIn = true;

            // we don't set roles here. It's done from the class calling this constructor, or elsewhere afterwards
        }
    }

    public class PublicUser<T> {

    }
}

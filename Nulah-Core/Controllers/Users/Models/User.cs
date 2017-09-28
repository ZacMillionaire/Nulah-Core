using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Users.Models {
    public class User<T> {

        /// <summary>
        ///     <para>
        /// Users are identified by whatever id field the OAuth provider users.
        ///     </para>
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     <para>
        /// The users OAuth access token, stored for future requests.
        ///     </para>
        ///     <para>
        /// This is not a token for the OAuth request, but a token to access OAuth resources.
        ///     </para>
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        ///     <para>
        /// User data that does not differ between OAuth providers
        ///     </para>
        /// </summary>
        public UserDetails Details { get; set; }

        /// <summary>
        ///     <para>
        /// OAuth Provider specific profile details, values here are not guaranteed across different providers, and will vary depending on provider chosen.
        ///     </para>
        /// </summary>
        public T ProviderProfile { get; set; }


        /// <summary>
        ///     <para>
        /// Compares the provider profile data and returns if they differ
        ///     </para>
        /// </summary>
        /// <param name="New"></param>
        /// <returns></returns>
        public bool ProviderProfileHasChanged(User<T> New) {
            return JsonConvert.SerializeObject(this.ProviderProfile) != JsonConvert.SerializeObject(New.ProviderProfile);
        }

    }
}

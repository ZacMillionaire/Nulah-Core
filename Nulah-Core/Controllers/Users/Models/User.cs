using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Users.Models {

    /// <summary>
    ///     <para>
    /// A User is the base that a PublicUser class is created from.
    ///     </para>
    /// </summary>
    public class User {

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
        public string ProviderProfile { get; set; }


        /// <summary>
        ///     <para>
        /// Compares the provider profile data and returns if they differ
        ///     </para>
        /// </summary>
        /// <param name="New"></param>
        /// <returns></returns>
        public bool ProviderProfileHasChanged(User New) {
            return this.ProviderProfile != New.ProviderProfile;
        }

        /// <summary>
        ///     <para>
        /// Serialises and stores a provider profile data class.
        ///     </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ProviderClass"></param>
        /// <returns></returns>
        public void SetProviderProfile<T>(T ProviderClass) {
            this.ProviderProfile = JsonConvert.SerializeObject(ProviderClass);
        }

        /// <summary>
        ///     <para>
        /// Set the ProviderProfile to a previously serialized string.
        ///     </para><para>
        /// Checks to see that the serialised string can be successfully deserialized
        ///     </para>
        /// </summary>
        /// <param name="SerializedProfile"></param>
        public void SetProviderProfile<T>(string SerializedProfile) {

            try {
                JsonConvert.DeserializeObject<T>(SerializedProfile);
            } catch(Exception e) {
                throw new ArgumentException($"Failed to set provider value, could not deserialize from string to {typeof(T).AssemblyQualifiedName}");
            }
            this.ProviderProfile = SerializedProfile;
        }

        /// <summary>
        ///     <para>
        /// Deserialises the provider data and returns it as it's defined class.
        ///     </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetProviderProfile<T>() {
            try {
                return JsonConvert.DeserializeObject<T>(this.ProviderProfile);
            } catch(Exception e) {
                throw new JsonSerializationException($"Unable to deserialize ProviderProfile string into ");
            }
        }
    }
}

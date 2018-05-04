using Newtonsoft.Json;
using Nulah.Users.Models;
using StackExchange.Redis;
using System;

namespace Nulah.Users {
    public class UserManager {
        private readonly IDatabase _Redis;
        private readonly string _BaseKey;
        private readonly string _UserTableKey;

        private const string USER_HASH_PROFILE = "Profile";


        public UserManager(IDatabase Redis, string BaseKey) {

            // Check if the basekey given ends with a colon, and throw an exception if it doesn't.
            // Why not just append a colon here instead of throwing? Reasons.
            if(!BaseKey.EndsWith(':')) {
                throw new ArgumentException("Base key must end with a colon");
            }

            _Redis = Redis;
            _BaseKey = BaseKey;

            _UserTableKey = $"{BaseKey}Users";
        }

        /// <summary>
        ///     <para>
        /// Returns a users Redis key, based on their User data.
        ///     </para>
        ///     <para>
        /// Called during UserOAuth
        ///     </para>
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Provider"></param>
        /// <returns></returns>
        public string GetUserTableKey(User UserData) {
            return $"{_UserTableKey}:{UserData.ProviderShort}-{UserData.Id}";
        }

        /// <summary>
        ///     <para>
        /// Creates a new user from User data, or returns their existing profile, updating their last login time.
        ///     </para>
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        public User GetOrCreateUser(User UserData) {

            var userKey = GetUserTableKey(UserData);
            User user = GetUserFromCache(userKey);

            if(user != null) {
                UpdateLastSeen(user);
            } else {
                user = UserData;
                /*
                // Create a new user record
                user = new PublicUser() {
                    Id = UserData.Id,
                    LastSeenUTC = DateTime.UtcNow,
                    Name = UserData.Name,
                    Provider = UserData.Provider,
                    ProviderShort = UserData.ProviderShort,
                    isExpressMode = false,
                    Zoom = new Models.Maps.ZoomOptions(), // Will have defaults set on creation
                    Marker = new Models.Maps.MarkerOptions(), // Will have defaults set on creation.
                    Preferences = new Preferences()
                };*/
            }

            // Add whatever data we have to the cache
            CreateOrUpdateCachedUser(user);

            return user;
        }

        public User GetUserFromCache(string UserKey) {
            if(_Redis.HashExists(UserKey, USER_HASH_PROFILE)) {

                // Get stored user from the cache, and update the last seen
                User user = JsonConvert.DeserializeObject<User>(_Redis.HashGet(UserKey, USER_HASH_PROFILE));
                return user;
            }

            return null;
        }


        /// <summary>
        ///     <para>
        /// Updates the last seen time for a cached user
        ///     </para>
        /// </summary>
        /// <param name="User"></param>
        private void UpdateLastSeen(User User) {
            if(User == null) {
                throw new ArgumentNullException("User given cannot be null.");
            }

            User.LastSeenUTC = DateTime.UtcNow;
        }

        /// <summary>
        ///     <para>
        /// Adds a PublicUser to the cache. 
        ///     </para>
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        private User CreateOrUpdateCachedUser(User User) {
            if(User == null) {
                throw new ArgumentNullException("User given cannot be null.");
            }

            try {
                _Redis.HashSet(GetUserTableKey(User), USER_HASH_PROFILE, JsonConvert.SerializeObject(User));
            } catch(Exception e) {
                throw new Exception("Redis Exception on creating user entry.", e);
            }

            return User;
        }
    }
}

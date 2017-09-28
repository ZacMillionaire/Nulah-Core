using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using NulahCore.Models.User;
using NulahCore.Models;
using NulahCore.Controllers.Users.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;

namespace NulahCore.Controllers.Providers.GitHub {
    public class GitHubLoginProvider : ILoginProvider<GitHubProfile, GitHubOAuthProfile> {

        private const string HASH_ProfileData = "Profile";
        /*
        private const string HASH_PublicData = "PublicData";
        private const string HASH_AccessToken = "GitHub_AccessToken";
        private const string KEY_UserProfile_Tokened = "{0}Users:{1}";


        public PublicUser<GitHubProfile> CreatePublicUserProfile(OAuthProfile OAuthUserData, IDatabase Redis, AppSetting Settings) {
            throw new NotImplementedException();
        }

        public Task<PublicUser<GitHubProfile>> RefreshPublicUserProfile(string RedisUserKeyId, AppSetting Settings, IDatabase Redis) {
            throw new NotImplementedException();
        }

        public User<GitHubProfile> Register(OAuthProfile OAuthData, IDatabase Redis, AppSetting Settings) {

            string KEY_UserProfile = $"{Settings.Redis.BaseKey}Users:{OAuthData.id}";
            User<GitHubProfile> UserProfile;

            if(!Redis.HashExists(KEY_UserProfile, HASH_ProfileData)) {
                UserProfile = CreatePublicUser(OAuthData, Redis, Settings);
            }

            Redis.HashSet(KEY_UserProfile, HASH_PublicData, JsonConvert.SerializeObject(PublicProfile));

            throw new NotImplementedException();
        }

        */

        /// <summary>
        ///     <para>
        /// Creates and returns a User&lt;GitHubProfile&gt;, based on their given OAuth profile.
        ///     </para>
        /// </summary>
        /// <param name="OAuthData"></param>
        /// <returns></returns>
        public User<GitHubProfile> CreatePublicUser(GitHubOAuthProfile OAuthData) {

            var redisUser = new User<GitHubProfile> {
                Id = OAuthData.id, // their GitHub userId
                Details = new UserDetails {
                    DisplayName = OAuthData.name ?? OAuthData.login, // Use a display name if set on github, otherwise display their login name
                    RegisteredUTC = DateTime.UtcNow, // set registered and last updated to the same time
                    LastUpdatedUTC = DateTime.UtcNow,
                    TimezoneId = new TimeSpan(0, 0, 0) // set the timezone offset to 0 to indicate no change should be made when converting dates on the frontend
                },
                AccessToken = OAuthData.access_token,
                ProviderProfile = new GitHubProfile {
                    GitProfile = OAuthData.html_url,
                    Hireable = OAuthData.hireable,
                    ID = OAuthData.id,
                    DisplayName = OAuthData.login, // fallback if name below is null
                    PublicName = OAuthData.name,
                    url_repo = OAuthData.repos_url, // public only
                    url_gists = OAuthData.gists_url.Split('{')[0], // public only, drop the curly brace param
                    GitHubUserSince = OAuthData.created_at,
                    GitHubBio = OAuthData.bio,
                    GitHubUserApi = OAuthData.url, // use this to refresh stats
                    PublicRepoCount = OAuthData.public_repos,
                    PublicGistCount = OAuthData.public_gists,
                    Blog = OAuthData.blog,
                    Company = OAuthData.company,
                    EmailAddress = OAuthData.email,
                    Gravatar = OAuthData.avatar_url
                }
            };

            return redisUser;
        }

        /// <summary>
        ///     <para>
        /// Fetches and deserializes an OAuth profile response
        ///     </para>
        /// </summary>
        /// <param name="response"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<GitHubOAuthProfile> GetOAuthProfile(HttpResponseMessage response, OAuthCreatingTicketContext context) {

            var user = JsonConvert.DeserializeObject<GitHubOAuthProfile>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });

            user.access_token = context.AccessToken;

            return user;
        }

        /// <summary>
        ///     <para>
        /// Creates an entry in the database for a user profile.
        ///     </para><para>
        /// If a user has already been registered, their existing data will be pulled from the database, and the provider specific data will be overriden.
        ///     </para><para>
        /// Though, the case that a user is registering and 
        ///     </para>
        /// </summary>
        /// <param name="providerUser"></param>
        /// <param name="redis"></param>
        /// <param name="settings"></param>
        public User<GitHubProfile> RegisterUser(User<GitHubProfile> NewUserProfile, IDatabase Redis, AppSetting Settings) {

            string KEY_UserProfile = $"{Settings.Redis.BaseKey}Users:{NewUserProfile.Id}";

            // If this is the first time this user has been seen, add their profile to the database.
            if(!Redis.HashExists(KEY_UserProfile, HASH_ProfileData)) {
                Redis.HashSet(KEY_UserProfile, HASH_ProfileData, JsonConvert.SerializeObject(NewUserProfile));
                return NewUserProfile;
            } else {
                // User has been seen before, pull their existing user data
                var ExistingUserProfile = JsonConvert.DeserializeObject<User<GitHubProfile>>(Redis.HashGet(KEY_UserProfile, HASH_ProfileData));

                // If the data we received from the OAuth has changed to what we have, update and return
                if(ExistingUserProfile.ProviderProfileHasChanged(NewUserProfile)) {
                    ExistingUserProfile.ProviderProfile = NewUserProfile.ProviderProfile;
                    ExistingUserProfile.Details.LastUpdatedUTC = DateTime.UtcNow;
                    Redis.HashSet(KEY_UserProfile, HASH_ProfileData, JsonConvert.SerializeObject(ExistingUserProfile));
                }

                return ExistingUserProfile;
            }
        }
    }
}

using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NulahCore.Controllers.GitHub;
using NulahCore.Controllers.Users.Models;
using NulahCore.Extensions.Logging;
using NulahCore.Filters;
using NulahCore.Models;
using NulahCore.Models.User;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Users {
    public class UserProfile {

        private const string HASH_PublicData = "PublicData";
        private const string HASH_ProfileData = "Profile";
        private const string HASH_AccessToken = "GitHub_AccessToken";
        private const string KEY_UserProfile_Tokened = "{0}Users:{1}";

        /// <summary>
        /// First time registration from a retrieved GitHubProfile.
        /// Does nothing if the profile has already been added.
        /// Creates a hash with Profile data from GitHub, and PublicData which is their PublicUser object for views.
        /// </summary>
        /// <param name="GitHubProfile"></param>
        /// <returns></returns>
        internal static void Register(GitHubProfile GitHubProfile, IDatabase Redis, AppSetting Settings) {

            string KEY_UserProfile = $"{Settings.Redis.BaseKey}Users:{GitHubProfile.id}";

            if(!Redis.HashExists(KEY_UserProfile, "Profile")) {
                var redisUser = new User {
                    GitProfile = GitHubProfile.html_url,
                    Hireable = GitHubProfile.hireable,
                    ID = GitHubProfile.id,
                    DisplayName = GitHubProfile.login, // fallback if name below is null
                    PublicName = GitHubProfile.name,
                    url_repo = GitHubProfile.repos_url, // public only
                    url_gists = GitHubProfile.gists_url.Split('{')[0], // public only, drop the curly brace param
                    GitHubUserSince = GitHubProfile.created_at,
                    GitHubBio = GitHubProfile.bio,
                    GitHubUserApi = GitHubProfile.url, // use this to refresh stats
                    PublicRepoCount = GitHubProfile.public_repos,
                    PublicGistCount = GitHubProfile.public_gists,
                    Blog = GitHubProfile.blog,
                    Company = GitHubProfile.company,
                    EmailAddress = GitHubProfile.email,
                    Gravatar = GitHubProfile.avatar_url
                };

                Redis.HashSet(KEY_UserProfile, HASH_ProfileData, JsonConvert.SerializeObject(redisUser));
                Redis.HashSet(KEY_UserProfile, HASH_PublicData, JsonConvert.SerializeObject(UserProfile.CreatePublicUserProfile(redisUser)));
            }

            Redis.HashSet(KEY_UserProfile, HASH_AccessToken, GitHubProfile.access_token);
        }

        /// <summary>
        /// Updating existing data from a retrieved GitHubProfile.
        /// Creates a hash with Profile data from GitHub, and PublicData which is their PublicUser object for views.
        /// Does nothing if the user profile does not exist.
        /// </summary>
        /// <param name="GitHubProfile"></param>
        /// <returns></returns>
        internal static void Update(GitHubProfile GitHubProfile, IDatabase Redis, AppSetting Settings) {

            string KEY_UserProfile = $"{Settings.Redis.BaseKey}Users:{GitHubProfile.id}";

            if(Redis.HashExists(KEY_UserProfile, "Profile")) {
                var redisUser = new User {
                    GitProfile = GitHubProfile.html_url,
                    Hireable = GitHubProfile.hireable,
                    ID = GitHubProfile.id,
                    DisplayName = GitHubProfile.login, // fallback if name below is null
                    PublicName = GitHubProfile.name,
                    url_repo = GitHubProfile.repos_url, // public only
                    url_gists = GitHubProfile.gists_url.Split('{')[0], // public only, drop the curly brace param
                    GitHubUserSince = GitHubProfile.created_at,
                    GitHubBio = GitHubProfile.bio,
                    GitHubUserApi = GitHubProfile.url, // use this to refresh stats
                    PublicRepoCount = GitHubProfile.public_repos,
                    PublicGistCount = GitHubProfile.public_gists,
                    Blog = GitHubProfile.blog,
                    Company = GitHubProfile.company,
                    EmailAddress = GitHubProfile.email,
                    Gravatar = GitHubProfile.avatar_url
                };

                Redis.HashSet(KEY_UserProfile, HASH_ProfileData, JsonConvert.SerializeObject(redisUser));
                Redis.HashSet(KEY_UserProfile, HASH_PublicData, JsonConvert.SerializeObject(UserProfile.CreatePublicUserProfile(redisUser)));
            }
        }

        public static async Task RegisterUser(OAuthCreatingTicketContext context, IDatabase Redis, AppSetting Settings) {
            // Retrieve user info by passing an Authorization header with the value token {accesstoken};
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("token", context.AccessToken);

            // Extract the user info object
            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();
            var a = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<GitHubProfile>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });

            user.access_token = context.AccessToken;
            // Add the Name Identifier claim for htmlantiforgery
            context.Identity.AddClaims(
                new List<Claim> {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.id.ToString(),
                        ClaimValueTypes.String,
                        context.Options.ClaimsIssuer
                    ),
                    new Claim(
                        "RedisKey",
                        $"{Settings.Redis.BaseKey}Users:{user.id}",
                        ClaimValueTypes.String,
                        context.Options.ClaimsIssuer
                    )
                }
            );

            UserProfile.Register(user, Redis, Settings);
        }

        /// <summary>
        /// Creates a PublicUser for views
        /// </summary>
        /// <param name="ProfileData"></param>
        /// <returns></returns>
        internal static PublicUser CreatePublicUserProfile(User GitHubUserData) {
            PublicUser UserData = new PublicUser() {
                Blog = ( GitHubUserData.Blog == string.Empty ) ? null : GitHubUserData.Blog,
                Company = ( GitHubUserData.Company == string.Empty ) ? null : GitHubUserData.Company,
                DisplayName = GitHubUserData.PublicName == null ? GitHubUserData.DisplayName : GitHubUserData.PublicName,
                GitHubUrl = GitHubUserData.GitProfile,
                Hireable = GitHubUserData.Hireable,
                LastUpdated = DateTime.UtcNow,
                MemberSince = DateTime.UtcNow,
                UserId = GitHubUserData.ID
            };

            UserData.IsLoggedIn = true;
            List<Role> UserRoles = new List<Role> {
                Role.IsLoggedIn,
                Role.CanComment
            };

            //UserProfile.GetAdditionalRoles(UserId,Redis,Settings) // Pull further set roles such as Admin roles from another location

            UserData.Roles = UserRoles.ToArray();

            return UserData;
        }


        /// <summary>
        /// Refreshes the PublicData hash in redis by making a request to the users GitHub UserProfile Api
        /// </summary>
        /// <param name="GitHubProfileUri"></param>
        /// <returns></returns>
        internal static async Task RefreshPublicUserProfile(int GitHubProfileId, AppSetting Settings, IDatabase Redis) {

            string KEY_UserProfile = $"{Settings.Redis.BaseKey}Users:{GitHubProfileId}";

            var accessToken = Redis.HashGet(KEY_UserProfile, HASH_AccessToken);

            var UpdatedProfile = await GitHubApi.Get<GitHubProfile>("https://api.github.com/users/ZacMillionaire", accessToken);

            Update(UpdatedProfile, Redis, Settings);
        }

        internal static PublicUser GetUserById(string UserId, IDatabase Redis, AppSetting Settings) {
            var redisKey = string.Format(KEY_UserProfile_Tokened, Settings.Redis.BaseKey, UserId);
            if(Redis.HashExists(redisKey, HASH_PublicData)) {
                return JsonConvert.DeserializeObject<PublicUser>(Redis.HashGet(redisKey, HASH_PublicData));
            }
            return null;
        }

        /// <summary>
        /// Gets a users profile data based on their redis key from a user claims object.
        /// </summary>
        /// <param name="UserGitHubId"></param>
        /// <returns></returns>
        internal static PublicUser GetUser(string RedisKey, IDatabase Redis) {
            // What if ProfileData doesn't exist for some weird fucking reason
            if(Redis.HashExists(RedisKey, HASH_PublicData)) {
                return JsonConvert.DeserializeObject<PublicUser>(Redis.HashGet(RedisKey, HASH_PublicData));
            } else {
                // then we just return a default userprofile
                return new PublicUser();
            }
        }
    }

}

using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NulahCore.Controllers.GitHub;
using NulahCore.Controllers.Providers;
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
        private const string HASH_ProviderAccessToken = "AccessToken";

        /// <summary>
        ///     <para>
        /// Registers or logs in a user if their Id can be found.
        ///     </para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="Redis"></param>
        /// <param name="Settings"></param>
        /// <returns></returns>
        internal static async Task RegisterUser(OAuthCreatingTicketContext context, Startup.Provider LoginProvider /* change this later */, IDatabase Redis, AppSetting Settings) {
            // Retrieve user info by passing an Authorization header with the value token {accesstoken};
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue(LoginProvider.AuthorizationHeader, context.AccessToken);

            // Extract the user info object from the OAuth response
            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();
            var oauthres = await response.Content.ReadAsStringAsync();

            if(Settings.Provider == "GitHub") {

                var GitHubProvider = new GitHubLoginProvider(Redis, Settings);

                // fetch the OAuth profile from the provider
                var user = await GitHubProvider.GetOAuthProfile(response, context);
                // Create the user from it
                var providerUser = GitHubProvider.CreateUserFromOAuth(user);

                // Register the profile
                var RegisteredUser = GitHubProvider.RegisterUser(providerUser);

                // Add the Name Identifier claim for htmlantiforgery tokens, and the users redis key location.
                context.Identity.AddClaims(
                    new List<Claim> {
                        new Claim(
                            ClaimTypes.NameIdentifier,
                            RegisteredUser.UserId.ToString(),
                            ClaimValueTypes.String,
                            context.Options.ClaimsIssuer
                        ),
                        new Claim(
                            "RedisKey",
                            $"{Settings.Redis.BaseKey}Users:{RegisteredUser.UserId}",
                            ClaimValueTypes.String,
                            context.Options.ClaimsIssuer
                        )
                    }
                );
            } else if(Settings.Provider == "Discord") {
                throw new NotImplementedException("Discord not really worth it");
                var DiscordProvider = new DiscordLoginProvider(Redis, Settings);
                var a = await DiscordProvider.GetOAuthProfile(response, context);
            }
        }

        /// <summary>
        ///     <para>
        /// Almost sure this doesn't do anything
        ///     </para>
        /// </summary>
        internal static void Login() {
            throw new NotImplementedException("What?");
        }

        internal static void Logout(PublicUser LoggingOutUser, IDatabase Redis, AppSetting Settings) {

            if(LoggingOutUser == null) {
                throw new NullReferenceException($"Received null PublicUser data for logout.");
            }

            string KEY_UserProfile = $"{Settings.Redis.BaseKey}Users:{LoggingOutUser.UserId}";

            // confirm that the public data exists, then update the logged out
            if(Redis.HashExists(KEY_UserProfile, HASH_PublicData)) {
                LoggingOutUser.IsLoggedIn = false;
                LoggingOutUser.Roles = new Role[] { Role.IsLoggedOut }; // Clear the role list and set it to Role.IsLoggedOut
                Redis.HashSet(KEY_UserProfile, HASH_PublicData, JsonConvert.SerializeObject(LoggingOutUser));
                Redis.HashDelete(KEY_UserProfile, HASH_ProviderAccessToken); // remove the access token. Not a big deal if it remains, but eh
            } else {
                throw new NullReferenceException($"Missing Profile data for {LoggingOutUser.UserId}.");
            }
        }


        internal static PublicUser GetUserById(string UserId, IDatabase Redis, AppSetting Settings) {

            string KEY_UserProfile = $"{Settings.Redis.BaseKey}Users:{UserId}";

            if(Redis.HashExists(KEY_UserProfile, HASH_PublicData)) {
                return JsonConvert.DeserializeObject<PublicUser>(Redis.HashGet(KEY_UserProfile, HASH_PublicData));
            }
            return null;
        }

        /// <summary>
        ///     <para>
        /// Get's a users cached profile using their database key from their user claims object.
        ///     </para>
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

        /// <summary>
        /// Refreshes the PublicData hash in redis by making a request to the users GitHub UserProfile Api
        /// </summary>
        /// <param name="GitHubProfileUri"></param>
        /// <returns></returns>
        internal static async Task<PublicUser> RefreshPublicUserProfile(int GitHubProfileId, AppSetting Settings, IDatabase Redis) {

            var GitHubProvider = new GitHubLoginProvider(Redis, Settings);

            return await GitHubProvider.UpdateProviderDetails(GitHubProfileId);
        }

        /*
        /// <summary>
        /// Creates a PublicUser for views. Called on new user registration, user data update.
        /// </summary>
        /// <param name="ProfileData"></param>
        /// <returns></returns>
        internal static PublicUser CreatePublicUserProfile(User GitHubUserData, IDatabase Redis, AppSetting Settings) {

            string KEY_UserProfile = $"{Settings.Redis.BaseKey}Users:{GitHubUserData.ID}";

            PublicUser UserData;

            // if the user already has a public data profile, update everything but the member since. New user data otherwise
            if(Redis.HashExists(KEY_UserProfile, HASH_PublicData)) {
                UserData = JsonConvert.DeserializeObject<PublicUser>(Redis.HashGet(KEY_UserProfile, "Profile"));
                UserData.Blog = ( GitHubUserData.Blog == string.Empty ) ? null : GitHubUserData.Blog;
                UserData.Company = ( GitHubUserData.Company == string.Empty ) ? null : GitHubUserData.Company;
                UserData.DisplayName = GitHubUserData.PublicName ?? GitHubUserData.DisplayName;
                UserData.GitHubUrl = GitHubUserData.GitProfile;
                UserData.Hireable = GitHubUserData.Hireable;
                UserData.LastUpdated = DateTime.UtcNow;
            } else {
                UserData = new PublicUser() {
                    Blog = ( GitHubUserData.Blog == string.Empty ) ? null : GitHubUserData.Blog,
                    Company = ( GitHubUserData.Company == string.Empty ) ? null : GitHubUserData.Company,
                    DisplayName = GitHubUserData.PublicName ?? GitHubUserData.DisplayName,
                    GitHubUrl = GitHubUserData.GitProfile,
                    Hireable = GitHubUserData.Hireable,
                    LastUpdated = DateTime.UtcNow,
                    MemberSince = DateTime.UtcNow,
                    UserId = GitHubUserData.ID
                };
            }

            // Set the new/updated userdata to logged in, and create/refresh roles
            SetLoggedInStateAndRoles(UserData, Redis, Settings);

            return UserData;
        }

        /// <summary>
        ///     <para>
        /// Sets a user to logged in, and populates their roles
        ///     </para>
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Redis"></param>
        /// <param name="Settings"></param>
        /// <returns></returns>
        internal static PublicUser SetLoggedInStateAndRoles(PublicUser User, IDatabase Redis, AppSetting Settings) {
            User.IsLoggedIn = true;
            List<Role> UserRoles = new List<Role> {
                Role.IsLoggedIn,
                Role.CanComment
            };

            // Pull further set roles such as Admin roles from another location
            UserRoles.AddRange(GetAdditionalRoles(User, Redis, Settings));

            User.Roles = UserRoles.ToArray();
            return User;
        }

        /// <summary>
        ///     <para>
        /// Generates additional roles based on various criteria.
        ///     </para>
        /// </summary>
        /// <param name="UserData"></param>
        /// <param name="Redis"></param>
        /// <param name="Settings"></param>
        /// <returns></returns>
        private static List<Role> GetAdditionalRoles(PublicUser UserData, IDatabase Redis, AppSetting Settings) {
            List<Role> AdditionalRoles = new List<Role>();
            // add global administrator permission
            if(Settings.GlobalAdministrators.Contains(UserData.UserId)) {
                AdditionalRoles.Add(Role.GlobalAdministrator);
            }

            return AdditionalRoles;
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
        }*/
    }

}

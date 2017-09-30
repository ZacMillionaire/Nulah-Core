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
using NulahCore.Filters;
using NulahCore.Controllers.GitHub;

namespace NulahCore.Controllers.Providers.GitHub {
    public class GitHubLoginProvider : ILoginProvider<GitHubProfile, GitHubOAuthProfile> {

        private const string HASH_ProfileData = "Profile";
        private const string HASH_PublicData = "PublicData";
        private const string HASH_ProviderData = "ProviderData";
        private const string HASH_ProviderAccessToken = "AccessToken";
        private readonly string KEY_UserProfile;
        private readonly IDatabase _redis;
        private readonly AppSetting _settings;

        public GitHubLoginProvider(IDatabase Redis, AppSetting Settings) {
            _redis = Redis;
            _settings = Settings;
            KEY_UserProfile = $"{_settings.Redis.BaseKey}Users:{{0}}"; // set the base key, and a token for string.Formats later
        }

        /// <summary>
        ///     <para>
        /// Creates and returns a User, based on their given OAuth profile.
        ///     </para><para>
        /// 
        ///     </para>
        /// </summary>
        /// <param name="OAuthData"></param>
        /// <returns></returns>
        public User CreateUserFromOAuth(GitHubOAuthProfile OAuthData) {

            var redisUser = new User {
                Id = OAuthData.id, // their GitHub userId
                Details = new UserDetails {
                    DisplayName = OAuthData.name ?? OAuthData.login, // Use a display name if set on github, otherwise display their login name
                    RegisteredUTC = DateTime.UtcNow, // set registered and last updated to the same time
                    LastUpdatedUTC = DateTime.UtcNow,
                    TimezoneAdjustment = new TimeSpan(0, 0, 0) // set the timezone offset to 0 to indicate no change should be made when converting dates on the frontend
                },
                AccessToken = OAuthData.access_token
            };

            // set the provider profile data
            redisUser.SetProviderProfile<GitHubProfile>(new GitHubProfile {
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
            });

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
        ///     </para>
        /// </summary>
        /// <param name="providerUser"></param>
        /// <param name="redis"></param>
        /// <param name="settings"></param>
        public PublicUser RegisterUser(User NewUserProfile) {

            //string KEY_UserProfile = $"{_settings.Redis.BaseKey}Users:{NewUserProfile.Id}";

            string ProfileKey = string.Format(KEY_UserProfile, NewUserProfile.Id);

            // update the accesstoken
            _redis.HashSet(ProfileKey, HASH_ProviderAccessToken, NewUserProfile.AccessToken);

            // If this is the first time this user has been seen, add their profile to the database.
            if(!_redis.HashExists(ProfileKey, HASH_ProfileData)) {

                _redis.HashSet(ProfileKey, HASH_ProfileData, JsonConvert.SerializeObject(NewUserProfile));
                CreateProviderData(NewUserProfile.Id, NewUserProfile.ProviderProfile);

                return CreatePublicUser(NewUserProfile);
            } else {
                // User has been seen before, pull their existing user data
                var ExistingUserProfile = JsonConvert.DeserializeObject<User>(_redis.HashGet(ProfileKey, HASH_ProfileData));

                // If the data we received from the OAuth has changed to what we have, update and return
                if(ExistingUserProfile.ProviderProfileHasChanged(NewUserProfile)) {
                    ExistingUserProfile.SetProviderProfile<GitHubProfile>(NewUserProfile.ProviderProfile);
                    ExistingUserProfile.Details.LastUpdatedUTC = DateTime.UtcNow;
                    _redis.HashSet(ProfileKey, HASH_ProfileData, JsonConvert.SerializeObject(ExistingUserProfile));
                    CreateProviderData(NewUserProfile.Id, NewUserProfile.ProviderProfile);
                }

                return CreatePublicUser(ExistingUserProfile);
            }
        }

        /// <summary>
        ///     <para>
        /// Replaces provider data in the database with new data.
        ///     </para>
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="ProviderData"></param>
        private void CreateProviderData(int UserId, string ProviderData) {
            string ProfileKey = string.Format(KEY_UserProfile, UserId);

            _redis.HashSet(ProfileKey, HASH_ProviderData, ProviderData);
        }

        /// <summary>
        ///     <para>
        /// Refreshes a users provider details
        ///     </para>
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<PublicUser> UpdateProviderDetails(int UserId) {
            string ProfileKey = string.Format(KEY_UserProfile, UserId);

            var accessToken = _redis.HashGet(ProfileKey, HASH_ProviderAccessToken);
            var User = JsonConvert.DeserializeObject<User>(_redis.HashGet(ProfileKey, HASH_ProfileData));

            var UpdatedProfile = await GitHubApi.Get<GitHubOAuthProfile>("https://api.github.com/users/ZacMillionaire", accessToken);

            User.SetProviderProfile<GitHubProfile>(CreateUserFromOAuth(UpdatedProfile).ProviderProfile);

            // cheat and use the register user method. I know it's dumb, but until I can be bothered to change the name
            // to something better, this is what its called
            return RegisterUser(User);
        }

        public PublicUser GetPublicUser() {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     <para>
        /// Creates a PublicUser from a user profile details, and adds it to the database.
        ///     </para>
        /// </summary>
        /// <param name="UserProfile"></param>
        /// <returns></returns>
        public PublicUser CreatePublicUser(User UserProfile) {
            string ProfileKey = string.Format(KEY_UserProfile, UserProfile.Id);

            PublicUser publicUser = new PublicUser(UserProfile);

            List<Role> UserRoles = new List<Role> {
                Role.IsLoggedIn,
                Role.CanComment
            };

            // Pull further set roles such as Admin roles from another location
            UserRoles.AddRange(GetAdditionalRoles(UserProfile));

            publicUser.Roles = UserRoles.ToArray();

            _redis.HashSet(ProfileKey, HASH_PublicData, JsonConvert.SerializeObject(publicUser));

            return publicUser;
        }

        /// <summary>
        ///     <para>
        /// Gets any additional roles from other locations such as appsettings, or based on certain provider details.
        ///     </para>
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        private List<Role> GetAdditionalRoles(User UserData) {

            List<Role> AdditionalRoles = new List<Role>();

            /*
            // Provider specific role example
            var providerData = UserData.GetProviderProfile<GitHubProfile>();

            if(providerData.SomePropertyYouWantToCheck != WhateverValue) {
                AdditionalRoles.Add(Role.SomeProviderSpecificRole);
            }
            */

            // add global administrator permission, based on Provider data Id
            if(_settings.GlobalAdministrators.Contains(UserData.Id)) {
                AdditionalRoles.Add(Role.GlobalAdministrator);
            }

            return AdditionalRoles;
        }

        public Role[] GetUserRoles(int UserId) {
            throw new NotImplementedException();
        }
    }
}

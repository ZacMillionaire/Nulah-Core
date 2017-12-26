using NulahCore.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using NulahCore.Controllers.Users.Models;
using NulahCore.Filters;
using NulahCore.Models.User;
using System.Net.Http;
using NulahCore.Controllers.Providers.Discord;
using Newtonsoft.Json;

namespace NulahCore.Controllers.Providers {
    public class DiscordLoginProvider : ILoginProvider<DiscordProfile, DiscordOAuthProfile> {

        private const string HASH_ProfileData = "Profile";
        private const string HASH_PublicData = "PublicData";
        private const string HASH_ProviderData = "ProviderData";
        private const string HASH_ProviderAccessToken = "AccessToken";
        private readonly string KEY_UserProfile;
        private readonly IDatabase _redis;
        private readonly AppSetting _settings;

        public DiscordLoginProvider(IDatabase Redis, AppSetting Settings) {
            _redis = Redis;
            _settings = Settings;
            KEY_UserProfile = $"{_settings.Redis.BaseKey}Users:{{0}}"; // set the base key, and a token for string.Formats later
        }

        public PublicUser CreatePublicUser(User UserProfile) {
            throw new NotImplementedException();
        }

        public User CreateUserFromOAuth(DiscordOAuthProfile OAuthData) {
            throw new NotImplementedException();
        }

        public async Task<DiscordOAuthProfile> GetOAuthProfile(HttpResponseMessage response, OAuthCreatingTicketContext context) {
            var user = JsonConvert.DeserializeObject<DiscordOAuthProfile>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });

            user.access_token = context.AccessToken;
            // store the rest of the token stuff such as refresh token, and expires or whatever
            // idk if I really want to bother with it currently

            return user;
        }

        public PublicUser GetPublicUser() {
            throw new NotImplementedException();
        }

        public Role[] GetUserRoles(int UserId) {
            throw new NotImplementedException();
        }

        public PublicUser RegisterUser(User NewUserProfile) {
            throw new NotImplementedException();
        }

        public Task<PublicUser> UpdateProviderDetails(int UserId) {
            throw new NotImplementedException();
        }
    }
}

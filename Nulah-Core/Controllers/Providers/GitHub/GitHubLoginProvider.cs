using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using NulahCore.Models.User;
using NulahCore.Models;
using NulahCore.Controllers.Users.Models;

namespace NulahCore.Controllers.Providers.GitHub {
    public class GitHubLoginProvider : ILoginProvider<GitHubProfile, OAuthProfile> {

        private const string HASH_PublicData = "PublicData";
        private const string HASH_ProfileData = "Profile";
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

        public User<GitHubProfile> CreatePublicUser(OAuthProfile OAuthData, IDatabase Redis, AppSetting Settings) {

            var redisUser = new User<GitHubProfile> {
                Id = OAuthData.id, // their GitHub userId
                DisplayName = OAuthData.name ?? OAuthData.login, // Use a display name if set on github, otherwise display their login name
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
    }
}

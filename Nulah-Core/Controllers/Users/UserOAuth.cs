using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Newtonsoft.Json;
using Nulah.Users;
using Nulah.Users.Models;
using NulahCore.Controllers.Users.Models;
using NulahCore.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Users {
    public class UserOAuth {

        internal static async Task RegisterUser(OAuthCreatingTicketContext context, Provider LoginProvider /* change this later */, IDatabase Redis, AppSetting Settings) {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue(LoginProvider.AuthorizationHeader, context.AccessToken);

            // Extract the user info object from the OAuth response
            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();
            var oauthres = await response.Content.ReadAsStringAsync();
            /**/
            // I hope this should be generic enough for most OAuth /me responses.
            // I'm almost sure they'll always have a name field, and I'm 100% sure they'll always have an id field.
            var identity = JsonConvert.DeserializeObject<User>(oauthres, new JsonSerializerSettings {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });
            identity.ProviderShort = LoginProvider.ProviderShort;
            identity.Provider = LoginProvider.AuthenticationScheme;
            if(identity.Name == null) {
                identity.Name = identity.Login;
            }

            CreateUser(identity, context, Redis, Settings);
        }

        // If we're here it's probably because of Reddit's OAuth being broken/garbage, or ASP .Net Core 2.0 having a fucking stupid auth library.
        // Throw a dart at a board and thats your answer.
        // If you were using GitHub or Facebook, you'll probably have got a useful error of some sort in the url.
        internal static Task OAuthRemoteFailure(RemoteFailureContext context, Provider loginProvider, IDatabase redis, AppSetting applicationSettings) {
            //throw new Exception("Fuck it");
            return Task.FromException(new Exception($"OAuth remote failure, {context.Failure.Message}{context.Failure.StackTrace}"));
        }

        private static void CreateUser(User LoggingInUser, OAuthCreatingTicketContext OAuthContext, IDatabase Redis, AppSetting Settings) {
            var userController = new UserManager(Redis, Settings.Redis.BaseKey);
            var userKey = userController.GetUserTableKey(LoggingInUser);

            OAuthContext.Identity.AddClaims(
                new List<Claim> {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        $"{LoggingInUser.ProviderShort}-{LoggingInUser.Id}",
                        ClaimValueTypes.String,
                        OAuthContext.Options.ClaimsIssuer
                    ),
                    new Claim(
                        ClaimTypes.GivenName,
                        LoggingInUser.Name,
                        ClaimValueTypes.String,
                        OAuthContext.Options.ClaimsIssuer
                    ),
                    new Claim(
                        "RedisKey",
                        userKey,
                        ClaimValueTypes.String,
                        OAuthContext.Options.ClaimsIssuer
                    )
                }
            );

            userController.GetOrCreateUser(LoggingInUser);
        }

    }
}

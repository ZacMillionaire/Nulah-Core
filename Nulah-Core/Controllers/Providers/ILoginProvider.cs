using Microsoft.AspNetCore.Authentication.OAuth;
using NulahCore.Controllers.Users.Models;
using NulahCore.Filters;
using NulahCore.Models;
using NulahCore.Models.User;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Providers {
    public interface ILoginProvider<T, U> where U : IOAuthProvider {
        PublicUser CreatePublicUser(User UserProfile);
        User CreateUserFromOAuth(U OAuthData);
        Task<U> GetOAuthProfile(HttpResponseMessage response, OAuthCreatingTicketContext context);
        PublicUser GetPublicUser();
        Role[] GetUserRoles(int UserId);
        PublicUser RegisterUser(User NewUserProfile);
        Task<PublicUser> UpdateProviderDetails(int UserId);
    }
}

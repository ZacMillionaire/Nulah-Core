using NulahCore.Controllers.Users.Models;
using NulahCore.Models;
using NulahCore.Models.User;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Providers {
    public interface ILoginProvider<T, U> where U : IOAuthProvider {

        User<T> Register(U PublicProfile, IDatabase Redis, AppSetting Settings);

        User<T> CreatePublicUser(U OAuthData, IDatabase Redis, AppSetting Settings);

        /// <summary>
        /// Creates a PublicUser Profile from an OAuth Result
        /// </summary>
        /// <param name="OAuthUserData">Deserialized data from a successful OAuth request</param>
        /// <param name="Redis"></param>
        /// <param name="Settings"></param>
        /// <returns></returns>
        PublicUser<T> CreatePublicUserProfile(U OAuthUserData, IDatabase Redis, AppSetting Settings);

        /// <summary>
        /// [Async]
        ///     <para>
        /// Refreshes a users profile data using their OAuth access token from Redis, where RedisUserKeyId is from
        /// PublicUser<T>.UserId
        ///     </para><para>
        /// The value of UserId depends on the most appropriate field returned from an OAuth request when logging in,
        /// and is provider defined.
        ///     </para>
        /// </summary>
        /// <param name="RedisUserKeyId"></param>
        /// <param name="Settings"></param>
        /// <param name="Redis"></param>
        /// <returns></returns>
        Task<PublicUser<T>> RefreshPublicUserProfile(string RedisUserKeyId, AppSetting Settings, IDatabase Redis);

        /// <summary>
        /// Refreshes a users profile from an OAuth endpoint, where ProviderProfile is from RefreshPublicUserProfile
        /// </summary>
        /// <param name="ProviderProfile"></param>
        /// <param name="Redis"></param>
        /// <param name="Settings"></param>
        /// <returns></returns>
        //PublicUser<T> Update(T ProviderProfile, IDatabase Redis, AppSetting Settings);
    }
}

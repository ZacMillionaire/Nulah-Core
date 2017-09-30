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

    }
}

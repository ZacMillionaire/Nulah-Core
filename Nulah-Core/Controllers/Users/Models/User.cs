using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Users.Models {
    public class User<T> {
        public int Id { get; set; }
        /// <summary>
        /// Login Name, will be displayed by default
        /// </summary>
        public string DisplayName { get; set; }

        public T ProviderProfile { get; set; }
    }
}

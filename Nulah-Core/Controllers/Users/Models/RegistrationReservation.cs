using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Users.Models {

    public class RegistrationReservation //...AppreciationStation
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool EmailExists { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool ValidToken { get; set; }
    }
}

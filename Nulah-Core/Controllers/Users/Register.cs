using Newtonsoft.Json;
using NulahCore.Controllers.Users.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Users {
    public class Register {
        private const string KEY_User_Pending = "Users:Pending:";

        private readonly IDatabase _redis;

        public Register(IDatabase Redis) {
            _redis = Redis;
        }

        public RegistrationReservation PreRegisterEmailAddress(string EmailAddress) {

            string EmailHash = HashEmail(EmailAddress);

            if(!EmailAddressInUse(EmailHash)) {
                return AddRegistration(EmailHash, EmailAddress);
            } else {
                return new RegistrationReservation {
                    EmailExists = true
                };
            }
        }

        private string HashEmail(string EmailAddress) {
            using(SHA256 hash = SHA256.Create()) {
                return String.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(EmailAddress))
                  .Take(15)
                  .Select(item => item.ToString("x2")));
            }
        }

        private bool EmailAddressInUse(string EmailHash) {
            string redisKey = KEY_User_Pending + EmailHash;

            // Check to see if a hash exists from the hash of the email given
            var pendingConfirmations = _redis.HashExists(redisKey, "Reservation"); //RedisStore.Deserialise<RegistrationReservation>(_redis.ListRange(KEY_User_Pending, 0, -1));
            if(pendingConfirmations) {
                return true;
            }
            return false;
        }

        private RegistrationReservation AddRegistration(string EmailHash, string EmailAddress) {

            string redisKey = KEY_User_Pending + EmailHash;

            RegistrationReservation reservation = new RegistrationReservation {
                Email = EmailAddress,
                Expires = DateTime.UtcNow.AddDays(1),
                Token = Guid.NewGuid().ToString(),
                EmailExists = false
            };

            _redis.HashSet(redisKey, "Reservation", JsonConvert.SerializeObject(reservation));
            _redis.KeyExpire(redisKey, reservation.Expires);

            return reservation;
        }

        //private RegistrationReservation RefreshRegistration(string EmailAddress) {

        //}
    }
}

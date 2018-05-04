using System;
using System.Collections.Generic;
using System.Text;

namespace Nulah.Users.Models {
    public class UserDetails {
        /// <summary>
        ///     <para>
        /// Name that will be displayed as the users name.
        ///     </para>
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     <para>
        /// Date the user registered in UTC
        ///     </para>
        /// </summary>
        public DateTime RegisteredUTC { get; set; }

        /// <summary>
        ///     <para>
        /// Date the users profile was last updated.
        ///     </para>
        /// </summary>
        public DateTime LastUpdatedUTC { get; set; }

        /// <summary>
        ///     <para>
        /// Used for adjusting front end dates, based on a users selection of timezone.
        ///     </para><para>
        /// Defaults to [00:00:00], as all dates are UTC by default, so no adjustment will be made.
        ///     </para><para>
        /// TimeSpan from TimeZoneInfo.FindSystemTimeZoneById(TimezoneId).BaseUtcOffset, where TimezoneId is from TimeZoneInfo.GetSystemTimeZones()[n].Id
        ///     </para>
        /// </summary>
        public TimeSpan TimezoneAdjustment { get; set; }
    }
}

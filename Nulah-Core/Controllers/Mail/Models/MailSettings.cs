using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Mail.Models {
    public class MailSettings {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string HtmlTemplate { get; set; }
        public string TextTemplate { get; set; }
        public Dictionary<string, string> Replacements { get; set; }
    }
}

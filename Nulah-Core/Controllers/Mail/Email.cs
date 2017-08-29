using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using NulahCore.Models;
using StackExchange.Redis;
using Newtonsoft.Json;
using NulahCore.Controllers.Mail.Models;

namespace NulahCore.Controllers {
    public class Email {

        private readonly MailSettings _details;
        private readonly string _apiKey;
        /*
        public Mail(string ToEmail, string TemplateString) {
            _toEmail = ToEmail;
            _textTemplate = TemplateString;
        }
        */
        public Email(MailSettings Details, AppSetting Settings) {
            _details = Details;
            //_apiKey = Settings.Api_Mailgun;
        }

        public async void Send(IDatabase Redis, AppSetting Settings) {

            string baseUrl = "https://api.mailgun.net/v3/mail.moar.ws/messages";
            string TextBody = RenderMailBody(_details.TextTemplate, _details.Replacements);
            string HtmlBody = RenderMailBody(_details.HtmlTemplate, _details.Replacements);

            var client = baseUrl
                .SetQueryParams(new {
                    from = _details.From,
                    to = _details.To,
                    subject = _details.Subject,
                    text = TextBody,
                    html = HtmlBody
                })
                .WithBasicAuth("api", _apiKey);

            Redis.ListLeftPush(Settings.Redis.BaseKey + "Mail", JsonConvert.SerializeObject(new {
                from = _details.From,
                to = _details.To,
                subject = _details.Subject,
                text = TextBody,
                html = HtmlBody
            }));

            var res = await client.PostAsync(new StringContent(string.Empty));
        }

        private string RenderMailBody(string Template, Dictionary<string, string> Replacements) {
            var rendered = Template;
            foreach(var replacement in Replacements) {
                rendered = rendered.Replace("{{" + replacement.Key + "}}", replacement.Value);
            }
            return rendered;
        }
    }
}

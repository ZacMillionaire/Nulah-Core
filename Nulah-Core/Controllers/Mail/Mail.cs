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

namespace NulahCore.Controllers {
    public class Mail {

        private readonly string _toEmail;
        private readonly string _textTemplate;
        private readonly string _htmlTemplate;
        private readonly Dictionary<string, string> _replacements;
        private readonly string _apiKey;
        /*
        public Mail(string ToEmail, string TemplateString) {
            _toEmail = ToEmail;
            _textTemplate = TemplateString;
        }
        */
        public Mail(string ToEmail, string TextTemplateString, string HtmlTemplateString, Dictionary<string, string> TemplateReplacements, AppSetting Settings) {
            _toEmail = ToEmail;
            _textTemplate = TextTemplateString;
            _htmlTemplate = HtmlTemplateString;
            _replacements = TemplateReplacements;
            _apiKey = Settings.Api_Mailgun;
        }

        public async void SendMail(IDatabase Redis, AppSetting Settings) {

            string baseUrl = "https://api.mailgun.net/v3/mail.moar.ws/messages";
            string TextBody = RenderMailBody(_textTemplate, _replacements);
            string HtmlBody = RenderMailBody(_htmlTemplate, _replacements);

            var client = baseUrl
                .SetQueryParams(new {
                    from = "User Registration <noreply@moar.ws>",
                    to = _toEmail,
                    subject = "New Registration Token",
                    text = TextBody,
                    html = HtmlBody
                })
                .WithBasicAuth("api", _apiKey);

            Redis.ListLeftPush(Settings.Redis.BaseKey + "Mail", JsonConvert.SerializeObject(new {
                from = "User Registration <noreply@moar.ws>",
                to = _toEmail,
                subject = "New Registration Token",
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

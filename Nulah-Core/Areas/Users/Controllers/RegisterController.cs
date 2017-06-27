using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Controllers;
using System.ComponentModel.DataAnnotations;
using StackExchange.Redis;
using NulahCore.Controllers.Users;
using Microsoft.Extensions.FileProviders;
using NulahCore.Models;
using NulahCore.Controllers.Mail.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Areas.Users.Controllers {

    public class RegisterForm {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
    }

    public class ConfirmRegistrationForm {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Token { get; set; }
    }

    [Area("Users")]
    public class RegisterController : Controller {
        private readonly IDatabase _redis;
        private readonly AppSetting _settings;

        public RegisterController(IDatabase Redis, AppSetting Settings) {
            _redis = Redis;
            _settings = Settings;
        }

        [HttpGet]
        [Route("~/Register")]
        public IActionResult Index() {
            return View();
        }

        [HttpPost]
        [Route("~/Register")]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterPost([FromForm]RegisterForm FormData) {
            var preRegistration = new Register(_redis, _settings).PreRegisterEmailAddress(FormData.EmailAddress);

            // For loading email templates from an embedded resource.
            // File must be set to an embedded resource for this to work, won't be editable during runtime.
            //var tpl = ResourceLoader.LoadEmbeddedResource(typeof(RegisterController), "NewRegistration.html");
            var emailTemplatesHtml = ResourceLoader.LoadContentResource("wwwroot/Templates/NewRegistration.html", _settings);
            var emailTemplatesText = ResourceLoader.LoadContentResource("wwwroot/Templates/NewRegistration.txt", _settings);

            if(preRegistration.EmailExists) {
                return View("Registration_Pending");
            } else {
                Dictionary<string, string> emailValues = new Dictionary<string, string> {
                    {"EmailAddress",preRegistration.Email },
                    {"Expires",preRegistration.Expires.ToString("R") },
                    {"Token",preRegistration.Token }
                };

                MailSettings emailSettings = new MailSettings {
                    From = "User Registration <noreply@moar.ws>",
                    To = preRegistration.Email,
                    HtmlTemplate = emailTemplatesHtml,
                    TextTemplate = emailTemplatesText,
                    Replacements = emailValues,
                    Subject = "New Registration Token"
                };
                new Email(emailSettings, _settings).Send(_redis, _settings);

                return View("Registration_New");
            }
        }

        [HttpGet]
        [Route("~/Register/Confirm")]
        public IActionResult ConfirmRegistration() {
            return View();
        }

        [HttpPost]
        [Route("~/Register/Confirm")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmRegistrationPost([FromForm]ConfirmRegistrationForm FormData) {
            var preRegistration = new Register(_redis, _settings).ConfirmEmailAddress(FormData);
            return null;
        }
    }
}

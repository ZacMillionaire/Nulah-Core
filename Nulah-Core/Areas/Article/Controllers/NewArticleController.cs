using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Filters;
using NulahCore.Models.User;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Areas.Blog.Controllers {
    [Area("Article")]
    public class NewArticleController : Controller {
        [HttpGet]
        [Route("~/Article/New")]
        [UserFilter(new[] { Role.IsLoggedIn })]
        public IActionResult NewArticle() {
            return View();
        }
    }
}

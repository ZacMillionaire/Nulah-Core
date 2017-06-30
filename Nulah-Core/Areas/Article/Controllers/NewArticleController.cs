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
        [UserFilter(new[] { Role.IsLoggedIn, Role.CanAuthor },"~/Article/Error")]
        public IActionResult NewArticle() {
            return View();
        }
    }
}

// articles can be about a single repo "Repo posts"
// or about a recent commit (or series of commits in 1 post)
// get this data from the github apis
// repos
// https://api.github.com/users/ZacMillionaire/repos
// https://api.github.com/repos/ZacMillionaire/acervuline
// commit history
// https://api.github.com/repos/ZacMillionaire/acervuline/commits
// only want the sha, maybe (probably) message, date posted and maybe author?
// maybe have a way so you can comment on commits you've made to other repo's that have been PR'd

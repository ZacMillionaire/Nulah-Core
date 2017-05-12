using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NulahCore.Filters;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Areas.Users.Controllers
{
    [Area("Users")]
    public class LoginController : Controller
    {
        [HttpGet]
        [Route("~/Login")]
        [UserFilter(Role.IsLoggedOut)]
        public IActionResult Index()
        {
            return View();
        }
    }
}

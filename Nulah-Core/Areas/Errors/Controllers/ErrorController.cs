using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Areas.Errors.Controllers {
    [Area("Errors")]
    public class ErrorController : Controller {
        [Route("~/Error/404")]
        public IActionResult ErrorNotFound() {
            return View();
        }

        [Route("~/Error/{StatusCode}")]
        public IActionResult ErrorUnhandled(int StatusCode) {
            HttpContext.Response.StatusCode = StatusCode;
            return View();
        }
    }
}

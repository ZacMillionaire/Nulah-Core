using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http;
using NulahCore.Controllers.Images;
using NulahCore.Models;
using NulahCore.Controllers.Images.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NulahCore.Areas.Api.Controllers
{
    [Area("Api")]
    public class ImageUploadController : Controller
    {
        private readonly IDatabase _redis;
        private readonly AppSetting _appSettings;

        public ImageUploadController(IDatabase redis, AppSetting settings)
        {
            _redis = redis;
            _appSettings = settings;
        }

        [HttpGet]
        [Route("/Api/Image/Upload")]
        public UploadedFile Upload(IFormFile Image)
        {
            return new ImageHandler().UploadImage(Image, _appSettings.ContentRoot + "\\content\\images", _redis);
            // return StatusApi.GetRedisStatus(_redis);
        }
    }
}

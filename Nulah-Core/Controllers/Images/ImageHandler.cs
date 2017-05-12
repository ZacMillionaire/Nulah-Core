using Microsoft.AspNetCore.Http;
using NulahCore.Controllers.Images.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NulahCore.Controllers.Images
{
    public class ImageHandler
    {
        public UploadedFile UploadImage(IFormFile Image, string UploadDirectory, IDatabase Redis)
        {
            string UploadedFileName = Upload(Image, UploadDirectory);
            return new UploadedFile
            {
                FileName = Image.FileName,
                Source = "/content/images/" + Image.FileName
            };
        }

        private string Upload(IFormFile Image, string UploadDirectory)
        {
            var guid = Guid.NewGuid();
            // Messy way to leave only alphanumeric + underscores and dashes, then replace any spaces with an underscore
            var newFileName = guid.ToString().Split('-')[0] + "-" + Regex.Replace(Image.FileName, @"[^a-zA-Z0-9 _\-]", "").Replace(" ", "_");

            if (Image.Length > 0)
            {
                using (var stream = new FileStream(UploadDirectory + "\\" + newFileName, FileMode.Create))
                {
                    Image.CopyTo(stream);
                }

                return UploadDirectory + "\\" + newFileName;
            }
            else
            {
                throw new Exception("File length 0");
            }
        }
    }
}

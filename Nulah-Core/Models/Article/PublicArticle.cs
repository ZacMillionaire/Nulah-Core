using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Models.Article {
    public class PublicArticle {
        [DataType(DataType.Text)]
        [MaxLength(300, ErrorMessage = "Max length of 300")]
        [MinLength(25)]
        [Required]
        public string Title { get; set; }

        [DataType(DataType.Text)]
        [Required]
        public string Body { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Published { get; set; }

        public DateTime Created { get; set; }
    }
}

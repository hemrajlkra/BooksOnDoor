using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksOnDoor.Models.Models
{
    public class MailData
    {
        [Required]
        public string EmailToId { get; set; }
        public string OrgEmailId = "honeylakhara91111@gmail.com";
        [Required]
        public string EmailToName { get; set; }
        [Required]
        public string EmailSubject { get; set; }
        [Required]
        public string EmailBody { get; set; }
        public string? ReCaptchaToken { get; set; }

    }
}

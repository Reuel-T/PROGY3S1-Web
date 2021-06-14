using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_WEB_CLIENT.ViewModels
{
    public class NewUserVM
    {
        [Required]
        [MinLength(3,ErrorMessage ="Minimum length is 3")]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordConfirm { get; set; }
    }
}

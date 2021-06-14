using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_WEB_CLIENT.ViewModels
{
    public class LoginVM
    {
        [Required]
        [MinLength(1, ErrorMessage = "Please enter a username")]
        public string Username { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Please enter a password")]
        public string Password { get; set; }
    }
}

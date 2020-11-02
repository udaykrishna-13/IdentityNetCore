using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityNetCore.Models
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage ="Invalid or missing email.")]
        [DataType(DataType.EmailAddress,ErrorMessage ="Invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Invalid or Missing Password")]
        [DataType(DataType.Password,ErrorMessage ="Invalid Password")]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string Department { get; set; }
    }
}

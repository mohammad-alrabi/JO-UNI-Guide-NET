using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public class LoginModel
    {
        public class LoginViewModel
        {
            [Required(ErrorMessage = "Please enter your email")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Please enter your password")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
    }
}

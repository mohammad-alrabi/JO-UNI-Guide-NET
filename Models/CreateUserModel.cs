using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public class CreateUserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } 

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role {  get; set; } //عشان تختار رح يكون ادمن ولا طالب 
    }
}

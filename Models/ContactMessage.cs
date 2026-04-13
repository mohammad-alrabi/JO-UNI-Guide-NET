using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } =string.Empty;
        [Required]
        public string Subject { get; set; } = string.Empty;
        [Required]
        public string Message {  get; set; } = string.Empty;

        [ValidateNever]
        public DateTime SentDate { get; set; } = DateTime.Now;



    }
}

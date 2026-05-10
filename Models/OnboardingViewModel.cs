using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public class OnboardingViewModel
    {
        [Required]
        public CertificateType CertificateType { get; set; }

        [Required]
        public double? OriginalGrade { get; set; }

        [Required(ErrorMessage = "Please select your preferred university type")]
        public string PreferredUniType { get; set; }
    } 
}
    

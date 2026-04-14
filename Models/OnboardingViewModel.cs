using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public class OnboardingViewModel
    {
        [Required(ErrorMessage = "Please enter your GPA")]
        [Range(50, 100, ErrorMessage = "GPA must be between 50 and 100")]
        public double GPA { get; set; }

        [Required(ErrorMessage = "Please select your Tawjihi track")]
        public string TawjihiTrack { get; set; }

        [Required(ErrorMessage = "Please select your preferred university type")]
        public string PreferredUniType { get; set; }
    } 
}
    

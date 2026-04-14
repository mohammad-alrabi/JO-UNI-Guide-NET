using Microsoft.AspNetCore.Identity;

namespace JO_UNI_Guide.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? Governorate { get; set; }
        public double? GPA { get; set; }
        public string? TawjihiTrack { get; set; }
        public string? PreferredUniType { get; set; }
        public bool IsOnboarded { get; set; } = false;
    }
}

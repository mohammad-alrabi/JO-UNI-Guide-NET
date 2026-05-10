using Microsoft.AspNetCore.Identity;

namespace JO_UNI_Guide.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? Governorate { get; set; }

        public CertificateType CertificateType { get; set; } = CertificateType.Tawjihi;

        // الدرجة الأصلية كما أدخلها الطالب
        public double? OriginalGrade { get; set; }

        // المعادل المحسوب تلقائياً (0-100) — هاد اللي رح يُستخدم بكل المقارنات
        public double? EquivalentGrade { get; set; }

        public string? PreferredUniType { get; set; }
        public bool IsOnboarded { get; set; } = false;
    }
}

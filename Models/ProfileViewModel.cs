namespace JO_UNI_Guide.Models
{
    public class ProfileViewModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Governorate { get; set; }

        // معلومات أكاديمية
       
        public CertificateType CertificateType { get; set; }

        public double? OriginalGrade { get; set; }

        public double? EquivalentGrade { get; set; }
        public string? PreferredUniType { get; set; }

        // إحصائيات
        public int FavoritesCount { get; set; }

    }
}

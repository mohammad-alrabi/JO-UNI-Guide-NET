using JO_UNI_Guide.interfaces;
using JO_UNI_Guide.Models;

namespace JO_UNI_Guide.Service
{
    public class GradeConverterFactory
    {
        public static IGradeConverter GetConverter(CertificateType type)
        {
            return type switch
            {
                CertificateType.Tawjihi => new TawjihiConverter(),
                CertificateType.IB => new IBConverter(),
                CertificateType.IGCSE => new IGCSEConverter(),
                CertificateType.AmericanHighSchool => new AmericanConverter(),
                CertificateType.Saudi => new SaudiConverter(),
                _ => new TawjihiConverter()
            };
        }
    }
}
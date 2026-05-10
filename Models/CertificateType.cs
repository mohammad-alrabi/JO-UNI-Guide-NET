using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public enum CertificateType
    {
        [Display(Name = "Jordanian Tawjihi")]
        Tawjihi,

        [Display(Name = "Saudi")]
        Saudi,

        [Display(Name = "American Diploma")]
        AmericanDiploma,

        [Display(Name = "IGCSE")]
        IGCSE,

        [Display(Name = "IB")]
        IB,

        [Display(Name = "AmericanHighSchool")]
        AmericanHighSchool,


        [Display(Name = "Other")]
        Other
    }
}

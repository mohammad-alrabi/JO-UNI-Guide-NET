using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public enum UniversityType 
    {
        Governmental, // حكومية
        Private       // خاصة
    }
    public class University
    {
        [Key]
        public int University_ID { get; set; }

        [Required (ErrorMessage ="اسم الجامعة مطلوب")]
        public string? Name { get; set; }
        public string? Logo { get; set; }
        public string? Location { get; set; }
        public string? Rank_QS { get; set; }
        public string? Description { get; set; }
        public string? WebsiteUrl { get; set; } 
        public string? Email { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? ContactPhone { get; set; }
        [Required]
        [Display(Name = "University Type")]
        public UniversityType Type { get; set; }

        //Relation => الجامعة الوحدة بتحتوي على عدة كليات one-to-many
        public virtual ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();



    }
}

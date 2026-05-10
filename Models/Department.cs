using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO_UNI_Guide.Models
{
    public enum TawjihiTrack
    {
        [Display(Name = "صحي")]
        Health,

        [Display(Name = "هندسي")]
        Engineering,

        [Display(Name = "علوم وتكنولوجيا")]
        ScienceAndTechnology,

        [Display(Name = "لغات وعلوم اجتماعية")]
        Humanities,

        [Display(Name = "أعمال")]
        Business,

        [Display(Name = "قانون وشرعي")]
        LawAndSharia
    }
    public class Department
    {
        [Key]
        public int Department_ID { get; set; }
        [Required]
        public string? DepartmentName { get; set; }
        public string? Details { get; set; }

        // Relation with the faculty
        [ForeignKey("Faculty")]
        public int? Faculty_ID { get; set; }
        [ValidateNever]
        public virtual Faculty? Faculty { get; set; }
 
        [Display(Name = "Price Per Hour (JOD)")]
        public decimal HourPrice { get; set; }
        [Display(Name = "Total Credit Hours")]
        public int TotalCreditHours { get; set; }
        public double MinEquivalentGrade { get; set; }

        //Realtion => القسم الواحد بحتوي على عدة مواد 
        [ValidateNever]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}

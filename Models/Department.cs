using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO_UNI_Guide.Models
{
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

        // معدل القبول 
        [Display(Name = "Acceptance Rate (%)")]
        public decimal AcceptanceRate { get; set; }
        //سعر الساعه 
        [Display(Name = "Price Per Hour (JOD)")]
        public decimal HourPrice { get; set; }
        [Display(Name = "Total Credit Hours")]
        public int TotalCreditHours { get; set; }
        //Realtion => القسم الواحد بحتوي على عدة مواد 
        [ValidateNever]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}

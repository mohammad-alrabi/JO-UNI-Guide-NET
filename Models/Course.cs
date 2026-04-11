using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO_UNI_Guide.Models
{
    public class Course
    {
        [Key]
        public int Course_ID { get; set; }
        [Required]
        public string? Course_Name { get; set; }
        public string? Details { get; set; }

        //Realtion with Department
        [ForeignKey("Department")] 

        public int Department_ID { get; set; }
        [ValidateNever]
        public virtual Department Department { get; set; }
    }
}

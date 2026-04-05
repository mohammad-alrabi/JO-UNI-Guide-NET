using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO_UNI_Guide.Models
{
    public class Department
    {
        [Key]
        public int Department_ID { get; set; }
        [Required]
        public string DepartmentName { get; set; }
        public string Details { get; set; }

        // Relation with the faculty
        [ForeignKey("Faculty")]
        public int Faculty_ID { get; set; }
        public virtual Faculty Faculty { get; set; }

        //Realtion => القسم الواحد بحتوي على عدة مواد 
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public class Course
    {
        [Key]
        public int Course_ID { get; set; }
        [Required]
        public string Course_Name { get; set; }
        public string Details { get; set; }

        //Realtion with Department
        public int Department_ID { get; set; }
        public virtual Department Department { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO_UNI_Guide.Models
{
    public class Faculty
    {
        [Key]
        public int Faculty_ID  { get; set; }

        [Required (ErrorMessage ="اسم الكلية مطلوب")]
        public string Name { get; set; }
        public string Details { get; set; }
        public string Faculty_Dean { get; set; }
        public string Location { get; set; }

        //Relation => ربط الكليات مع الجامعات 
        [ForeignKey("University")]
        public int University_ID { get; set; }
        //للوصول للبيانات الخاصة بالجامعة من ال
        public virtual University University { get; set; }

        //Realtion => الكلية الوحدة بتحتوي على عدة اقسام one-to-many
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}

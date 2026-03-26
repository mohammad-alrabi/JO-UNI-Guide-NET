using System.ComponentModel.DataAnnotations;

namespace JO_UNI_Guide.Models
{
    public class University
    {
        [Key]
        public int University_ID { get; set; }

        [Required (ErrorMessage ="اسم الجامعة مطلوب")]
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Location { get; set; }
        public string Rank_QS { get; set; }

        //Relation => الجامعة الوحدة بتحتوي على عدة كليات one-to-many
        public virtual ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();



    }
}

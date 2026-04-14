namespace JO_UNI_Guide.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        // ربط مع الطالب
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        // ربط مع التخصص (Department)
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public DateTime SavedDate { get; set; } = DateTime.UtcNow;
    }
}

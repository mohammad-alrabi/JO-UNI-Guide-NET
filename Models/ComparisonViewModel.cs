namespace JO_UNI_Guide.Models
{
    public class ComparisonViewModel : Department
    {
        // قائمة التخصصات التي اختار الطالب مقارنتها
        public List<Department> SelectedDepartments { get; set; } = new List<Department>();
    }
}

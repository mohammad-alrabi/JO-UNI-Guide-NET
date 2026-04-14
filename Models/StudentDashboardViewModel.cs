namespace JO_UNI_Guide.Models
{
    public class StudentDashboardViewModel
    {
        public string? Name { get; set; }
        public double? GPA { get; set; }
        public int MatchingMajorsCount { get; set; }
        public List<Department>? TopMajors { get; set; }

    }
}

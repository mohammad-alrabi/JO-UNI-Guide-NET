namespace JO_UNI_Guide.Models
{
    public class DashboardViewModel
    {
        public int UniversitiesCount { get; set; }
        public int FacultiesCount { get; set; }
        public int DepartmentsCount { get; set; }
        public int CoursesCount { get; set; }
        public int AdminsCount { get; set; }
    
        public List<University> LatestUniversities { get; set; }
        public List<Course> LatestCourses { get; set; }
    }
}

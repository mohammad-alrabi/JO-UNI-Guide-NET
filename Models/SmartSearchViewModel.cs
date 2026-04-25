namespace JO_UNI_Guide.Models
{
    public class SmartSearchViewModel 
    {
        public string? Keyword { get; set; }
        public decimal? MaxHourPrice { get; set; }
        public double? StudentGPA { get; set; }
        public TawjihiTrack? Track { get; set; }
        public UniversityType? UniType { get; set; }
        public List<Department> Results { get; set; } = new List<Department>();
    }
}

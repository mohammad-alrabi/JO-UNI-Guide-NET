namespace JO_UNI_Guide.Models
{
    public class UniversitiesListViewModel
    {
        public List<University> PublicUniversities { get; set; } = new List<University>();

        public List<University> PrivateUniversities { get; set; } = new List<University>();
    }
}

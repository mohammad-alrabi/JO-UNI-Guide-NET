using JO_UNI_Guide.Models;

public class StudentDashboardViewModel
{
    public string? Name { get; set; }
    public double? GPA { get; set; }
    public int MatchingMajorsCount { get; set; }
    public List<Department>? TopMajors { get; set; }

    // استخدم GetValueOrDefault عشان نتجنب nullable comparison warning
    public string GpaLevel =>
        GPA.GetValueOrDefault() >= 90 ? "High" :
        GPA.GetValueOrDefault() >= 75 ? "Medium" : "Low";

    public string GpaColorClass =>
        GPA.GetValueOrDefault() >= 90 ? "bg-success" :
        GPA.GetValueOrDefault() >= 75 ? "bg-warning text-dark" : "bg-danger";
}

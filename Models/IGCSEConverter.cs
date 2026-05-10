using JO_UNI_Guide.interfaces;

namespace JO_UNI_Guide.Models
{
    public class IGCSEConverter : IGradeConverter
    {
        public double Convert(double grade)
        {
            if (grade < 0) return 0;
            if (grade > 8) grade = 8;

            return Math.Round((grade / 8.0) * 100, 2);
        }
    }
}
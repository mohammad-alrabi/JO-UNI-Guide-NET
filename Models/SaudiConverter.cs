using JO_UNI_Guide.interfaces;

namespace JO_UNI_Guide.Models
{
    public class SaudiConverter : IGradeConverter
    {
        public double Convert(double grade)
        {
            if (grade < 0) return 0;
            if (grade > 100) return 100;

            return grade;
        }
    }
}
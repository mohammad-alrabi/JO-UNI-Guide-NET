using JO_UNI_Guide.interfaces;

namespace JO_UNI_Guide.Models
{
    public class AmericanConverter : IGradeConverter
    {
        public double Convert(double grade)
        {
            return Math.Clamp((grade / 4.0) * 100, 0, 100);
        }
    }
}
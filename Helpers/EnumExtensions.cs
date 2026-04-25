using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace JO_UNI_Guide.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>()
                ?.Name ?? enumValue.ToString();
        }

    }
}

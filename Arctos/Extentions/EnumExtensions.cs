using System.ComponentModel;

namespace Arctos
{
    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var attributes = value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Any())
            {
                var description = (attributes.First() as DescriptionAttribute)?.Description;
                if (description != null)
                    return description;
            }

            return value.ToString().SplitCamelCase().Replace("_", " ");
        }
    }
}

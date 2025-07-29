using System.Text.RegularExpressions;

namespace Arctos
{
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        public static string TrimEndSubString(this string input, string subString)
        {
            return input.EndsWith(subString) ? input[..input.LastIndexOf(subString, StringComparison.Ordinal)] : input;
        }
        public static string TrimStartSubString(this string input, string subString)
        {
            if (!input.StartsWith(subString))
                return input;
            var index = input.IndexOf(subString, StringComparison.Ordinal);
            return (index < 0)
                ? input
                : input.Remove(index, subString.Length);
        }
    }
}

using System;

namespace Totem.Infrastructure
{
    public static class StringExtensions
    {
        public static bool EqualsCaseInsensitive(this string source, string compare)
        {
            return source?.IndexOf(compare, StringComparison.InvariantCultureIgnoreCase) == 0 && source.Length == compare.Length;
        }
    }
}

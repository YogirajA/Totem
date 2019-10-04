using System;

namespace Totem.Infrastructure
{
    public static class StringExtensions
    {
        public static bool EqualsCaseInsensitive(this string source, string compare)
        {
            return string.Equals(source, compare, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

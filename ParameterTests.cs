namespace System
{
    internal static class ParameterTests
    {
        public static void NotNull<T>(this T @this, string paramName) where T : class
        {
            if (ReferenceEquals(@this, null))
            {
                throw new ArgumentNullException(paramName, ErrorStrings.CannotBeNull(paramName));
            }
        }

        public static void NotNullOrEmpty(this string @this, string paramName)
        {
            if (string.IsNullOrEmpty(@this))
            {
                throw new ArgumentNullException(paramName, ErrorStrings.CannotBeNull(paramName));
            }
        }

        public static void NotNullOrWhiteSpace(this string @this, string paramName)
        {
            if (string.IsNullOrWhiteSpace(@this))
            {
                throw new ArgumentNullException(paramName, ErrorStrings.CannotBeNull(paramName));
            }
        }
    }
}

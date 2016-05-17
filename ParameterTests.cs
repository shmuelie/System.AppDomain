using System.Reflection;

namespace System
{
    internal static class ParameterTests
    {
        public static void NotNull<T>(this T @this, string paramName) where T : class
        {
            if (ReferenceEquals(@this, null))
            {
#pragma warning disable HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
                throw new ArgumentNullException(paramName, ErrorStrings.CannotBeNull(paramName));
#pragma warning restore HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
            }
        }

        public static void NotNullOrEmpty(this string @this, string paramName)
        {
            if (string.IsNullOrEmpty(@this))
            {
#pragma warning disable HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
                throw new ArgumentNullException(paramName, ErrorStrings.CannotBeNullOrEmpty(paramName));
#pragma warning restore HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
            }
        }

        public static void NotNullOrWhiteSpace(this string @this, string paramName)
        {
            if (string.IsNullOrWhiteSpace(@this))
            {
#pragma warning disable HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
                throw new ArgumentNullException(paramName, ErrorStrings.CannotBeNullOrWhitespace(paramName));
#pragma warning restore HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
            }
        }

        public static void InstanceOf<T>(this T @this, string paramName, Type checkType)
        {
            if (!checkType.IsInstanceOfType(@this))
            {
#pragma warning disable HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
                throw new ArgumentException(ErrorStrings.MustBeReal(paramName, checkType), paramName);
#pragma warning restore HeapAnalyzerExplicitNewObjectRule // Explicit new reference type allocation
            }
        }
    }
}

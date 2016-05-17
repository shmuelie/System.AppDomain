namespace System
{
    internal static class ErrorStrings
    {
        private const string NULL = "null (Nothing in Visual Basic)";

        public static string CannotBeNull(string paramName)
        {
            return $"'{paramName}' cannot be {NULL}";
        }
        public static string CannotBeNullOrEmpty(string paramName)
        {
            return $"'{paramName}' cannot be empty or {NULL}";
        }

        public static string CannotBeNullOrWhitespace(string paramName)
        {
            return $"'{paramName}' cannot be empty, only whitespace, or {NULL}";
        }
    }
}

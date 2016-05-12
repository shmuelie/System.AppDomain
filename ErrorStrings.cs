namespace System
{
    public static class ErrorStrings
    {
        public static string CannotBeNull(string paramName)
        {
            return $"'{paramName}' cannot be null (Nothing in Visual Basic)";
        }
        public static string CannotBeNullOrEmpty(string paramName)
        {
            return $"'{paramName}' cannot be empty or null (Nothing in Visual Basic)";
        }

        public static string CannotBeNullOrWhitespace(string paramName)
        {
            return $"'{paramName}' cannot be empty, only whitespace, or null (Nothing in Visual Basic)";
        }
    }
}

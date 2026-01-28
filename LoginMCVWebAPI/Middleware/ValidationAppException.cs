namespace LoginMCVWebAPI.Middleware
{
    public class ValidationAppException : CustomAppException
    {
        public Dictionary<string, string[]> ValidationErrors { get; }

        public ValidationAppException(Dictionary<string, string[]> validationErrors)
            : base("Validation failed", 400, "VALIDATION_ERROR")
        {
            ValidationErrors = validationErrors;
        }
    }
}

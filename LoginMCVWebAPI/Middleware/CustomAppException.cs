namespace LoginMCVWebAPI.Middleware
{
    public class CustomAppException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public string Details { get; }

        public CustomAppException(string message, int statusCode = 500,
            string errorCode = "INTERNAL_ERROR", string details = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details;
        }

        public CustomAppException(string message, Exception innerException,
            int statusCode = 500, string errorCode = "INTERNAL_ERROR")
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

}
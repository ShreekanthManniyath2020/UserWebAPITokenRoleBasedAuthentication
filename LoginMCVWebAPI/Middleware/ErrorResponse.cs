namespace LoginMCVWebAPI.Middleware
{
    public class ErrorResponse
    {
        public string Type { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Instance { get; set; }
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; }

        public static ErrorResponse FromException(Exception exception,
            HttpContext context, bool includeDetails)
        {
            var errorCode = exception is CustomAppException customEx
                ? customEx.ErrorCode
                : "INTERNAL_ERROR";

            var statusCode = exception is CustomAppException customEx2
                ? customEx2.StatusCode
                : 500;

            return new ErrorResponse
            {
                Type = exception.GetType().Name,
                ErrorCode = errorCode,
                Message = includeDetails ? exception.Message : "An error occurred",
                Details = includeDetails ? exception.ToString() : null,
                Instance = context.Request.Path,
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier,
                AdditionalInfo = GetAdditionalInfo(exception)
            };
        }

        private static Dictionary<string, object> GetAdditionalInfo(Exception exception)
        {
            var info = new Dictionary<string, object>();

            if (exception is ValidationAppException validationEx)
            {
                info.Add("ValidationErrors", validationEx.ValidationErrors);
            }

            return info;
        }
    }
}

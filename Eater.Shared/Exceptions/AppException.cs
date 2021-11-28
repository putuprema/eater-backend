namespace Eater.Shared.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; set; }

        public AppException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public dynamic GetResponse() => new { Message };
    }
}

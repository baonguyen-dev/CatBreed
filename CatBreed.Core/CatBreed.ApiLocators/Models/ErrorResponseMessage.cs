using System;
namespace CatBreed.ApiLocators.Models
{
    public class ErrorResponseMessage
    {
        public string ErrorMessage { get; set; }
        public string ExceptionType { get; set; }

        public string InnerExceptionType { get; set; } = null;
        public int? ErrorCode { get; set; } = null;
        public string ErrorDetails { get; set; } = null;

    }
}

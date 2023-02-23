using System;
using System.Net.Http;

namespace CatBreed.ApiLocators.Models
{
    public class ServiceInvokeException : Exception
    {
        public HttpResponseMessage ResponseMessage { get; }
        public ErrorResponseMessage ErrorResponseDetails { get; set; }

        public ServiceInvokeException(HttpResponseMessage responseMessage, ErrorResponseMessage errorResponse)
        {
            ResponseMessage = responseMessage;
            ErrorResponseDetails = errorResponse;
        }
    }
}

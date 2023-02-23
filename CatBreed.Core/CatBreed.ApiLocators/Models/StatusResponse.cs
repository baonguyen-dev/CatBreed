using System;
using System.Net;

namespace CatBreed.ApiLocators.Models
{
    public class StatusResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }

        public StatusResponse()
        {
        }
    }
}

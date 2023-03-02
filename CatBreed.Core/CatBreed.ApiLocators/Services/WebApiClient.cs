using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CatBreed.ApiLocators.Models;
using CatBreed.ServiceLocators.Services;
using Newtonsoft.Json;

namespace CatBreed.ApiLocators.Services
{
    public class WebApiClient : IWebApiClient
    {
        private const string EP_SERVERTIME = "/api/ServerUtilities/ServerTime";

        public static Uri BaseUrl { get; private set; }
        public static string UserAgent { get; private set; }

        private static readonly HttpClient _httpClient;

        //public WebApiClient(Uri baseurl, string useragent)
        //{
        //    BaseUrl = baseurl;
        //    UserAgent = useragent;
        //}

        public void Init(Uri uri, string userAgent)
        {
            BaseUrl = uri;
            UserAgent = userAgent;
        }

        static WebApiClient() 
        {
            HttpClientHandler clientHandler = new HttpClientHandler();

            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            _httpClient = new HttpClient(clientHandler);

            if (!string.IsNullOrEmpty(UserAgent))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            }
        }

        protected async Task<T> SendRequest<T>(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            ErrorResponseMessage error = null;
            String content;

            T value = default(T);

            try
            {
                response = await _httpClient.SendAsync(request);
                content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(content))
                {
                }

                if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(content))
                {
                    value = JsonConvert.DeserializeObject<T>(content);
                }
                else if (!response.IsSuccessStatusCode && !string.IsNullOrEmpty(content))
                {
                    error = JsonConvert.DeserializeObject<ErrorResponseMessage>(content);
                    throw new ServiceInvokeException(response, error);
                }
            }
            catch (ServiceInvokeException sie)
            {
                throw sie;
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
            }

            return value;
        }
    }
}

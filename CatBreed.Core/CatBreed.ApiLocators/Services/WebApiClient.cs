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

        public static string ProxyHost { get; set; }
        public static string ProxyPort { get; set; }
        public static string ProxyUser { get; set; }
        public static string ProxyPass { get; set; }

        public Uri BaseUrl { get; private set; }
        public string UserAgent { get; private set; }

        public WebApiClient(Uri baseurl, string useragent)
        {
            BaseUrl = baseurl;
            UserAgent = useragent;
        }

        protected void Init(Uri uri, string userAgent)
        {
            BaseUrl = uri;
            UserAgent = userAgent;
        }

        public WebApiClient() { }

        HttpClient GetHttpClient()
        {
            if (!string.IsNullOrWhiteSpace(ProxyHost) && !string.IsNullOrWhiteSpace(ProxyPort))
            {
                WebProxy proxy = new WebProxy(new Uri(string.Format("http://{0}:{1}", ProxyHost.TrimEnd('/'), ProxyPort)));

                HttpClientHandler httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true,
                    UseDefaultCredentials = true
                };

                if (!string.IsNullOrWhiteSpace(ProxyUser) && !string.IsNullOrWhiteSpace(ProxyPass))
                {
                    httpClientHandler.Credentials = new NetworkCredential(ProxyUser, ProxyPass);
                    httpClientHandler.UseDefaultCredentials = false;
                }

                return new HttpClient(httpClientHandler);
            }

            return new HttpClient();
        }

        protected async Task<Stream> SendRequest(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            ErrorResponseMessage error = null;
            Stream content = null;

            var client = GetHttpClient();

            if (!string.IsNullOrEmpty(UserAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            }

            try
            {
                response = await client.SendAsync(request);
                content = await response.Content.ReadAsStreamAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        string stringContent = response.Content.ReadAsStringAsync().Result;
                        error = JsonConvert.DeserializeObject<ErrorResponseMessage>(stringContent);
                        throw new ServiceInvokeException(response, error);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
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
                client?.Dispose();
            }

            return content;
        }

        protected async Task<StatusResponse<T>> Request<T>(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            ErrorResponseMessage error = null;
            String content;

            StatusResponse<T> value = new StatusResponse<T>();
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            var client = new HttpClient(clientHandler);

            if (!string.IsNullOrEmpty(UserAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            }

            try
            {
                response = await client.SendAsync(request);
                content = await response.Content.ReadAsStringAsync();

                value.StatusCode = response.StatusCode;

                if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(content))
                {
                    value.Data = JsonConvert.DeserializeObject<T>(content);
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
                client?.Dispose();
            }

            return value;
        }

        protected async Task<T> SendRequest<T>(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            ErrorResponseMessage error = null;
            String content;

            T value = default(T);
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            var client = new HttpClient(clientHandler);

            if (!string.IsNullOrEmpty(UserAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            }

            try
            {
                response = await client.SendAsync(request);
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
                client?.Dispose();
            }

            return value;
        }
    }
}

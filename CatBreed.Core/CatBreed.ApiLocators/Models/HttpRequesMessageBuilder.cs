using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace CatBreed.ApiLocators.Models
{
    public enum HttpContentType
    {
        StringContent,
        FormUrlEncodedContent,
        ByteArrayContent,
        MultipartForm,
        StreamContent
    }

    public class HttpRequesMessageBuilder
    {
        public const string JSON = "application/json";
        public const string URLENCODED = "application/x-www-form-urlencoded";
        public const string TEXTPLAIN = "text/plain";
        public const string OCTETSTREAM = "octet-stream";
        public const string XML = "application/xml";
        public const string TEXTXML = "text/xml";

        private HttpMethod Method = HttpMethod.Get;

        private string BaseUrl;
        private string ApiEndpoint;
        private List<KeyValuePair<string, IEnumerable<string>>> Headers;
        private List<KeyValuePair<string, string>> Parameters;
        private HttpContent Content = null;
        public HttpRequesMessageBuilder SetBaseUrl(string baseurl)
        {
            this.BaseUrl = baseurl;
            return this;
        }

        public HttpRequesMessageBuilder SetHttpContent(object content, HttpContentType httpContentType = HttpContentType.StringContent, string mediaType = JSON)
        {
            switch (httpContentType)
            {
                case HttpContentType.StringContent:
                    var stringContent = "";
                    if (mediaType.Contains(URLENCODED))
                        stringContent = "data:" + WebUtility.UrlEncode(content.ToString());
                    else
                        stringContent = JsonConvert.SerializeObject(content);

                    Content = string.IsNullOrEmpty(mediaType) ? new StringContent(stringContent, Encoding.UTF8) : new StringContent(stringContent, Encoding.UTF8, mediaType);
                    break;
                case HttpContentType.FormUrlEncodedContent:
                    var dictionary = (Dictionary<string, string>)content;
                    Content = new FormUrlEncodedContent(dictionary);
                    break;
                case HttpContentType.StreamContent:
                    Content = (StreamContent)content;
                    break;
            }
            return this;
        }

        public HttpRequesMessageBuilder SetApiEndpoint(string apiendpoint)
        {
            this.ApiEndpoint = apiendpoint;
            return this;
        }

        public HttpRequesMessageBuilder SetMethod(HttpMethod method)
        {
            this.Method = method;
            return this;
        }

        public HttpRequesMessageBuilder AddHeader(string key, string value)
        {
            if (Headers == null)
            {
                Headers = new List<KeyValuePair<string, IEnumerable<string>>>();
            }

            if (!Headers.Any(kvp => kvp.Key.Equals(key)))
            {
                var values = new List<string>();
                var kvp = new KeyValuePair<string, IEnumerable<string>>(key, values);
                Headers.Add(kvp);
            }

            (Headers.First(kvp => kvp.Key.Equals(key)).Value as List<string>).Add(value);

            return this;
        }

        public HttpRequesMessageBuilder AddParameter(string key, string value)
        {
            if (Parameters == null)
            {
                Parameters = new List<KeyValuePair<string, string>>();
            }

            Parameters.Add(new KeyValuePair<string, string>(key, value));

            return this;
        }

        public HttpRequesMessageBuilder AddNonce()
        {
            string nonce = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 15);
            return this.AddParameter("nonce", nonce);
        }

        private UriBuilder GetRequestUriBuilder()
        {
            var tmpUri = new Uri(BaseUrl);
            var builder = new UriBuilder(tmpUri);
            builder.Scheme = tmpUri.Scheme;
            builder.Host = tmpUri.Host;
            builder.Port = tmpUri.Port;
            if (!string.IsNullOrEmpty(ApiEndpoint))
                builder.Path = ApiEndpoint;

            return builder;
        }

        public HttpRequestMessage Build()
        {
            var builder = GetRequestUriBuilder();

            if (Parameters != null && Parameters.Any())
            {
                var sb = new StringBuilder();
                foreach (var p in Parameters.OrderBy(kvp => kvp.Key))
                {
                    if (sb.Length != 0)
                    {
                        sb.Append("&");
                    }

                    sb.Append(p.Key);
                    sb.Append("=");
                    sb.Append(p.Value);
                }
                builder.Query = sb.ToString();
            }

            var uri = builder.Uri;

            var request = new HttpRequestMessage(this.Method, uri);
            if (Headers != null)
            {
                foreach (var header in Headers)
                {
                    foreach (var headervalue in header.Value)
                    {
                        request.Headers.Add(header.Key, headervalue);
                    }
                }
            }

            if (Content != null)
                request.Content = Content;

            return request;
        }
    }
}
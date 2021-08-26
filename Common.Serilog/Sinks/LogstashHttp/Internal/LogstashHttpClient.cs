using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog.Sinks.Http;

namespace Common.Serilog.Sinks.LogstashHttp.Internal
{
    internal class LogstashHttpClient : IHttpClient
    {
        private readonly HttpClient _httpClient;

        public LogstashHttpClient(string username, string password)
        {
            // Create custom HttpClient for Serilog's Http sink that supplies
            // basic auth headers with request for authentication with the logstash
            // harvester.
            var httpSecret = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            _httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(username))
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {httpSecret}");
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return _httpClient.PostAsync(requestUri, content);
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}

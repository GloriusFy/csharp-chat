using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Core.Service
{
    class HttpService : IHttpService
    {
        private static readonly HttpService _httpService = new HttpService();
        private static readonly HttpClient _httpClient = new HttpClient();
        static HttpService()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.BaseAddress = new Uri("http://localhost:44302");
        }
        private HttpService() { }
        public static HttpService HttpServiceInstance
        {
            get => _httpService;
        }

        public async Task<HttpResponseMessage> PostData(string endpoint, string jsonContent)
        {
            var response = await _httpClient.PostAsync($"{endpoint}",
               new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            return response;
        }

        public async Task<HttpResponseMessage> GetData(string endponit)
        {
            var response = await _httpClient.GetAsync(endponit);
            return response;
        }

        public void SetEndPoint(Uri endpoint)
        {
            _httpClient.BaseAddress = endpoint;
        }
    }
}

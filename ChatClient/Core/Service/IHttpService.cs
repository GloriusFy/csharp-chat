using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatClient.Core.Service
{
    public interface IHttpService
    {
        Task<HttpResponseMessage> PostData(string endpoint, string jsonContent);
        Task<HttpResponseMessage> GetData(string endponit);
        void SetEndPoint(Uri endpoint);
    }
}

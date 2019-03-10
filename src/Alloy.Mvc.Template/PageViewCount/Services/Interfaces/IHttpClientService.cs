using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AlloyTemplates.PageViewCount.Services.Interfaces
{
    public interface IHttpClientService
    {
        Task<T> GetResult<TDelegatingHandler, T>(string baseUrl, string path)
            where TDelegatingHandler : DelegatingHandler;

        Task<TL> PostData<TDelegatingHandler, T, TL>(string baseUrl, string path, T data)
            where TDelegatingHandler : DelegatingHandler;

        Task<TOut> Post<TDelegatingHandler, TOut>(string baseUrl, string path)
            where TDelegatingHandler : DelegatingHandler;

        Task PostData<TDelegatingHandler, TIn>(string baseUrl, string path, TIn data)
            where TDelegatingHandler : DelegatingHandler;


        Task<Stream> DownloadAsync<TDelegatingHandler>(string baseUrl, string path)
            where TDelegatingHandler : DelegatingHandler;
    }
}

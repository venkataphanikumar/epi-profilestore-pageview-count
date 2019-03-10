using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AlloyTemplates.PageViewCount.Services.Interfaces;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace AlloyTemplates.PageViewCount.Services
{
    [ServiceConfiguration(ServiceType = typeof(IHttpClientService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class HttpClientService: IHttpClientService
    {
        public static Dictionary<string, HttpClient> HttpClients { get; set; } = new Dictionary<string, HttpClient>();

        public async Task<T> GetResult<TDelegatingHandler, T>(string baseUrl, string path)
            where TDelegatingHandler : DelegatingHandler
        {
            var client = InitializeHttpClient<TDelegatingHandler>(baseUrl);
            var response = await client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead);
            return await EnsureAndReadResult<T>(response);
        }


        public async Task<TL> PostData<TDelegatingHandler, T, TL>(string baseUrl, string path, T data)
            where TDelegatingHandler : DelegatingHandler
        {
            var client = InitializeHttpClient<TDelegatingHandler>(baseUrl);
            var response = await client.PostAsJsonAsync(path, data);
            return await EnsureAndReadResult<TL>(response);
        }

        public async Task<TOut> Post<TDelegatingHandler,TOut>(string baseUrl, string path) where TDelegatingHandler : DelegatingHandler
        {
            var client = InitializeHttpClient<TDelegatingHandler>(baseUrl);
            var response = await client.PostAsync(path, null);
            return await EnsureAndReadResult<TOut>(response);
        }

        public async Task PostData<TDelegatingHandler, TIn>(string baseUrl, string path, TIn data)
            where TDelegatingHandler : DelegatingHandler
        {
            var client = InitializeHttpClient<TDelegatingHandler>(baseUrl);
            var response = await client.PostAsJsonAsync(path, data);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Stream> DownloadAsync<TDelegatingHandler>(string baseUrl, string path)
            where TDelegatingHandler : DelegatingHandler

        {
            var client = InitializeHttpClient<TDelegatingHandler>(baseUrl);
            var response = await client.GetAsync(path);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        HttpClient InitializeHttpClient<TDelegatingHandler>(string baseUrl) where TDelegatingHandler: DelegatingHandler
        {
            if (!HttpClients.TryGetValue(typeof(TDelegatingHandler).FullName, out HttpClient client))
            {
                client = HttpClientFactory.Create(ServiceLocator.Current.GetInstance<TDelegatingHandler>());

                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            return client;
        }

        static async Task<TL> EnsureAndReadResult<TL>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var resultContent = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonConvert.DeserializeObject<TL>(resultContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error from Enterprise API. Can't parse the content: {resultContent}", ex);
            }
        }

    }
}

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AlloyTemplates.PageViewCount.DelegatingHandlers
{
  public  class ProfileStoreDelegatingHandlers: DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            var appKey = request.RequestUri.ParseQueryString()["appKey"];

            if (!string.IsNullOrWhiteSpace(appKey))
            {
                request.Headers.Add("Authorization", $"epi-single {appKey}");
            }
          
            response = await base.SendAsync(request, cancellationToken);
            return response;
        }

    }
}

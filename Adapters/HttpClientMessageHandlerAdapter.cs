using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AasSharpClient.Adapters
{
    // Adapter that exposes an HttpClient as an HttpMessageHandler so it can be
    // passed into libraries that expect a HttpMessageHandler constructor (like BaSyx SimpleHttpClient).
    // The adapter forwards requests to the provided HttpClient instance and does NOT
    // dispose the inner client when disposed.
    public sealed class HttpClientMessageHandlerAdapter : HttpMessageHandler
    {
        private readonly HttpClient _client;

        public HttpClientMessageHandlerAdapter(HttpClient client)
        {
            _client = client ?? throw new System.ArgumentNullException(nameof(client));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Forward the request to the provided HttpClient. Do not alter the request.
            return _client.SendAsync(request, cancellationToken);
        }
    }
}

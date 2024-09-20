using Microsoft.Net.Http.Headers;

namespace ScaleUP.ApiGateways.Aggregator.Extensions
{
    public interface ITokenProvider
    {
        string GetTokenAsync();
    }

    public class AppTokenProvider : ITokenProvider
    {
        private string? _token;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppTokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetTokenAsync()
        {
            if (_token == null)
            {
                _token = _httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Authorization];
            }

            return _token;
        }
    }
}
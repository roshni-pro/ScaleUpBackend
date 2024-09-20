using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ScaleUP.BuildingBlocks.Logging
{
    public static class LogHelper
    {
        public static string RequestPayload = "";

        public static async void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;


            if (string.IsNullOrEmpty(request.ContentType) || !request.ContentType.ToLower().Contains("grpc"))
            {
                diagnosticContext.Set("RequestBody", RequestPayload);

                string responseBodyPayload = await ReadResponseBody(httpContext.Response);
                diagnosticContext.Set("ResponseBody", responseBodyPayload);

                // Set all the common properties available for every request
                diagnosticContext.Set("Host", request.Host);
                diagnosticContext.Set("Protocol", request.Protocol);
                diagnosticContext.Set("Scheme", request.Scheme);

                // Only set it if available. You're not sending sensitive data in a querystring right?!
                if (request.QueryString.HasValue)
                {
                    diagnosticContext.Set("QueryString", request.QueryString.Value);
                }

                // Set the content-type of the Response at this point
                diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

                // Retrieve the IEndpointFeature selected for the request
                var endpoint = httpContext.GetEndpoint();
                if (endpoint is object) // endpoint != null
                {
                    diagnosticContext.Set("EndpointName", endpoint.DisplayName);
                }
            }
        }

        private static async Task<string> ReadResponseBody(HttpResponse response)
        {
                
            response.Body.Seek(0, SeekOrigin.Begin);
            string responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return $"{responseBody}";
        }

        public static string GetLogIndexName(WebApplicationBuilder builder)
        {
            return $"ScaleUP_applogs_{builder.Environment.EnvironmentName?.ToLowerInvariant().Replace(".", "_")}_{builder.Environment.ApplicationName?.ToLowerInvariant().Replace(".", "_")}_{DateTime.UtcNow:yyyy_MM}";
        }

        public static string GetLogIndexName(HostBuilderContext builder)
        {
            return $"ScaleUP_applogs_{builder.HostingEnvironment.EnvironmentName?.ToLowerInvariant().Replace(".", "_")}_{builder.HostingEnvironment.ApplicationName?.ToLowerInvariant().Replace(".", "_")}_{DateTime.UtcNow:yyyy_MM}";
        }

        public static string GetLogIndexName(HostBuilderContext builder, string environment)
        {
            return $"ScaleUP_applogs_{environment.Replace(".", "_")}_{builder.HostingEnvironment.ApplicationName?.ToLowerInvariant().Replace(".", "_")}_{DateTime.UtcNow:yyyy_MM}";
        }
    }
}

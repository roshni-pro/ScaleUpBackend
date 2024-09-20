using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace ScaleUP.BuildingBlocks.Logging
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        ActivitySource activitySource = new ActivitySource(nameof(RequestResponseLoggingMiddleware));
        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var activity = Activity.Current;
            if (string.IsNullOrEmpty(context.Request.ContentType) || !context.Request.ContentType.ToLower().Contains("grpc"))
            {
                // Read and log request body data
                string requestBodyPayload = await ReadRequestBody(context.Request);
                //LogHelper.RequestPayload = requestBodyPayload;
                activity.SetTag("requestProtocol", context.Request.Protocol);
                activity.SetTag("requestHeaders", JsonConvert.SerializeObject(context.Request.Headers));
                activity.SetTag("requestBody", requestBodyPayload);


                // Read and log response body data
                // Copy a pointer to the original response body stream
                var originalResponseBodyStream = context.Response.Body;

                // Create a new memory stream...
                using (var responseBody = new MemoryStream())
                {
                    // ...and use that for the temporary response body
                    context.Response.Body = responseBody;

                    // Continue down the Middleware pipeline, eventually returning to this class
                    await _next(context);

                    responseBody.Position = 0;
                    string responseString = new StreamReader(responseBody).ReadToEnd(); //responseBody is ""

                    activity.SetTag("responseHeaders", JsonConvert.SerializeObject(context.Response.Headers));

                    activity.SetTag("responseBody", responseString);

                    responseBody.Position = 0;

                    // Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                    await responseBody.CopyToAsync(originalResponseBodyStream);
                }
            }
            else
                await _next(context);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            HttpRequestRewindExtensions.EnableBuffering(request);

            var body = request.Body;
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            string requestBody = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            request.Body = body;

            return $"{requestBody}";
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}

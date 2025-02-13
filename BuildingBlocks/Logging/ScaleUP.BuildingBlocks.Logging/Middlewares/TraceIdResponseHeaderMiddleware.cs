﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;

namespace ScaleUP.BuildingBlocks.Logging.Middlewares
{
    public class TraceIdResponseHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceIdResponseHeaderMiddleware(RequestDelegate next)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
        }

        [DebuggerStepThrough]
        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.ContentType.ToLower().Contains("grpc"))
            {
                var traceId = Activity.Current!.TraceId.ToString();

                using (LogContext.PushProperty("TraceId", traceId.ToString()))
                {
                    if (!context.Response.Headers.ContainsKey("TraceId"))
                    {
                        context.Response.Headers.Add("TraceId", traceId);
                    }

                    await _next(context);
                }
            }
            else await _next(context);
        }
    }

    public static class TraceIdResponseHeaderMiddlewareResgistration
    {
        public static WebApplication UseTraceIdResponseHeader(this WebApplication builder)
        {
            builder.UseMiddleware<TraceIdResponseHeaderMiddleware>();

            return builder;
        }

    }
}

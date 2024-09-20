using Grpc.Core;
using Grpc.Core.Interceptors;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

namespace ScaleUP.BuildingBlocks.Logging
{
    public class GrpcServerInterceptor : Interceptor
    {
        ILogger _logger = Log.ForContext<GrpcServerInterceptor>();
        ActivitySource activitySource = new ActivitySource(nameof(GrpcServerInterceptor));


        public GrpcServerInterceptor()
        {

        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var activity = Activity.Current;
            //_logger.Information(" --- Global Custom Interceptor Invoked  --- ");

            activity.SetTag("requestHeaders", JsonConvert.SerializeObject(context.RequestHeaders));

            activity.SetTag("requestBody", JsonConvert.SerializeObject(request));
            var response = await base.UnaryServerHandler(request, context, continuation);
            activity.SetTag("responseBody", JsonConvert.SerializeObject(response));


            return response;
        }
    }


    public class GrpcClientInterceptor : Interceptor
    {
        ILogger _logger = Log.ForContext<GrpcClientInterceptor>();
        ActivitySource activitySource = new ActivitySource(nameof(GrpcClientInterceptor));

        public GrpcClientInterceptor()
        {

        }

        //public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        //{
        //    _logger.Information($"{Environment.NewLine}GRPC Request{Environment.NewLine}Method: {context.Method}{Environment.NewLine}Data: {JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented)}");

        //    var response = await base.UnaryServerHandler(request, context, continuation);

        //    _logger.Information($"{Environment.NewLine}GRPC Response{Environment.NewLine}Method: {context.Method}{Environment.NewLine}Data: {JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented)}");

        //    return response;
        //}

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request
           , ClientInterceptorContext<TRequest, TResponse> context
           , AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var activity = Activity.Current;
            activity.SetTag("requestBody", JsonConvert.SerializeObject(request));
            //activity.SetTag("requestHeaders", JsonConvert.SerializeObject(context.Options.Headers));

            var response = continuation(request, context);
            var resposeBody = response.ResponseAsync.Result;
            activity.SetTag("responseBody", JsonConvert.SerializeObject(resposeBody));


            return response;
        }

        //private async Task<TResponse> MyAsyncStuff<TResponse>(AsyncUnaryCall<TResponse> responseAsync)
        //{
        //    return await responseAsync;
        //}
    }
}

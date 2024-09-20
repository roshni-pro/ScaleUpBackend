using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using System.Net;

namespace ScaleUP.ApiGateways.Aggregator.Extensions
{
    public static class GrpsExtensions
    {
        public static GrpcChannel CreateAuthenticatedChannel(string address, IHttpContextAccessor _httpContextAccessor)
        {
            string? token = _httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Authorization];
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    metadata.Add("Authorization", $"{token}");
                }
                return Task.CompletedTask;
            });


            // SslCredentials is used here because this channel is using TLS.
            // CallCredentials can't be used with ChannelCredentials.Insecure on non-TLS channels.
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
           // var client= channel.CreateGrpcService<typeof(T)>();
            return channel;
        }

        public static GrpcChannel CreateAllowAnonymousChannel(string address)
        {
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                return Task.CompletedTask;
            });

            // SslCredentials is used here because this channel is using TLS.
            // CallCredentials can't be used with ChannelCredentials.Insecure on non-TLS channels.
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
            return channel;
        }

    }


   
}

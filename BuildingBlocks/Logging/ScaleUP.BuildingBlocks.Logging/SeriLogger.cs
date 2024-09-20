using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.Logging
{
    public static class SeriLogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
            (context, configuration) =>
            {
                var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");

                configuration
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.FromGlobalLogContext()
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(elasticUri))
                        {
                            IndexFormat = $"ScaleUP_applogs_{context.HostingEnvironment.EnvironmentName?.ToLowerInvariant().Replace(".", "_")}_{context.HostingEnvironment.ApplicationName?.ToLowerInvariant().Replace(".", "_")}_{DateTime.UtcNow:yyyy_MM}",
                            AutoRegisterTemplate = false,
                            NumberOfShards = 2,
                            NumberOfReplicas = 1,
                            ModifyConnectionSettings = x => x.BasicAuthentication(context.Configuration.GetValue<string>("ElasticConfiguration:UserName"), context.Configuration.GetValue<string>("ElasticConfiguration:Password"))
                        })
                    .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                    .Enrich.WithProperty("ContentRootPath", context.HostingEnvironment.ContentRootPath)
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithCorrelationId()
                    .Enrich.WithExceptionDetails()
                    .Enrich.FromLogContext()
                    .ReadFrom.Configuration(context.Configuration);
            };
    }
}

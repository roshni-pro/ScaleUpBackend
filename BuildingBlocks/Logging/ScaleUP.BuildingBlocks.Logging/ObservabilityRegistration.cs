using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Logfmt;
using Serilog.Sinks.Grafana.Loki;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;


namespace ScaleUP.BuildingBlocks.Logging
{
    public static class ObservabilityRegistration
    {
        public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
        {
            //string JaegerUrl = builder.Environment.EnvironmentName == "Development" ? "http://localhost:4317" : Environment.GetEnvironmentVariable("JaegerUrl");
            //string LokiURL = builder.Environment.EnvironmentName == "Development" ? "http://localhost:3100" : Environment.GetEnvironmentVariable("LokiURL");

            string JaegerUrl = builder.Configuration.GetValue<string>("JaegerUrl");
            string LokiURL = builder.Configuration.GetValue<string>("LokiURL");
            builder.Services.AddLogging(x =>
                {
                    x.ClearProviders();
                    x.AddSerilog(dispose: true);
                    //x.AddOpenTelemetry(o=> o.AttachLogsToActivityEvent())
                });

            builder.Services.AddHttpLogging(options => // <--- Setup logging
            {
                // Specify all that you need here:
                options.LoggingFields = HttpLoggingFields.RequestHeaders |
                                        HttpLoggingFields.RequestBody |
                                        HttpLoggingFields.ResponseHeaders |
                                        HttpLoggingFields.ResponseBody;
            });

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            // This is required if the collector doesn't expose an https endpoint. By default, .NET
            // only allows http2 (required for gRPC) to secure endpoints.
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var configuration = builder.Configuration;

            string appName = $"{builder.Environment.ApplicationName} - {builder.Environment.EnvironmentName}";

            var lokiLabels = new List<LokiLabel> { new LokiLabel { Key = "Application", Value = appName } };


            //builder.Host.AddSerilog();


            builder.Services
                    .AddOpenTelemetry()
                    .WithTracing(tracing =>
                    {
                        tracing
                            .AddSource(appName)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appName))
                            .SetErrorStatusOnException()
                            .SetSampler(new AlwaysOnSampler())
                            .AddAspNetCoreInstrumentation(options =>
                            {
                                // automatically sets Activity Status to Error if an unhandled exception is thrown
                                options.EnrichWithException = (activity, exception) =>
                                {
                                    activity.SetTag("exceptionType", exception.GetType().ToString());
                                    activity.SetTag("stackTrace", exception.StackTrace);
                                };
                            })
                                .AddSqlClientInstrumentation(instrumentationOptions =>
                                {
                                    instrumentationOptions.RecordException = true;
                                    instrumentationOptions.SetDbStatementForText = true;
                                    instrumentationOptions.SetDbStatementForStoredProcedure = true;

                                })
                                .AddEntityFrameworkCoreInstrumentation(instrumentationOptions =>
                                {
                                    instrumentationOptions.SetDbStatementForText = true;
                                    instrumentationOptions.SetDbStatementForStoredProcedure = true;

                                })
                                .AddHttpClientInstrumentation(options =>
                                {
                                    options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                                    {
                                        activity.SetTag("requestVersion", httpRequestMessage.Version);
                                    };
                                    // Note: Only called on .NET & .NET Core runtimes.
                                    options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                                    {
                                        activity.SetTag("responseVersion", httpResponseMessage.Version);
                                    };
                                    // Note: Called for all runtimes.
                                    options.EnrichWithException = (activity, exception) =>
                                    {
                                        activity.SetTag("exceptionType", exception.GetType().ToString());
                                        activity.SetTag("stackTrace", exception.StackTrace);
                                    };

                                })
                                .AddGrpcClientInstrumentation(options =>
                                {
                                    //options.EnrichWithHttpRequestMessage
                                    options.SuppressDownstreamInstrumentation = true;
                                })

                                ;

                        //tracing.AddOtlpExporter();
                        tracing
                            .AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(JaegerUrl);
                                //options.ExportProcessorType = ExportProcessorType.Batch;
                                //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;

                            })
                            ;
                    });


            builder.Logging.AddOpenTelemetry(logging =>
            {
                
                logging.AddOtlpExporter(
                "logging",
                options =>
                {
                    options.Endpoint = new Uri(JaegerUrl);

                }

                );
                logging.AttachLogsToActivityEvent();
                logging.IncludeFormattedMessage = true;
                logging.ParseStateValues = true;
            })
                .AddFilter<OpenTelemetryLoggerProvider>("*", LogLevel.Warning)
                ;



            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()

            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithEnvironmentName()
            .Enrich.With<ActivityEnricher>()
            //.WriteTo.OpenTelemetry("http://103.73.191.115:4318", Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf)
            .WriteTo.GrafanaLoki(LokiURL, lokiLabels /*,new string[] { "TraceId"} */
            , textFormatter: new LogfmtFormatter(opt =>
            {
                opt.IncludeAllProperties();
                opt.PreserveCase();
                opt.PreserveSerilogLogLevels();
                opt.OnDoubleQuotes(q => q.ConvertToSingle());
                opt.OnException(e => e
                     // log message and level (err) and exception type
                     .LogExceptionData(LogfmtExceptionDataFormat.Type | LogfmtExceptionDataFormat.Message | LogfmtExceptionDataFormat.Level)
                     // Log full stack trace
                     .LogStackTrace(LogfmtStackTraceFormat.SingleLine)
                 );
                opt.UseComplexPropertySeparator("->");
            })
            )
            .CreateLogger();

            //Serilog.Debugging.SelfLog.Enable(Console.Error);

            return builder;
        }

        public static IServiceCollection AddObservability(this IServiceCollection Services, string JaegerUrl, string LokiURL, string ApplicationName, string EnvironmentName)
        {

            string appName = $"{ApplicationName} - {EnvironmentName}";

            Services.AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(dispose: true);

            });

            Services.AddHttpLogging(options => // <--- Setup logging
            {
                // Specify all that you need here:
                options.LoggingFields = HttpLoggingFields.RequestHeaders |
                                        HttpLoggingFields.RequestBody |
                                        HttpLoggingFields.ResponseHeaders |
                                        HttpLoggingFields.ResponseBody;
            });

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            // This is required if the collector doesn't expose an https endpoint. By default, .NET
            // only allows http2 (required for gRPC) to secure endpoints.
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var lokiLabels = new List<LokiLabel> { new LokiLabel { Key = "Application", Value = appName } };



            Services
                    .AddOpenTelemetry()
                    .WithTracing(tracing =>
                    {
                        tracing
                            .AddSource(appName)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appName))
                            .SetErrorStatusOnException()
                            .SetSampler(new AlwaysOnSampler())

                            .AddMassTransitInstrumentation(options =>
                            {
                                //options.TracedOperations= n

                            })

                                .AddSqlClientInstrumentation(instrumentationOptions =>
                                {
                                    instrumentationOptions.RecordException = true;
                                    instrumentationOptions.SetDbStatementForText = true;
                                    instrumentationOptions.SetDbStatementForStoredProcedure = true;

                                })
                                .AddEntityFrameworkCoreInstrumentation(instrumentationOptions =>
                                {
                                    instrumentationOptions.SetDbStatementForText = true;
                                    instrumentationOptions.SetDbStatementForStoredProcedure = true;

                                })
                                .AddHttpClientInstrumentation(options =>
                                {
                                    options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                                    {
                                        activity.SetTag("requestVersion", httpRequestMessage.Version);
                                    };
                                    // Note: Only called on .NET & .NET Core runtimes.
                                    options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                                    {
                                        activity.SetTag("responseVersion", httpResponseMessage.Version);
                                    };
                                    // Note: Called for all runtimes.
                                    options.EnrichWithException = (activity, exception) =>
                                    {
                                        activity.SetTag("exceptionType", exception.GetType().ToString());
                                        activity.SetTag("theError", exception.InnerException != null ? exception.ToString() + Environment.NewLine + exception.InnerException.ToString() : exception.ToString());
                                        activity.SetTag("stackTrace", exception.StackTrace);
                                    };

                                })
                                .AddGrpcClientInstrumentation(options =>
                                {
                                    options.SuppressDownstreamInstrumentation = true;
                                })
                                .AddGrpcCoreInstrumentation()
                                ;

                        tracing
                            .AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(JaegerUrl);

                            })
                            ;
                    });



            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()

            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithEnvironmentName()
            .Enrich.With<ActivityEnricher>()
            .WriteTo.OpenTelemetry()
            .WriteTo.GrafanaLoki(LokiURL, lokiLabels
            , textFormatter: new LogfmtFormatter(opt =>
            {
                opt.IncludeAllProperties();
                opt.PreserveCase();
                opt.PreserveSerilogLogLevels();
                opt.OnDoubleQuotes(q => q.ConvertToSingle());
                opt.OnException(e => e
                     // log message and level (err) and exception type
                     .LogExceptionData(LogfmtExceptionDataFormat.Type | LogfmtExceptionDataFormat.Message | LogfmtExceptionDataFormat.Level)
                     // Log full stack trace
                     .LogStackTrace(LogfmtStackTraceFormat.SingleLine)
                 );
                opt.UseComplexPropertySeparator("->");
            })
            )
            .CreateLogger();

            //Serilog.Debugging.SelfLog.Enable(Console.Error);

            return Services;
        }

        private static async Task<string> ReadRequestBody(HttpRequest request)
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


        private static async Task<string> ReadResponseBody(HttpResponse response)
        {


            var responseStream = new MemoryStream();
            string responseBody = "";
            if (Convert.ToInt32(response.ContentLength) > 0)
            {
                responseStream.Seek(0, SeekOrigin.Begin);

                // read our own memory stream 
                using (StreamReader sr = new StreamReader(responseStream))
                {
                    responseBody = sr.ReadToEnd();
                }

            }
            return $"{responseBody}";

        }


        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            if (col != null && col.Count > 0)
                return col.Cast<string>()
                .Select(s => new { Key = s, Value = col[s] })
                .ToDictionary(p => p.Key, p => p.Value);
            else return null;
        }

    }


    public class ActivityEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;

            if (activity != null)
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty("SpanId", new ScalarValue(activity.GetSpanId())));
                logEvent.AddPropertyIfAbsent(new LogEventProperty("TraceId", new ScalarValue(activity.GetTraceId())));
                logEvent.AddPropertyIfAbsent(new LogEventProperty("ParentId", new ScalarValue(activity.GetParentId())));
            }
        }
    }

    internal static class ActivityExtensions
    {
        public static string GetSpanId(this Activity activity)
        {
            return activity.IdFormat switch
            {
                ActivityIdFormat.Hierarchical => activity.Id,
                ActivityIdFormat.W3C => activity.SpanId.ToHexString(),
                _ => null,
            } ?? string.Empty;
        }

        public static string GetTraceId(this Activity activity)
        {
            return activity.IdFormat switch
            {
                ActivityIdFormat.Hierarchical => activity.RootId,
                ActivityIdFormat.W3C => activity.TraceId.ToHexString(),
                _ => null,
            } ?? string.Empty;
        }

        public static string GetParentId(this Activity activity)
        {
            return activity.IdFormat switch
            {
                ActivityIdFormat.Hierarchical => activity.ParentId,
                ActivityIdFormat.W3C => activity.ParentSpanId.ToHexString(),
                _ => null,
            } ?? string.Empty;
        }
    }


    //public class OpenTracingSink : ILogEventSink
    //{
    //    private readonly ITracer _tracer;
    //    private readonly IFormatProvider _formatProvider;

    //    public OpenTracingSink(ITracer tracer, IFormatProvider formatProvider)
    //    {
    //        _tracer = tracer;
    //        _formatProvider = formatProvider;
    //    }

    //    public void Emit(LogEvent logEvent)
    //    {
    //        ISpan span = _tracer.ActiveSpan;

    //        if (span == null)
    //        {
    //            // Creating a new span for a log message seems brutal so we ignore messages if we can't attach it to an active span.
    //            return;
    //        }

    //        var fields = new Dictionary<string, object>
    //    {
    //        { "component", logEvent.Properties["SourceContext"] },
    //        { "level", logEvent.Level.ToString() }
    //    };

    //        fields[LogFields.Event] = "log";

    //        try
    //        {
    //            fields[LogFields.Message] = logEvent.RenderMessage(_formatProvider);
    //            fields["message.template"] = logEvent.MessageTemplate.Text;

    //            if (logEvent.Exception != null)
    //            {
    //                fields[LogFields.ErrorKind] = logEvent.Exception.GetType().FullName;
    //                fields[LogFields.ErrorObject] = logEvent.Exception;
    //            }

    //            if (logEvent.Properties != null)
    //            {
    //                foreach (var property in logEvent.Properties)
    //                {
    //                    fields[property.Key] = property.Value;
    //                }
    //            }
    //        }
    //        catch (Exception logException)
    //        {
    //            fields["mbv.common.logging.error"] = logException.ToString();
    //        }

    //        span.Log(fields);
    //    }
    //}

    //public static class OpenTracingSinkExtensions
    //{
    //    public static LoggerConfiguration OpenTracing(
    //              this LoggerSinkConfiguration loggerConfiguration,
    //              IFormatProvider formatProvider = null)
    //    {
    //        return loggerConfiguration.Sink(new OpenTracingSink(GlobalTracer.Instance, formatProvider));
    //    }
    //}
}

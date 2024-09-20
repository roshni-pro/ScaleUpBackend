using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ScaleUP.BuildingBlocks.Logging;
using Serilog;
using ScaleUP.Global.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ScaleUP.ApiGateways.Aggregator;
using ScaleUP.ApiGateways.Aggregator.Application;
using ProtoBuf.Grpc.ClientFactory;
using ScaleUP.ApiGateways.Aggregator.Persistence;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.ConsumerAppHome.Interfaces;
using Grpc.Core;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.Extensions;
using ProtoBuf.Grpc.Server;
using IdentityModel.AspNetCore.OAuth2Introspection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog.Formatting.Json;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Serilog.Exceptions;
using ScaleUP.BuildingBlocks.Logging;
using ScaleUP.Global.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;



var builder = WebApplication.CreateBuilder(args);


Environment.SetEnvironmentVariable("GRPC_TRACE", "api");
Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "debug");
Grpc.Core.GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.AddObservability();

builder.Services.AddOtherServices(builder.Configuration);
builder.Services.AddAllServiceAsGrpc(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDiskStorageHealthCheck(delegate (DiskStorageOptions diskStorageOptions)
    {
        diskStorageOptions.AddDrive(@"C:\", minimumFreeMegabytes: 5000);
    }
    , "C Drive", HealthStatus.Degraded
    );

builder.Services.AddCodeFirstGrpc(options =>
{
    options.Interceptors.Add<GrpcServerInterceptor>();
});


builder.Services.AddGlobalAuthenticationServices(builder);

builder.Services.AddSingleton<GrpcClientInterceptor>();

builder.Services.AddApplicationServices();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (builder.Environment.EnvironmentName.ToLower() != "production")
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseHealthChecks("/health");
app.UseRequestResponseLoggingMiddleware();



// // app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
});
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

//SeedData.InsertDefaultData(app);
app.Run();

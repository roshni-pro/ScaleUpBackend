using ScaleUP.Services.LedgerAPI;
using ScaleUP.Services.LedgerAPI.Application;
using ProtoBuf.Grpc.Server;
using ScaleUP.Services.LedgerAPI.GRPC.Server;
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
using ScaleUP.Services.LedgerAPI.Constants;
using ScaleUP.Services.LedgerAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddHttpContextAccessor();

builder.AddObservability();

builder.Services.AddHealthChecks()
    .AddSqlServer(EnvironmentConstants.DbContext)
    .AddDiskStorageHealthCheck(delegate (DiskStorageOptions diskStorageOptions)
    {
        diskStorageOptions.AddDrive(@"C:\", minimumFreeMegabytes: 5000);
    }
    , "C Drive", HealthStatus.Degraded
    )
    ;


builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddApplicationServices();

builder.Services.AddGlobalAuthenticationServices(builder);


//builder.Services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
//            .AddOAuth2Introspection(options =>
//            {
//                options.Authority = "https://localhost:7008/";
//                options.ClientId = "crmApi";
//                options.ClientSecret = "secret";

//            });

//builder.Services.AddAuthorization();


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

builder.Services.AddCodeFirstGrpc(options =>
{
    options.Interceptors.Add<GrpcServerInterceptor>();
});


builder.Services.AddSingleton<GrpcServerInterceptor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Configure the HTTP request pipeline.
//if (builder.Environment.EnvironmentName.ToLower() != "production")
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LedgerApplicationDbContext>();
    db.Database.Migrate();
}


app.UseRouting();
app.UseHealthChecks("/health");

app.UseRequestResponseLoggingMiddleware();

app.UseAuthentication();

app.UseAuthorization();


// app.UseHttpsRedirection();


app.MapControllers();

app.MapGrpcService<LedgerGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
});

app.Run();




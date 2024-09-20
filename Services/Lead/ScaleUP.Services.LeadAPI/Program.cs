using HealthChecks.System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ScaleUP.Services.LeadAPI;
using ScaleUP.Services.LeadAPI.Application;
using Serilog.Formatting.Json;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Serilog.Exceptions;
using ScaleUP.BuildingBlocks.Logging;
using ProtoBuf.Grpc.Server;
using ScaleUP.Services.LeadAPI.GRPC.Server;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Services.LeadAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.LeadAPI.Constants;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddGlobalAuthenticationServices(builder);

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


// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCodeFirstGrpc(options =>
{
    options.Interceptors.Add<GrpcServerInterceptor>();
});


builder.Services.AddSingleton<GrpcServerInterceptor>();
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
// Configure the HTTP request pipeline.
//if (builder.Environment.EnvironmentName.ToLower() != "production")
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();


app.UseStaticFiles();

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//           Path.Combine(builder.Environment.ContentRootPath, "Templates")),
//    RequestPath = "/Templates"
//});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeadApplicationDbContext>();
    db.Database.Migrate();
}

app.UseRouting();
app.UseHealthChecks("/health");

app.UseRequestResponseLoggingMiddleware();


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
app.MapGrpcService<LeadGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


SeedData.InsertDefaultData(app);

app.Run();

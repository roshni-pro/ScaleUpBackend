using ScaleUP.Services.MediaAPI.Application;
using ScaleUP.Services.MediaAPI;
using Serilog.Formatting.Json;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using ScaleUP.BuildingBlocks.Logging;
using Serilog.Exceptions;
using ScaleUP.Global.Infrastructure.Common;
using ProtoBuf.Grpc.Server;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Media.Interfaces;
using ScaleUP.Services.MediaAPI.GRPC.Server;
using ScaleUP.Services.MediaAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ScaleUP.Services.MediaAPI.Constants;
using Wkhtmltopdf.NetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddGlobalAuthenticationServices(builder);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.AddObservability();



builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();


builder.Services.AddControllers();

builder.Services.AddWkhtmltopdf();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c =>
{
    //c.TagActionsBy(api => new[] { api.GroupName });
    //c.DocInclusionPredicate((name, api) => true);

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

//app.Services.GetService<ApplicationDbContext>().Database.Migrate();

// Configure the HTTP request pipeline.
//if (builder.Environment.EnvironmentName.ToLower() != "production")
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}


app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
app.UseRequestResponseLoggingMiddleware();



app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<MediaGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");



app.Run();

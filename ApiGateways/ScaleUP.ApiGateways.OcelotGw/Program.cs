using HealthChecks.System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using ScaleUP.ApiGateways.OcelotGw.Constants;
using ScaleUP.BuildingBlocks.Logging;
using Ocelot.Cache.CacheManager;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.AddObservability();


builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddDiskStorageHealthCheck(delegate (DiskStorageOptions diskStorageOptions)
    {
        diskStorageOptions.AddDrive(@"C:\", minimumFreeMegabytes: 5000);
    }
    , "C Drive", HealthStatus.Degraded
    );
;


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var ocelotFileName = EnvironmentConstants.EnvironmentName == "Development" ? "ocelot-localdev.json" :
    (EnvironmentConstants.EnvironmentName == "LocalDevelopmentHttps" ? "ocelot-localdev-https.json" :
   (EnvironmentConstants.EnvironmentName == "QA" ? "ocelot-qa.json" :
  (EnvironmentConstants.EnvironmentName == "UAT"? "ocelot-uat.json" : "ocelot.json")));
builder.Configuration.AddJsonFile(ocelotFileName, optional: false, reloadOnChange: true);
var ocelot = builder.Services.AddOcelot(builder.Configuration)
    //.AddCacheManager(x =>
    //            {
    //                x.WithDictionaryHandle();
    //            }) 
    ;

builder.Services.AddSwaggerForOcelot(builder.Configuration);



var app = builder.Build();
app.UseStatusCodePages();
// Configure the HTTP request pipeline.
//if (builder.Environment.EnvironmentName.ToLower() != "production")
//{
//    //app.UseSwagger();
//    app.UseSwaggerForOcelotUI(opt =>
//    {
//        opt.PathToSwaggerGenerator = "/swagger/docs";
//    });
//}

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});


app.UseCors(builder => builder
     .AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

app.UseRouting();

app.UseHealthChecks("/health");

app.UseRequestResponseLoggingMiddleware();


// app.UseHttpsRedirection();

app.UseOcelot().Wait();


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


app.Run();

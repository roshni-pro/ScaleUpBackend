using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ScaleUP.Services.IdentityAPI.Persistence;
using ScaleUP.Global.Infrastructure.Common.Models.Identity;
using ScaleUP.Services.IdentityAPI;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ScaleUP.BuildingBlocks.Logging;
using ProtoBuf.Grpc.Server;
using ScaleUP.Services.IdentityAPI.GRPC.Server;
using ScaleUP.Services.IdentityAPI.Manager;
using ScaleUP.Services.IdentityAPI.Constants;
using System.Security.Cryptography.X509Certificates;
using Serilog;
using Microsoft.OpenApi.Models;
using ScaleUP.Services.IdentityModels.Master;
using ScaleUP.Global.Infrastructure.Constants;
//using ScaleUP.Global.Infrastructure.Persistence;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddCodeFirstGrpc(options =>
{
    options.Interceptors.Add<GrpcServerInterceptor>();
});


builder.Services.AddSingleton<GrpcServerInterceptor>();
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



var migrationAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;


builder.Services.AddDbContext<ApplicationDBContext>(options =>
                options.UseSqlServer(EnvironmentConstants.DbContext
                ,
                builder => builder.MigrationsAssembly(migrationAssembly)
                    ));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>(
    )
        .AddDefaultTokenProviders();



var identityServerBuilder = builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
})
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlServer(EnvironmentConstants.DbContext, sql => sql.MigrationsAssembly(migrationAssembly));

    })
     .AddOperationalStore(options =>
     {
         options.ConfigureDbContext = b => b.UseSqlServer(EnvironmentConstants.DbContext, sql => sql.MigrationsAssembly(migrationAssembly));

     })
     .AddAspNetIdentity<ApplicationUser>()
    ;

if (builder.Environment.IsEnvironment("Development"))
    identityServerBuilder.AddDeveloperSigningCredential();
else
{
    identityServerBuilder.AddSigningCredential(new X509Certificate2($"{Environment.CurrentDirectory}/https/scaleupcert.pfx", builder.Configuration.GetValue<string>("Kestrel:Endpoints:HttpsInlineCertFile:Certificate:Password")));
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("tokens", p =>
    {
        p.AddAuthenticationSchemes("jwt");
        p.RequireAuthenticatedUser();
    });


    //options.DefaultPolicy = new AuthorizationPolicyBuilder()
    //    .RequireAuthenticatedUser()
    //    .Build();
});

Serilog.ILogger logger = Log.Logger;

logger.Information(EnvironmentConstants.DbContext);


builder.Services.AddControllers();
builder.Services.AddScoped<IUserCreateManager, UserCreateManager>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.AddSecurityDefinition("OpenID", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = new Uri($"{EnvironmentConstants.IdentityUrl}/.well-known/openid-configuration")
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

app.UseCors(builder => builder
     .AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());
//app.UseMiddleware<RequestResponseLoggingMiddleware>();


//app.UseHttpLogging().UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest);



#region Create superadmin and api scopes
try
{
    using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
    {
        serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
        serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
        serviceScope.ServiceProvider.GetRequiredService<ApplicationDBContext>().Database.Migrate();

        var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        var u = serviceScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        if (!context.Clients.Any())
        {
            foreach (var client in Config.GetClients())
            {
                context.Clients.Add(client.ToEntity());
            }

            context.SaveChanges();
        }


        if (!context.IdentityResources.Any())
        {
            foreach (var resource in Config.GetIdentityResources())
            {
                context.IdentityResources.Add(resource.ToEntity());
            }

            context.SaveChanges();
        }

        if (!context.ApiScopes.Any())
        {
            foreach (var scope in Config.GetApiScopes())
            {
                context.ApiScopes.Add(scope.ToEntity());
            }
            context.SaveChanges();
        }

        if (!context.ApiResources.Any())
        {
            foreach (var api in Config.GetAllApiResources())
            {
                context.ApiResources.Add(api.ToEntity());
            }

            context.SaveChanges();
        }

        if (!u.Roles.Any())
        {
            u.Roles.AddRange(new IdentityRole
            {
                Name = "superadmin",
                NormalizedName = "superadmin".ToUpper()
            },
            new IdentityRole
            {
                Name = "system",
                NormalizedName = "system".ToUpper()
            },
            new IdentityRole
            {
                Name = "CompanyAdmin",
                NormalizedName = "CompanyAdmin".ToUpper()
            }
            );
            u.SaveChanges();
        }


        if (!u.Users.Any())
        {
            #region Superadmin
            var identityUser = new ApplicationUser();
            identityUser.UserName = "admin";
            PasswordHasher<IdentityUser> hasher = new PasswordHasher<IdentityUser>();
            identityUser.PasswordHash = hasher.HashPassword(identityUser, "123qwe");
            identityUser.Email = "admin@crm.com";
            identityUser.NormalizedEmail = "admin@crm.com".ToUpper();
            identityUser.NormalizedUserName = "admin".ToUpper();
            identityUser.SecurityStamp = Guid.NewGuid().ToString();
            identityUser.UserType = "superadmin";
            u.Users.Add(identityUser);

            u.SaveChanges();

            var superadminrole = u.Roles.FirstOrDefault(x => x.Name == "superadmin");
            if (superadminrole != null)
            {
                var role = new IdentityUserRole<string>();
                role.RoleId = superadminrole.Id;
                role.UserId = identityUser.Id;
                u.UserRoles.Add(role);
                u.SaveChanges();
            }
            #endregion
            #region System User
            var identityUser2 = new ApplicationUser();
            identityUser2.UserName = "systemuser";
            PasswordHasher<IdentityUser> hasher2 = new PasswordHasher<IdentityUser>();
            identityUser2.PasswordHash = hasher2.HashPassword(identityUser2, "123qwe");
            identityUser2.Email = "systemuser@crm.com";
            identityUser2.NormalizedEmail = "systemuser@crm.com".ToUpper();
            identityUser2.NormalizedUserName = "systemuser".ToUpper();
            identityUser2.SecurityStamp = Guid.NewGuid().ToString();
            identityUser2.UserType = UserTypeConstants.AdminUser;
            u.Users.Add(identityUser2);

            u.SaveChanges();

            var systemrole = u.Roles.FirstOrDefault(x => x.Name == "system");
            if (systemrole != null)
            {
                var role = new IdentityUserRole<string>();
                role.RoleId = systemrole.Id;
                role.UserId = identityUser2.Id;
                u.UserRoles.Add(role);
                u.SaveChanges();
            }
            #endregion
        }

        if (!u.AspNetPageMasters.Any())
        {
            u.AspNetPageMasters.AddRange(
                new AspNetPageMaster { PageName = "Country", RouteName = "/pages/admin/country", ClassName = "pi pi-globe", Sequence = 1, ParentId = 0, IsMaster = false, IsActive = false, IsDeleted = true },
                new AspNetPageMaster { PageName = "State", RouteName = "/pages/admin/state", ClassName = "pi pi-minus-circle", Sequence = 2, ParentId = 0, IsMaster = false, IsActive = false, IsDeleted = true },
                new AspNetPageMaster { PageName = "City", RouteName = "/pages/admin/city", ClassName = "pi pi-globe", Sequence = 3, ParentId = 0, IsMaster = false, IsActive = false, IsDeleted = true },
                new AspNetPageMaster { PageName = "Company", RouteName = "/pages/new-company-master", ClassName = "pi pi-sync", Sequence = 4, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "Role Page Permission", RouteName = "/pages/permission/page-permissions", ClassName = "pi pi-lock-open", Sequence = 5, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "User", RouteName = "/pages/permission/user-master", ClassName = "pi pi-user", Sequence = 6, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "Lead", RouteName = "/pages/lead", ClassName = "pi pi-users", Sequence = 7, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "Role", RouteName = "/pages/permission/user-role-management", ClassName = "pi pi-key", Sequence = 8, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "NBFC API", RouteName = "/pages/nbfc-url", ClassName = "pi pi-link", Sequence = 9, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "Account", RouteName = "/pages/loan-account/loanList", ClassName = "pi pi-wallet", Sequence = 10, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "Account Transaction", RouteName = "/pages/loan-account/transaction", ClassName = "pi pi-dollar", Sequence = 11, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "Invoices", RouteName = "/pages/new-company-master/invoices", ClassName = "pi pi-book", Sequence = 12, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "Template Master", RouteName = "/pages/admin/TemplateMaster", ClassName = "pi pi-book", Sequence = 13, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false },
                new AspNetPageMaster { PageName = "Invoice Master", RouteName = "/pages/invoice-master/invoice-list", ClassName = "pi pi-book", Sequence = 14, ParentId = 0, IsMaster = false, IsActive = true, IsDeleted = false }
                );
            u.SaveChanges();
        }
    }
}
catch (Exception ex)
{
    logger.Information(EnvironmentConstants.DbContext);
    throw ex;
}
#endregion

app.UseRouting();
app.UseHealthChecks("/health");

// app.UseHttpsRedirection();
app.UseRequestResponseLoggingMiddleware();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<IdentityGrpcService>();
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


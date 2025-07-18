using IdentityForge.Application;
using IdentityForge.Infrastructure;
using IdentityForge.Infrastructure.Data;
using IdentityForge.Web;
using IdentityForge.Web.Endpoints.V1;
using IdentityForge.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogWithDefaults();

builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddDefaultCors();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapVersionedEndpoints<IApiV1Endpoint>(new(1, 0));

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
    await app.InitialiseDatabaseAsync();
}

app.MapHealthChecks("api/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseRequestContextLogging();

app.UseSerilogRequestLoggingWithDefaults();

app.UseExceptionHandler();

app.UseDefaultCors();

app.UseAuthentication();

app.UseAuthorization();

await app.RunAsync();

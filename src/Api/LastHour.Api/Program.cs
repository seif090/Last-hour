using LastHour.Api.Caching.OutputCache;
using LastHour.Api.Compression;
using LastHour.Api.DependencyInjection;
using LastHour.Api.Endpoints;
using LastHour.Api.Middleware;
using LastHour.Api.OpenApi;
using LastHour.Api.RateLimiting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: null)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.AddLastHourApi(builder.Configuration);

    var app = builder.Build();

    app.UseLastHourExceptionMiddleware();

    if (app.Environment.IsDevelopment())
    {
        app.UseLastHourSwagger();
    }

    app.UseHttpsRedirection();
    app.UseLastHourResponseCompression();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseLastHourRateLimiting();
    app.UseLastHourOutputCache();

    app.MapLastHourEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "LastHour.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
}

using ServerControl;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IOMessageBroker>();

builder.Services.AddCors(options =>
{
    var corsConfig = builder.Configuration.GetSection("Cors:AllowAll");
    var origins = (corsConfig["Origins"] ?? "localhost").Split(',', StringSplitOptions.RemoveEmptyEntries);
    
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue<int>("Port"));
});

var app = builder.Build();
app.UseRouting();

app.UseCors("AllowAll");

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<IOHub>("/ioHub");
});

app.Run();
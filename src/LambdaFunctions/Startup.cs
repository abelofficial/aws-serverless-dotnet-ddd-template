using System;
using System.IO;
using LambdaFunctions.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LambdaFunctions;

public class Startup
{
    public static IConfiguration Configuration
    {
        get; private set;
    }

    public static void Configure()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.InstallServicesFromAssembly(Configuration);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LambdaFunctions.Settings;
public class LambdaFunctionsExtensionsInstaller : IExtensionsInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration config)
    {
    }
}

public static class ExtensionsInstaller
{
    public static void InstallServicesFromAssembly(this IServiceCollection services, IConfiguration config)
    {
        var applicationTypes = Assembly.Load("Application").ExportedTypes;
        var persistenceTypes = Assembly.Load("Infrastructure").ExportedTypes;
        var domainTypes = Assembly.Load("Domain").ExportedTypes;

        InstallServices(applicationTypes, services, config);
        InstallServices(persistenceTypes, services, config);
        InstallServices(domainTypes, services, config);
        InstallServices(Assembly.GetExecutingAssembly().ExportedTypes, services, config);
    }

    private static void InstallServices(IEnumerable<Type> assembly, IServiceCollection services, IConfiguration config)
    {
        var i = assembly.Where(x =>
            typeof(IExtensionsInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
        .Select(Activator.CreateInstance)
        .Cast<IExtensionsInstaller>()
        .ToList();
        foreach (var installer in i)
        {
            installer.InstallServices(services, config);
        }
    }
}
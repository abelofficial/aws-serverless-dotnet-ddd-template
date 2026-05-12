using Application.Handlers;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;
public class Installer : IExtensionsInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<ISayHelloHandler, SayHelloHandler>();
    }
}

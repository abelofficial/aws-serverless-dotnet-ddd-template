using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Interfaces;

public interface IExtensionsInstaller
{
    void InstallServices(IServiceCollection services, IConfiguration config);
}
using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Domain.Interfaces;
using LambdaFunctions.Models;
using LambdaFunctions.Settings;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: LambdaSerializer(typeof(CustomLambdaSerializer))]
namespace LambdaFunctions.Functions;
public abstract class BaseFunctions
{
    private readonly IServiceProvider _serviceProvider;

    protected BaseFunctions()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        var services = new ServiceCollection();
        Startup.ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    protected async Task<Response<T>> HandleResponse<T>(Func<Task<T>> lambdaFunction) where T : ISuccessfulResponse
    {
        try
        {
            var result = await lambdaFunction();
            Log.Debug("Generated response successfully: {@Result}", result);
            return new Response<T> { Data = result, Error = null };
        }
        catch (Exception error)
        {
            Log.Error("UnknownException: {@Error}", error);
            return new Response<T>
            {
                Error = new ServiceExceptionResponse
                {
                    Status = HttpStatusCode.GetName(HttpStatusCode.InternalServerError),
                    Message = "An error occurred while processing your request."
                }
            };
        }
    }
}
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

    protected async Task<Response<TResponse>> HandleResponse<TRequest, TResponse>(TRequest request, ILambdaContext context, Func<TRequest, Task<TResponse>> lambdaFunction) where TResponse : IResponse where TRequest : IRequest
    {
        try
        {
            Log.Information("Processing request: {@Request}", request);
            var result = await lambdaFunction(request);
            Log.Debug("Generated response successfully: {@Result}", result);
            return new Response<TResponse> { Data = result, Error = null };
        }
        catch (Exception error)
        {
            Log.Error("UnknownException: {@Error}", error);
            return new Response<TResponse>
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
using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Application.CQRS;
using Application.Results;
using LambdaFunctions.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using IRequest = Application.CQRS.IRequest;

[assembly: LambdaSerializer(typeof(CustomLambdaSerializer))]
namespace LambdaFunctions.Functions;
public abstract class BaseFunctions
{
    private readonly IServiceProvider _serviceProvider;
    protected readonly IMediator _mediator;

    protected BaseFunctions()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        var services = new ServiceCollection();
        Startup.ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetService<IMediator>();
    }

    protected async Task<Response<TResponse>> HandleResponse<TRequest, TResponse>(TRequest request, ILambdaContext context, Func<TRequest, Task<TResponse>> lambdaFunction)
    where TResponse : IResponse
    where TRequest : IRequest
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
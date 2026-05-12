using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Application.CQRS;
using Application.Results;
using LambdaFunctions.Settings;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using IRequest = Application.CQRS.IRequest;

[assembly: LambdaSerializer(typeof(CustomLambdaSerializer))]
namespace LambdaFunctions.Functions;
public abstract class BaseFunctions
{
    protected readonly IServiceProvider ServiceProvider;
    private static readonly JsonSerializerOptions ApiJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Dictionary<string, string> DefaultApiHeaders = new()
    {
        { "Content-Type", "application/json" },
        { "Access-Control-Allow-Origin", "*" },
        { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
        { "Access-Control-Allow-Methods", "POST,OPTIONS" }
    };

    protected BaseFunctions()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        var services = new ServiceCollection();
        Startup.ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }

    // Direct Lambda Access
    protected async Task<Response<TResponse>> HandleResponse<TRequest, TResponse>(
        TRequest request,
        ILambdaContext context,
        Func<TRequest, Task<TResponse>> lambdaFunction,
        Action<ApiExceptionPolicyHandler> configureExceptionPolicy = null)
    where TResponse : IResponse
    where TRequest : IRequest
    {
        var exceptionPolicy = new ApiExceptionPolicyHandler()
            .HandleException<Exception>(HttpStatusCode.InternalServerError);
        configureExceptionPolicy?.Invoke(exceptionPolicy);

        try
        {
            Log.Information("Processing request: {@Request}", request);
            var result = await lambdaFunction(request);
            Log.Debug("Generated response successfully: {@Result}", result);
            return new Response<TResponse> { Data = result, Error = null };
        }
        catch (Exception error)
        {
            exceptionPolicy.TryResolve(error, out var httpStatusCode, out var message);
            Log.Error("HandledException ({StatusCode}): {@Error}", (int)httpStatusCode, error);

            return new Response<TResponse>
            {
                Error = new ServiceExceptionResponse
                {
                    Status = ((int)httpStatusCode).ToString(),
                    Message = message
                }
            };
        }
    }

    // New overload for API Gateway responses
    protected async Task<APIGatewayProxyResponse> HandleApiGatewayResponse<TRequest, TResponse>(
        TRequest request,
        ILambdaContext context,
        Func<TRequest, Task<TResponse>> lambdaFunction,
        Action<ApiExceptionPolicyHandler> configureExceptionPolicy = null)
    where TResponse : IResponse
    where TRequest : IRequest
    {
        var exceptionPolicy = new ApiExceptionPolicyHandler()
            .HandleException<Exception>(HttpStatusCode.InternalServerError);
        configureExceptionPolicy?.Invoke(exceptionPolicy);

        try
        {
            Log.Information("Processing request: {@Request}", request);
            var result = await lambdaFunction(request);
            Log.Debug("Generated response successfully: {@Result}", result);

            var response = new Response<TResponse> { Data = result, Error = null };
            return BuildApiGatewayResponse(HttpStatusCode.OK, response);
        }
        catch (Exception error)
        {
            exceptionPolicy.TryResolve(error, out var httpStatusCode, out var message);
            Log.Error("HandledException ({StatusCode}): {@Error}", (int)httpStatusCode, error);

            var errorResponse = new Response<TResponse>
            {
                Error = new ServiceExceptionResponse
                {
                    Status = ((int)httpStatusCode).ToString(),
                    Message = message
                }
            };

            return BuildApiGatewayResponse(httpStatusCode, errorResponse);
        }
    }

    private static APIGatewayProxyResponse BuildApiGatewayResponse<TResponse>(HttpStatusCode statusCode, Response<TResponse> response)
    where TResponse : IResponse
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)statusCode,
            Headers = DefaultApiHeaders,
            Body = JsonSerializer.Serialize(response, ApiJsonOptions)
        };
    }

    // Helper method to extract request from API Gateway
    protected T ExtractRequestFromApiGateway<T>(APIGatewayProxyRequest apiRequest) where T : class
    {
        if (string.IsNullOrEmpty(apiRequest.Body))
            return null;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Deserialize<T>(apiRequest.Body, jsonOptions);
    }
}

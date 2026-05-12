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
                    Status = ((int)HttpStatusCode.InternalServerError).ToString(),
                    Message = "An error occurred while processing your request."
                }
            };
        }
    }

    // New overload for API Gateway responses
    protected async Task<APIGatewayProxyResponse> HandleApiGatewayResponse<TRequest, TResponse>(TRequest request, ILambdaContext context, Func<TRequest, Task<TResponse>> lambdaFunction)
    where TResponse : IResponse
    where TRequest : IRequest
    {
        try
        {
            Log.Information("Processing request: {@Request}", request);
            var result = await lambdaFunction(request);
            Log.Debug("Generated response successfully: {@Result}", result);

            var response = new Response<TResponse> { Data = result, Error = null };
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                    { "Access-Control-Allow-Methods", "POST,OPTIONS" }
                },
                Body = JsonSerializer.Serialize(response, jsonOptions)
            };
        }
        catch (Exception error)
        {
            Log.Error("UnknownException: {@Error}", error);
            var errorResponse = new Response<TResponse>
            {
                Error = new ServiceExceptionResponse
                {
                    Status = ((int)HttpStatusCode.InternalServerError).ToString(),
                    Message = "An error occurred while processing your request."
                }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                    { "Access-Control-Allow-Methods", "POST,OPTIONS" }
                },
                Body = JsonSerializer.Serialize(errorResponse, jsonOptions)
            };
        }
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

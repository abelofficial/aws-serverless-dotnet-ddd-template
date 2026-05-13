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

[assembly: LambdaSerializer(typeof(CustomLambdaSerializer))]
namespace LambdaFunctions.Functions;
public abstract class BaseFunctions<TRequest, TResponse>
    where TResponse : IResponse
    where TRequest : IRequest
{
    protected readonly IServiceProvider ServiceProvider;
    private static readonly JsonSerializerOptions _apiJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Dictionary<string, string> _defaultApiHeaders = new()
    {
        { "Content-Type", "application/json" },
        { "Access-Control-Allow-Origin", "*" },
        { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
        { "Access-Control-Allow-Methods", "POST,OPTIONS" }
    };

    private static readonly JsonSerializerOptions _requestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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

    public virtual async Task<object> Handler(object lambdaRequest, ILambdaContext context)
    {
        var exceptionPolicy = new ApiExceptionPolicyHandler()
            .HandleException<Exception>(HttpStatusCode.InternalServerError);
        ConfigureExceptionPolicy(exceptionPolicy);

        var isApiGatewayRequest = TryGetApiGatewayRequest(lambdaRequest, out var apiGatewayRequest);
        var typedRequest = ParseRequest(lambdaRequest, apiGatewayRequest);

        try
        {
            Log.Information("Processing request: {@Request}", typedRequest);
            var result = await HandleRequest(typedRequest, context);
            Log.Debug("Generated response successfully: {@Result}", result);

            var response = new Response<TResponse> { Data = result, Error = null };
            return isApiGatewayRequest
                ? BuildApiGatewayResponse(HttpStatusCode.OK, response)
                : response;
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

            return isApiGatewayRequest
                ? BuildApiGatewayResponse(httpStatusCode, errorResponse)
                : errorResponse;
        }
    }

    protected abstract Task<TResponse> HandleRequest(TRequest request, ILambdaContext context);

    protected abstract void ConfigureExceptionPolicy(ApiExceptionPolicyHandler exceptionPolicy);

    private TRequest ParseRequest(object lambdaRequest, APIGatewayProxyRequest apiGatewayRequest)
    {
        if (apiGatewayRequest != null)
        {
            return ExtractRequestFromApiGateway(apiGatewayRequest);
        }

        if (lambdaRequest is TRequest directRequest)
        {
            return directRequest;
        }

        if (lambdaRequest is string payload)
        {
            return DeserializeRequest(payload);
        }

        if (lambdaRequest is JsonElement jsonElement)
        {
            return DeserializeRequest(jsonElement.GetRawText());
        }

        if (lambdaRequest == null)
        {
            return default;
        }

        var serializedPayload = JsonSerializer.Serialize(lambdaRequest, _requestJsonOptions);
        return DeserializeRequest(serializedPayload);
    }

    private static bool TryGetApiGatewayRequest(object lambdaRequest, out APIGatewayProxyRequest apiGatewayRequest)
    {
        if (lambdaRequest is APIGatewayProxyRequest typedRequest)
        {
            apiGatewayRequest = typedRequest;
            return true;
        }

        if (lambdaRequest is JsonElement jsonElement && IsApiGatewayPayload(jsonElement))
        {
            apiGatewayRequest = JsonSerializer.Deserialize<APIGatewayProxyRequest>(jsonElement.GetRawText(), _requestJsonOptions);
            return apiGatewayRequest != null;
        }

        apiGatewayRequest = null;
        return false;
    }

    private static bool IsApiGatewayPayload(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return jsonElement.TryGetProperty("httpMethod", out _)
               || jsonElement.TryGetProperty("requestContext", out _)
               || jsonElement.TryGetProperty("body", out _);
    }

    private static TRequest DeserializeRequest(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TRequest>(payload, _requestJsonOptions);
    }

    private static APIGatewayProxyResponse BuildApiGatewayResponse(HttpStatusCode statusCode, Response<TResponse> response)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)statusCode,
            Headers = _defaultApiHeaders,
            Body = JsonSerializer.Serialize(response, _apiJsonOptions)
        };
    }

    private TRequest ExtractRequestFromApiGateway(APIGatewayProxyRequest apiRequest)
    {
        if (string.IsNullOrEmpty(apiRequest.Body))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TRequest>(apiRequest.Body, _requestJsonOptions);
    }
}

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace LambdaFunctions.Settings;

public class CustomLambdaSerializer : ILambdaSerializer
{

    public T Deserialize<T>(Stream requestStream)
    {
        var policy = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        using var reader = new StreamReader(requestStream);
        var json = reader.ReadToEnd();

        // If it's an API Gateway request, handle the body extraction
        if (typeof(T) == typeof(APIGatewayProxyRequest))
        {
            var apiRequest = JsonSerializer.Deserialize<APIGatewayProxyRequest>(json, policy);

            // If there's a body, deserialize it to the actual request type
            if (!string.IsNullOrEmpty(apiRequest.Body))
            {
                // You can add logic here to determine the actual request type
                // For now, let's assume it's SayHelloRequest
                var bodyRequest = JsonSerializer.Deserialize<Application.Queries.SayHelloRequest>(apiRequest.Body, policy);
                // You could set this on the API request or handle it differently
            }

            return (T)(object)apiRequest;
        }

        return JsonSerializer.Deserialize<T>(json, policy);
    }

    public void Serialize<T>(T response, Stream responseStream)
    {
        var policy = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(response, policy);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        responseStream.Write(bytes, 0, bytes.Length);
    }

}
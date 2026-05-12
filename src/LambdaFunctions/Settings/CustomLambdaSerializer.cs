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
        using var reader = new StreamReader(requestStream);
        var json = reader.ReadToEnd();
        var policy = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        return JsonSerializer.Deserialize<T>(json, policy);
    }

    public void Serialize<T>(T response, Stream responseStream)
    {
        var policy = new JsonSerializerOptions
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

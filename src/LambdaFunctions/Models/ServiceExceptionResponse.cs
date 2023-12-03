using System.Collections.Generic;
using System.Text.Json.Serialization;
using Domain.Interfaces;

namespace LambdaFunctions.Models;

public class ServiceExceptionResponse : IServiceExceptionResponse
{
    [JsonPropertyName("status")]
    public string Status
    {
        get; set;
    }
    [JsonPropertyName("message")]
    public string Message
    {
        get; set;
    }
    [JsonPropertyName("errors")]
    public List<string> Errors
    {
        get; set;
    }
}
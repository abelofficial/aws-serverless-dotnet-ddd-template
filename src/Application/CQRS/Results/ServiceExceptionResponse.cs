using System.Text.Json.Serialization;
using Domain.Interfaces;

namespace Application.Results;

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
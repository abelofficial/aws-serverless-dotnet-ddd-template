using Application.CQRS;

namespace Application.Results;

public class SayHelloResponse : IResponse
{
    public string Message
    {
        get;
        set;
    }
}
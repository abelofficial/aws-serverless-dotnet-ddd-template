using Application.CQRS;

namespace Application.Queries;

public class SayHelloRequest : IRequest
{
    public string Name
    {
        get;
        set;
    }
}
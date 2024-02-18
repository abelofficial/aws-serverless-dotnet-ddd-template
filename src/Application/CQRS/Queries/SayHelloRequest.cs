using Application.CQRS;

using ICQRSRequest = MediatR.IRequest<Application.Results.SayHelloResponse>;

namespace Application.Queries;

public class SayHelloRequest : IRequest, ICQRSRequest
{
    public string Name
    {
        get;
        set;
    }
}
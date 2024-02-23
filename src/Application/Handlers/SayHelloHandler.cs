using Application.Queries;
using Application.Results;
using MediatR;

namespace Application.Handlers;

public class SayHelloHandler : IRequestHandler<SayHelloRequest, SayHelloResponse>
{
    public SayHelloHandler()
    {
    }

    public async Task<SayHelloResponse> Handle(SayHelloRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new SayHelloResponse() { Message = $"Hello there {request.Name}" });
    }
}
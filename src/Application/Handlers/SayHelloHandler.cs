using Application.Queries;
using Application.Results;
using Domain.Errors;

namespace Application.Handlers;

public interface ISayHelloHandler
{
    /// <summary>
    /// Handles the SayHelloRequest and returns a SayHelloResponse.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SayHelloResponse> Handle(SayHelloRequest request, CancellationToken cancellationToken);
}

public class SayHelloHandler: ISayHelloHandler
{
    public async Task<SayHelloResponse> Handle(SayHelloRequest request, CancellationToken cancellationToken)
    {
        if(request.Name == "error")
        {
            throw new InvalidOperationException("Name can't be 'error'");
        }
        return await Task.FromResult(new SayHelloResponse() { Message = $"Hello there {request.Name}, how are you?" });
    }
}

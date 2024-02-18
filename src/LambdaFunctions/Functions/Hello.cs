using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Application.Queries;
using Application.Results;

namespace LambdaFunctions.Functions;

public class Hello : BaseFunctions
{

    public Hello() : base()
    {

    }

    public async Task<Response<SayHelloResponse>> SayHello(SayHelloRequest request, ILambdaContext context)
    {
        return await HandleResponse(request, context, async (req) =>
        {
            return await _mediator.Send(request);
        });
    }
}


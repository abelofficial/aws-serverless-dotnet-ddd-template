using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Application.Queries;

namespace LambdaFunctions.Functions;

public class Hello : BaseFunctions
{

    public Hello() : base()
    {
    }

    public async Task<APIGatewayProxyResponse> SayHello(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var sayHelloRequest = ExtractRequestFromApiGateway<SayHelloRequest>(request);

        return await HandleApiGatewayResponse(sayHelloRequest, context, async (req) =>
        {
            return await _mediator.Send(req);
        });
    }
}


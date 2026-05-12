using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Application.Handlers;
using Application.Queries;

namespace LambdaFunctions.Functions;

public class Hello : BaseFunctions
{

    public async Task<APIGatewayProxyResponse> SayHello(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var sayHelloRequest = ExtractRequestFromApiGateway<SayHelloRequest>(request);

        var service = ServiceProvider.GetService(typeof(ISayHelloHandler)) as ISayHelloHandler;
        return await HandleApiGatewayResponse(sayHelloRequest, context, async (req) =>
        {
            if (service != null)
            {
                return await service.Handle(req, CancellationToken.None);
            }

            throw new Exception("Service not found");
        });
    }
}


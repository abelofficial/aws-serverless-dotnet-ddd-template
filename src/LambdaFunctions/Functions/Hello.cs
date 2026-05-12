using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Application.CQRS;
using Application.Handlers;
using Application.Queries;
using Application.Results;
using Domain.Errors;
using LambdaFunctions.Exceptions;

namespace LambdaFunctions.Functions;

public class Hello : BaseFunctions
{

    public async Task<Response<SayHelloResponse>> SayHelloDirect(SayHelloRequest request, ILambdaContext context)
    {
        var service = ServiceProvider.GetService(typeof(ISayHelloHandler)) as ISayHelloHandler;
        return await HandleResponse(request, context, async (req) =>
        {
            if (req == null)
            {
                throw new BadRequestException("Request body is required.");
            }

            if (service != null)
            {
                return await service.Handle(req, CancellationToken.None);
            }

            throw new NotFoundException("Service not found");
        }, policy =>
        {
            policy.HandleException<BadRequestException>(HttpStatusCode.BadRequest);
            policy.HandleException<InvalidOperationException>(HttpStatusCode.BadRequest);
            policy.HandleException<NotFoundException>(HttpStatusCode.NotFound);
        });
    }

    public async Task<APIGatewayProxyResponse> SayHello(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var sayHelloRequest = ExtractRequestFromApiGateway<SayHelloRequest>(request);

        var service = ServiceProvider.GetService(typeof(ISayHelloHandler)) as ISayHelloHandler;
        return await HandleApiGatewayResponse(sayHelloRequest, context, async (req) =>
        {
            if (req == null)
            {
                throw new BadRequestException("Request body is required.");
            }

            if (service != null)
            {
                return await service.Handle(req, CancellationToken.None);
            }

            throw new NotFoundException("Service not found");
        }, policy =>
        {
            policy.HandleException<BadRequestException>(HttpStatusCode.BadRequest, "Bad Request");
            policy.HandleException<InvalidOperationException>(HttpStatusCode.BadRequest);
            policy.HandleException<NotFoundException>(HttpStatusCode.NotFound);
        });
    }
}


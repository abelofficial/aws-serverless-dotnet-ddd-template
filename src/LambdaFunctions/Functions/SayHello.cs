using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Application.Handlers;
using Application.Queries;
using Application.Results;
using Domain.Errors;
using LambdaFunctions.Exceptions;

namespace LambdaFunctions.Functions;

public class SayHello : BaseFunctions<SayHelloRequest, SayHelloResponse>
{
    public override Task<object> Handler(object request, ILambdaContext context)
    {
        return base.Handler(request, context);
    }

    protected override async Task<SayHelloResponse> HandleRequest(SayHelloRequest request, ILambdaContext context)
    {
        var service = ServiceProvider.GetService(typeof(ISayHelloHandler)) as ISayHelloHandler;

        if (request == null)
        {
            throw new BadRequestException("Request body is required.");
        }

        if (service != null)
        {
            return await service.Handle(request, CancellationToken.None);
        }

        throw new NotFoundException("Service not found");
    }

    protected override void ConfigureExceptionPolicy(ApiExceptionPolicyHandler policy)
    {
        policy.HandleException<BadRequestException>(HttpStatusCode.BadRequest, "Bad Request");
        policy.HandleException<InvalidOperationException>(HttpStatusCode.BadRequest);
        policy.HandleException<NotFoundException>(HttpStatusCode.NotFound);
    }
}


using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Domain.Interfaces;
using LambdaFunctions.Models;
using Microsoft.Extensions.Configuration;
using Serilog;

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
            return await Task.FromResult(new SayHelloResponse() { Message = $"Hello there {req.Name}" });
        });
    }
}

public class SayHelloRequest : IRequest
{
    public string Name
    {
        get;
        set;
    }
}

public class SayHelloResponse : IResponse
{
    public SayHelloResponse()
    {
    }

    public string Message
    {
        get;
        set;
    }
}
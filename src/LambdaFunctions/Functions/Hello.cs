using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Domain.Interfaces;
using LambdaFunctions.Models;
using Serilog;

namespace LambdaFunctions.Functions;

public class Hello : BaseFunctions
{
    public Hello() : base()
    {
    }

    public async Task<Response<SayHelloResponse>> SayHello(SayHelloRequest request, ILambdaContext context)
    {
        Log.Information("Processing request: {@Request}", request);
        return await HandleResponse(async () =>
        {
            return await Task.FromResult(new SayHelloResponse() { Message = $"Hello there {request.Name}" });
        });
    }
}

public class SayHelloRequest
{
    public string Name
    {
        get;
        set;
    }
}

public class SayHelloResponse : ISuccessfulResponse
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
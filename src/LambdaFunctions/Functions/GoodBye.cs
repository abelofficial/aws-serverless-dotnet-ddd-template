using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Application.Queries;
using Application.Results;
using Serilog;

namespace LambdaFunctions.Functions;

public class GoodBye : BaseFunctions
{

    public GoodBye() : base()
    {
    }

    public async Task<Response<SayHelloResponse>> SayGoodBye(SayHelloRequest request, ILambdaContext context)
    {
        try
        {
            Log.Information("SayGoodBye called with request: {@Request}", request);

            return await HandleResponse(request, context, async (req) =>
            {
                Log.Information("Calling mediator with request: {@Request}", req);
                var result = await _mediator.Send(req);
                Log.Information("Mediator returned: {@Result}", result);
                return result;
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SayGoodBye");
            throw;
        }
    }
}


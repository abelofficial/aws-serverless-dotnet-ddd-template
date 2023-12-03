using Domain.Interfaces;

namespace LambdaFunctions.Models;

public class Response<T> where T : ISuccessfulResponse
{
    public IServiceExceptionResponse Error
    {
        get; set;
    }
    public T Data
    {
        get; set;
    }
}
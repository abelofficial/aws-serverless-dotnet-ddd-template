using Application.CQRS;
using Domain.Interfaces;

namespace Application.Results;

public class Response<T> where T : IResponse
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
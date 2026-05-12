using System;

namespace LambdaFunctions.Exceptions;

public sealed class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}


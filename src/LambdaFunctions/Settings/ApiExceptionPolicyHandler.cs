using System;
using System.Collections.Generic;
using System.Net;

namespace LambdaFunctions.Functions;

public sealed class ApiExceptionPolicyHandler
{
    private readonly Dictionary<Type, (HttpStatusCode StatusCode, string Message)> _mappings = new();

    public ApiExceptionPolicyHandler HandleException<TException>(HttpStatusCode statusCode, string message = null)
        where TException : Exception
    {
        _mappings[typeof(TException)] = (statusCode, message);
        return this;
    }

    public bool TryResolve(Exception exception, out HttpStatusCode statusCode, out string message)
    {
        var currentType = exception.GetType();
        while (currentType != null && typeof(Exception).IsAssignableFrom(currentType))
        {
            if (_mappings.TryGetValue(currentType, out var mapping))
            {
                statusCode = mapping.StatusCode;
                message = string.IsNullOrWhiteSpace(mapping.Message)
                    ? GetDefaultMessage(exception)
                    : mapping.Message;
                return true;
            }

            currentType = currentType.BaseType;
        }

        statusCode = HttpStatusCode.InternalServerError;
        message = GetDefaultMessage(exception);
        return false;
    }

    private static string GetDefaultMessage(Exception exception)
    {
        return string.IsNullOrWhiteSpace(exception.Message)
            ? "An error occurred while processing your request."
            : exception.Message;
    }
}


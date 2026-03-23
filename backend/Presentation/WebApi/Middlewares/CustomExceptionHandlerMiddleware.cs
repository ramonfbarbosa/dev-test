using Application.Common.Exceptions;
using Application.Common.Models;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebApi.Middlewares;

public class CustomExceptionHandlerMiddleware
{
    private readonly ILogger<CustomExceptionHandlerMiddleware> _log;
    private readonly RequestDelegate _next;

    public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _log.LogError(exception, exception.Message);
        var code = HttpStatusCode.InternalServerError;
        ResponseError result = null;
        switch (exception)
        {
            case ApiValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = new ResponseError
                {
                    Message = "Ocorreram erros de validação.",
                    Errors = validationException.Failures?.Select(x => new ResponseErrorItem
                    {
                        Key = x.Key,
                        Value = x.Value
                    })
                };
                break;
            case BadRequestException badRequestException:
                code = HttpStatusCode.BadRequest;
                result = new ResponseError { Message = badRequestException.Message };
                break;
            case NotFoundException notFoundException:
                code = HttpStatusCode.NotFound;
                result = new ResponseError { Message = notFoundException.Message };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        if (result == null)
        {
            result = new ResponseError { Message = exception.Message };
        }
        return context.Response.WriteAsync(result.ToJson());
    }
}

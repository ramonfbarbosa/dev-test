using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    private const string DefaultFailureMessage = "A operação não pôde ser concluída.";

    protected IMediator Mediator { get; }

    protected ApiControllerBase(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected IActionResult CreateResponse(
        string failureMessage = DefaultFailureMessage,
        int successStatusCode = StatusCodes.Status200OK,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        return CreateResponse(true, successStatusCode, failureMessage, failureStatusCode);
    }

    protected IActionResult CreateResponse<TResponse>(
        TResponse response,
        int successStatusCode = StatusCodes.Status200OK,
        string failureMessage = DefaultFailureMessage,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        if (!IsSuccessfulResponse(response))
        {
            return CreateFailureResponse(failureStatusCode, failureMessage);
        }
        return CreateSuccessResponse(response, successStatusCode);
    }

    protected IActionResult CreateResponse<TResponse>(
        TResponse response,
        Func<TResponse, bool> successPredicate,
        int successStatusCode = StatusCodes.Status200OK,
        string failureMessage = DefaultFailureMessage,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        if (successPredicate == null)
        {
            throw new ArgumentNullException(nameof(successPredicate));
        }
        if (!successPredicate(response))
        {
            return CreateFailureResponse(failureStatusCode, failureMessage);
        }
        return CreateResponse(response, successStatusCode, failureMessage, failureStatusCode);
    }

    private static bool IsSuccessfulResponse<TResponse>(TResponse response)
    {
        if (response is null)
        {
            return false;
        }
        if (response is bool success)
        {
            return success;
        }
        return response is not Guid id || id != Guid.Empty;
    }

    private IActionResult CreateSuccessResponse<TResponse>(TResponse response, int successStatusCode)
    {
        return successStatusCode switch
        {
            StatusCodes.Status200OK => response is Unit or bool ? Ok() : Ok(response),
            StatusCodes.Status202Accepted => response is Unit or bool ? Accepted() : Accepted(response),
            StatusCodes.Status204NoContent => NoContent(),
            >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices => response is Unit or bool
                ? StatusCode(successStatusCode)
                : StatusCode(successStatusCode, response),
            _ => throw new ArgumentOutOfRangeException(
                nameof(successStatusCode),
                successStatusCode,
                "O status de sucesso informado é inválido.")
        };
    }

    private IActionResult CreateFailureResponse(int failureStatusCode, string failureMessage)
    {
        var error = new ResponseError
        {
            Message = failureMessage
        };

        return failureStatusCode switch
        {
            StatusCodes.Status400BadRequest => BadRequest(error),
            StatusCodes.Status401Unauthorized => Unauthorized(error),
            StatusCodes.Status403Forbidden => StatusCode(StatusCodes.Status403Forbidden, error),
            StatusCodes.Status404NotFound => NotFound(error),
            StatusCodes.Status409Conflict => Conflict(error),
            StatusCodes.Status422UnprocessableEntity => UnprocessableEntity(error),
            >= StatusCodes.Status400BadRequest and < StatusCodes.Status500InternalServerError => StatusCode(failureStatusCode, error),
            _ => throw new ArgumentOutOfRangeException(
                nameof(failureStatusCode),
                failureStatusCode,
                "O status de falha informado é inválido.")
        };
    }
}

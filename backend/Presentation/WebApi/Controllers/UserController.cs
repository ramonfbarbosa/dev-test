using Application.Users.Commands.Create;
using Application.Users.Commands.ConfirmEmail;
using Application.Users.Commands.ResendConfirmationEmail;
using Application.Users.Commands.ToggleActive;
using Application.Users.Commands.Update;
using Application.Users.Queries.FilteredUsersQuery;
using Application.Users.Queries.UserByIdQuery;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UserController : ApiControllerBase
{
    public UserController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommandRequest request)
    {
        var userId = await Mediator.Send(request);
        return CreateResponse(new { id = userId });
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedList<FilteredUsersQueryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FilteredUsersQueryRequest request)
    {
        var response = await Mediator.Send(request);
        return CreateResponse(response);
    }

    [AllowAnonymous]
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(ConfirmUserEmailCommandResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmUserEmailCommandRequest request)
    {
        var response = await Mediator.Send(request);
        return CreateResponse(response);
    }

    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleActive([FromRoute] Guid id)
    {
        await Mediator.Send(new ToggleUserActiveCommandRequest { Id = id });
        return CreateResponse();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserByIdQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await Mediator.Send(new UserByIdQueryRequest { Id = id });
        return CreateResponse(response);
    }

    [HttpPost("{id}/resend-confirmation-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendConfirmationEmail([FromRoute] Guid id)
    {
        await Mediator.Send(new ResendConfirmationEmailCommandRequest { Id = id });
        return CreateResponse();
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserCommandRequest request)
    {
        request.Id = id;
        await Mediator.Send(request);
        return CreateResponse();
    }
}

using Application.Users.Commands.Login;
using Application.Users.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ApiControllerBase
{
    public LoginController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginCommandRequest command)
    {
        var response = await Mediator.Send(command);
        return CreateResponse(response);
    }
}

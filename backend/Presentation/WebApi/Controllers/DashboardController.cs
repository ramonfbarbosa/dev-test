using Application.Dashboard.Queries.DashboardQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ApiControllerBase
{
    public DashboardController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var response = await Mediator.Send(new DashboardQueryRequest());
        return CreateResponse(response);
    }
}

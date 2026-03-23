using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Clients.Commands.Import;
using Application.Clients.Queries.FilteredClientsQuery;
using Application.Clients.Queries.ClientByIdQuery;
using Application.Clients.Queries.ListClientImportsQuery;
using Application.Clients.Queries.GetClientImportErrorsQuery;
using Application.Clients.Commands.Delete;
using Application.Clients.Commands.Update;
using Application.Clients.Commands.Create;
using Application.Clients.Queries.ExportClientsQuery;
using Application.Common.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientController : ApiControllerBase
{
    public ClientController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateClientCommandRequest request)
    {
        var clientId = await Mediator.Send(request);
        return CreateResponse(new { id = clientId });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateClientCommandRequest request)
    {
        request.Id = id;
        await Mediator.Send(request);
        return CreateResponse();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedList<FilteredClientsQueryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAll([FromQuery] FilteredClientsQueryRequest request)
    {
        var response = await Mediator.Send(request);
        return CreateResponse(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientByIdQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await Mediator.Send(new ClientByIdQueryRequest { Id = id });
        return CreateResponse(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteClientCommandRequest { Id = id });
        return CreateResponse();
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportClientsCommandResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name);

        var request = new ImportClientsCommandRequest
        {
            FileName = file?.FileName,
            FileSizeInBytes = file?.Length ?? 0,
            UploadedByUserId = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
            UploadedByUserName = userName ?? string.Empty
        };
        if (file is not null)
        {
            await using var fileStream = file.OpenReadStream();
            request.SetFileStream(fileStream);
            var response = await Mediator.Send(request, cancellationToken);
            return CreateResponse(response, successStatusCode: StatusCodes.Status202Accepted);
        }
        var emptyFileResponse = await Mediator.Send(request, cancellationToken);
        return CreateResponse(emptyFileResponse, successStatusCode: StatusCodes.Status202Accepted);
    }

    [HttpGet("imports")]
    [ProducesResponseType(typeof(PagedList<ListClientImportsQueryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListImports([FromQuery] ListClientImportsQueryRequest request)
    {
        var response = await Mediator.Send(request);
        return CreateResponse(response);
    }

    [HttpGet("imports/{id}/errors")]
    [ProducesResponseType(typeof(GetClientImportErrorsQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImportErrors([FromRoute] Guid id)
    {
        var response = await Mediator.Send(new GetClientImportErrorsQueryRequest { Id = id });
        return CreateResponse(response);
    }

    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Export()
    {
        var response = await Mediator.Send(new ExportClientsQueryRequest());
        return File(response.Content, response.ContentType, response.FileName);
    }
}

using Application.Common.Interfaces;
using Application.Common.Models;
using Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Queries.ListClientImportsQuery;

public class ListClientImportsQueryHandler : IRequestHandler<ListClientImportsQueryRequest, PagedList<ListClientImportsQueryResponse>>
{
    private readonly IClientControlContext _context;

    public ListClientImportsQueryHandler(IClientControlContext context)
    {
        _context = context;
    }

    public Task<PagedList<ListClientImportsQueryResponse>> Handle(
        ListClientImportsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var imports = _context.ClientImports
            .AsEnumerable()
            .Select(i =>
            {
                var effectiveStatus = ResolveEffectiveStatus(i.Status, i.FailureCount);
                return new ListClientImportsQueryResponse
                {
                    Id = i.Id,
                    OriginalFileName = i.OriginalFileName,
                    Status = effectiveStatus,
                    StatusText = GetStatusText(effectiveStatus),
                    UploadedByUserName = i.UploadedByUserName,
                    TotalRows = i.TotalRows,
                    ImportedRows = i.ImportedRows,
                    FailureCount = i.FailureCount,
                    ErrorMessage = i.ErrorMessage,
                    CreatedAt = i.CreatedAt,
                    StartedAt = i.StartedAt,
                    FinishedAt = i.FinishedAt,
                };
            })
            .ToList();

        var sorted = ApplySorting(imports, request.SortBy, request.SortDirection);
        var result = PagedList<ListClientImportsQueryResponse>.Create(sorted, request.Page, request.PageSize);
        return Task.FromResult(result);
    }

    private static string GetStatusText(ClientImportStatus status) => status switch
    {
        ClientImportStatus.Pending => "Pendente",
        ClientImportStatus.Processing => "Processando",
        ClientImportStatus.Processed => "Concluído",
        ClientImportStatus.Failed => "Falhou",
        ClientImportStatus.ProcessedWithErrors => "Concluído com erros",
        _ => status.ToString()
    };

    private static ClientImportStatus ResolveEffectiveStatus(ClientImportStatus status, int failureCount)
    {
        if (status == ClientImportStatus.Processed && failureCount > 0)
            return ClientImportStatus.ProcessedWithErrors;
        return status;
    }

    private static IEnumerable<ListClientImportsQueryResponse> ApplySorting(
        List<ListClientImportsQueryResponse> items, string sortBy, string sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLower() switch
        {
            "filename" or "originalfilename" => isDescending
                ? items.OrderByDescending(i => i.OriginalFileName)
                : items.OrderBy(i => i.OriginalFileName),
            "status" => isDescending
                ? items.OrderByDescending(i => i.Status)
                : items.OrderBy(i => i.Status),
            "uploadedby" or "uploadedbyusername" => isDescending
                ? items.OrderByDescending(i => i.UploadedByUserName)
                : items.OrderBy(i => i.UploadedByUserName),
            _ => isDescending
                ? items.OrderByDescending(i => i.CreatedAt)
                : items.OrderBy(i => i.CreatedAt),
        };
    }
}

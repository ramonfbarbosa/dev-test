using Domain;
using System;

namespace Application.Clients.Queries.ListClientImportsQuery;

public class ListClientImportsQueryResponse
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; }
    public ClientImportStatus Status { get; set; }
    public string StatusText { get; set; }
    public string UploadedByUserName { get; set; }
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int FailureCount { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}

using System.Collections.Generic;

namespace Application.Clients.Queries.GetClientImportErrorsQuery;

public class GetClientImportErrorsQueryResponse
{
    public string OriginalFileName { get; set; }
    public string StatusText { get; set; }
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int FailureCount { get; set; }
    public List<ImportErrorDetail> Errors { get; set; } = [];
}

public class ImportErrorDetail
{
    public int LineNumber { get; set; }
    public string Message { get; set; }
}

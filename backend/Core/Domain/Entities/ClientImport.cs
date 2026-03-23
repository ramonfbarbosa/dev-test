using System;

namespace Domain.Entities;

public class ClientImport : BaseEntity
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public ClientImportStatus Status { get; set; } = ClientImportStatus.Pending;
    public Guid UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int FailureCount { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string ErrorMessage { get; set; }

    public ClientImport() { }

    public ClientImport(string originalFileName, string storedFileName, Guid uploadedByUserId, string uploadedByUserName)
    {
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        UploadedByUserId = uploadedByUserId;
        UploadedByUserName = uploadedByUserName;
        Status = ClientImportStatus.Pending;
    }

    public void MarkAsProcessing()
    {
        Status = ClientImportStatus.Processing;
        StartedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed(int totalRows, int importedRows, int failureCount)
    {
        Status = failureCount > 0 ? ClientImportStatus.ProcessedWithErrors : ClientImportStatus.Processed;
        TotalRows = totalRows;
        ImportedRows = importedRows;
        FailureCount = failureCount;
        FinishedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(int totalRows, int importedRows, int failureCount, string errorMessage)
    {
        Status = ClientImportStatus.Failed;
        TotalRows = totalRows;
        ImportedRows = importedRows;
        FailureCount = failureCount;
        ErrorMessage = errorMessage;
        FinishedAt = DateTime.UtcNow;
    }
}

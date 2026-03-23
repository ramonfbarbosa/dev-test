using System;
using System.Collections.Generic;

namespace Application.Clients.Imports.Models;

public class ClientImportSummary
{
    public string FileName { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime FinishedAtUtc { get; set; }
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public List<ClientImportFailure> Failures { get; set; } = [];

    public static ClientImportSummary Create(string fileName)
    {
        return new ClientImportSummary
        {
            FileName = fileName,
            StartedAtUtc = DateTime.UtcNow
        };
    }

    public void RegisterParseFailure(ClientImportFailure failure)
    {
        TotalRows++;
        Failures.Add(failure);
    }

    public void RegisterRowAttempt()
    {
        TotalRows++;
    }

    public void RegisterImportedRow()
    {
        ImportedRows++;
    }

    public void AddFailure(int lineNumber, string message)
    {
        Failures.Add(new ClientImportFailure
        {
            LineNumber = lineNumber,
            Message = message ?? string.Empty
        });
    }

    public void Finish()
    {
        FinishedAtUtc = DateTime.UtcNow;
    }
}

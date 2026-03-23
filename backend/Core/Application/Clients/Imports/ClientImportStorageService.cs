using Application.Clients.Imports.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Imports;

public class ClientImportStorageService
{
    private readonly IHostEnvironment _environment;
    private readonly ClientImportOptions _options;
    public long MaxFileSizeInBytes => _options.MaxFileSizeInBytes;
    public string PendingDirectoryPath => Path.Combine(RootDirectoryPath, _options.PendingDirectoryName);
    public string ProcessingDirectoryPath => Path.Combine(RootDirectoryPath, _options.ProcessingDirectoryName);
    public string ProcessedDirectoryPath => Path.Combine(RootDirectoryPath, _options.ProcessedDirectoryName);
    public string FailedDirectoryPath => Path.Combine(RootDirectoryPath, _options.FailedDirectoryName);
    private string RootDirectoryPath => Path.Combine(_environment.ContentRootPath, _options.RootDirectory);

    public ClientImportStorageService(IHostEnvironment environment, IOptions<ClientImportOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(RootDirectoryPath);
        Directory.CreateDirectory(PendingDirectoryPath);
        Directory.CreateDirectory(ProcessingDirectoryPath);
        Directory.CreateDirectory(ProcessedDirectoryPath);
        Directory.CreateDirectory(FailedDirectoryPath);
    }

    public IEnumerable<string> GetFilesAwaitingProcessing()
    {
        EnsureDirectories();
        return Directory
            .EnumerateFiles(PendingDirectoryPath, "*.csv")
            .Concat(Directory.EnumerateFiles(ProcessingDirectoryPath, "*.csv"))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string> SavePendingFileAsync(Stream inputStream, string fileName, CancellationToken cancellationToken)
    {
        EnsureDirectories();
        var extension = Path.GetExtension(fileName);
        var safeFileName = $"client-import-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(PendingDirectoryPath, safeFileName);
        if (inputStream.CanSeek)
        {
            inputStream.Position = 0;
        }
        await using var destinationStream = File.Create(destinationPath);
        await inputStream.CopyToAsync(destinationStream, cancellationToken);
        return destinationPath;
    }

    public string MoveToProcessing(string filePath)
    {
        EnsureDirectories();
        return MoveToDirectory(filePath, ProcessingDirectoryPath);
    }

    public string MoveToProcessed(string filePath)
    {
        EnsureDirectories();
        return MoveToDirectory(filePath, ProcessedDirectoryPath);
    }

    public string MoveToFailed(string filePath)
    {
        EnsureDirectories();
        return MoveToDirectory(filePath, FailedDirectoryPath);
    }

    public async Task WriteSummaryAsync(string importedFilePath, ClientImportSummary summary, CancellationToken cancellationToken)
    {
        var summaryPath = Path.ChangeExtension(importedFilePath, ".result.json");
        var summaryJson = JsonSerializer.Serialize(summary, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(summaryPath, summaryJson, cancellationToken);
    }

    private static string MoveToDirectory(string filePath, string targetDirectoryPath)
    {
        var currentDirectory = Path.GetDirectoryName(filePath);
        if (string.Equals(currentDirectory, targetDirectoryPath, StringComparison.OrdinalIgnoreCase))
        {
            return filePath;
        }
        var destinationPath = Path.Combine(targetDirectoryPath, Path.GetFileName(filePath));
        File.Move(filePath, destinationPath, true);
        return destinationPath;
    }
}

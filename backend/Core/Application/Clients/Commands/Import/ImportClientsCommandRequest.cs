using MediatR;
using System;
using System.IO;

namespace Application.Clients.Commands.Import;

public class ImportClientsCommandRequest : IRequest<ImportClientsCommandResponse>
{
    public string FileName { get; set; }
    public long FileSizeInBytes { get; set; }
    public Stream FileStream { get; private set; }
    public Guid UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; }

    public void SetFileStream(Stream fileStream)
    {
        FileStream = fileStream;
    }
}

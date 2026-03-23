namespace Application.Clients.Imports.Models;

public class ClientImportFailure
{
    public int LineNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

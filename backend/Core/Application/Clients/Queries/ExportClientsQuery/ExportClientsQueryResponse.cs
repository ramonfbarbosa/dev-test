namespace Application.Clients.Queries.ExportClientsQuery;

public class ExportClientsQueryResponse
{
    public byte[] Content { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; } = "text/csv";
}

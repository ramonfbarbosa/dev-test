using Application.Common.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Clients.Queries.ExportClientsQuery;

public class ExportClientsQueryHandler : IRequestHandler<ExportClientsQueryRequest, ExportClientsQueryResponse>
{
    private readonly IClientControlContext _context;

    public ExportClientsQueryHandler(IClientControlContext context)
    {
        _context = context;
    }

    public Task<ExportClientsQueryResponse> Handle(ExportClientsQueryRequest request, CancellationToken cancellationToken)
    {
        var clients = _context.Clients
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToList();
        var csv = new StringBuilder();
        csv.AppendLine("Nome,Sobrenome,Email,Telefone,Documento,Data de Nascimento,CEP,Rua,Número,Complemento,Bairro,Cidade,Estado");
        foreach (var client in clients)
        {
            csv.AppendLine(string.Join(",",
                Escape(client.FirstName),
                Escape(client.LastName),
                Escape(client.Email),
                Escape(client.PhoneNumber),
                Escape(client.DocumentNumber),
                client.BirthDate.ToString("dd/MM/yyyy"),
                Escape(client.Address?.PostalCode),
                Escape(client.Address?.AddressLine),
                Escape(client.Address?.Number),
                Escape(client.Address?.Complement),
                Escape(client.Address?.Neighborhood),
                Escape(client.Address?.City),
                Escape(client.Address?.State)
            ));
        }
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return Task.FromResult(new ExportClientsQueryResponse
        {
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),
            FileName = $"clientes_{timestamp}.csv"
        });
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}

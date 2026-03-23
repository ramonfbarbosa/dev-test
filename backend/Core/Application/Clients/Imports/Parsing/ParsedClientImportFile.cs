using Application.Clients.Imports.Models;
using System.Collections.Generic;

namespace Application.Clients.Imports.Parsing;

public class ParsedClientImportFile
{
    public IReadOnlyCollection<ParsedClientImportRow> Rows { get; }
    public IReadOnlyCollection<ClientImportFailure> ParseFailures { get; }

    public ParsedClientImportFile(
        IReadOnlyCollection<ParsedClientImportRow> rows,
        IReadOnlyCollection<ClientImportFailure> parseFailures)
    {
        Rows = rows;
        ParseFailures = parseFailures;
    }
}

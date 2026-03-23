using System.Collections.Generic;

namespace Application.Clients.Imports.Parsing;

public class ParsedClientImportRow
{
    public int LineNumber { get; }
    private readonly string[] _fields;
    private readonly IReadOnlyDictionary<string, int> _headerMap;

    public ParsedClientImportRow(int lineNumber, string[] fields, IReadOnlyDictionary<string, int> headerMap)
    {
        LineNumber = lineNumber;
        _fields = fields;
        _headerMap = headerMap;
    }

    public string GetValue(string columnName)
    {
        if (!_headerMap.TryGetValue(columnName, out var index))
        {
            return string.Empty;
        }
        if (index < 0 || index >= _fields.Length)
        {
            return string.Empty;
        }
        return _fields[index]?.Trim() ?? string.Empty;
    }
}

using Application.Clients.Imports.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Application.Clients.Imports.Parsing;

public class ClientImportCsvParser
{
    private static readonly IReadOnlyDictionary<string, string[]> ColumnAliases = new Dictionary<string, string[]>
    {
        ["firstName"] = ["firstname", "first_name"],
        ["lastName"] = ["lastname", "last_name"],
        ["phoneNumber"] = ["phonenumber", "phone_number"],
        ["email"] = ["email", "e-mail"],
        ["documentNumber"] = ["documentnumber", "document_number", "document", "cpf"],
        ["birthDate"] = ["birthdate", "birth_date", "dataNascimento", "datanascimento"],
        ["postalCode"] = ["postalcode", "postal_code", "cep", "address.postalcode"],
        ["addressLine"] = ["addressline", "address_line", "logradouro", "endereco", "address.addressline"],
        ["number"] = ["number", "numero", "address.number"],
        ["complement"] = ["complement", "complemento", "address.complement"],
        ["neighborhood"] = ["neighborhood", "bairro", "address.neighborhood"],
        ["city"] = ["city", "cidade", "address.city"],
        ["state"] = ["state", "estado", "uf", "address.state"]
    };

    private static readonly string[] RequiredColumns =
    [
        "firstName",
        "lastName",
        "phoneNumber",
        "email",
        "documentNumber",
        "birthDate",
        "postalCode",
        "addressLine",
        "number",
        "neighborhood",
        "city",
        "state"
    ];

    public ParsedClientImportFile Parse(string filePath)
    {
        var delimiter = DetectDelimiter(filePath);
        using var parser = CreateParser(filePath, delimiter);
        var headers = ReadHeaders(parser);
        var headerMap = BuildHeaderMap(headers);
        ValidateRequiredColumns(headerMap);
        return ReadRows(parser, headerMap);
    }

    private static TextFieldParser CreateParser(string filePath, string delimiter)
    {
        var parser = new TextFieldParser(filePath, Encoding.UTF8)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true
        };
        parser.SetDelimiters(delimiter);
        return parser;
    }

    private static string[] ReadHeaders(TextFieldParser parser)
    {
        if (parser.EndOfData)
        {
            throw new InvalidDataException("O arquivo CSV está vazio.");
        }
        var headers = parser.ReadFields();
        if (headers == null || headers.Length == 0)
        {
            throw new InvalidDataException("Não foi possível identificar o cabeçalho do CSV.");
        }
        return headers;
    }

    private static ParsedClientImportFile ReadRows(TextFieldParser parser, IReadOnlyDictionary<string, int> headerMap)
    {
        var rows = new List<ParsedClientImportRow>();
        var parseFailures = new List<ClientImportFailure>();
        var lineNumber = 1;
        while (!parser.EndOfData)
        {
            lineNumber++;

            try
            {
                var fields = parser.ReadFields();
                if (fields == null || fields.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }
                rows.Add(new ParsedClientImportRow(lineNumber, fields, headerMap));
            }
            catch (MalformedLineException ex)
            {
                parseFailures.Add(new ClientImportFailure
                {
                    LineNumber = lineNumber,
                    Message = ex.Message
                });
            }
        }
        return new ParsedClientImportFile(rows, parseFailures);
    }

    private static string DetectDelimiter(string filePath)
    {
        var firstLine = File.ReadLines(filePath)
            .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
        if (string.IsNullOrWhiteSpace(firstLine))
        {
            return ",";
        }
        var commaCount = firstLine.Count(character => character == ',');
        var semicolonCount = firstLine.Count(character => character == ';');
        return semicolonCount > commaCount ? ";" : ",";
    }

    private static Dictionary<string, int> BuildHeaderMap(string[] headers)
    {
        var normalizedHeaders = headers
            .Select((header, index) => new { Header = NormalizeHeader(header), Index = index })
            .Where(item => !string.IsNullOrWhiteSpace(item.Header))
            .GroupBy(item => item.Header)
            .ToDictionary(group => group.Key, group => group.First().Index, StringComparer.OrdinalIgnoreCase);
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in ColumnAliases)
        {
            var index = FindHeaderIndex(normalizedHeaders, column.Value);
            if (index.HasValue)
            {
                headerMap[column.Key] = index.Value;
            }
        }
        return headerMap;
    }

    private static int? FindHeaderIndex(IReadOnlyDictionary<string, int> normalizedHeaders, IEnumerable<string> aliases)
    {
        foreach (var alias in aliases)
        {
            if (normalizedHeaders.TryGetValue(NormalizeHeader(alias), out var columnIndex))
            {
                return columnIndex;
            }
        }
        return null;
    }

    private static void ValidateRequiredColumns(IReadOnlyDictionary<string, int> headerMap)
    {
        var missingColumns = RequiredColumns
            .Where(column => !headerMap.ContainsKey(column))
            .ToArray();
        if (missingColumns.Length > 0)
        {
            throw new InvalidDataException($"O CSV não possui todas as colunas obrigatórias. Ausentes: {string.Join(", ", missingColumns)}.");
        }
    }

    private static string NormalizeHeader(string header)
    {
        return (header ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", string.Empty);
    }
}

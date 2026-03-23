using Application.Clients.Commands.Create;
using Application.Clients.Imports.Parsing;
using Application.Clients.Models;
using System;
using System.Globalization;

namespace Application.Clients.Imports;

public class ClientImportRequestFactory
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly string[] SupportedBirthDateFormats =
    [
        "dd/MM/yyyy",
        "d/M/yyyy",
        "dd-MM-yyyy",
        "d-M-yyyy",
        "yyyy-MM-dd",
        "yyyy/MM/dd"
    ];

    public bool TryCreate(ParsedClientImportRow row, out CreateClientCommandRequest request, out string errorMessage)
    {
        request = null;
        errorMessage = string.Empty;
        if (!TryParseBirthDate(row.GetValue("birthDate"), out var birthDate))
        {
            errorMessage = "Data de nascimento inválida. Use formatos como dd/MM/yyyy ou yyyy-MM-dd.";
            return false;
        }
        request = new CreateClientCommandRequest
        {
            FirstName = row.GetValue("firstName"),
            LastName = row.GetValue("lastName"),
            PhoneNumber = row.GetValue("phoneNumber"),
            Email = row.GetValue("email"),
            DocumentNumber = row.GetValue("documentNumber"),
            BirthDate = birthDate,
            Address = new AddressModel
            {
                PostalCode = row.GetValue("postalCode"),
                AddressLine = row.GetValue("addressLine"),
                Number = row.GetValue("number"),
                Complement = row.GetValue("complement"),
                Neighborhood = row.GetValue("neighborhood"),
                City = row.GetValue("city"),
                State = row.GetValue("state")
            }
        };
        return true;
    }

    private static bool TryParseBirthDate(string value, out DateTime birthDate)
    {
        if (DateTime.TryParseExact(value, SupportedBirthDateFormats, PtBrCulture, DateTimeStyles.None, out birthDate))
        {
            birthDate = birthDate.Date;
            return true;
        }
        if (DateTime.TryParseExact(value, SupportedBirthDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
        {
            birthDate = birthDate.Date;
            return true;
        }
        if (DateTime.TryParse(value, PtBrCulture, DateTimeStyles.None, out birthDate))
        {
            birthDate = birthDate.Date;
            return true;
        }
        return false;
    }
}

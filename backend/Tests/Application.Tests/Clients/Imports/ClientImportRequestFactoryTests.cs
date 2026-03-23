using Application.Clients.Imports;
using Application.Clients.Imports.Parsing;
using System;
using System.Collections.Generic;

namespace Application.Tests.Clients.Imports;

public class ClientImportRequestFactoryTests
{
    private readonly ClientImportRequestFactory _factory = new();

    [Theory]
    [InlineData("15/03/1990")]
    [InlineData("1990-03-15")]
    [InlineData("15-03-1990")]
    [InlineData("1990/03/15")]
    public void TryCreate_WhenBirthDateIsValid_ReturnsTrue(string birthDateValue)
    {
        var headerMap = BuildFullHeaderMap();
        var fields = BuildFields(headerMap, birthDate: birthDateValue);
        var row = new ParsedClientImportRow(2, fields, headerMap);

        var success = _factory.TryCreate(row, out var request, out var error);

        Assert.True(success);
        Assert.NotNull(request);
        Assert.Equal(string.Empty, error);
        Assert.Equal(new DateTime(1990, 3, 15), request.BirthDate);
    }

    [Fact]
    public void TryCreate_WhenBirthDateIsInvalid_ReturnsFalse()
    {
        var headerMap = BuildFullHeaderMap();
        var fields = BuildFields(headerMap, birthDate: "not-a-date");
        var row = new ParsedClientImportRow(2, fields, headerMap);

        var success = _factory.TryCreate(row, out var request, out var error);

        Assert.False(success);
        Assert.Null(request);
        Assert.Contains("Data de nascimento inválida", error);
    }

    [Fact]
    public void TryCreate_MapsAllFieldsCorrectly()
    {
        var headerMap = BuildFullHeaderMap();
        var fields = BuildFields(headerMap);
        var row = new ParsedClientImportRow(2, fields, headerMap);

        var success = _factory.TryCreate(row, out var request, out _);

        Assert.True(success);
        Assert.Equal("Maria", request.FirstName);
        Assert.Equal("Silva", request.LastName);
        Assert.Equal("11999990000", request.PhoneNumber);
        Assert.Equal("maria@test.local", request.Email);
        Assert.Equal("12345678900", request.DocumentNumber);
        Assert.Equal("01001000", request.Address.PostalCode);
        Assert.Equal("Rua X", request.Address.AddressLine);
        Assert.Equal("10", request.Address.Number);
        Assert.Equal("Apto 1", request.Address.Complement);
        Assert.Equal("Centro", request.Address.Neighborhood);
        Assert.Equal("São Paulo", request.Address.City);
        Assert.Equal("SP", request.Address.State);
    }

    private static Dictionary<string, int> BuildFullHeaderMap() => new()
    {
        ["firstName"] = 0,
        ["lastName"] = 1,
        ["phoneNumber"] = 2,
        ["email"] = 3,
        ["documentNumber"] = 4,
        ["birthDate"] = 5,
        ["postalCode"] = 6,
        ["addressLine"] = 7,
        ["number"] = 8,
        ["complement"] = 9,
        ["neighborhood"] = 10,
        ["city"] = 11,
        ["state"] = 12
    };

    private static string[] BuildFields(Dictionary<string, int> headerMap, string birthDate = "15/03/1990")
    {
        var fields = new string[13];
        fields[headerMap["firstName"]] = "Maria";
        fields[headerMap["lastName"]] = "Silva";
        fields[headerMap["phoneNumber"]] = "11999990000";
        fields[headerMap["email"]] = "maria@test.local";
        fields[headerMap["documentNumber"]] = "12345678900";
        fields[headerMap["birthDate"]] = birthDate;
        fields[headerMap["postalCode"]] = "01001000";
        fields[headerMap["addressLine"]] = "Rua X";
        fields[headerMap["number"]] = "10";
        fields[headerMap["complement"]] = "Apto 1";
        fields[headerMap["neighborhood"]] = "Centro";
        fields[headerMap["city"]] = "São Paulo";
        fields[headerMap["state"]] = "SP";
        return fields;
    }
}

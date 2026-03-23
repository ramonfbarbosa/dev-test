using System;

namespace Domain.Entities;

public class Client : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Email { get; private set; }
    public string DocumentNumber { get; private set; }
    public DateTime BirthDate { get; private set; }
    public Address Address { get; set; }

    public Client() { }

    public Client(string firstName, string lastName, string phoneNumber, string email, string documentNumber, DateTime birthdate, Address address)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
        BirthDate = birthdate;
        DocumentNumber = documentNumber;
        Address = address;
    }

    public void Update(string firstName, string lastName, string phoneNumber, string email, string documentNumber, DateTime birthdate, Address address)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
        DocumentNumber = documentNumber;
        BirthDate = birthdate;
        Address = address;
    }
}

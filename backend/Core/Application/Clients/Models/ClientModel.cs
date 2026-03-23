using System;

namespace Application.Clients.Models
{
    public class ClientModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public AddressModel Address { get; set; }
    }
}

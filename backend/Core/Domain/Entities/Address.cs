namespace Domain.Entities
{
    public class Address
    {
        public string PostalCode { get; private set; }
        public string AddressLine { get; private set; }
        public string Number { get; private set; }
        public string Complement { get; private set; }
        public string Neighborhood { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }

        public Address(string postalCode, string addressLine, string number, string complement, string neighborhood, string city, string state)
        {
            PostalCode = postalCode;
            AddressLine = addressLine;
            Number = number;
            Complement = complement;
            Neighborhood = neighborhood;
            City = city;
            State = state;
        }
    }
}

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ClientEntityTypeConfiguration : BaseEntityTypeConfiguration<Client>
{
    public override void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable(nameof(Client));
        builder.Property(b => b.FirstName).IsRequired().HasColumnType("varchar(100)");
        builder.Property(b => b.LastName).IsRequired().HasColumnType("varchar(100)");
        builder.Property(b => b.PhoneNumber).IsRequired().HasColumnType("varchar(15)");
        builder.Property(b => b.DocumentNumber).IsRequired().HasColumnType("varchar(20)");
        builder.Property(b => b.Email).IsRequired().HasColumnType("varchar(255)");
        builder.Property(b => b.BirthDate).IsRequired().HasColumnType("datetime(6)");
        builder.OwnsOne(c => c.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.PostalCode).IsRequired().HasColumnType("varchar(10)");
            addressBuilder.Property(a => a.AddressLine).IsRequired().HasColumnType("varchar(200)");
            addressBuilder.Property(a => a.Number).IsRequired().HasColumnType("varchar(10)");
            addressBuilder.Property(a => a.Complement).HasColumnType("varchar(100)");
            addressBuilder.Property(a => a.Neighborhood).IsRequired().HasColumnType("varchar(100)");
            addressBuilder.Property(a => a.City).IsRequired().HasColumnType("varchar(100)");
            addressBuilder.Property(a => a.State).IsRequired().HasColumnType("varchar(2)");
        });
    }
}

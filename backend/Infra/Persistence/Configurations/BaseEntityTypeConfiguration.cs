using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Persistence.Configurations;

public abstract class BaseEntityTypeConfiguration<TBase> : IEntityTypeConfiguration<TBase> where TBase : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TBase> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property<Guid>(b => b.Id)
            .ValueGeneratedOnAdd()
            .IsRequired(true);

        builder.Property<DateTime>(b => b.CreatedAt)
           .UsePropertyAccessMode(PropertyAccessMode.Field)
           .IsRequired(true);

        builder.Property<DateTime?>(b => b.ModifiedAt)
           .UsePropertyAccessMode(PropertyAccessMode.Field)
           .IsRequired(false);
    }
}

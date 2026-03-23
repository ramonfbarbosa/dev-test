using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ClientImportEntityTypeConfiguration : BaseEntityTypeConfiguration<ClientImport>
{
    public override void Configure(EntityTypeBuilder<ClientImport> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(ClientImport));
        builder.Property(b => b.OriginalFileName).HasColumnType("varchar(255)").IsRequired();
        builder.Property(b => b.StoredFileName).HasColumnType("varchar(255)").IsRequired();
        builder.Property(b => b.Status).HasConversion<string>().HasColumnType("varchar(20)").IsRequired();
        builder.Property(b => b.UploadedByUserId).IsRequired();
        builder.Property(b => b.UploadedByUserName).HasColumnType("varchar(50)").IsRequired();
        builder.Property(b => b.TotalRows).IsRequired();
        builder.Property(b => b.ImportedRows).IsRequired();
        builder.Property(b => b.FailureCount).IsRequired();
        builder.Property(b => b.StartedAt).HasColumnType("datetime(6)").IsRequired(false);
        builder.Property(b => b.FinishedAt).HasColumnType("datetime(6)").IsRequired(false);
        builder.Property(b => b.ErrorMessage).HasColumnType("varchar(1000)").IsRequired(false);
    }
}

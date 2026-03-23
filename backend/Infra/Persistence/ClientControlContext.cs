using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence;

public class ClientControlContext : DbContext, IClientControlContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ClientImport> ClientImports { get; set; }

    public ClientControlContext(DbContextOptions<ClientControlContext> options) : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == DateTime.MinValue)
                {
                    entry.Entity.SetCreatedAt(DateTime.UtcNow);
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetModifiedAt(DateTime.UtcNow);
                entry.Property(x => x.CreatedAt).IsModified = false;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    public void AttachModelToContext<T>(T entity)
    {
        base.Attach(entity);
    }

    public IExecutionStrategy CreateExecutionStrategy()
    {
        return Database.CreateExecutionStrategy();
    }

    public void SetModifiedState<T>(T entity)
    {
        base.Entry(entity).State = EntityState.Modified;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientControlContext).Assembly);
        modelBuilder.Entity<User>().HasData
        (
            new User("admin", "admin@clientcontrol.local", BCrypt.Net.BCrypt.HashPassword("admin"), Profile.Administrator, true)
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef")
            }
        );
    }
}

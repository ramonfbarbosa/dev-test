using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface IClientControlContext
{
    DbSet<Client> Clients { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<ClientImport> ClientImports { get; set; }

    IExecutionStrategy CreateExecutionStrategy();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    void SetModifiedState<T>(T entity);
    void AttachModelToContext<T>(T entity);
}

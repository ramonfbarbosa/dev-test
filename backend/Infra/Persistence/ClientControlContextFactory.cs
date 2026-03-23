using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class ClientControlContextFactory : DesignTimeDbContextFactoryBase<ClientControlContext>
{
    protected override ClientControlContext CreateNewInstance(DbContextOptions<ClientControlContext> options)
    {
        return new ClientControlContext(options);
    }
}

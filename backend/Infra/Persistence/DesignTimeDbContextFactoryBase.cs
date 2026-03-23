using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Persistence;

public abstract class DesignTimeDbContextFactoryBase<TContext> : 
    IDesignTimeDbContextFactory<TContext> where TContext : DbContext
{
    public TContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory() +
            string.Format(".{0}..{0}..{0}Presentation{0}WebApi", Path.DirectorySeparatorChar);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var connectionString = configuration.GetConnectionString("ConnectionString");
        return Create(connectionString: connectionString);
    }

    protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

    private TContext Create(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException($"Connection string is null or empty.", nameof(connectionString));
        }
        Console.WriteLine($"DesignTimeDbContextFactoryBase.Create(string): Connection string: '{connectionString}'.");
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
        return CreateNewInstance(optionsBuilder.Options);
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRM.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var conn = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
                   ?? "Host=localhost;Port=5432;Database=crm;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(conn);
        return new AppDbContext(optionsBuilder.Options);
    }
}

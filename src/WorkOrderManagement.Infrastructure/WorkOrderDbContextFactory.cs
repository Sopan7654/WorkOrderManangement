using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WorkOrderManagement.Infrastructure.Data;

namespace WorkOrderManagement.Infrastructure;

/// <summary>
/// Provides a WorkOrderDbContext at design time for EF Core migrations.
/// The connection string here is used only during "dotnet ef migrations add" and "dotnet ef database update"
/// — it is not used at runtime.
/// </summary>
public class WorkOrderDbContextFactory : IDesignTimeDbContextFactory<WorkOrderDbContext>
{
    public WorkOrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkOrderDbContext>();

        // Design-time connection: checks environment variable or defaults to local SQL Server / SQLite
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Server=localhost;Database=WorkOrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        if (connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) ||
            (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && !connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase)))
        {
            optionsBuilder.UseSqlite(connectionString);
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        return new WorkOrderDbContext(optionsBuilder.Options);
    }
}

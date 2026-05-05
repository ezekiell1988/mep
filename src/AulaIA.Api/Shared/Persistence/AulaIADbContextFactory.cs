using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AulaIA.Api.Shared.Persistence;

/// <summary>
/// Solo se usa por las herramientas EF Core en tiempo de diseño (dotnet ef migrations add).
/// No se instancia en producción.
/// </summary>
internal sealed class AulaIADbContextFactory : IDesignTimeDbContextFactory<AulaIADbContext>
{
    public AulaIADbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connStr = config["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Database:ConnectionString no está configurado.");

        var optionsBuilder = new DbContextOptionsBuilder<AulaIADbContext>();
        optionsBuilder.UseNpgsql(connStr);

        return new AulaIADbContext(optionsBuilder.Options);
    }
}

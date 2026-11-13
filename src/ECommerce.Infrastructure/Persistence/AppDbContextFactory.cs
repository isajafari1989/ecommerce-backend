using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiProjectDir = ResolveApiProjectDirectory();
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(apiProjectDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true);

        var userSecretsId = TryReadUserSecretsId(apiProjectDir);
        if (!string.IsNullOrWhiteSpace(userSecretsId))
        {
            configurationBuilder.AddUserSecrets(userSecretsId);
        }

        var configuration = configurationBuilder
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "Set ConnectionStrings:DefaultConnection in appsettings.json, " +
                "appsettings.Development.json, user secrets, or the " +
                "ConnectionStrings__DefaultConnection environment variable.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectDirectory()
    {
        var current = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            Path.Combine(current, "..", "ECommerce.Api"),
            current,
            Path.Combine(current, "src", "ECommerce.Api")
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(Path.Combine(fullPath, "appsettings.json")))
                return fullPath;
        }

        throw new InvalidOperationException(
            "Could not find ECommerce.Api/appsettings.json. " +
            "Run Entity Framework tools from src/ECommerce.Infrastructure or the repository root.");
    }

    private static string? TryReadUserSecretsId(string apiProjectDir)
    {
        var projectFile = Path.Combine(apiProjectDir, "ECommerce.Api.csproj");
        if (!File.Exists(projectFile))
            return null;

        var match = Regex.Match(
            File.ReadAllText(projectFile),
            "<UserSecretsId>\\s*([^<\\s]+)\\s*</UserSecretsId>");

        return match.Success ? match.Groups[1].Value : null;
    }
}

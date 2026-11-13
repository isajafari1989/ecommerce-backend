using System;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.Infrastructure.Data;

public static class DbSeeder
{
    public const string PasswordKey = "AdminSeed:Password";

    public static async Task SeedAdminUserAsync(AppDbContext context, IConfiguration configuration)
    {
        if (await context.Users.AnyAsync(u => u.Role == "Admin"))
            return;

        var password = configuration[PasswordKey];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "No admin user exists and AdminSeed:Password is not configured. " +
                "From src/ECommerce.Api, set a user secret: " +
                "dotnet user-secrets set \"AdminSeed:Password\" \"<password>\". " +
                "Or set the AdminSeed__Password environment variable. " +
                "The password is hashed with BCrypt before it is stored.");
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            FullName = "System Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Admin"
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}

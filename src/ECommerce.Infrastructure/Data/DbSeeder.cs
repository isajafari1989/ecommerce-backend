using System;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.Persistence;


namespace ECommerce.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAdminUserAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Role == "Admin"))
            return;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            FullName = "System Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
            Role = "Admin"
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
        
    }
}
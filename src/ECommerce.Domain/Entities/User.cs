namespace ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;

public class User
{
    public Guid  Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = default!;
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = "User"; //Default Role
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    //Refresh Tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    //Relationships
    public ICollection<Product> Products { get; set; } = new List<Product>();
    

}
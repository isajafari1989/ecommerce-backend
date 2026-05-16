using System;

namespace ECommerce.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    
    public DateTime Expires { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public string ReasonRevoked { get; set; } = string.Empty;

    public bool IsRevoked => RevokedAt != null;
    public bool IsExpired => DateTime.UtcNow >= Expires;
    
    //Relations
    public Guid UserId { get; set; }

    public User? User { get; set; }
    
}
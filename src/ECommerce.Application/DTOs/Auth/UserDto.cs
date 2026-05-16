namespace ECommerce.Application.DTOs.Auth;

using System;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? FullName { get; set; }
    public string Role { get; set; } = default!;
}


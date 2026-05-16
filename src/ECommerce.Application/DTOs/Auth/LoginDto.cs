namespace ECommerce.Application.DTOs.Auth;

public class LoginDto
{
    public string Identifier { get; set; } = default!;
    //UserName Or Email
    public string Password { get; set; } = default!;
}


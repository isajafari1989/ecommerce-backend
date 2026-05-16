namespace ECommerce.Application.Interfaces;

using ECommerce.Domain.Entities;

public interface IJwtTokenGenerator
{
    public string GenerateToken(User user);
}
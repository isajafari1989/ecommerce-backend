using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.Interfaces;

namespace ECommerce.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;
    
    public string? Username =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.Name);
    
    
    public string? Role =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.Role);
    
    
}
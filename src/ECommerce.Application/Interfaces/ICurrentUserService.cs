using System;

namespace ECommerce.Application.Interfaces;

public interface ICurrentUserService
{
     Guid? UserId { get; }
     string? Username { get; }

     string? Role { get; }
    
    
}
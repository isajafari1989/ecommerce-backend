namespace ECommerce.Application.Repositories.Users;
using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;


public interface IUserRepository
{
    Task<bool> ExistsAsync(Guid id);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    
    //Refresh Tokens
    Task AddRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken token, string reason);
    
}


namespace ECommerce.Infrastructure.Repositories.Users;

using ECommerce.Domain.Entities;
using ECommerce.Application.Repositories.Users;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;


public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    
    //Check if the user with this Id exists or not
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Users.AnyAsync(x => x.Id == id);
    }
    
    // 🔹 Get user by ID
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
 
    // 🔹 Get user by email
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    //Get user by username
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Username == username);
    }
    
    //Add new user
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }
    
    //Update existing user
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
    }
    
    // Add a refresh token linked to a user
    public async Task AddRefreshTokenAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }
    
    //Retrieve refresh token
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }
    
    //Revoke refresh token
    public async Task RevokeRefreshTokenAsync(RefreshToken token, string reason)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.ReasonRevoked = reason;
        _context.RefreshTokens.Update(token);
        
    }



}

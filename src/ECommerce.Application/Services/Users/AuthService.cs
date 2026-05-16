namespace ECommerce.Application.Services.Users;

using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Domain.Entities;
using System.Linq;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories.Users;
using BCrypt.Net;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    
    
    //------------------------------
    //CONSTRUCTOR
    //------------------------------
    public AuthService(IUserRepository userRepository, IJwtTokenGenerator tokenGenerator, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
    }
    
    //------------------------------
    //REGISTER USER
    //------------------------------
    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var usernameExists = await _userRepository.GetByUsernameAsync(dto.Username);
        if (usernameExists != null)
            return false;
        
        var email = dto.Email.ToLower();
        var emailExists = await _userRepository.GetByEmailAsync(email);
        if (emailExists != null)
            return false;

        var user = new User
        {
            Username = dto.Username.ToLower(),
            FullName = dto.FullName.ToLower(),
            Email = email.ToLower(),
            PasswordHash = BCrypt.HashPassword(dto.Password),
            Role = "User"
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    
    
    //----------------------------
    //PRIVATE HELPER
    //----------------------------
    private RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[64];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            Expires = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow
        };
    }
    
    
    //-------------------------------------------------------------
    //LOGIN
    //----------------------------------------------------------------
    public async Task<RefreshTokenDto> LoginAsync(LoginDto dto)
    {
        User? user;
        var identifier = dto.Identifier.Trim();

        if (identifier.Contains("@"))
        {
            user = await _userRepository.GetByEmailAsync(identifier.ToLower());
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(identifier.ToLower());
        }

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User is inactive.");

        if (!BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        // Generate JWT
        var accessToken = _tokenGenerator.GenerateToken(user);

        // Generate refresh token
        var refreshToken = GenerateRefreshToken();

        user.RefreshTokens.Add(refreshToken);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new RefreshTokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }
    
    
    //Refresh Token
    public async Task<RefreshTokenDto> RefreshAsync(RefreshRequestDto refreshRequest)
    {
        var existing = await _userRepository.GetRefreshTokenAsync(refreshRequest.RefreshToken)
                       ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (existing.IsExpired) throw new UnauthorizedAccessException("Expired refresh token.");
        if (existing.IsRevoked) throw new UnauthorizedAccessException("Revoked refresh token.");

        var user = await _userRepository.GetByIdAsync(existing.UserId)
                   ?? throw new UnauthorizedAccessException("User not found.");
        
        //Rotate token
        var newRefreshToken = GenerateRefreshToken();
        existing.RevokedAt = DateTime.UtcNow;
        existing.ReasonRevoked = "Replaced by new token";

        user.RefreshTokens.Add(newRefreshToken);
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var newJwt = _tokenGenerator.GenerateToken(user);

        return new RefreshTokenDto
        {
            AccessToken = newJwt,
            RefreshToken = newRefreshToken.Token
        };
    }
    
    //----------------------------
    //LOGOUT/ REVOKE TOKENS
    //----------------------------
    public async Task LogoutAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new UnauthorizedAccessException("User not found.");

        foreach (var token in user.RefreshTokens
                     .Where(t => !t.IsRevoked && !t.IsExpired))
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "User logout";
        }

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }
    
    
    
}



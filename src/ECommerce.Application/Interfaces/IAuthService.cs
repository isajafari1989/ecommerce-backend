namespace ECommerce.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Auth;
using System;


public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterDto dto);

    Task<RefreshTokenDto> LoginAsync(LoginDto dto);

    Task<RefreshTokenDto> RefreshAsync(RefreshRequestDto refreshTokenStr);

    Task LogoutAsync(Guid userId);
}


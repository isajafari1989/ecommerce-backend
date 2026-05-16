using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ECommerce.Application.Interfaces;
using ECommerce.Application.DTOs.Auth;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;


namespace ECommerce.Api.Controllers;

/// <summary>
/// Provides authentication endpoints for user registration,
/// login, token refreshing, logout, and retrieving information
/// about the currently authenticated user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">
    /// Authentication service responsible for handling
    /// user registration, login, token generation and token refresh logic.
    /// </param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <remarks>
    /// Creates a new user in the system.
    ///
    /// The username and email must be unique.
    ///
    /// Example request:
    ///
    /// POST /api/auth/register
    ///
    /// {
    ///   "username": "john_doe",
    ///   "FullName": "John Doefski",
    ///   "email": "john@gmail.com",
    ///   "password": "Password123!"
    /// }
    /// </remarks>
    /// <param name="dto">User registration data.</param>
    /// <returns>
    /// Returns 200 if registration succeeds.  
    /// Returns 400 if the username or email already exists.
    /// </returns>
    /// <response code="200">User successfully created.</response>
    /// <response code="400">Username or email already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var created = await _authService.RegisterAsync(dto);

        if (!created)
            return BadRequest("Username or email already exists.");

        return Ok("User created successfully.");
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <remarks>
    /// Login can be performed using either:
    ///
    /// - Username
    /// - Email
    ///
    /// Example request:
    ///
    /// POST /api/auth/login
    ///
    /// {
    ///   "identifier": "john_doe",
    ///   "password": "Password123!"
    /// }
    ///
    /// OR
    ///
    /// {
    ///   "identifier": "john@gmail.com",
    ///   "password": "Password123!"
    /// }
    ///
    /// Successful authentication returns:
    ///
    /// - AccessToken (JWT)
    /// - RefreshToken
    /// </remarks>
    /// <param name="dto">User login credentials.</param>
    /// <returns>JWT access token and refresh token.</returns>
    /// <response code="200">Authentication successful.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RefreshTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    /// <remarks>
    /// Refresh tokens allow clients to obtain a new access token
    /// without requiring the user to login again.
    ///
    /// Example request:
    ///
    /// POST /api/auth/refresh
    ///
    /// {
    ///   "refreshToken": "your_refresh_token_here"
    /// }
    ///
    /// The refresh token will be rotated (invalidated and replaced).
    /// </remarks>
    /// <param name="dto">Refresh token request.</param>
    /// <returns>New access token and refresh token.</returns>
    /// <response code="200">New tokens generated successfully.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        try
        {
            var result = await _authService.RefreshAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    /// <summary>
    /// Logs out the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Revokes all active refresh tokens belonging to the user,
    /// effectively logging them out from the system.
    ///
    /// Requires a valid JWT token.
    ///
    /// Authorization header example:
    ///
    /// Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Logout confirmation message.</returns>
    /// <response code="200">User logged out successfully.</response>
    /// <response code="401">User not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdStr == null)
            return Unauthorized();
        
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        await _authService.LogoutAsync(userId);

        return Ok("User logged out successfully.");
    }

    /// <summary>
    /// Returns information about the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Requires a valid JWT token.
    ///
    /// Authorization header example:
    ///
    /// Authorization: Bearer &lt;access_token&gt;
    ///
    /// Example response:
    ///
    /// {
    ///   "userId": "36b2a63b-e81d-4f48-9d65-1d9ec5e447f7",
    ///   "username": "john_doe",
    ///   "role": "User"
    /// }
    /// </remarks>
    /// <returns>User identity information extracted from the JWT.</returns>
    /// <response code="200">User information returned.</response>
    /// <response code="401">Invalid or missing JWT token.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            UserId = userId,
            Username = username,
            Role = role
        });
    }
}

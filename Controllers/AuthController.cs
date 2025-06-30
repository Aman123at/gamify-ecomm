namespace GamifyApi.Controllers;

using System.Security.Claims;
using GamifyApi.Dtos;
using GamifyApi.Middlwares;
using GamifyApi.Models;
using GamifyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    private readonly GamifyDbContext _context;

    public AuthController(GamifyDbContext context, AuthService authService)
    {
        _authService = authService;
        _context = context;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">User registration information</param>
    /// <returns>Success message upon successful registration</returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Invalid registration data provided</response>
    /// <response code="409">Email already registered</response>
    /// <response code="500">Internal server error during registration</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        if (!request.IsEmailValid() || !request.IsPasswordValid() || !request.IsFullNameValid())
            return BadRequest("Invalid registration data.");

        try
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (existingUser)
                return Conflict("Email already registered.");


            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                ProfilePictureUrl = request.ProfilePictureUrl,
                City = request.City,
                State = request.State,
                Country = request.Country
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");


        }
        catch (System.Exception)
        {            // Log the exception (not implemented here)
            return StatusCode(500, "An error occurred while registering the user.");
        }



    }

    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    /// <param name="request">User login credentials</param>
    /// <returns>JWT token for authentication</returns>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="400">Invalid login data provided</response>
    /// <response code="401">Invalid email or password</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error during authentication</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
    {
        if (!request.IsEmailValid() || !request.IsPasswordValid())
            return BadRequest("Invalid login data.");

        try
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (!existingUser)
                return NotFound("User not found.");
        }
        catch (System.Exception)
        {
            // Log the exception (not implemented here)
            return StatusCode(500, "An error occurred while checking user existence.");
        }

        var user = await _authService.Authenticate(request.Email, request.Password);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        var token = _authService.GenerateToken(user);
        return Ok(new { Token = token });
    }

    /// <summary>
    /// Get current user profile information
    /// </summary>
    /// <returns>User profile details including name, email, and role</returns>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error while fetching profile</response>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public Task<IActionResult> GetUserProfile()
    {
        try
        {
            return Task.FromResult<IActionResult>(Ok(new
            {
                FullName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            }));
        }
        catch (System.Exception)
        {
            // Log the exception (not implemented here)
            return Task.FromResult<IActionResult>(StatusCode(500, "An error occurred while fetching user profile."));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DogGroomingAPI.Data;
using DogGroomingAPI.DTOs;
using DogGroomingAPI.Models;
using DogGroomingAPI.Services;
using System.Security.Claims;

namespace DogGroomingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
        {
            return BadRequest(new { message = "Username already exists" });
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        var user = new User
        {
            Username = registerDto.Username,
            PasswordHash = passwordHash,
            FirstName = registerDto.FirstName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            FirstName = user.FirstName,
            UserId = user.Id
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            FirstName = user.FirstName,
            UserId = user.Id
        });
    }

    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> ValidateToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            FirstName = user.FirstName,
            UserId = user.Id
        });
    }
}

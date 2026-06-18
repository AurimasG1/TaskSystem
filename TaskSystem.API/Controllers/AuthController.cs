using Microsoft.AspNetCore.Mvc;
using TaskSystem.Services.Implementation;
using TaskSystem.Services.Interface;

namespace TaskSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly JwtService _jwt;

    public AuthController(IUserService users, JwtService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public record RegisterRequest(string Email, string Password, string Role = "user");

    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var ok = await _users.RegisterAsync(req.Email, req.Password, req.Role);
        if (!ok)
            return BadRequest("User already exists");

        return Ok("Registered");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _users.LoginAsync(req.Email, req.Password);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var token = _jwt.GenerateToken(user.Id, user.Email, user.Role);
        return Ok(new { token });
    }
}

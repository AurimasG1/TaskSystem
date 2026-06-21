using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Common.DTO;
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

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var user = await _users.RegisterAsync(req.Email, req.Password, req.Role);
        return Ok(new { message = "Registered", userId = user.Id });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _users.LoginAsync(req.Email, req.Password);
        var token = _jwt.GenerateToken(user!.Id, user.Email, user.Role);

        return Ok(new { token });
    }
}

using Mapster;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Auth.AuthLogin;
using TaskSystem.Application.Commands.Auth.AuthRefreshToken;
using TaskSystem.Application.Commands.Auth.AuthRegister;
using TaskSystem.Application.DTO.Requests.Auth;

namespace TaskSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthRegisterHandler _registerHandler;
    private readonly AuthLoginHandler _loginHandler;
    private readonly RefreshTokenHandler _refreshHandler;

    public AuthController(
        AuthRegisterHandler registerHandler,
        AuthLoginHandler loginHandler,
        RefreshTokenHandler refreshHandler
    )
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _refreshHandler = refreshHandler;
    }

    // ---------------------------
    // REGISTER
    // ---------------------------
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRegisterRequest request)
    {
        var command = request.Adapt<AuthRegisterCommand>();
        var result = await _registerHandler.Handle(command);
        return Ok(result);
    }

    // ---------------------------
    // LOGIN
    // ---------------------------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
    {
        var command = request.Adapt<AuthLoginCommand>();
        var result = await _loginHandler.Handle(command);
        return Ok(result);
    }

    // ---------------------------
    // REFRESH TOKEN
    // ---------------------------
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _refreshHandler.Handle(command);
        return Ok(result);
    }
}

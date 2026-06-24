using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Users.LoginUser;
using TaskSystem.Application.Commands.Users.RegisterUser;
using TaskSystem.Application.DTO.Auth;

namespace TaskSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _register;
    private readonly LoginUserHandler _login;

    public AuthController(RegisterUserHandler register, LoginUserHandler login)
    {
        _register = register;
        _login = login;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request) =>
        Ok(
            await _register.Handle(
                new RegisterUserCommand(request.Email, request.Password, request.AdminCode)
            )
        );

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest dto) =>
        Ok(await _login.Handle(new LoginUserCommand(dto.Email, dto.Password)));
}

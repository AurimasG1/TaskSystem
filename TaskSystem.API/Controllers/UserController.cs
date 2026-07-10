using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Users.UserChangePassword;
using TaskSystem.Application.Commands.Users.UserDelete;
using TaskSystem.Application.Commands.Users.UserRegister;
using TaskSystem.Application.Commands.Users.UserUpdate;
using TaskSystem.Application.DTO.Requests.Users;
using TaskSystem.Application.Queries.Users.GetUserById;

namespace TaskSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly GetUserByIdHandler _getById;
    private readonly UserUpdateHandler _update;
    private readonly UserDeleteHandler _delete;
    private readonly UserChangePasswordHandler _changePassword;
    private readonly UserRegisterHandler _userRegisterHandler;

    public UserController(
        GetUserByIdHandler getById,
        UserUpdateHandler update,
        UserDeleteHandler delete,
        UserChangePasswordHandler changePassword,
        UserRegisterHandler userRegisterHandler
    )
    {
        _getById = getById;
        _update = update;
        _delete = delete;
        _changePassword = changePassword;
        _userRegisterHandler = userRegisterHandler;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("me")]
    public async Task<IActionResult> Me() =>
        Ok(await _getById.Handle(new GetUserByIdQuery(UserId)));

    [Authorize(Roles = "user,admin")]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UserUpdateRequest dto)
    {
        var command = new UserUpdateCommand(UserId, dto.FirstName, dto.LastName);
        var result = await _update.Handle(command);
        return Ok(result);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(UserChangePasswordRequest dto)
    {
        await _changePassword.Handle(
            new UserChangePasswordCommand(UserId, dto.OldPassword, dto.NewPassword)
        );

        return Ok(new { message = "Password changed" });
    }

    [Authorize(Roles = "user,admin")]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe()
    {
        await _delete.Handle(new UserDeleteCommand(UserId));
        return NoContent();
    }

    [Authorize(Roles = "onboarding,admin")]
    [HttpPost("register-profile")]
    public async Task<IActionResult> RegisterProfile(UserRegisterRequest dto)
    {
        var command = new UserRegisterCommand(UserId, dto.FirstName, dto.LastName);
        var result = await _userRegisterHandler.Handle(command);
        return Ok(result);
    }
}

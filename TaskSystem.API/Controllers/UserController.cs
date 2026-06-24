using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Users.ChangePassword;
using TaskSystem.Application.Commands.Users.DeleteUser;
using TaskSystem.Application.Commands.Users.UpdateUser;
using TaskSystem.Application.DTO.Users;
using TaskSystem.Application.Queries.Users.GetUserById;

namespace TaskSystem.API.Controllers;

[Authorize(Roles = "user,admin")]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly GetUserByIdHandler _getById;
    private readonly UpdateUserHandler _update;
    private readonly DeleteUserHandler _delete;
    private readonly ChangePasswordHandler _changePassword;

    public UserController(
        GetUserByIdHandler getById,
        UpdateUserHandler update,
        DeleteUserHandler delete,
        ChangePasswordHandler changePassword
    )
    {
        _getById = getById;
        _update = update;
        _delete = delete;
        _changePassword = changePassword;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("me")]
    public async Task<IActionResult> Me() =>
        Ok(await _getById.Handle(new GetUserByIdQuery(UserId)));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateUserRequest dto) =>
        Ok(await _update.Handle(new UpdateUserCommand(UserId, dto.Email, dto.UserName)));

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest dto)
    {
        await _changePassword.Handle(
            new ChangePasswordCommand(UserId, dto.OldPassword, dto.NewPassword)
        );
        return Ok(new { message = "Password changed" });
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe()
    {
        await _delete.Handle(new DeleteUserCommand(UserId));
        return NoContent();
    }
}

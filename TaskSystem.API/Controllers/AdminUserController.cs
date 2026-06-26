using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Users.DeleteUser;
using TaskSystem.Application.Commands.Users.UpdateUser;
using TaskSystem.Application.DTO.Users;
using TaskSystem.Application.Queries.Users.GetUserByEmail;
using TaskSystem.Application.Queries.Users.GetUserById;

namespace TaskSystem.API.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/users")]
public class AdminUserController : ControllerBase
{
    private readonly GetUserByIdHandler _getById;
    private readonly GetUserByEmailHandler _getByEmail;
    private readonly AdminUpdateUserHandler _update;
    private readonly DeleteUserHandler _delete;

    public AdminUserController(
        GetUserByIdHandler getById,
        GetUserByEmailHandler getByEmail,
        AdminUpdateUserHandler update,
        DeleteUserHandler delete
    )
    {
        _getById = getById;
        _getByEmail = getByEmail;
        _update = update;
        _delete = delete;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _getById.Handle(new GetUserByIdQuery(id)));

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email) =>
        Ok(await _getByEmail.Handle(new GetUserByEmailQuery(email)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AdminUpdateUserRequest dto) =>
        Ok(await _update.Handle(new AdminUpdateUserCommand(id, dto.Email, dto.UserName, dto.Role)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _delete.Handle(new DeleteUserCommand(id));
        return NoContent();
    }
}

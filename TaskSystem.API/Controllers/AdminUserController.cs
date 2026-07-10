using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Users.UserDelete;
using TaskSystem.Application.Commands.Users.UserUpdate;
using TaskSystem.Application.Queries.Users;
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
    private readonly UserAdminUpdateHandler _update;
    private readonly UserDeleteHandler _delete;

    public AdminUserController(
        GetUserByIdHandler getById,
        GetUserByEmailHandler getByEmail,
        UserAdminUpdateHandler update,
        UserDeleteHandler delete
    )
    {
        _getById = getById;
        _getByEmail = getByEmail;
        _update = update;
        _delete = delete;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _getById.Handle(new GetUserByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var result = await _getByEmail.Handle(new GetUserByEmailQuery(email));
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UserAdminUpdateCommand dto)
    {
        var command = dto with { Id = id };
        var result = await _update.Handle(command);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _delete.Handle(new UserDeleteCommand(id));
        return NoContent();
    }
}

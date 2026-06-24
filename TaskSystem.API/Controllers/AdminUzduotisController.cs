using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Uzduotys.DeleteUzduotis;
using TaskSystem.Application.Commands.Uzduotys.ResetLastUzduotis;
using TaskSystem.Application.Queries.Uzduotys.GetAllUzduotys;
using TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserId;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserEmail;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserId;

namespace TaskSystem.API.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/uzduotys")]
public class AdminUzduotisController : ControllerBase
{
    private readonly GetAllUzduotysHandler _getAll;
    private readonly GetUzduotysByUserIdHandler _getByUserId;
    private readonly GetUzduotysByUserEmailHandler _getByEmail;
    private readonly GetLastUzduotisByUserIdHandler _getLast;
    private readonly ResetLastUzduotisHandler _reset;
    private readonly DeleteUzduotisHandler _delete;
    private readonly AdminDeleteUzduotisHandler _adminDelete;

    public AdminUzduotisController(
        GetAllUzduotysHandler getAll,
        GetUzduotysByUserIdHandler getByUserId,
        GetUzduotysByUserEmailHandler getByEmail,
        GetLastUzduotisByUserIdHandler getLast,
        ResetLastUzduotisHandler reset,
        DeleteUzduotisHandler delete,
        AdminDeleteUzduotisHandler adminDelete
    )
    {
        _getAll = getAll;
        _getByUserId = getByUserId;
        _getByEmail = getByEmail;
        _getLast = getLast;
        _reset = reset;
        _delete = delete;
        _adminDelete = adminDelete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _getAll.Handle(new GetAllUzduotysQuery()));

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId) =>
        Ok(await _getByUserId.Handle(new GetUzduotysByUserIdQuery(userId)));

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email) =>
        Ok(await _getByEmail.Handle(new GetUzduotysByUserEmailQuery(email)));

    [HttpGet("last/{userId:int}")]
    public async Task<IActionResult> GetLast(int userId) =>
        Ok(await _getLast.Handle(new GetLastUzduotisByUserIdQuery(userId)));

    [HttpPost("reset-last/{userId:int}")]
    public async Task<IActionResult> ResetLast(int userId) =>
        Ok(await _reset.Handle(new ResetLastUzduotisCommand(userId)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _adminDelete.Handle(new AdminDeleteUzduotisCommand(id));
        return NoContent();
    }
}

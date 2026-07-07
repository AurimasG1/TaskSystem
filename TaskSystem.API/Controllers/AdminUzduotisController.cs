using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Uzduotys.ResetLast;
using TaskSystem.Application.Commands.Uzduotys.UzduotisDelete;
using TaskSystem.Application.Commands.Uzduotys.UzduotisResetLast;
using TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserProfileId;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserProfileId;

namespace TaskSystem.API.Controllers;

[ApiController]
[Route("api/admin/uzduotys")]
public class AdminUzduotisController : ControllerBase
{
    private readonly GetUzduotysByUserProfileIdHandler _getAll;
    private readonly GetLastUzduotisByUserProfileIdHandler _getLast;
    private readonly UzduotisResetLastHandler _resetLast;
    private readonly UzduotisAdminDeleteHandler _delete;

    public AdminUzduotisController(
        GetUzduotysByUserProfileIdHandler getAll,
        GetLastUzduotisByUserProfileIdHandler getLast,
        UzduotisResetLastHandler resetLast,
        UzduotisAdminDeleteHandler delete
    )
    {
        _getAll = getAll;
        _getLast = getLast;
        _resetLast = resetLast;
        _delete = delete;
    }

    [HttpGet("{userProfileId:int}")]
    public async Task<IActionResult> GetAll(int userProfileId)
    {
        var result = await _getAll.Handle(new GetUzduotysByUserProfileIdQuery(userProfileId));
        return Ok(result);
    }

    [HttpGet("{userProfileId:int}/last")]
    public async Task<IActionResult> GetLast(int userProfileId)
    {
        var result = await _getLast.Handle(new GetLastUzduotisByUserProfileIdQuery(userProfileId));
        return Ok(result);
    }

    [HttpPost("{userProfileId:int}/reset-last")]
    public async Task<IActionResult> ResetLast(int userProfileId)
    {
        var result = await _resetLast.Handle(new UzduotisResetLastCommand(userProfileId));
        return Ok(result);
    }

    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> Delete(int taskId)
    {
        await _delete.Handle(new UzduotisAdminDeleteCommand(taskId));
        return NoContent();
    }
}

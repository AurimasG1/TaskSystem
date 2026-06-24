using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Uzduotys.CreateUzduotis;
using TaskSystem.Application.Commands.Uzduotys.DeleteUzduotis;
using TaskSystem.Application.Commands.Uzduotys.UpdateUzduotis;
using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserId;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotisById;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserId;

namespace TaskSystem.API.Controllers;

[Authorize(Roles = "user,admin")]
[ApiController]
[Route("api/user/uzduotys")]
public class UserUzduotisController : ControllerBase
{
    private readonly CreateUzduotisHandler _create;
    private readonly UpdateUzduotisHandler _update;
    private readonly DeleteUzduotisHandler _delete;
    private readonly GetUzduotisByIdHandler _getById;
    private readonly GetUzduotysByUserIdHandler _getByUserId;
    private readonly GetLastUzduotisByUserIdHandler _getLast;

    public UserUzduotisController(
        CreateUzduotisHandler create,
        UpdateUzduotisHandler update,
        DeleteUzduotisHandler delete,
        GetUzduotisByIdHandler getById,
        GetUzduotysByUserIdHandler getByUserId,
        GetLastUzduotisByUserIdHandler getLast
    )
    {
        _create = create;
        _update = update;
        _delete = delete;
        _getById = getById;
        _getByUserId = getByUserId;
        _getLast = getLast;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UzduotisRequestDto dto)
    {
        var command = new CreateUzduotisCommand(dto.Title, dto.Description, UserId);
        var created = await _create.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy() =>
        Ok(await _getByUserId.Handle(new GetUzduotysByUserIdQuery(UserId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _getById.Handle(new GetUzduotisByIdQuery(id));
        if (item.UserId != UserId)
            return Forbid();
        return Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UzduotisUpdateRequestDto dto)
    {
        var command = new UpdateUzduotisCommand(
            id,
            dto.Title,
            dto.Description,
            dto.StatusId,
            UserId
        );
        return Ok(await _update.Handle(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _delete.Handle(new DeleteUzduotisCommand(id, UserId));
        return NoContent();
    }

    [HttpGet("last")]
    public async Task<IActionResult> GetLast() =>
        Ok(await _getLast.Handle(new GetLastUzduotisByUserIdQuery(UserId)));
}

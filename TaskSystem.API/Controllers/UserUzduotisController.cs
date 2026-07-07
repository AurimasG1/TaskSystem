using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Uzduotys.ResetLast;
using TaskSystem.Application.Commands.Uzduotys.UzduotisCreate;
using TaskSystem.Application.Commands.Uzduotys.UzduotisDelete;
using TaskSystem.Application.Commands.Uzduotys.UzduotisResetLast;
using TaskSystem.Application.Commands.Uzduotys.UzduotisUpdate;
using TaskSystem.Application.Common;
using TaskSystem.Application.DTO.Requests.Uzduotys;
using TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserProfileId;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotisById;
using TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserProfileId;

namespace TaskSystem.API.Controllers;

[Authorize(Roles = "user,admin")]
[ApiController]
[Route("api/user/uzduotys")]
public class UserUzduotisController : ControllerBase
{
    private readonly UzduotisCreateHandler _create;
    private readonly UzduotisUpdateHandler _update;
    private readonly UzduotisDeleteHandler _delete;
    private readonly UzduotisResetLastHandler _resetLast;
    private readonly GetUzduotisByIdHandler _getById;
    private readonly GetUzduotysByUserProfileIdHandler _getByProfile;
    private readonly GetLastUzduotisByUserProfileIdHandler _getLast;

    public UserUzduotisController(
        UzduotisCreateHandler create,
        UzduotisUpdateHandler update,
        UzduotisDeleteHandler delete,
        UzduotisResetLastHandler resetLast,
        GetUzduotisByIdHandler getById,
        GetUzduotysByUserProfileIdHandler getByProfile,
        GetLastUzduotisByUserProfileIdHandler getLast
    )
    {
        _create = create;
        _update = update;
        _delete = delete;
        _resetLast = resetLast;
        _getById = getById;
        _getByProfile = getByProfile;
        _getLast = getLast;
    }

    private int UserProfileId => int.Parse(User.FindFirst("profileId")!.Value);

    // POST /api/user/uzduotys
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UzduotisCreateRequest dto)
    {
        var command = new UzduotisCreateCommand(dto.Title, dto.Description, UserProfileId);
        var created = await _create.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // GET /api/user/uzduotys/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await _getByProfile.Handle(new GetUzduotysByUserProfileIdQuery(UserProfileId));
        return Ok(result);
    }

    // GET /api/user/uzduotys/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _getById.Handle(new GetUzduotisByIdQuery(id));
        if (item.UserProfileId != UserProfileId)
            return Forbid("Not your task");
        return Ok(item);
    }

    // PUT /api/user/uzduotys/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UzduotisUpdateRequest dto)
    {
        var command = new UzduotisUpdateCommand(
            id,
            Optional<string>.FromNullable(dto.Title),
            Optional<string>.FromNullable(dto.Description),
            Optional<int>.FromNullable(dto.StatusId),
            UserProfileId
        );
        var updated = await _update.Handle(command);
        return Ok(updated);
    }

    // DELETE /api/user/uzduotys/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _delete.Handle(new UzduotisDeleteCommand(id, UserProfileId));
        return NoContent();
    }

    // GET /api/user/uzduotys/last
    [HttpGet("last")]
    public async Task<IActionResult> GetLast()
    {
        var result = await _getLast.Handle(new GetLastUzduotisByUserProfileIdQuery(UserProfileId));
        return Ok(result);
    }

    // POST /api/user/uzduotys/reset-last
    [HttpPost("reset-last")]
    public async Task<IActionResult> ResetLast()
    {
        var result = await _resetLast.Handle(new UzduotisResetLastCommand(UserProfileId));
        return Ok(result);
    }
}

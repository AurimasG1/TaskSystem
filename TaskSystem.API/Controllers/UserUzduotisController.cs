using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Common.DTO;
using TaskSystem.Entities;
using TaskSystem.Services.Interface;

namespace TaskSystem.API.Controllers;

[Authorize(Roles = "user,admin")]
[ApiController]
[Route("api/user/uzduotys")]
public class UserUzduotisController : ControllerBase
{
    private readonly IUzduotisService _service;

    public UserUzduotisController(IUzduotisService service)
    {
        _service = service;
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UzduotisRequestDto request)
    {
        int userId = GetUserId();
        var created = await _service.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyUzduotys()
    {
        int userId = GetUserId();
        var items = await _service.GetByUserIdAsync(userId);

        return items.Count == 0 ? NotFound("You have no tasks") : Ok(items);
    }

    // 3. Peržiūrėti konkretų filmą
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        int userId = GetUserId();

        var entity = await _service.GetEntityByIdAsync(id);
        if (entity == null)
            return NotFound();

        if (entity.UserId != userId)
            return Forbid();

        var dto = await _service.GetByIdAsync(id);
        return Ok(dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UzduotisUpdateRequestDto request)
    {
        int userId = GetUserId();

        var entity = await _service.GetEntityByIdAsync(id);
        if (entity == null || entity.UserId != userId)
            return Forbid();

        var success = await _service.UpdateAsync(id, request, userId);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetUserId();

        var entity = await _service.GetEntityByIdAsync(id);
        if (entity == null || entity.UserId != userId)
            return Forbid();

        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }

    // 6. Paskutinis naudotojo filmas
    [HttpGet("last")]
    public async Task<IActionResult> GetLast()
    {
        int userId = GetUserId();
        var item = await _service.GetLastByUserIdAsync(userId);

        return item == null ? NotFound("You have no tasks") : Ok(item);
    }
}

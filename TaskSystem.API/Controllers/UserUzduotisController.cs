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

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        int userId = GetUserId();
        var item = await _service.GetByIdAsync(id);

        if (item?.UserId != userId)
            throw new UnauthorizedAccessException("Not your task");

        return Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UzduotisUpdateRequestDto request)
    {
        int userId = GetUserId();
        var updated = await _service.UpdateAsync(id, request, userId);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetUserId();
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("last")]
    public async Task<IActionResult> GetLast()
    {
        int userId = GetUserId();
        var item = await _service.GetLastByUserIdAsync(userId);
        return Ok(item);
    }
}

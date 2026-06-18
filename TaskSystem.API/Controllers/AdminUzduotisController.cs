using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Services.Interface;

namespace TaskSystem.API.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/uzduotys")]
public class AdminUzduotisController : ControllerBase
{
    private readonly IUzduotisService _service;

    public AdminUzduotisController(IUzduotisService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("by-user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var items = await _service.GetByUserIdAsync(userId);

        return items.Count == 0 ? NotFound($"No tasks found for user {userId}") : Ok(items);
    }

    [HttpGet("by-user-email/{email}")]
    public async Task<IActionResult> GetByUserEmail(string email)
    {
        var items = await _service.GetByUserEmailAsync(email);

        return items.Count == 0 ? NotFound($"No tasks found for email {email}") : Ok(items);
    }

    [HttpGet("last/by-user/{userId:int}")]
    public async Task<IActionResult> GetLastByUser(int userId)
    {
        var item = await _service.GetLastByUserIdAsync(userId);

        return item == null ? NotFound($"User {userId} has no tasks") : Ok(item);
    }

    [HttpPut("last/reset/{userId:int}")]
    public async Task<IActionResult> ResetLastUzduotis(int userId)
    {
        var success = await _service.ResetLastUzduotisAsync(userId);

        return success ? NoContent() : NotFound($"User {userId} has no tasks to reset");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);

        return success ? NoContent() : NotFound($"Task {id} not found");
    }
}

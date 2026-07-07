using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Commands.Admin;
using TaskSystem.Application.DTO.Requests.Admin;
using TaskSystem.Infrastructure.Admin;

namespace TaskSystem.API.Controllers;

public class AdminController : ControllerBase
{
    private readonly AdminPromoteWithTokenHandler _promote;
    private readonly AdminPromotionTokenGenerator _generate;

    public AdminController(
        AdminPromoteWithTokenHandler promote,
        AdminPromotionTokenGenerator generate
    )
    {
        _promote = promote;
        _generate = generate;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [Authorize(Roles = "user,onboarding")]
    [HttpPost("promote-with-token")]
    public async Task<IActionResult> PromoteWithToken([FromBody] AdminPromoteWithTokenRequest dto)
    {
        var command = new PromoteAdminWithTokenCommand(UserId, dto.Token);

        var result = await _promote.Handle(command);

        if (!result)
            return BadRequest("Invalid or expired token.");

        return Ok("You are now admin.");
    }

    [Authorize(Roles = "admin")]
    [HttpPost("generate-promotion-token")]
    public async Task<IActionResult> GeneratePromotionToken()
    {
        var token = await _generate.GenerateAsync();
        return Ok(new { token });
    }
}

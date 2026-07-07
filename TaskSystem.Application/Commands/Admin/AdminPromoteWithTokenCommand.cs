// namespace TaskSystem.Application.Commands.Admin
// {
//     public record PromoteAdminWithTokenCommand(string Token) : IRequest<bool>;
// }

namespace TaskSystem.Application.Commands.Admin;

public record PromoteAdminWithTokenCommand(int UserId, string Token);

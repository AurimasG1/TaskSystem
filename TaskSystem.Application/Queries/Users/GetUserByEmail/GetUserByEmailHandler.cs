using TaskSystem.Application.DTO.Users;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Users.GetUserByEmail;

public class GetUserByEmailHandler
{
    private readonly IUserRepository _userRepo;

    public GetUserByEmailHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<UserDto> Handle(GetUserByEmailQuery request)
    {
        // 1. Load user (no tracking)
        var user =
            await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new UserNotFoundException(request.Email);

        // 2. Return DTO
        return new UserDto(user.Id, user.Email.Value, user.Role, user.UserName);
    }
}

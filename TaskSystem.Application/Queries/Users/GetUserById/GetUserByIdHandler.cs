using TaskSystem.Application.DTO.Users;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Users.GetUserById;

public class GetUserByIdHandler
{
    private readonly IUserRepository _userRepo;

    public GetUserByIdHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request)
    {
        // 1. Load user (no tracking)
        var user =
            await _userRepo.GetByIdAsync(request.Id) ?? throw new UserNotFoundException(request.Id);

        // 2. Return DTO
        return new UserDto(user.Id, user.Email.Value, user.UserName, user.Role);
    }
}

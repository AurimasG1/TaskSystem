using TaskSystem.Application.DTO.Responses.Users;
using TaskSystem.Application.Queries.Users.GetUserByEmail;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Users;

public class GetUserByEmailHandler
{
    private readonly IUserRepository _repo;

    public GetUserByEmailHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<UserDto> Handle(GetUserByEmailQuery request)
    {
        var user =
            await _repo.GetByEmailAsync(request.Email)
            ?? throw new Exception($"User with email {request.Email} not found.");

        return new UserDto(
            UserId: user.Id,
            ProfileId: user.Profile.Id,
            FirstName: user.Profile.FirstName,
            LastName: user.Profile.LastName,
            Email: user.EmailValue,
            Role: user.Profile.Role
        );
    }
}

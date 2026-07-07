using TaskSystem.Application.DTO.Responses.Users;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Users.GetUserById;

public class GetUserByIdHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IUserProfileRepository _profileRepo;

    public GetUserByIdHandler(IUserRepository userRepo, IUserProfileRepository profileRepo)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request)
    {
        // 1. Login user
        var user =
            await _userRepo.GetByIdAsync(request.UserId) ?? throw new Exception("User not found");

        // 2. Domain profile
        var profile =
            await _profileRepo.GetByUserIdAsync(request.UserId)
            ?? throw new Exception("User profile not found");

        // 3. DTO mapping (teisingas)
        return new UserDto(
            UserId: user.Id,
            ProfileId: profile.Id,
            FirstName: profile.FirstName,
            LastName: profile.LastName,
            Email: user.EmailValue,
            Role: profile.Role
        );
    }
}

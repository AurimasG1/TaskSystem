using Mapster;
using TaskSystem.Application.DTO.Responses.Users;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.UserUpdate;

public class UserUpdateHandler
{
    private readonly IUserProfileRepository _profileRepo;

    public UserUpdateHandler(IUserProfileRepository profileRepo)
    {
        _profileRepo = profileRepo;
    }

    public async Task<UserDto> Handle(UserUpdateCommand request)
    {
        // 1. Gauti profilį (tracked)
        var profile =
            await _profileRepo.GetByUserIdForUpdateAsync(request.UserId)
            ?? throw new Exception("Profile not found");

        // 2. Atnaujinti tik leidžiamus laukus
        profile.FirstName = request.FirstName;
        profile.LastName = request.LastName;

        // 3. Išsaugoti
        await _profileRepo.UpdateAsync(profile);
        await _profileRepo.SaveChangesAsync();

        // 4. Grąžinti DTO
        return profile.Adapt<UserDto>();
    }
}

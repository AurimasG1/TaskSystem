using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserEmail;

public class GetUzduotysByUserEmailHandler
{
    private readonly IUzduotisRepository _taskRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUserProfileRepository _profileRepo;

    public GetUzduotysByUserEmailHandler(
        IUzduotisRepository taskRepo,
        IUserRepository userRepo,
        IUserProfileRepository profileRepo
    )
    {
        _taskRepo = taskRepo;
        _userRepo = userRepo;
        _profileRepo = profileRepo;
    }

    public async Task<IEnumerable<UzduotisDto>> Handle(GetUzduotysByUserEmailQuery request)
    {
        // 1. Login user
        var user =
            await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new UserNotFoundException($"User with email {request.Email} not found.");

        // 2. Domain profile
        var profile =
            await _profileRepo.GetByUserIdAsync(user.Id)
            ?? throw new UserProfileNotFoundException($"Profile for user {user.Id} not found.");

        // 3. Tasks by profile
        var tasks = await _taskRepo.GetByUserProfileIdAsync(profile.Id);

        // 4. Mapping
        return tasks.Select(t => new UzduotisDto(
            t.Id,
            t.TitleValue,
            t.Description,
            t.StatusId,
            t.UserProfileId,
            t.CreatedAt,
            t.UpdatedAt
        ));
    }
}

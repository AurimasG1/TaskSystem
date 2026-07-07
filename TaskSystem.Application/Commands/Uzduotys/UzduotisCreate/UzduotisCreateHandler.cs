using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Uzduotys.UzduotisCreate;

public class UzduotisCreateHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUserProfileRepository _profileRepo;

    public UzduotisCreateHandler(IRepository<Uzduotis> repo, IUserProfileRepository profileRepo)
    {
        _repo = repo;
        _profileRepo = profileRepo;
    }

    public async Task<UzduotisDto> Handle(UzduotisCreateCommand request)
    {
        // 1. Check if user profile exists
        var profile =
            await _profileRepo.GetByIdAsync(request.UserProfileId)
            ?? throw new UserProfileNotFoundException(request.UserProfileId);

        // 2. Create entity
        var task = new Uzduotis
        {
            UserProfileId = request.UserProfileId,
            Description = request.Description,
            StatusId = 1, // default
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        task.SetTitle(request.Title);

        // 3. Save
        await _repo.AddAsync(task);
        await _repo.SaveChangesAsync();

        // 4. Return DTO
        return new UzduotisDto(
            task.Id,
            task.TitleValue,
            task.Description,
            task.StatusId,
            task.UserProfileId,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}

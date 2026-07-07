using TaskSystem.Application.Commands.Uzduotys.UzduotisResetLast;
using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Uzduotys.ResetLast;

public class UzduotisResetLastHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUzduotisRepository _taskRepo;

    public UzduotisResetLastHandler(IRepository<Uzduotis> repo, IUzduotisRepository taskRepo)
    {
        _repo = repo;
        _taskRepo = taskRepo;
    }

    public async Task<UzduotisDto> Handle(UzduotisResetLastCommand request)
    {
        // 1. Load last task for this profile
        var task =
            await _taskRepo.GetLastByUserProfileIdAsync(request.UserProfileId)
            ?? throw new UzduotisNotFoundException(
                $"UserProfile {request.UserProfileId} has no tasks."
            );

        // 2. Apply domain reset logic
        task.Reset();

        // 3. Save
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

using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Uzduotys.UzduotisUpdate;

public class UzduotisUpdateHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUzduotisRepository _taskRepo;

    public UzduotisUpdateHandler(IRepository<Uzduotis> repo, IUzduotisRepository taskRepo)
    {
        _repo = repo;
        _taskRepo = taskRepo;
    }

    public async Task<UzduotisDto> Handle(UzduotisUpdateCommand request)
    {
        // 1. Load task
        var task =
            await _taskRepo.GetByIdForUpdateAsync(request.Id)
            ?? throw new UzduotisNotFoundException(request.Id);

        // 2. Ownership check
        if (task.UserProfileId != request.UserProfileId)
            throw new UnauthorizedAccessException("You cannot edit this task.");

        // 3. Update fields
        if (request.Title.HasValue)
        {
            if (request.Title.Value is null)
                throw new ArgumentException("Title cannot be null.");

            task.SetTitle(request.Title.Value);
        }

        if (request.Description.HasValue)
            task.Description = request.Description.Value;

        if (request.StatusId.HasValue)
            task.StatusId = request.StatusId.Value;

        task.UpdatedAt = DateTime.UtcNow;

        // 4. Save
        await _repo.SaveChangesAsync();

        // 5. Return DTO
        return new UzduotisDto(
            task.Id,
            task.Title.Value,
            task.Description,
            task.StatusId,
            task.UserProfileId,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}

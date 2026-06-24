using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Uzduotys.UpdateUzduotis;

public class UpdateUzduotisHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUzduotisRepository _taskRepo;

    public UpdateUzduotisHandler(IRepository<Uzduotis> repo, IUzduotisRepository taskRepo)
    {
        _repo = repo;
        _taskRepo = taskRepo;
    }

    public async Task<UzduotisDto> Handle(UpdateUzduotisCommand request)
    {
        // 1. Load task
        var task =
            await _taskRepo.GetByIdForUpdateAsync(request.Id)
            ?? throw new UzduotisNotFoundException(request.Id);

        // 2. Ownership check
        if (task.UserId != request.UserId)
            throw new UnauthorizedAccessException("You cannot edit this task.");

        // 3. Update fields
        task.Title = request.Title;
        task.Description = request.Description;
        task.StatusId = request.StatusId;
        task.UpdatedAt = DateTime.UtcNow;

        // 4. Save
        await _repo.SaveChangesAsync();

        // 5. Return DTO
        return new UzduotisDto(
            task.Id,
            task.Title,
            task.Description,
            task.StatusId,
            task.UserId,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}

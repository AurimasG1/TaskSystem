using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Uzduotys.ResetLastUzduotis;

public class ResetLastUzduotisHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUzduotisRepository _taskRepo;

    public ResetLastUzduotisHandler(IRepository<Uzduotis> repo, IUzduotisRepository taskRepo)
    {
        _repo = repo;
        _taskRepo = taskRepo;
    }

    public async Task<UzduotisDto> Handle(ResetLastUzduotisCommand request)
    {
        // 1. Load last task
        var task =
            await _taskRepo.GetLastByUserIdAsync(request.UserId)
            ?? throw new UzduotisNotFoundException($"User {request.UserId} has no tasks.");

        // 2. Reset logic
        task.Title = "(reset) " + task.Title;
        task.Description = null;
        task.StatusId = 1;
        task.UpdatedAt = DateTime.UtcNow;

        // 3. Save
        await _repo.SaveChangesAsync();

        // 4. Return DTO
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

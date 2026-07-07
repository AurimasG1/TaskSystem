using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Uzduotys.UzduotisDelete;

public class UzduotisDeleteHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUzduotisRepository _taskRepo;

    public UzduotisDeleteHandler(IRepository<Uzduotis> repo, IUzduotisRepository taskRepo)
    {
        _repo = repo;
        _taskRepo = taskRepo;
    }

    public async Task Handle(UzduotisDeleteCommand request)
    {
        var task =
            await _taskRepo.GetByIdForUpdateAsync(request.Id)
            ?? throw new UzduotisNotFoundException(request.Id);

        if (task.UserProfileId != request.UserProfileId)
            throw new UnauthorizedAccessException("You cannot delete this task.");

        await _repo.DeleteAsync(task);
        await _repo.SaveChangesAsync();
    }
}

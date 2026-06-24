using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Uzduotys.DeleteUzduotis;

public class DeleteUzduotisHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUzduotisRepository _taskRepo;

    public DeleteUzduotisHandler(IRepository<Uzduotis> repo, IUzduotisRepository taskRepo)
    {
        _repo = repo;
        _taskRepo = taskRepo;
    }

    public async Task Handle(DeleteUzduotisCommand request)
    {
        var task =
            await _taskRepo.GetByIdForUpdateAsync(request.Id)
            ?? throw new UzduotisNotFoundException(request.Id);

        if (task.UserId != request.UserId)
            throw new UnauthorizedAccessException("You cannot delete this task.");

        await _repo.DeleteAsync(task);
        await _repo.SaveChangesAsync();
    }
}

using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Uzduotys.UzduotisDelete;

public class UzduotisAdminDeleteHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUzduotisRepository _taskRepo;

    public UzduotisAdminDeleteHandler(IRepository<Uzduotis> repo, IUzduotisRepository taskRepo)
    {
        _repo = repo;
        _taskRepo = taskRepo;
    }

    public async Task Handle(UzduotisAdminDeleteCommand request)
    {
        var task =
            await _taskRepo.GetByIdForUpdateAsync(request.Id)
            ?? throw new UzduotisNotFoundException(request.Id);

        await _repo.DeleteAsync(task);
        await _repo.SaveChangesAsync();
    }
}

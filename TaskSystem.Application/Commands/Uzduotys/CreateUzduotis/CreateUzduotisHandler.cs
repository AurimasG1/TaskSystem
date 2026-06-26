using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Domain.ValueObjects;

namespace TaskSystem.Application.Commands.Uzduotys.CreateUzduotis;

public class CreateUzduotisHandler
{
    private readonly IRepository<Uzduotis> _repo;
    private readonly IUserRepository _userRepo;

    public CreateUzduotisHandler(IRepository<Uzduotis> repo, IUserRepository userRepo)
    {
        _repo = repo;
        _userRepo = userRepo;
    }

    public async Task<UzduotisDto> Handle(CreateUzduotisCommand request)
    {
        // 1. Check if user exists
        var user =
            await _userRepo.GetByIdAsync(request.UserId)
            ?? throw new UserNotFoundException(request.UserId);

        // 2. Create entity
        var task = new Uzduotis
        {
            UserId = request.UserId,
            Description = request.Description,
            StatusId = 1,
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
            task.Title.Value,
            task.Description,
            task.StatusId,
            task.UserId,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}

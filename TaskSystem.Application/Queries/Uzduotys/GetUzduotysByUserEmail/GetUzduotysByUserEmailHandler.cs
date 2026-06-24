using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserEmail;

public class GetUzduotysByUserEmailHandler
{
    private readonly IUzduotisRepository _taskRepo;
    private readonly IUserRepository _userRepo;

    public GetUzduotysByUserEmailHandler(IUzduotisRepository taskRepo, IUserRepository userRepo)
    {
        _taskRepo = taskRepo;
        _userRepo = userRepo;
    }

    public async Task<IEnumerable<UzduotisDto>> Handle(GetUzduotysByUserEmailQuery request)
    {
        var user =
            await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new UserNotFoundException($"User with email {request.Email} not found.");

        var tasks = await _taskRepo.GetByUserIdAsync(user.Id);

        return tasks.Select(t => new UzduotisDto(
            t.Id,
            t.Title,
            t.Description,
            t.StatusId,
            t.UserId,
            t.CreatedAt,
            t.UpdatedAt
        ));
    }
}

using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserId;

public class GetLastUzduotisByUserIdHandler
{
    private readonly IUzduotisRepository _repo;

    public GetLastUzduotisByUserIdHandler(IUzduotisRepository repo)
    {
        _repo = repo;
    }

    public async Task<UzduotisDto> Handle(GetLastUzduotisByUserIdQuery request)
    {
        var task =
            await _repo.GetLastByUserIdAsync(request.UserId)
            ?? throw new UzduotisNotFoundException($"User {request.UserId} has no tasks.");

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

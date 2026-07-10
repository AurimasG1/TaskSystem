using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserProfileId;

public class GetLastUzduotisByUserProfileIdHandler
{
    private readonly IUzduotisRepository _repo;

    public GetLastUzduotisByUserProfileIdHandler(IUzduotisRepository repo)
    {
        _repo = repo;
    }

    public async Task<UzduotisDto> Handle(GetLastUzduotisByUserProfileIdQuery request)
    {
        var task =
            await _repo.GetLastByUserProfileIdAsync(request.UserProfileId)
            ?? throw new UzduotisNotFoundException($"User {request.UserProfileId} has no tasks.");

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

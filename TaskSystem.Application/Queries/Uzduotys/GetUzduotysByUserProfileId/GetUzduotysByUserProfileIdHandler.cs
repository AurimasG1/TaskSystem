using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserProfileId;

public class GetUzduotysByUserProfileIdHandler
{
    private readonly IUzduotisRepository _repo;

    public GetUzduotysByUserProfileIdHandler(IUzduotisRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<UzduotisDto>> Handle(GetUzduotysByUserProfileIdQuery request)
    {
        var tasks = await _repo.GetByUserProfileIdAsync(request.UserProfileId);

        return tasks.Select(t => new UzduotisDto(
            t.Id,
            t.Title.Value,
            t.Description,
            t.StatusId,
            t.UserProfileId,
            t.CreatedAt,
            t.UpdatedAt
        ));
    }
}

using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetAllUzduotys;

public class GetAllUzduotysHandler
{
    private readonly IUzduotisRepository _repo;

    public GetAllUzduotysHandler(IUzduotisRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UzduotisDto>> Handle(GetAllUzduotysQuery request)
    {
        var list = await _repo.GetAllAsync();

        return list.Select(u => new UzduotisDto(
                u.Id,
                u.TitleValue,
                u.Description,
                u.StatusId,
                u.UserProfileId,
                u.CreatedAt,
                u.UpdatedAt
            ))
            .ToList();
    }
}

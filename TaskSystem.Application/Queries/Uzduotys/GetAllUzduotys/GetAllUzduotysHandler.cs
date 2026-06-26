using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetAllUzduotys;

public class GetAllUzduotysHandler
{
    private readonly IUzduotisRepository _repo;

    public GetAllUzduotysHandler(IUzduotisRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<UzduotisDto>> Handle(GetAllUzduotysQuery request)
    {
        var tasks = await _repo.GetAllAsync();

        return tasks.Select(t => new UzduotisDto(
            t.Id,
            t.Title.Value,
            t.Description,
            t.StatusId,
            t.UserId,
            t.CreatedAt,
            t.UpdatedAt
        ));
    }
}

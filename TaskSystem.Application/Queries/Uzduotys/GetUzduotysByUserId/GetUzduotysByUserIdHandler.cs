using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetUzduotysByUserId;

public class GetUzduotysByUserIdHandler
{
    private readonly IUzduotisRepository _repo;

    public GetUzduotysByUserIdHandler(IUzduotisRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<UzduotisDto>> Handle(GetUzduotysByUserIdQuery request)
    {
        var tasks = await _repo.GetByUserIdAsync(request.UserId);

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

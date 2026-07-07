using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetTopUzduotys;

public class GetTopUzduotysHandler
{
    private readonly IUzduotisRepository _taskRepo;

    public GetTopUzduotysHandler(IUzduotisRepository taskRepo)
    {
        _taskRepo = taskRepo;
    }

    public async Task<List<UzduotisDto>> Handle(GetTopUzduotysQuery request)
    {
        var tasks = await _taskRepo.GetTopAsync(request.Count);

        return tasks
            .Select(t => new UzduotisDto(
                t.Id,
                t.Title.Value,
                t.Description,
                t.StatusId,
                t.UserProfileId,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToList();
    }
}

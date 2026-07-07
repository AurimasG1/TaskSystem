using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Queries.Uzduotys.GetUzduotisById;

public class GetUzduotisByIdHandler
{
    private readonly IUzduotisRepository _repo;

    public GetUzduotisByIdHandler(IUzduotisRepository repo)
    {
        _repo = repo;
    }

    public async Task<UzduotisDto> Handle(GetUzduotisByIdQuery request)
    {
        var task =
            await _repo.GetByIdAsync(request.Id) ?? throw new UzduotisNotFoundException(request.Id);

        return new UzduotisDto(
            task.Id,
            task.TitleValue,
            task.Description,
            task.StatusId,
            task.UserProfileId,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}

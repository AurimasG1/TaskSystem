namespace TaskSystem.Application.DTO.Requests.Uzduotys
{
    public record UzduotisUpdateRequest(string Title, string? Description, int StatusId);
}

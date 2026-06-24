namespace TaskSystem.Application.Commands.Uzduotys.UpdateUzduotis
{
    public record UpdateUzduotisCommand(
        int Id,
        string Title,
        string? Description,
        int StatusId,
        int UserId
    );
}

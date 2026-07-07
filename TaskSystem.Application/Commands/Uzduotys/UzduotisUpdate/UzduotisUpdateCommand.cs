using TaskSystem.Application.Common;

namespace TaskSystem.Application.Commands.Uzduotys.UzduotisUpdate
{
    public record UzduotisUpdateCommand(
        int Id,
        Optional<string> Title,
        Optional<string> Description,
        Optional<int> StatusId,
        int UserProfileId
    );
}

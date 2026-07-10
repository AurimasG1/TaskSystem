using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.UserDelete;

public class UserDeleteHandler
{
    private readonly IUserRepository _userRepo;

    public UserDeleteHandler(IUserRepository userRepo, IRepository<User> repo)
    {
        _userRepo = userRepo;
    }

    public async Task Handle(UserDeleteCommand request)
    {
        // 1. Load user with tracking
        var user =
            await _userRepo.GetByIdForUpdateAsync(request.UserId)
            ?? throw new UserNotFoundException(request.UserId);

        // 2. Delete
        await _userRepo.DeleteAsync(user);

        // 3. Save
        await _userRepo.SaveChangesAsync();
    }
}

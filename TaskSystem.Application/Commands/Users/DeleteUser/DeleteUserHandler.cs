using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.DeleteUser;

public class DeleteUserHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IRepository<User> _repo;

    public DeleteUserHandler(IUserRepository userRepo, IRepository<User> repo)
    {
        _userRepo = userRepo;
        _repo = repo;
    }

    public async Task Handle(DeleteUserCommand request)
    {
        // 1. Load user with tracking
        var user =
            await _userRepo.GetByIdForUpdateAsync(request.UserId)
            ?? throw new UserNotFoundException(request.UserId);

        // 2. Delete
        await _repo.DeleteAsync(user);

        // 3. Save
        await _repo.SaveChangesAsync();
    }
}

using TaskSystem.Application.DTO.Users;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.UpdateUser;

public class UpdateUserHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IRepository<User> _repo;

    public UpdateUserHandler(IUserRepository userRepo, IRepository<User> repo)
    {
        _userRepo = userRepo;
        _repo = repo;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request)
    {
        // 1. Get user for update (tracking ON)
        var user =
            await _userRepo.GetByIdForUpdateAsync(request.Id)
            ?? throw new UserNotFoundException(request.Id);

        // 2. Check if email is taken by another user
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing != null && existing.Id != request.Id)
            throw new UserAlreadyExistsException(request.Email);

        // 3. Update fields
        user.Email = request.Email.ToLowerInvariant().Trim();
        user.UserName = request.UserName;

        // 4. Save changes
        await _repo.SaveChangesAsync();

        // 5. Return DTO
        return new UserDto(user.Id, user.Email, user.Role, user.UserName);
    }
}

using Mapster;
using TaskSystem.Application.DTO.Responses.Users;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.UserUpdate;

public class UserAdminUpdateHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IRepository<User> _repo;

    public UserAdminUpdateHandler(IUserRepository userRepo, IRepository<User> repo)
    {
        _userRepo = userRepo;
        _repo = repo;
    }

    public async Task<UserDto> Handle(UserAdminUpdateCommand request)
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
        request.Adapt(user);

        // 4. Save changes
        await _repo.SaveChangesAsync();

        // 5. Return DTO
        return user.Adapt<UserDto>();
    }
}

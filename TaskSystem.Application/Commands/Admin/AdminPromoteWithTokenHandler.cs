// using TaskSystem.Domain.Interfaces;

// namespace TaskSystem.Application.Commands.Admin
// {
//     public class PromoteAdminWithTokenHandler : IRequestHandler<PromoteAdminWithTokenCommand, bool>
//     {
//         private readonly IAdminPromotionTokenRepository _tokenRepo;
//         private readonly IUserRepository _userRepo;
//         private readonly ICurrentUserService _currentUser;

//         public PromoteAdminWithTokenHandler(
//             IAdminPromotionTokenRepository tokenRepo,
//             IUserRepository userRepo,
//             ICurrentUserService currentUser
//         )
//         {
//             _tokenRepo = tokenRepo;
//             _userRepo = userRepo;
//             _currentUser = currentUser;
//         }

//         public async Task<bool> Handle(PromoteAdminWithTokenCommand request, CancellationToken ct)
//         {
//             var token = await _tokenRepo.GetAsync(request.Token);

//             if (token == null || token.Used || token.ExpiresAt < DateTime.UtcNow)
//                 return false;

//             var user = await _userRepo.GetByIdForUpdateAsync(_currentUser.UserId);

//             if (user == null)
//                 return false;

//             user.Role = "admin";
//             token.Used = true;

//             await _userRepo.SaveChangesAsync();
//             await _tokenRepo.SaveChangesAsync();

//             return true;
//         }
//     }
// }

using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Admin;

public class AdminPromoteWithTokenHandler
{
    private readonly IAdminPromotionTokenRepository _tokenRepo;
    private readonly IUserRepository _userRepo;

    public AdminPromoteWithTokenHandler(
        IAdminPromotionTokenRepository tokenRepo,
        IUserRepository userRepo
    )
    {
        _tokenRepo = tokenRepo;
        _userRepo = userRepo;
    }

    public async Task<bool> Handle(PromoteAdminWithTokenCommand request)
    {
        var token = await _tokenRepo.GetAsync(request.Token);

        if (token == null || token.Used || token.ExpiresAt < DateTime.UtcNow)
            return false;

        var user = await _userRepo.GetByIdForUpdateAsync(request.UserId);

        if (user == null)
            return false;

        user.Role = "admin";
        token.Used = true;

        await _userRepo.SaveChangesAsync();
        await _tokenRepo.SaveChangesAsync();

        return true;
    }
}

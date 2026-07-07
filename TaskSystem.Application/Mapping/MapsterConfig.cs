using Mapster;
using TaskSystem.Application.Commands.Auth.AuthLogin;
using TaskSystem.Application.Commands.Auth.AuthRegister;
using TaskSystem.Application.Commands.Users.UserRegister;
using TaskSystem.Application.DTO.Requests.Auth;
using TaskSystem.Application.DTO.Requests.Users;
using TaskSystem.Application.DTO.Responses.Users;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Mapping;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        // Prevent Mapster from touching User entity
        TypeAdapterConfig<UserRegisterRequest, User>.NewConfig().Ignore("*");
        TypeAdapterConfig<AuthRegisterRequest, User>.NewConfig().Ignore("*");
        TypeAdapterConfig<AuthRegisterCommand, User>.NewConfig().Ignore("*");

        // DTO → Command
        TypeAdapterConfig<UserRegisterRequest, UserRegisterCommand>.NewConfig();

        // Command → Entity (UserProfile)
        TypeAdapterConfig<UserRegisterCommand, UserProfile>.NewConfig().Ignore(dest => dest.Id);

        // Entity → DTO
        TypeAdapterConfig<User, UserDto>
            .NewConfig()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Email, src => src.EmailValue)
            .Map(dest => dest.Role, src => src.Role)
            .Map(dest => dest.ProfileId, src => src.Profile.Id)
            .Map(dest => dest.FirstName, src => src.Profile.FirstName)
            .Map(dest => dest.LastName, src => src.Profile.LastName);

        TypeAdapterConfig<UserProfile, UserDto>
            .NewConfig()
            .Map(dest => dest.ProfileId, src => src.Id)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName);

        TypeAdapterConfig<AuthRegisterRequest, AuthRegisterCommand>.NewConfig();
        TypeAdapterConfig<AuthLoginRequest, AuthLoginCommand>.NewConfig();
    }
}

using AutoMapper;
using TaskSystem.Application.DTO;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Mapping;

public class UzduotisProfile : Profile
{
    public UzduotisProfile()
    {
        CreateMap<Uzduotis, UzduotisResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name));
    }
}

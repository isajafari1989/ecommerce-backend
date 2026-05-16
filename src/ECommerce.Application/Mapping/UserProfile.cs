using AutoMapper;
using ECommerce.Domain.Entities;
using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        //return to client
        CreateMap<User, UserDto>();

        //DTO -> Domain
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

    }
}
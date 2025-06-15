using AutoMapper;
using Backend.Application.Models;
using Backend.Domain.Entities;
using Backend.Infrastructure.Services;

namespace Backend.Configurations.Mappers;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Genre, opt => opt.MapFrom((src, dest, _, ctx) => 
            {
                var deezerApiClient = ctx.Items["DeezerApiClient"] as DeezerApiClient;
                return deezerApiClient?.ToDisplayString(src.Genre) ?? src.Genre.ToString();
            }))
            .ForMember(dest => dest.PlayersCount, opt => opt.MapFrom(src => src.Players.Count));
    }
}
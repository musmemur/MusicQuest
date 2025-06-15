using AutoMapper;
using Backend.Application.Models;
using Backend.Domain.Entities;

namespace Backend.Configurations.Mappers;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserWithPlaylistsDto>()
            .ForMember(dest => dest.Playlists, opt => opt.MapFrom(src => src.Playlists));

        CreateMap<Playlist, PlaylistDto>()
            .ForMember(dest => dest.Tracks, opt => opt.MapFrom(src => 
                src.PlaylistTracks.Select(pt => pt.Track)));

        CreateMap<Track, TrackDto>();
    }
}
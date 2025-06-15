using Backend.Configurations.Options;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Infrastructure.DataBase;
using Backend.Infrastructure.DataBase.GameSessions;
using Backend.Infrastructure.DataBase.Players;
using Backend.Infrastructure.DataBase.Playlists;
using Backend.Infrastructure.DataBase.PlaylistTracks;
using Backend.Infrastructure.DataBase.QuizQuestions;
using Backend.Infrastructure.DataBase.Rooms;
using Backend.Infrastructure.DataBase.Tracks;
using Backend.Infrastructure.DataBase.Users;
using Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;

namespace Backend.Configurations.DI;

public static class InfrastructureDependencies
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddScoped<AppDbContext>();
        services.AddScoped<ImageSaver>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        services.AddScoped<IPlaylistTrackRepository, PlaylistTrackRepository>();
        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        
        services.AddHttpClient<DeezerApiClient>();
        services.Configure<MinioSettings>(configuration.GetSection("Minio"));
        
        return services;
    }
}
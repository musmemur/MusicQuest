using Backend.Api.Models;
using Backend.Application.Abstractions;
using Backend.Application.Services;
using Backend.Configurations.Mappers;
using Backend.Configurations.Validators;
using FluentValidation;

namespace Backend.Configurations.DI;

public static class ApplicationDependencies
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IGameSessionService, GameSessionService>();
        services.AddScoped<IQuizQuestionService, QuizQuestionService>();
        services.AddScoped<IPlaylistService, PlaylistService>();
        services.AddAutoMapper(typeof(RoomProfile).Assembly, typeof(UserProfile).Assembly);
        
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>();
        services.AddScoped<IValidator<CreateRoomRequest>, CreateRoomValidator>();
        
        return services;
    }
}
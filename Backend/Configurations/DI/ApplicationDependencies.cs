using Backend.Api.Models;
using Backend.Api.Services;
using Backend.Application.Services;
using Backend.Configurations.Mappers;
using Backend.Configurations.Validators;
using FluentValidation;

namespace Backend.Configurations.DI;

public static class ApplicationDependencies
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<RoomService>();
        services.AddScoped<GameSessionService>();
        services.AddScoped<QuizQuestionService>();
        services.AddScoped<PlaylistService>();
        services.AddAutoMapper(typeof(RoomProfile).Assembly);
        services.AddAutoMapper(typeof(UserProfile).Assembly);
        
        // Валидаторы
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>();
        services.AddScoped<IValidator<CreateRoomRequest>, CreateRoomValidator>();
        
        return services;
    }
}
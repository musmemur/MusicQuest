using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend;
using Backend.Api.Hubs;
using Backend.Api.Services;
using Backend.Configurations.Options;
using Backend.Configurations.Validators;
using Backend.DataBase;
using Backend.Domain.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Services;
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
using Backend.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите токен в формате: Bearer {ваш_токен}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        []
    }});
});
builder.Services.AddControllers();
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ImageSaver>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<GameSessionService>();
builder.Services.AddScoped<QuizQuestionService>();
builder.Services.AddScoped<PlaylistService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();
builder.Services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<IPlaylistTrackRepository, PlaylistTrackRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>();
builder.Services.AddScoped<IValidator<CreateRoomRequest>, CreateRoomValidator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserService>();
builder.Services.AddHttpClient<DeezerApiClient>();
builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));
builder.Services
    .AddSignalR()
    .AddJsonProtocol()
    .AddHubOptions<QuizHub>(options => {
        options.EnableDetailedErrors = true;
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseWebSockets();
app.MapHub<QuizHub>("/quizhub");
using var scope = app.Services.CreateScope();
await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
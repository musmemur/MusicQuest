using Backend.Api.Hubs;
using Backend.Configurations.DI;
using Backend.Configurations.Swagger;
using Backend.Infrastructure.DataBase;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services
    .AddCustomSwagger()
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

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
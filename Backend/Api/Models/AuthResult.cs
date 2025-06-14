namespace Backend.Api.Models;

public record AuthResult(string Token, Guid UserId, string Username, string? UserPhoto);
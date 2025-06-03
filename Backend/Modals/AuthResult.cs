namespace Backend.Modals;

public record AuthResult(string Token, Guid UserId, string Username, string? UserPhoto);
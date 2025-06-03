namespace Backend.Modals;

public record CreateUserRequest(string Username, string Password, Photo? UserPhoto);
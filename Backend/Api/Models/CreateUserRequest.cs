namespace Backend.Api.Models;

public record CreateUserRequest(string Username, string Password, Photo? UserPhoto);
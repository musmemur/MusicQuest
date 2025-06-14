namespace Backend.Api.Models;

public record CreateRoomRequest(string Genre, int QuestionCount, Guid UserHostId);

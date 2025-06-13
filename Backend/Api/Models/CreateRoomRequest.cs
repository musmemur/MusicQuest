namespace Backend.Models;

public class CreateRoomRequest
{
    public string Genre { get; set; }
    public int QuestionCount { get; set; }
    public Guid UserHostId { get; set; }
}
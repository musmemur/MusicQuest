namespace Backend.Dto;

public class CreateRoomDto
{
    public string Genre { get; set; }
    public int QuestionCount { get; set; }
    public Guid UserHostId { get; set; }
}
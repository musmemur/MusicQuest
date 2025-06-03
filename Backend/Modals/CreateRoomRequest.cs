namespace Backend.Modals;

public class CreateRoomRequest
{
    public string Genre { get; set; }
    public int QuestionsCount { get; set; } = 5;
}
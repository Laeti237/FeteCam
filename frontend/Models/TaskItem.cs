namespace FeteCam.Models;

public class TaskItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Responsible { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public int? ServerId { get; set; }
    public int? ServerEventId { get; set; }
    public string Status { get; set; } = "À faire";
    public string StatusColor { get; set; } = "#F9DEDC";
    public string StatusTextColor { get; set; } = "#B3261E";
    public bool IsDone { get; set; }
}

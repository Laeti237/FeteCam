namespace FeteCam.Models;

public class EventItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = "En préparation";
    public string StatusColor { get; set; } = "#FBEBCB";
    public string StatusTextColor { get; set; } = "#8A5200";
    public int GuestsConfirmed { get; set; }
    public int GuestsTotal { get; set; }
    public decimal Budget { get; set; }
    public decimal BudgetSpent { get; set; }
    public int TasksDone { get; set; }
    public int TasksTotal { get; set; }
}

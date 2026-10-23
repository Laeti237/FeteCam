namespace FeteCam.Models;

public class BudgetItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Category { get; set; } = string.Empty;
    public decimal Allocated { get; set; }
    public decimal Spent { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public bool IsOverBudget => Spent > Allocated;
}

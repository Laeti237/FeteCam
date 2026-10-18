namespace FeteCam.Models;

public class ContributionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ContributorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
}

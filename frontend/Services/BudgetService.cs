using System.Collections.ObjectModel;
using FeteCam.Models;

namespace FeteCam.Services;

public class BudgetService
{
    private static BudgetService? _instance;
    public static BudgetService Instance => _instance ??= new BudgetService();

    public ObservableCollection<BudgetItem> Items { get; } = new();

    private BudgetService()
    {
        var mariage = EventService.Instance.Events[0];
        var anniversaire = EventService.Instance.Events[1];
        var veillee = EventService.Instance.Events[2];

        Items.Add(new BudgetItem { Category = "Traiteur", Allocated = 700000, Spent = 600000, EventId = mariage.Id, EventName = mariage.Name });
        Items.Add(new BudgetItem { Category = "Salle", Allocated = 400000, Spent = 400000, EventId = mariage.Id, EventName = mariage.Name });
        Items.Add(new BudgetItem { Category = "Boissons", Allocated = 200000, Spent = 210000, EventId = mariage.Id, EventName = mariage.Name });
        Items.Add(new BudgetItem { Category = "Décoration", Allocated = 250000, Spent = 90000, EventId = mariage.Id, EventName = mariage.Name });
        Items.Add(new BudgetItem { Category = "Musique", Allocated = 150000, Spent = 60000, EventId = mariage.Id, EventName = mariage.Name });

        Items.Add(new BudgetItem { Category = "Gâteau", Allocated = 60000, Spent = 45000, EventId = anniversaire.Id, EventName = anniversaire.Name });
        Items.Add(new BudgetItem { Category = "Décoration", Allocated = 80000, Spent = 55000, EventId = anniversaire.Id, EventName = anniversaire.Name });
        Items.Add(new BudgetItem { Category = "Animation", Allocated = 150000, Spent = 90000, EventId = anniversaire.Id, EventName = anniversaire.Name });

        Items.Add(new BudgetItem { Category = "Fleurs", Allocated = 40000, Spent = 15000, EventId = veillee.Id, EventName = veillee.Name });
        Items.Add(new BudgetItem { Category = "Transport", Allocated = 50000, Spent = 30000, EventId = veillee.Id, EventName = veillee.Name });
        Items.Add(new BudgetItem { Category = "Café & jus", Allocated = 30000, Spent = 12000, EventId = veillee.Id, EventName = veillee.Name });
    }

    public void AddExpense(string category, decimal amount, string eventId, string eventName)
    {
        if (string.IsNullOrWhiteSpace(category) || amount <= 0) return;

        var item = Items.FirstOrDefault(i =>
            i.EventId == eventId &&
            string.Equals(i.Category, category, StringComparison.OrdinalIgnoreCase));

        if (item != null)
        {
            item.Spent += amount;
        }
        else
        {
            Items.Add(new BudgetItem
            {
                Category = category.Trim(),
                Allocated = 0,
                Spent = amount,
                EventId = eventId,
                EventName = eventName
            });
        }
    }

    public IReadOnlyList<string> CategoriesFor(string eventId) =>
        Items.Where(i => i.EventId == eventId)
             .Select(i => i.Category)
             .Distinct(StringComparer.OrdinalIgnoreCase)
             .ToList();
}

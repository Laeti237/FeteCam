using System.Collections.ObjectModel;
using FeteCam.Models;

namespace FeteCam.Services;

public class EventService
{
    private static EventService? _instance;
    public static EventService Instance => _instance ??= new EventService();

    public ObservableCollection<EventItem> Events { get; } = new();

    public EventItem Current { get; set; }

    private EventService()
    {
        var mariage = new EventItem
        {
            Name = "Mariage Alain & Rose",
            Date = "14 déc.",
            Location = "Douala",
            Status = "Confirmé",
            StatusColor = "#10B981",
            StatusTextColor = "#FFFFFF"
        };
        var anniversaire = new EventItem
        {
            Name = "Anniversaire 50 ans",
            Date = "28 nov.",
            Location = "Yaoundé",
            Status = "En préparation",
            StatusColor = "#FBEBCB",
            StatusTextColor = "#8A5200"
        };
        var veillee = new EventItem
        {
            Name = "Veillée Mama Thérèse",
            Date = "02 nov.",
            Location = "Bafoussam",
            Status = "Bientôt",
            StatusColor = "#FEE2E2",
            StatusTextColor = "#B91C1C"
        };

        Events.Add(mariage);
        Events.Add(anniversaire);
        Events.Add(veillee);
        Current = mariage;
    }

    public EventItem? GetById(string id) => Events.FirstOrDefault(e => e.Id == id);

    public EventItem? GetByName(string name) =>
        Events.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
}

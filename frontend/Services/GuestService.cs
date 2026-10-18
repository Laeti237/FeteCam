using System.Collections.ObjectModel;
using FeteCam.Models;

namespace FeteCam.Services;

public class GuestService
{
    private static GuestService? _instance;
    public static GuestService Instance => _instance ??= new GuestService();

    public ObservableCollection<GuestItem> Guests { get; } = new();

    private GuestService()
    {
        Guests.Add(new GuestItem { Name = "Marie Ngono", Phone = "+237 671 22 33 44", PersonCount = 2, Status = "Confirmé", StatusColor = "#10B981", StatusTextColor = "#FFFFFF" });
        Guests.Add(new GuestItem { Name = "Paul Tchoumi", Phone = "+237 699 12 34 56", PersonCount = 1, Status = "En attente", StatusColor = "#F59E0B", StatusTextColor = "#FFFFFF" });
        Guests.Add(new GuestItem { Name = "Famille Kamga", Phone = "+237 655 88 99 00", PersonCount = 5, Status = "Confirmé", StatusColor = "#10B981", StatusTextColor = "#FFFFFF" });
        Guests.Add(new GuestItem { Name = "Sylvie Eto'o", Phone = "+237 691 00 11 22", PersonCount = 1, Status = "Absent", StatusColor = "#EF4444", StatusTextColor = "#FFFFFF" });
        Guests.Add(new GuestItem { Name = "Jean Fotso", Phone = "+237 655 44 33 22", PersonCount = 2, Status = "En attente", StatusColor = "#F59E0B", StatusTextColor = "#FFFFFF" });
    }

    public void Add(string name, string phone = "", int personCount = 1, string status = "En attente")
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        Guests.Add(new GuestItem
        {
            Name = name.Trim(),
            Phone = phone.Trim(),
            PersonCount = personCount,
            Status = status
        });
    }

    public bool Remove(string id)
    {
        var item = Guests.FirstOrDefault(r => r.Id == id);
        if (item != null)
        {
            return Guests.Remove(item);
        }
        return false;
    }
}
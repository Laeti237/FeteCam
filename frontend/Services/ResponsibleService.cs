using System.Collections.ObjectModel;
using FeteCam.Models;

namespace FeteCam.Services;

public class ResponsibleService
{
    private static ResponsibleService? _instance;
    public static ResponsibleService Instance => _instance ??= new ResponsibleService();

    public ObservableCollection<ResponsibleItem> Responsibles { get; } = new();

    private ResponsibleService()
    {
        Responsibles.Add(new ResponsibleItem { Name = "Djon", Role = "Coordinateur principal", Phone = "+237 670 11 22 33", TaskCount = 2 });
        Responsibles.Add(new ResponsibleItem { Name = "Laeticia", Role = "Protocole & Accueil", Phone = "+237 690 44 55 66", TaskCount = 1 });
        Responsibles.Add(new ResponsibleItem { Name = "Patricia", Role = "Logistique & Boissons", Phone = "+237 655 77 88 99", TaskCount = 1 });
    }

    public ResponsibleItem? FindByName(string name)
        => Responsibles.FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

    public void Add(string name, string role = "Membre du comité", string phone = "")
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        Responsibles.Add(new ResponsibleItem
        {
            Name = name.Trim(),
            Role = string.IsNullOrWhiteSpace(role) ? "Membre du comité" : role.Trim(),
            Phone = phone.Trim()
        });
    }

    public bool Remove(string id)
    {
        var item = Responsibles.FirstOrDefault(r => r.Id == id);
        return item != null && Responsibles.Remove(item);
    }
}

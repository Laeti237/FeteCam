using System.Collections.ObjectModel;
using FeteCam.Models;

namespace FeteCam.Services;

public class GuestService
{
    private static GuestService? _instance;
    public static GuestService Instance => _instance ??= new GuestService();

    public ObservableCollection<GuestItem> Guests { get; } = new();

    public static readonly string[] Statuses = { "Confirmé", "En attente", "Absent" };

    private bool _refreshing;

    private GuestService()
    {
        var mariage = EventService.Instance.Events[0];
        var anniversaire = EventService.Instance.Events[1];
        var veillee = EventService.Instance.Events[2];

        Guests.Add(new GuestItem { Name = "Marie Ngono", Phone = "+237 671 22 33 44", PersonCount = 2, Status = "Confirmé", StatusColor = "#10B981", StatusTextColor = "#FFFFFF", EventId = mariage.Id, EventName = mariage.Name });
        Guests.Add(new GuestItem { Name = "Paul Tchoumi", Phone = "+237 699 12 34 56", PersonCount = 1, Status = "En attente", StatusColor = "#F59E0B", StatusTextColor = "#FFFFFF", EventId = mariage.Id, EventName = mariage.Name });
        Guests.Add(new GuestItem { Name = "Famille Kamga", Phone = "+237 655 88 99 00", PersonCount = 5, Status = "Confirmé", StatusColor = "#10B981", StatusTextColor = "#FFFFFF", EventId = mariage.Id, EventName = mariage.Name });
        Guests.Add(new GuestItem { Name = "Sylvie Eto'o", Phone = "+237 691 00 11 22", PersonCount = 1, Status = "Absent", StatusColor = "#EF4444", StatusTextColor = "#FFFFFF", EventId = mariage.Id, EventName = mariage.Name });
        Guests.Add(new GuestItem { Name = "Jean Fotso", Phone = "+237 655 44 33 22", PersonCount = 2, Status = "En attente", StatusColor = "#F59E0B", StatusTextColor = "#FFFFFF", EventId = anniversaire.Id, EventName = anniversaire.Name });
        Guests.Add(new GuestItem { Name = "Clarisse Mballa", Phone = "+237 677 55 66 77", PersonCount = 3, Status = "Confirmé", StatusColor = "#10B981", StatusTextColor = "#FFFFFF", EventId = anniversaire.Id, EventName = anniversaire.Name });
        Guests.Add(new GuestItem { Name = "Samuel Njoya", Phone = "+237 690 11 22 33", PersonCount = 4, Status = "En attente", StatusColor = "#F59E0B", StatusTextColor = "#FFFFFF", EventId = veillee.Id, EventName = veillee.Name });
        Guests.Add(new GuestItem { Name = "Rachel Bella", Phone = "+237 655 77 88 99", PersonCount = 2, Status = "Confirmé", StatusColor = "#10B981", StatusTextColor = "#FFFFFF", EventId = veillee.Id, EventName = veillee.Name });
    }

    private static void ApplyStatusColors(GuestItem item)
    {
        switch (item.Status)
        {
            case "Confirmé":
                item.StatusColor = "#10B981";
                break;
            case "Absent":
                item.StatusColor = "#EF4444";
                break;
            default:
                item.StatusColor = "#F59E0B";
                break;
        }
        item.StatusTextColor = "#FFFFFF";
    }

    public async Task RefreshAsync()
    {
        if (!ApiService.IsAuthenticated || _refreshing) return;
        _refreshing = true;
        try
        {
            foreach (var evt in EventService.Instance.Events.ToList())
            {
                var sid = await ApiService.Instance.EnsureEventIdAsync(evt.Name, evt.Location, evt.Status);
                if (sid is null) break;

                var remote = await ApiService.Instance.GetGuestsAsync(sid.Value);
                if (remote is null) break;

                foreach (var rg in remote)
                {
                    var local = Guests.FirstOrDefault(g => g.ServerId == rg.Id)
                        ?? Guests.FirstOrDefault(g =>
                            g.EventId == evt.Id && g.ServerId is null &&
                            string.Equals(g.Name, rg.Name, StringComparison.OrdinalIgnoreCase));

                    if (local is null)
                    {
                        var item = new GuestItem
                        {
                            Name = rg.Name,
                            Phone = rg.Phone,
                            PersonCount = rg.PersonCount,
                            Status = rg.Status,
                            EventId = evt.Id,
                            EventName = evt.Name,
                            ServerId = rg.Id,
                            ServerEventId = sid
                        };
                        ApplyStatusColors(item);
                        Guests.Add(item);
                    }
                    else
                    {
                        local.ServerId = rg.Id;
                        local.ServerEventId = sid;
                        local.Phone = rg.Phone;
                        local.PersonCount = rg.PersonCount;
                        local.Status = rg.Status;
                        ApplyStatusColors(local);
                    }
                }

                foreach (var g in Guests.Where(g => g.EventId == evt.Id && g.ServerId is null).ToList())
                {
                    var created = await ApiService.Instance.CreateGuestAsync(
                        sid.Value, g.Name, g.Phone, g.PersonCount, g.Status);
                    if (created is null) break;
                    g.ServerId = created.Id;
                    g.ServerEventId = sid;
                }
            }
        }
        finally
        {
            _refreshing = false;
        }
    }

    public async Task AddAsync(string name, string phone = "", int personCount = 1, string status = "En attente", string eventId = "", string eventName = "")
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        int? serverEventId = null;
        int? serverId = null;

        if (ApiService.IsAuthenticated)
        {
            var evt = EventService.Instance.GetByName(eventName);
            serverEventId = await ApiService.Instance.EnsureEventIdAsync(
                eventName,
                evt?.Location ?? "",
                evt?.Status ?? status);
            if (serverEventId is not null)
            {
                var created = await ApiService.Instance.CreateGuestAsync(
                    serverEventId.Value, name.Trim(), phone.Trim(), personCount, status);
                serverId = created?.Id;
            }
        }

        var item = new GuestItem
        {
            Name = name.Trim(),
            Phone = phone.Trim(),
            PersonCount = personCount,
            Status = status,
            EventId = eventId,
            EventName = eventName,
            ServerId = serverId,
            ServerEventId = serverId is null ? null : serverEventId
        };
        ApplyStatusColors(item);
        Guests.Add(item);
    }

    public async Task SetStatusAsync(GuestItem item, string status)
    {
        if (string.IsNullOrWhiteSpace(status) || item.Status == status) return;

        item.Status = status;
        ApplyStatusColors(item);

        if (ApiService.IsAuthenticated && item.ServerId is int gid)
        {
            var evt = EventService.Instance.GetByName(item.EventName);
            var seid = item.ServerEventId
                ?? await ApiService.Instance.EnsureEventIdAsync(
                    item.EventName,
                    evt?.Location ?? "",
                    evt?.Status ?? "En préparation");

            if (seid is int eventId)
                await ApiService.Instance.UpdateGuestAsync(
                    eventId, gid, item.Name, item.Phone, item.PersonCount, item.Status);
        }
    }

    public async Task RemoveAsync(string id)
    {
        var item = Guests.FirstOrDefault(g => g.Id == id);
        if (item is null) return;

        Guests.Remove(item);

        if (item.ServerId is int gid && item.ServerEventId is int seid)
            await ApiService.Instance.DeleteGuestAsync(seid, gid);
    }

    public void Add(string name, string phone = "", int personCount = 1, string status = "En attente", string eventId = "", string eventName = "")
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        Guests.Add(new GuestItem
        {
            Name = name.Trim(),
            Phone = phone.Trim(),
            PersonCount = personCount,
            Status = status,
            EventId = eventId,
            EventName = eventName
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

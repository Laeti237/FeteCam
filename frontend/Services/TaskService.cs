using System.Collections.ObjectModel;
using FeteCam.Models;

namespace FeteCam.Services;

public class TaskService
{
    private static TaskService? _instance;
    public static TaskService Instance => _instance ??= new TaskService();

    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public static readonly string[] Statuses = { "À faire", "En cours", "Terminé" };

    private bool _refreshing;

    private TaskService()
    {
        var mariage = EventService.Instance.Events[0];
        var anniversaire = EventService.Instance.Events[1];
        var veillee = EventService.Instance.Events[2];

        Tasks.Add(new TaskItem { Title = "Réserver la salle", Responsible = "Djon", DueDate = "Avant le 20 nov.", Status = "À faire", StatusColor = "#F9DEDC", StatusTextColor = "#B3261E", EventId = mariage.Id, EventName = mariage.Name });
        Tasks.Add(new TaskItem { Title = "Commander les boissons", Responsible = "Djon", DueDate = "Avant le 01 déc.", Status = "En cours", StatusColor = "#FBEBCB", StatusTextColor = "#8A5200", EventId = mariage.Id, EventName = mariage.Name });
        Tasks.Add(new TaskItem { Title = "Envoyer les invitations", Responsible = "Laeticia", DueDate = "Fait le 02 nov.", Status = "Terminé", StatusColor = "#DDF0E8", StatusTextColor = "#0B6B4F", IsDone = true, EventId = mariage.Id, EventName = mariage.Name });
        Tasks.Add(new TaskItem { Title = "Choisir le traiteur", Responsible = "Patricia", DueDate = "Fait le 28 oct.", Status = "Terminé", StatusColor = "#DDF0E8", StatusTextColor = "#0B6B4F", IsDone = true, EventId = mariage.Id, EventName = mariage.Name });

        Tasks.Add(new TaskItem { Title = "Réserver le gâteau", Responsible = "Laeticia", DueDate = "Avant le 20 nov.", Status = "À faire", StatusColor = "#F9DEDC", StatusTextColor = "#B3261E", EventId = anniversaire.Id, EventName = anniversaire.Name });
        Tasks.Add(new TaskItem { Title = "Inviter les amis proches", Responsible = "Patricia", DueDate = "Avant le 25 nov.", Status = "En cours", StatusColor = "#FBEBCB", StatusTextColor = "#8A5200", EventId = anniversaire.Id, EventName = anniversaire.Name });

        Tasks.Add(new TaskItem { Title = "Préparer la sonorisation", Responsible = "Djon", DueDate = "Avant le 30 oct.", Status = "À faire", StatusColor = "#F9DEDC", StatusTextColor = "#B3261E", EventId = veillee.Id, EventName = veillee.Name });
        Tasks.Add(new TaskItem { Title = "Acheter les bougies", Responsible = "Laeticia", DueDate = "Fait le 28 oct.", Status = "Terminé", StatusColor = "#DDF0E8", StatusTextColor = "#0B6B4F", IsDone = true, EventId = veillee.Id, EventName = veillee.Name });
    }

    private static void ApplyStatus(TaskItem item)
    {
        item.IsDone = item.Status == "Terminé";
        switch (item.Status)
        {
            case "Terminé":
                item.StatusColor = "#DDF0E8";
                item.StatusTextColor = "#0B6B4F";
                break;
            case "En cours":
                item.StatusColor = "#FBEBCB";
                item.StatusTextColor = "#8A5200";
                break;
            default:
                item.StatusColor = "#F9DEDC";
                item.StatusTextColor = "#B3261E";
                break;
        }
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

                var remote = await ApiService.Instance.GetTasksAsync(sid.Value);
                if (remote is null) break;

                foreach (var rt in remote)
                {
                    var local = Tasks.FirstOrDefault(t => t.ServerId == rt.Id)
                        ?? Tasks.FirstOrDefault(t =>
                            t.EventId == evt.Id && t.ServerId is null &&
                            string.Equals(t.Title, rt.Title, StringComparison.OrdinalIgnoreCase));

                    if (local is null)
                    {
                        var item = new TaskItem
                        {
                            Title = rt.Title,
                            Responsible = rt.AssigneeName ?? "",
                            DueDate = !string.IsNullOrWhiteSpace(rt.DueLabel)
                                ? rt.DueLabel!
                                : (rt.DueDate ?? "Sans échéance"),
                            Details = rt.Details ?? "",
                            Status = rt.Status,
                            EventId = evt.Id,
                            EventName = evt.Name,
                            ServerId = rt.Id,
                            ServerEventId = sid
                        };
                        ApplyStatus(item);
                        Tasks.Add(item);
                    }
                    else
                    {
                        local.ServerId = rt.Id;
                        local.ServerEventId = sid;
                        local.Responsible = rt.AssigneeName ?? "";
                        if (!string.IsNullOrWhiteSpace(rt.DueLabel))
                            local.DueDate = rt.DueLabel!;
                        else if (!string.IsNullOrWhiteSpace(rt.DueDate))
                            local.DueDate = rt.DueDate!;
                        local.Details = rt.Details ?? local.Details;
                        local.Status = rt.Status;
                        ApplyStatus(local);
                    }
                }

                foreach (var t in Tasks.Where(t => t.EventId == evt.Id && t.ServerId is null).ToList())
                {
                    var created = await ApiService.Instance.CreateTaskAsync(
                        sid.Value, t.Title, t.Responsible, t.DueDate, t.Status, t.Details);
                    if (created is null) break;
                    t.ServerId = created.Id;
                    t.ServerEventId = sid;
                }
            }
        }
        finally
        {
            _refreshing = false;
        }
    }

    public async Task AddAsync(string title, string responsible, string dueDate, string eventId, string eventName, string details = "")
    {
        if (string.IsNullOrWhiteSpace(title)) return;

        int? serverEventId = null;
        int? serverId = null;

        if (ApiService.IsAuthenticated)
        {
            var evt = EventService.Instance.GetByName(eventName);
            serverEventId = await ApiService.Instance.EnsureEventIdAsync(
                eventName,
                evt?.Location ?? "",
                evt?.Status ?? "En préparation");
            if (serverEventId is not null)
            {
                var created = await ApiService.Instance.CreateTaskAsync(
                    serverEventId.Value, title.Trim(), responsible,
                    string.IsNullOrWhiteSpace(dueDate) ? "" : dueDate.Trim(),
                    "À faire", details?.Trim() ?? "");
                serverId = created?.Id;
            }
        }

        var item = new TaskItem
        {
            Title = title.Trim(),
            Responsible = responsible,
            DueDate = string.IsNullOrWhiteSpace(dueDate) ? "Sans échéance" : dueDate.Trim(),
            Details = details?.Trim() ?? string.Empty,
            Status = "À faire",
            EventId = eventId,
            EventName = eventName,
            ServerId = serverId,
            ServerEventId = serverId is null ? null : serverEventId
        };
        ApplyStatus(item);
        Tasks.Add(item);
    }

    public async Task SetStatusAsync(TaskItem item, string status)
    {
        if (string.IsNullOrWhiteSpace(status) || item.Status == status) return;

        item.Status = status;
        ApplyStatus(item);

        if (ApiService.IsAuthenticated && item.ServerId is int tid)
        {
            var evt = EventService.Instance.GetByName(item.EventName);
            var seid = item.ServerEventId
                ?? await ApiService.Instance.EnsureEventIdAsync(
                    item.EventName,
                    evt?.Location ?? "",
                    evt?.Status ?? "En préparation");

            if (seid is int eventId)
                await ApiService.Instance.UpdateTaskAsync(
                    eventId, tid, item.Title, item.Responsible,
                    item.DueDate, item.Status, item.Details);
        }
    }

    public async Task RemoveAsync(string id)
    {
        var item = Tasks.FirstOrDefault(t => t.Id == id);
        if (item is null) return;

        Tasks.Remove(item);

        if (item.ServerId is int tid && item.ServerEventId is int seid)
            await ApiService.Instance.DeleteTaskAsync(seid, tid);
    }

    public void Add(string title, string responsible, string dueDate, string eventId, string eventName, string details = "")
    {
        if (string.IsNullOrWhiteSpace(title)) return;

        Tasks.Add(new TaskItem
        {
            Title = title.Trim(),
            Responsible = responsible,
            DueDate = string.IsNullOrWhiteSpace(dueDate) ? "Sans échéance" : dueDate.Trim(),
            Details = details?.Trim() ?? string.Empty,
            Status = "À faire",
            StatusColor = "#F9DEDC",
            StatusTextColor = "#B3261E",
            EventId = eventId,
            EventName = eventName
        });
    }

    public bool Remove(string id)
    {
        var item = Tasks.FirstOrDefault(t => t.Id == id);
        return item != null && Tasks.Remove(item);
    }
}

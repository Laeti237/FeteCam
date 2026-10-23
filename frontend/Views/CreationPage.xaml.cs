using System.Globalization;
using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class CreationPage : ContentPage
{
    private static readonly string[] DefaultStatuses = { "Confirmé", "En préparation", "Bientôt" };

    private readonly List<string> _statusChoices = new();
    private readonly List<Border> _statusChips = new();
    private string _selectedStatus = "En préparation";
    private EventItem? _editing;

    public CreationPage()
    {
        InitializeComponent();
        BuildStatusChips();
    }

    public CreationPage(EventItem evt) : this()
    {
        _editing = evt;
        Title = "Modifier l'événement";
        HeaderTitleLabel.Text = "Modifier l'événement";
        HeaderSubtitleLabel.Text = "Nom et informations de l'événement";
        NameEntry.Text = evt.Name;
        DateEntry.Text = evt.Date;
        LocationEntry.Text = evt.Location;
        BudgetEntry.Text = evt.Budget > 0 ? evt.Budget.ToString("N0", CultureInfo.InvariantCulture) : "";
        CreateButton.Text = "Enregistrer les modifications";
        SelectStatus(evt.Status);
    }

    private void BuildStatusChips()
    {
        _statusChoices.Clear();
        _statusChips.Clear();
        StatusChipsLayout.Children.Clear();
        foreach (var s in DefaultStatuses)
            AddStatusChip(s);
        SelectStatus(_selectedStatus);
    }

    private void AddStatusChip(string status)
    {
        if (_statusChoices.Contains(status))
            return;
        _statusChoices.Add(status);

        var chip = new Border
        {
            Padding = new Thickness(16, 8),
            Margin = new Thickness(0, 0, 8, 6),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            BackgroundColor = (Color)Application.Current!.Resources["Surface"],
            Stroke = new SolidColorBrush((Color)Application.Current!.Resources["Border"]),
            Content = new Label
            {
                Text = status,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current!.Resources["TextPrimary"]
            }
        };

        var value = status;
        var tgr = new TapGestureRecognizer();
        tgr.Tapped += (_, _) => SelectStatus(value);
        chip.GestureRecognizers.Add(tgr);

        _statusChips.Add(chip);
        StatusChipsLayout.Children.Add(chip);
    }

    private void SelectStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return;
        if (!_statusChoices.Contains(status))
            AddStatusChip(status);
        _selectedStatus = status;

        for (var i = 0; i < _statusChoices.Count; i++)
        {
            var selected = string.Equals(_statusChoices[i], status, StringComparison.Ordinal);
            var chip = _statusChips[i];
            chip.BackgroundColor = (Color)Application.Current!.Resources[selected ? "Primary" : "Surface"];
            chip.Stroke = selected
                ? Colors.Transparent
                : new SolidColorBrush((Color)Application.Current!.Resources["Border"]);
            ((Label)chip.Content!).TextColor = selected
                ? Colors.White
                : (Color)Application.Current!.Resources["TextPrimary"];
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        if (_editing is null)
        {
            await DisplayAlert("Succès", "Événement créé avec succès !", "OK");
            await Navigation.PopAsync();
            return;
        }

        var name = (NameEntry.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Modification", "Le nom de l'événement est obligatoire.", "OK");
            return;
        }

        var date = (DateEntry.Text ?? "").Trim();
        var location = (LocationEntry.Text ?? "").Trim();
        var status = string.IsNullOrWhiteSpace(_selectedStatus)
            ? _editing.Status
            : _selectedStatus;
        var budget = ParseBudget(BudgetEntry.Text);

        if (ApiService.IsAuthenticated)
        {
            var sid = await ApiService.Instance.EnsureEventIdAsync(
                _editing.Name, _editing.Location, _editing.Status);
            if (sid is int eventId)
                await ApiService.Instance.UpdateEventAsync(eventId, name, date, location, status);
        }

        _editing.Name = name;
        _editing.Date = date;
        _editing.Location = location;
        _editing.Status = status;
        _editing.Budget = budget;
        ApplyStatusColors(_editing);

        foreach (var t in TaskService.Instance.Tasks.Where(t => t.EventId == _editing.Id))
            t.EventName = name;
        foreach (var g in GuestService.Instance.Guests.Where(g => g.EventId == _editing.Id))
            g.EventName = name;
        foreach (var b in BudgetService.Instance.Items.Where(b => b.EventId == _editing.Id))
            b.EventName = name;

        await DisplayAlert("Succès", "Événement mis à jour !", "OK");
        await Navigation.PopAsync();
    }

    private static decimal ParseBudget(string? text)
    {
        var cleaned = (text ?? "")
            .Replace(" ", "")
            .Replace("\u202f", "")
            .Replace("\u00a0", "");
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? v
            : 0;
    }

    private static void ApplyStatusColors(EventItem evt)
    {
        switch (evt.Status)
        {
            case "Confirmé":
                evt.StatusColor = "#10B981";
                evt.StatusTextColor = "#FFFFFF";
                break;
            case "Bientôt":
                evt.StatusColor = "#FEE2E2";
                evt.StatusTextColor = "#B91C1C";
                break;
            default:
                evt.StatusColor = "#FBEBCB";
                evt.StatusTextColor = "#8A5200";
                break;
        }
    }
}

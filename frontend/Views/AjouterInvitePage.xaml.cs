namespace FeteCam.Views;

using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

public partial class AjouterInvitePage : ContentPage
{
    private EventItem? _selectedEvent;
    private string _selectedStatus = "En attente";

    public AjouterInvitePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BuildEventChips();
        BuildStatusChips();
    }

    private void BuildEventChips()
    {
        EventChips.Children.Clear();
        var events = EventService.Instance.Events;
        if (events.Count == 0)
        {
            SubtitleLabel.Text = "Aucun événement";
            return;
        }

        _selectedEvent ??= events[0];
        SubtitleLabel.Text = _selectedEvent.Name;

        foreach (var ev in events)
        {
            bool selected = ev.Id == _selectedEvent.Id;
            var chip = new Border
            {
                BackgroundColor = selected
                    ? (Color)Application.Current!.Resources["Primary"]
                    : (Color)Application.Current!.Resources["Surface"],
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
                Stroke = selected
                    ? Colors.Transparent
                    : (Color)Application.Current!.Resources["Border"],
                Padding = new Thickness(16, 8),
                Margin = new Thickness(0, 0, 8, 6)
            };
            var label = new Label
            {
                Text = ev.Name,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = selected
                    ? Colors.White
                    : (Color)Application.Current!.Resources["TextPrimary"]
            };
            chip.Content = label;

            var eventItem = ev;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                _selectedEvent = eventItem;
                SubtitleLabel.Text = eventItem.Name;
                BuildEventChips();
            };
            chip.GestureRecognizers.Add(tap);
            EventChips.Children.Add(chip);
        }
    }

    private void BuildStatusChips()
    {
        StatusChips.Children.Clear();
        string[] statuses = { "Confirmé", "En attente", "Absent" };

        foreach (var status in statuses)
        {
            bool selected = status == _selectedStatus;
            var chip = new Border
            {
                BackgroundColor = selected
                    ? (Color)Application.Current!.Resources["Primary"]
                    : (Color)Application.Current!.Resources["Surface"],
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
                Stroke = selected
                    ? Colors.Transparent
                    : (Color)Application.Current!.Resources["Border"],
                Padding = new Thickness(16, 8),
                Margin = new Thickness(0, 0, 8, 6)
            };
            var label = new Label
            {
                Text = status,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = selected
                    ? Colors.White
                    : (Color)Application.Current!.Resources["TextPrimary"]
            };
            chip.Content = label;

            var statusValue = status;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                _selectedStatus = statusValue;
                BuildStatusChips();
            };
            chip.GestureRecognizers.Add(tap);
            StatusChips.Children.Add(chip);
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Erreur", "Veuillez saisir le nom de l'invité.", "OK");
            return;
        }

        if (_selectedEvent == null)
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner un événement.", "OK");
            return;
        }

        string phone = PhoneEntry.Text?.Trim() ?? "";
        int count = 1;
        if (!string.IsNullOrWhiteSpace(CountEntry.Text) && int.TryParse(CountEntry.Text.Trim(), out int parsed) && parsed > 0)
        {
            count = parsed;
        }

        await GuestService.Instance.AddAsync(name, phone, count, _selectedStatus, _selectedEvent.Id, _selectedEvent.Name);
        await DisplayAlert("Succès", $"Invité ajouté à « {_selectedEvent.Name} ».", "OK");
        await Navigation.PopAsync();
    }
}

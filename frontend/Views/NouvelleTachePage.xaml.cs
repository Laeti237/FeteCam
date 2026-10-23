using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class NouvelleTachePage : ContentPage
{
    private string _selectedResponsible = "Djon";
    private EventItem? _selectedEvent;

    public NouvelleTachePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BuildEventChips();
        LoadResponsibles();
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

        _selectedEvent ??= EventService.Instance.Current;
        if (!events.Any(e => e.Id == _selectedEvent!.Id))
            _selectedEvent = events[0];
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
            chip.Content = new Label
            {
                Text = ev.Name,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = selected
                    ? Colors.White
                    : (Color)Application.Current!.Resources["TextPrimary"]
            };

            var eventItem = ev;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                _selectedEvent = eventItem;
                SubtitleLabel.Text = eventItem.Name;
                BuildEventChips();
                LoadResponsibles();
            };
            chip.GestureRecognizers.Add(tap);
            EventChips.Children.Add(chip);
        }
    }

    private void LoadResponsibles()
    {
        ResponsiblesSelectionLayout.Children.Clear();
        var list = ResponsibleService.Instance.Responsibles;

        if (list.Count > 0 && !list.Any(r => r.Name == _selectedResponsible))
        {
            _selectedResponsible = list[0].Name;
        }

        foreach (var r in list)
        {
            bool isSelected = r.Name == _selectedResponsible;
            var pill = CreateResponsiblePill(r.Name, isSelected);
            ResponsiblesSelectionLayout.Children.Add(pill);
        }

        // Shortcut "+ Nouveau"
        var addPill = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["Surface"],
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            Stroke = (Color)Application.Current!.Resources["Accent"],
            StrokeThickness = 1,
            Padding = new Thickness(14, 6),
            Margin = new Thickness(0, 0, 6, 6)
        };
        var addLabel = new Label
        {
            Text = "+ Nouveau",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["AccentDark"]
        };
        addPill.Content = addLabel;
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnAddNewResponsibleClicked;
        addPill.GestureRecognizers.Add(tap);
        ResponsiblesSelectionLayout.Children.Add(addPill);
    }

    private Border CreateResponsiblePill(string name, bool isSelected)
    {
        var primaryColor = (Color)Application.Current!.Resources["Primary"];
        var surfaceColor = (Color)Application.Current!.Resources["Surface"];
        var borderColor = (Color)Application.Current!.Resources["Border"];
        var textPrimary = (Color)Application.Current!.Resources["TextPrimary"];

        var pill = new Border
        {
            BackgroundColor = isSelected ? primaryColor : surfaceColor,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            Stroke = isSelected ? Colors.Transparent : borderColor,
            StrokeThickness = 1,
            Padding = new Thickness(14, 6),
            Margin = new Thickness(0, 0, 6, 6)
        };

        var label = new Label
        {
            Text = name,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = isSelected ? Colors.White : textPrimary
        };
        pill.Content = label;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) =>
        {
            _selectedResponsible = name;
            LoadResponsibles();
        };
        pill.GestureRecognizers.Add(tap);

        return pill;
    }

    private async void OnAddNewResponsibleClicked(object? sender, EventArgs e)
    {
        string name = await DisplayPromptAsync(
            "Nouveau responsable",
            "Entrez le nom du nouveau membre :",
            "Ajouter",
            "Annuler",
            "Ex. Paul Kamdem");

        if (string.IsNullOrWhiteSpace(name))
            return;

        ResponsibleService.Instance.Add(name);
        _selectedResponsible = name.Trim();
        LoadResponsibles();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        string title = TitleEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlert("Erreur", "Veuillez saisir le titre de la tâche.", "OK");
            return;
        }

        if (_selectedEvent == null)
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner un événement.", "OK");
            return;
        }

        string dueDate = DueDateEntry.Text?.Trim() ?? "";
        string details = DetailsEntry.Text?.Trim() ?? "";

        await TaskService.Instance.AddAsync(title, _selectedResponsible, dueDate, _selectedEvent.Id, _selectedEvent.Name, details);
        await DisplayAlert("Succès", $"Tâche créée pour « {_selectedEvent.Name} » et attribuée à {_selectedResponsible} !", "OK");
        await Navigation.PopAsync();
    }
}

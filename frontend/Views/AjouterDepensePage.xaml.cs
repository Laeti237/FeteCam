using System.Globalization;
using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class AjouterDepensePage : ContentPage
{
    private static readonly string[] DefaultCategories = { "Traiteur", "Salle", "Boissons", "Décoration", "Musique" };

    private EventItem? _selectedEvent;
    private string _selectedPoste = DefaultCategories[0];

    public AjouterDepensePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BuildEventChips();
        BuildPosteChips();
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
                BuildPosteChips();
            };
            chip.GestureRecognizers.Add(tap);
            EventChips.Children.Add(chip);
        }
    }

    private void BuildPosteChips()
    {
        PosteChips.Children.Clear();

        IReadOnlyList<string> categories = _selectedEvent != null
            ? BudgetService.Instance.CategoriesFor(_selectedEvent.Id)
            : Array.Empty<string>();
        if (categories.Count == 0)
        {
            categories = DefaultCategories;
        }

        if (!categories.Any(c => string.Equals(c, _selectedPoste, StringComparison.OrdinalIgnoreCase)))
        {
            _selectedPoste = categories[0];
        }

        foreach (var category in categories)
        {
            bool selected = string.Equals(category, _selectedPoste, StringComparison.OrdinalIgnoreCase);
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
                Text = category,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = selected
                    ? Colors.White
                    : (Color)Application.Current!.Resources["TextPrimary"]
            };

            var categoryValue = category;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                _selectedPoste = categoryValue;
                BuildPosteChips();
            };
            chip.GestureRecognizers.Add(tap);
            PosteChips.Children.Add(chip);
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string rawAmount = AmountEntry.Text?.Trim().Replace(" ", "").Replace("\u00A0", "") ?? "";
        if (!decimal.TryParse(rawAmount, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Erreur", "Veuillez saisir un montant valide.", "OK");
            return;
        }

        if (_selectedEvent == null)
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner un événement.", "OK");
            return;
        }

        BudgetService.Instance.AddExpense(_selectedPoste, amount, _selectedEvent.Id, _selectedEvent.Name);
        await DisplayAlert("Succès", $"Dépense ajoutée au poste « {_selectedPoste} » de « {_selectedEvent.Name} ».", "OK");
        await Navigation.PopAsync();
    }
}

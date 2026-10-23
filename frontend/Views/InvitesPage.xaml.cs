namespace FeteCam.Views;

using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

public partial class InvitesPage : ContentPage
{
    private GuestService _guestService => GuestService.Instance;
    private string _searchQuery = string.Empty;

    public InvitesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _guestService.RefreshAsync();
        LoadGuests();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchQuery = e.NewTextValue?.Trim() ?? string.Empty;
        LoadGuests();
    }

    private void LoadGuests()
    {
        GuestsContainer.Children.Clear();

        var all = _guestService.Guests;
        ConfirmedCountLabel.Text = all.Count(g => g.Status == "Confirmé").ToString();
        WaitingCountLabel.Text = all.Count(g => g.Status == "En attente").ToString();
        AbsentCountLabel.Text = all.Count(g => g.Status == "Absent").ToString();

        int totalPersons = all.Sum(g => g.PersonCount);
        int eventCount = all.Select(g => g.EventId).Distinct().Count();
        SubtitleLabel.Text = $"{all.Count} invités · {totalPersons} personnes · {eventCount} événement(s)";

        var filtered = all.AsEnumerable();
        if (!string.IsNullOrEmpty(_searchQuery))
        {
            filtered = filtered.Where(g =>
                g.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                g.Phone.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
        }
        var list = filtered.ToList();

        if (list.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = string.IsNullOrEmpty(_searchQuery)
                    ? "Aucun invité pour le moment."
                    : "Aucun invité ne correspond à votre recherche.",
                TextColor = (Color)Application.Current!.Resources["TextSecondary"],
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 30, 0, 0)
            };
            GuestsContainer.Children.Add(emptyLabel);
            return;
        }

        var eventOrder = EventService.Instance.Events.Select((ev, i) => new { ev.Name, Index = i })
            .ToDictionary(x => x.Name, x => x.Index);
        int OrderOf(string eventName) => eventOrder.TryGetValue(eventName, out int i) ? i : int.MaxValue;

        var groups = list
            .GroupBy(g => string.IsNullOrWhiteSpace(g.EventName) ? "Sans événement" : g.EventName)
            .OrderBy(g => OrderOf(g.Key))
            .ThenBy(g => g.Key);

        foreach (var group in groups)
        {
            GuestsContainer.Children.Add(CreateEventHeader(group.Key, group.Count(), group.Sum(g => g.PersonCount)));
            foreach (var item in group)
            {
                var card = CreateGuestCard(item);
                GuestsContainer.Children.Add(card);
            }
        }
    }

    private View CreateEventHeader(string eventName, int guestCount, int personCount)
    {
        var border = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["Primary"],
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Stroke = Colors.Transparent,
            Padding = new Thickness(14, 8),
            Margin = new Thickness(0, 8, 0, 2)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        grid.Children.Add(new Label
        {
            Text = eventName,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        });

        var countLabel = new Label
        {
            Text = $"{guestCount} invité(s) · {personCount} pers.",
            FontSize = 11,
            TextColor = Color.FromArgb("#DCE8E2"),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(countLabel, 1);
        grid.Children.Add(countLabel);

        border.Content = grid;
        return border;
    }

    private Border CreateGuestCard(GuestItem item)
    {
        var border = new Border
        {
            Style = (Style)Application.Current!.Resources["CardFrame"],
            Margin = new Thickness(0, 0, 0, 4)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // Avatar with initials
        var avatar = new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(21) },
            BackgroundColor = (Color)Application.Current!.Resources["Primary"],
            Stroke = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = item.Initials,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        grid.Children.Add(avatar);
        Grid.SetColumn(avatar, 0);

        // Info stack
        var infoStack = new VerticalStackLayout
        {
            Spacing = 2,
            Margin = new Thickness(12, 0, 0, 0),
            VerticalOptions = LayoutOptions.Center
        };

        var nameLabel = new Label
        {
            Text = item.Name,
            Style = (Style)Application.Current!.Resources["SectionTitle"]
        };
        infoStack.Children.Add(nameLabel);

        string subtitle = item.Phone;
        if (!string.IsNullOrWhiteSpace(item.Phone))
        {
            subtitle += $" · {item.PersonCount} personne(s)";
        }

        var subLabel = new Label
        {
            Text = subtitle,
            Style = (Style)Application.Current!.Resources["CaptionText"]
        };
        infoStack.Children.Add(subLabel);

        grid.Children.Add(infoStack);
        Grid.SetColumn(infoStack, 1);

        // Tap gesture to view details
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            Navigation.PushAsync(new DetailInvitePage(item.Name, item.Phone, item.Status, $" {item.PersonCount} personnes", item.Status == "Confirmé" ? "Table d'honneur" : "Non assignée", string.IsNullOrWhiteSpace(item.EventName) ? "Mariage Alain & Rose" : item.EventName));
        };
        grid.GestureRecognizers.Add(tapGesture);

        // Delete button — same tile size as status badge, red
        var deleteBtn = new Button
        {
            Text = "Supprimer",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["Error"],
            BackgroundColor = (Color)Application.Current!.Resources["ErrorLight"],
            WidthRequest = 84,
            HeightRequest = 32,
            Padding = new Thickness(6, 4),
            CornerRadius = 6,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        deleteBtn.Clicked += async (s, e) =>
        {
            bool confirm = await DisplayAlert(
                "Supprimer un invité",
                $"Voulez-vous retirer {item.Name} de la liste d'invités ?",
                "Supprimer",
                "Annuler");

            if (confirm)
            {
                await _guestService.RemoveAsync(item.Id);
                LoadGuests();
            }
        };

        grid.Children.Add(deleteBtn);
        Grid.SetColumn(deleteBtn, 3);

        // Status badge — same tile size as delete button, single color for all statuses
        var statusBorder = new Border
        {
            BackgroundColor = Color.FromArgb(item.StatusColor),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Stroke = Colors.Transparent,
            Padding = new Thickness(6, 4),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 84,
            HeightRequest = 32
        };
        Grid.SetColumn(statusBorder, 2);
        var statusLabel = new Label
        {
            Text = item.Status,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(item.StatusTextColor),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        statusBorder.Content = statusLabel;

        grid.Children.Add(statusBorder);

        border.Content = grid;
        return border;
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnDeleteGuestClicked(object? sender, EventArgs e)
    {
        // This is handled per-card in CreateGuestCard
    }

    private async void OnAddGuestClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AjouterInvitePage());
    }

    private async void OnGuest1Tapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DetailInvitePage("Marie Ngono", "+237 671 22 33 44", "Confirmé", "2 personnes", "Table d'honneur"));
    }

    private async void OnGuest2Tapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DetailInvitePage("Paul Tchoumi", "+237 699 12 34 56", "En attente", "1 personne", "Table Amis"));
    }

    private async void OnGuest3Tapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DetailInvitePage("Famille Kamga", "+237 655 88 99 00", "Confirmé", "5 personnes", "Table VIP"));
    }

    private async void OnGuest4Tapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DetailInvitePage("Sylvie Eto'o", "+237 691 00 11 22", "Absent", "1 personne", "Non assignée"));
    }

    private async void OnGuest5Tapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DetailInvitePage("Jean Fotso", "+237 655 44 33 22", "En attente", "2 personnes", "Table Collègues"));
    }

    private async void OnNavInvitesTapped(object? sender, EventArgs e)
    {
        // Already on InvitesPage
    }

    private async void OnNavBudgetTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new BudgetPage());
    }

    private async void OnNavTachesTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new TachesPage());
    }

    private async void OnNavProfilTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilPage());
    }

    private async void OnNavAccueilTapped(object? sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}

namespace FeteCam.Views;

using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

public partial class InvitesPage : ContentPage
{
    private GuestService _guestService => GuestService.Instance;

    public InvitesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadGuests();
    }

    private void LoadGuests()
    {
        GuestsContainer.Children.Clear();
        var list = _guestService.Guests;

        if (list.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "Aucun invité assigné pour le moment.",
                TextColor = (Color)Application.Current!.Resources["TextSecondary"],
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 30, 0, 0)
            };
            GuestsContainer.Children.Add(emptyLabel);
            return;
        }

        foreach (var item in list)
        {
            var card = CreateGuestCard(item);
            GuestsContainer.Children.Add(card);
        }
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
            subtitle += $" · {item.Phone}";
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
            Navigation.PushAsync(new DetailInvitePage(item.Name, item.Phone, item.Status, $" {item.PersonCount} personnes", item.Status == "Confirmé" ? "Table d'honneur" : "Non assignée"));
        };
        grid.GestureRecognizers.Add(tapGesture);

        // Delete button
        var deleteBtn = new Button
        {
            Text = "Supprimer",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["Error"],
            BackgroundColor = (Color)Application.Current!.Resources["ErrorLight"],
            HeightRequest = 32,
            Padding = new Thickness(10, 0),
            CornerRadius = 6,
            VerticalOptions = LayoutOptions.Center
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
                _guestService.Remove(item.Id);
                LoadGuests();
            }
        };

        grid.Children.Add(deleteBtn);
        Grid.SetColumn(deleteBtn, 3);

        // Status badge
        var statusBorder = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources[item.StatusColor],
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(6, 3),
            VerticalOptions = LayoutOptions.Center,
            HeightRequest = 24
        };
        Grid.SetColumn(statusBorder, 2);
        var statusLabel = new Label
        {
            Text = item.Status,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources[item.StatusTextColor ?? "TextPrimary"]
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

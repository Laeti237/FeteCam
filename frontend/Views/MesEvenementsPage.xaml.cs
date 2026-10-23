using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class MesEvenementsPage : ContentPage
{
    public MesEvenementsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEvents();
    }

    private void LoadEvents()
    {
        var list = EventService.Instance.Events.ToList();
        SubtitleLabel.Text = $"{list.Count} événement(s)";

        EventsContainer.Children.Clear();

        if (list.Count == 0)
        {
            EventsContainer.Children.Add(new Label
            {
                Text = "Aucun événement pour le moment. Créez votre premier événement.",
                Style = (Style)Application.Current!.Resources["CaptionText"],
                TextColor = (Color)Application.Current!.Resources["TextSecondary"],
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(20, 30, 20, 0)
            });
            return;
        }

        foreach (var item in list)
        {
            EventsContainer.Children.Add(CreateEventCard(item));
        }
    }

    private Border CreateEventCard(EventItem item)
    {
        var border = new Border
        {
            Style = (Style)Application.Current!.Resources["CardFrame"],
            Margin = new Thickness(0, 0, 0, 4)
        };

        var tgr = new TapGestureRecognizer();
        tgr.Tapped += async (_, _) =>
        {
            EventService.Instance.Current = item;
            await Navigation.PushAsync(new FicheEvenementPage());
        };
        border.GestureRecognizers.Add(tgr);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

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
                Text = GetInitials(item.Name),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        grid.Children.Add(avatar);
        Grid.SetColumn(avatar, 0);

        var infoStack = new VerticalStackLayout
        {
            Spacing = 2,
            Margin = new Thickness(12, 0, 0, 0),
            VerticalOptions = LayoutOptions.Center
        };
        infoStack.Children.Add(new Label
        {
            Text = item.Name,
            Style = (Style)Application.Current!.Resources["SectionTitle"]
        });
        infoStack.Children.Add(new Label
        {
            Text = $"{item.Date} · {item.Location}",
            Style = (Style)Application.Current!.Resources["CaptionText"]
        });
        grid.Children.Add(infoStack);
        Grid.SetColumn(infoStack, 1);

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb(item.StatusColor),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Stroke = Colors.Transparent,
            Padding = new Thickness(6, 4),
            HeightRequest = 32,
            VerticalOptions = LayoutOptions.Center
        };
        badge.Content = new Label
        {
            Text = item.Status,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(item.StatusTextColor),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        grid.Children.Add(badge);
        Grid.SetColumn(badge, 2);

        border.Content = grid;
        return border;
    }

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
        return name.Length <= 2 ? name.ToUpper() : name.Substring(0, 2).ToUpper();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnAddEventClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreationPage());
    }

    private async void OnNextEventClicked(object? sender, EventArgs e)
    {
        var list = EventService.Instance.Events;
        if (list.Count == 0)
        {
            await DisplayAlert("Mes événements", "Aucun événement enregistré.", "OK");
            return;
        }
        if (list.All(e => e.Id != EventService.Instance.Current.Id))
            EventService.Instance.Current = list[0];
        await Navigation.PushAsync(new FicheEvenementPage());
    }
}

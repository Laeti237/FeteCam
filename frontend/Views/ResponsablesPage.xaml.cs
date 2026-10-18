using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class ResponsablesPage : ContentPage
{
    public ResponsablesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadResponsibles();
    }

    private void LoadResponsibles()
    {
        ResponsiblesContainer.Children.Clear();
        var list = ResponsibleService.Instance.Responsibles;

        SubtitleLabel.Text = $"Mariage Alain & Rose · {list.Count} membres";

        if (list.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "Aucun responsable assigné pour le moment.",
                TextColor = (Color)Application.Current!.Resources["TextSecondary"],
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 30, 0, 0)
            };
            ResponsiblesContainer.Children.Add(emptyLabel);
            return;
        }

        foreach (var item in list)
        {
            var card = CreateResponsibleCard(item);
            ResponsiblesContainer.Children.Add(card);
        }
    }

    private Border CreateResponsibleCard(ResponsibleItem item)
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

        string subtitle = item.Role;
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
                "Supprimer un responsable",
                $"Voulez-vous retirer {item.Name} du comité d'organisation ?",
                "Supprimer",
                "Annuler");

            if (confirm)
            {
                ResponsibleService.Instance.Remove(item.Id);
                LoadResponsibles();
            }
        };

        grid.Children.Add(deleteBtn);
        Grid.SetColumn(deleteBtn, 2);

        border.Content = grid;
        return border;
    }

    private async void OnAddResponsibleClicked(object? sender, EventArgs e)
    {
        string name = await DisplayPromptAsync(
            "Nouveau responsable",
            "Entrez le nom complet du responsable :",
            "Suivant",
            "Annuler",
            "Ex. Jean Kamga");

        if (string.IsNullOrWhiteSpace(name))
            return;

        string role = await DisplayPromptAsync(
            "Rôle / Mission",
            $"Attribuez une fonction à {name.Trim()} :",
            "Suivant",
            "Ignorer",
            "Ex. Traiteur & Boissons",
            initialValue: "Membre du comité");

        string phone = await DisplayPromptAsync(
            "Téléphone",
            $"Numéro de téléphone de {name.Trim()} (optionnel) :",
            "Enregistrer",
            "Ignorer",
            "+237 6XX XX XX XX",
            keyboard: Keyboard.Telephone);

        ResponsibleService.Instance.Add(name, role, phone);
        LoadResponsibles();

        await DisplayAlert("Succès", $"{name.Trim()} a été ajouté(e) aux responsables.", "OK");
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class NouvelleTachePage : ContentPage
{
    private string _selectedResponsible = "Djon";

    public NouvelleTachePage()
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
        await DisplayAlert("Succès", $"Tâche créée et attribuée à {_selectedResponsible} !", "OK");
        await Navigation.PopAsync();
    }
}

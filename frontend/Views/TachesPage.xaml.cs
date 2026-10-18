using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class TachesPage : ContentPage
{
    private string _selectedResponsible = "Toutes";

    public TachesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshResponsiblesPills();
    }

    private void RefreshResponsiblesPills()
    {
        ResponsiblesFilterLayout.Children.Clear();

        // "Toutes" pill
        var allPill = CreateFilterPill("Toutes", _selectedResponsible == "Toutes");
        ResponsiblesFilterLayout.Children.Add(allPill);

        // Dynamically list members from ResponsibleService
        foreach (var resp in ResponsibleService.Instance.Responsibles)
        {
            var pill = CreateFilterPill(resp.Name, _selectedResponsible == resp.Name);
            ResponsiblesFilterLayout.Children.Add(pill);
        }

        // "+ Ajouter" shortcut pill
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
            Text = "+ Ajouter",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["AccentDark"]
        };
        addPill.Content = addLabel;
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnQuickAddResponsibleClicked;
        addPill.GestureRecognizers.Add(tap);
        ResponsiblesFilterLayout.Children.Add(addPill);
    }

    private Border CreateFilterPill(string name, bool isSelected)
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
            RefreshResponsiblesPills();
            FilterTasks();
        };
        pill.GestureRecognizers.Add(tap);

        return pill;
    }

    private void FilterTasks()
    {
        bool showAll = _selectedResponsible == "Toutes";

        TaskCard1.IsVisible = showAll || _selectedResponsible == "Djon";
        TaskCard2.IsVisible = showAll || _selectedResponsible == "Djon";
        TaskCard3.IsVisible = showAll || _selectedResponsible == "Laeticia";
        TaskCard4.IsVisible = showAll || _selectedResponsible == "Patricia";
    }

    private async void OnManageResponsiblesClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ResponsablesPage());
    }

    private async void OnQuickAddResponsibleClicked(object? sender, EventArgs e)
    {
        string name = await DisplayPromptAsync(
            "Nouveau responsable",
            "Entrez le nom du membre du comité :",
            "Suivant",
            "Annuler",
            "Ex. Jean Kamga");

        if (string.IsNullOrWhiteSpace(name))
            return;

        string role = await DisplayPromptAsync(
            "Rôle",
            $"Fonction attribuée à {name.Trim()} :",
            "Enregistrer",
            "Ignorer",
            "Ex. Salle & Logistique",
            initialValue: "Membre du comité");

        ResponsibleService.Instance.Add(name, role);
        RefreshResponsiblesPills();

        await DisplayAlert("Succès", $"{name.Trim()} a été ajouté au comité.", "OK");
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnAddTaskClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new NouvelleTachePage());
    }

    private async void OnNavInvitesTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new InvitesPage());
    }

    private async void OnNavBudgetTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new BudgetPage());
    }

    private async void OnNavTachesTapped(object? sender, EventArgs e)
    {
        // Already on TachesPage
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

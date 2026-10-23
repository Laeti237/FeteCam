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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await TaskService.Instance.RefreshAsync();
        RefreshResponsiblesPills();
        LoadTasks();
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
            LoadTasks();
        };
        pill.GestureRecognizers.Add(tap);

        return pill;
    }

    private void LoadTasks()
    {
        TasksContainer.Children.Clear();

        bool showAll = _selectedResponsible == "Toutes";
        var filtered = TaskService.Instance.Tasks
            .Where(t => showAll || t.Responsible == _selectedResponsible)
            .ToList();

        SubtitleLabel.Text = $"{filtered.Count} tâche(s) · {filtered.Select(t => t.EventId).Distinct().Count()} événement(s) · {_selectedResponsible}";

        if (filtered.Count == 0)
        {
            TasksContainer.Children.Add(new Label
            {
                Text = showAll
                    ? "Aucune tâche pour le moment."
                    : $"Aucune tâche pour {_selectedResponsible}.",
                TextColor = (Color)Application.Current!.Resources["TextSecondary"],
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 30, 0, 0)
            });
            return;
        }

        var eventOrder = EventService.Instance.Events.Select((ev, i) => new { ev.Name, Index = i })
            .ToDictionary(x => x.Name, x => x.Index);
        int OrderOf(string eventName) => eventOrder.TryGetValue(eventName, out int i) ? i : int.MaxValue;

        var groups = filtered
            .GroupBy(t => string.IsNullOrWhiteSpace(t.EventName) ? "Sans événement" : t.EventName)
            .OrderBy(g => OrderOf(g.Key))
            .ThenBy(g => g.Key);

        foreach (var group in groups)
        {
            TasksContainer.Children.Add(CreateEventHeader(group.Key, group.Count()));
            foreach (var task in group)
            {
                TasksContainer.Children.Add(CreateTaskCard(task));
            }
        }
    }

    private View CreateEventHeader(string eventName, int taskCount)
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
            Text = $"{taskCount} tâche(s)",
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

    private Border CreateTaskCard(TaskItem item)
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
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var infoStack = new VerticalStackLayout { Spacing = 2 };
        infoStack.Children.Add(new Label
        {
            Text = item.Title,
            Style = (Style)Application.Current!.Resources["SectionTitle"],
            TextColor = item.IsDone
                ? (Color)Application.Current!.Resources["TextSecondary"]
                : (Color)Application.Current!.Resources["TextPrimary"]
        });
        infoStack.Children.Add(new Label
        {
            Text = $"{item.DueDate} · {item.Responsible}",
            Style = (Style)Application.Current!.Resources["CaptionText"]
        });
        grid.Children.Add(infoStack);

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb(item.StatusColor),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Stroke = Colors.Transparent,
            Padding = new Thickness(6, 4),
            WidthRequest = 84,
            HeightRequest = 32,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
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

        var statusTap = new TapGestureRecognizer();
        statusTap.Tapped += async (_, _) => await OnTaskStatusChosen(item);
        badge.GestureRecognizers.Add(statusTap);

        Grid.SetColumn(badge, 1);
        grid.Children.Add(badge);

        border.Content = grid;
        return border;
    }

    private async Task OnTaskStatusChosen(TaskItem item)
    {
        var choice = await DisplayActionSheetAsync(
            $"Statut — {item.Title}",
            "Annuler",
            null,
            TaskService.Statuses);

        if (string.IsNullOrEmpty(choice) || choice == "Annuler" || choice == item.Status)
            return;

        await TaskService.Instance.SetStatusAsync(item, choice);
        LoadTasks();
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

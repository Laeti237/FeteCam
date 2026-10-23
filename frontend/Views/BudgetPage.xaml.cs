using System.Globalization;
using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class BudgetPage : ContentPage
{
    public BudgetPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadBudget();
    }

    private static string Fmt(decimal value) =>
        ((long)value).ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");

    private void LoadBudget()
    {
        ExpensesContainer.Children.Clear();

        var all = BudgetService.Instance.Items;
        int eventCount = all.Select(i => i.EventId).Distinct().Count();
        decimal totalAllocated = all.Sum(i => i.Allocated);
        decimal totalSpent = all.Sum(i => i.Spent);

        SubtitleLabel.Text = $"{eventCount} événement(s) · {Fmt(totalSpent)} / {Fmt(totalAllocated)} FCFA";
        TotalBudgetLabel.Text = $"{Fmt(totalAllocated)} FCFA";

        decimal reste = totalAllocated - totalSpent;
        ResteLabel.Text = $"{Fmt(Math.Abs(reste))} FCFA";

        double ratio = totalAllocated > 0 ? (double)(totalSpent / totalAllocated) : 0;
        SummaryProgress.Progress = Math.Min(ratio, 1.0);
        PercentLabel.Text = $"{Math.Round(ratio * 100)} % engagé";
        SpentLabel.Text = $"{Fmt(totalSpent)} FCFA dépensés";

        if (all.Count == 0)
        {
            ExpensesContainer.Children.Add(new Label
            {
                Text = "Aucune dépense pour le moment.",
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

        var groups = all
            .GroupBy(i => string.IsNullOrWhiteSpace(i.EventName) ? "Sans événement" : i.EventName)
            .OrderBy(g => OrderOf(g.Key))
            .ThenBy(g => g.Key);

        foreach (var group in groups)
        {
            ExpensesContainer.Children.Add(CreateEventHeader(
                group.Key,
                group.Sum(i => i.Spent),
                group.Sum(i => i.Allocated)));

            foreach (var item in group)
            {
                ExpensesContainer.Children.Add(CreateBudgetCard(item));
            }
        }
    }

    private View CreateEventHeader(string eventName, decimal spent, decimal allocated)
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

        var totalLabel = new Label
        {
            Text = $"{Fmt(spent)} / {Fmt(allocated)} FCFA",
            FontSize = 11,
            TextColor = Color.FromArgb("#DCE8E2"),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(totalLabel, 1);
        grid.Children.Add(totalLabel);

        border.Content = grid;
        return border;
    }

    private Border CreateBudgetCard(BudgetItem item)
    {
        var ratio = item.Allocated > 0 ? (double)(item.Spent / item.Allocated) : (item.Spent > 0 ? 1.0 : 0.0);
        bool over = item.IsOverBudget;

        Color cardBg, cardStroke, titleColor, captionColor, badgeBg, badgeTextColor, progressColor;
        string badgeText;

        if (over)
        {
            cardBg = (Color)Application.Current!.Resources["ErrorLight"];
            cardStroke = (Color)Application.Current!.Resources["Error"];
            titleColor = (Color)Application.Current!.Resources["Error"];
            captionColor = (Color)Application.Current!.Resources["Error"];
            badgeBg = (Color)Application.Current!.Resources["Surface"];
            badgeTextColor = (Color)Application.Current!.Resources["Error"];
            progressColor = (Color)Application.Current!.Resources["Error"];
            badgeText = "Dépassé";
        }
        else
        {
            cardBg = Colors.Transparent;
            cardStroke = Colors.Transparent;
            titleColor = (Color)Application.Current!.Resources["TextPrimary"];
            captionColor = (Color)Application.Current!.Resources["TextSecondary"];
            badgeBg = ratio >= 0.7
                ? (Color)Application.Current!.Resources["SuccessLight"]
                : (Color)Application.Current!.Resources["WarningLight"];
            badgeTextColor = ratio >= 0.7
                ? (Color)Application.Current!.Resources["Success"]
                : (Color)Application.Current!.Resources["Warning"];
            progressColor = ratio >= 1.0
                ? (Color)Application.Current!.Resources["Success"]
                : ratio >= 0.7
                    ? (Color)Application.Current!.Resources["PrimaryLight"]
                    : (Color)Application.Current!.Resources["Accent"];
            badgeText = $"{Math.Round(ratio * 100)} %";
        }

        var border = new Border
        {
            Style = (Style)Application.Current!.Resources["CardFrame"],
            Margin = new Thickness(0, 0, 0, 4)
        };

        if (over)
        {
            border.BackgroundColor = cardBg;
            border.Stroke = cardStroke;
            border.StrokeThickness = 0.5;
        }

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var infoStack = new VerticalStackLayout { Spacing = 2 };
        infoStack.Children.Add(new Label
        {
            Text = item.Category,
            Style = (Style)Application.Current!.Resources["SectionTitle"],
            TextColor = titleColor
        });
        infoStack.Children.Add(new Label
        {
            Text = $"{Fmt(item.Spent)} / {Fmt(item.Allocated)} FCFA",
            Style = (Style)Application.Current!.Resources["CaptionText"],
            TextColor = captionColor
        });
        grid.Children.Add(infoStack);

        var badge = new Border
        {
            BackgroundColor = badgeBg,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Stroke = Colors.Transparent,
            Padding = new Thickness(8, 3),
            VerticalOptions = LayoutOptions.Center
        };
        badge.Content = new Label
        {
            Text = badgeText,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = badgeTextColor
        };
        Grid.SetColumn(badge, 1);
        grid.Children.Add(badge);

        var progress = new ProgressBar
        {
            Progress = Math.Min(ratio, 1.0),
            ProgressColor = progressColor,
            HeightRequest = 5
        };
        Grid.SetRow(progress, 1);
        Grid.SetColumnSpan(progress, 2);
        grid.Children.Add(progress);

        border.Content = grid;
        return border;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AjouterDepensePage());
    }

    private async void OnNavInvitesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InvitesPage());
    }

    private async void OnNavBudgetTapped(object sender, EventArgs e)
    {
        // Already on BudgetPage
    }

    private async void OnNavTachesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TachesPage());
    }

    private async void OnNavProfilTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilPage());
    }

    private async void OnNavAccueilTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}

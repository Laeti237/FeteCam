using FeteCam.Services;

namespace FeteCam.Views;

public partial class FicheEvenementPage : ContentPage
{
    public FicheEvenementPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var evt = EventService.Instance.Current;
        EventTitleLabel.Text = evt.Name;
        EventSubtitleLabel.Text = string.IsNullOrWhiteSpace(evt.Date)
            ? evt.Location
            : $"{evt.Date} · {evt.Location}";
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnBilanClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BilanPage());
    }

    private async void OnNavInvitesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InvitesPage());
    }

    private async void OnNavContributionsTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ContributionsPage());
    }

    private async void OnNavBudgetTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BudgetPage());
    }

    private async void OnNavTachesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TachesPage());
    }

    private async void OnNavResponsablesTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ResponsablesPage());
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

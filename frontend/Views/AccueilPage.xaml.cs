using FeteCam.Services;

namespace FeteCam.Views;

public partial class AccueilPage : ContentPage
{
    public AccueilPage()
    {
        InitializeComponent();
    }

    private async void OnAddEventClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreationPage());
    }

    private async void OnMariageTapped(object? sender, EventArgs e)
        => await OpenEventAsync("Mariage Alain & Rose");

    private async void OnAnniversaireTapped(object? sender, EventArgs e)
        => await OpenEventAsync("Anniversaire 50 ans");

    private async void OnVeilleeTapped(object? sender, EventArgs e)
        => await OpenEventAsync("Veillée Mama Thérèse");

    private async Task OpenEventAsync(string name)
    {
        var evt = EventService.Instance.GetByName(name);
        if (evt is not null)
            EventService.Instance.Current = evt;
        await Navigation.PushAsync(new FicheEvenementPage());
    }

    private async void OnDeadlineTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TachesPage());
    }

    private async void OnNavInvitesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InvitesPage());
    }

    private async void OnNavBudgetTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BudgetPage());
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

using FeteCam.Services;

namespace FeteCam.Views;

public partial class ProfilPage : ContentPage
{
    public ProfilPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var acc = AccountService.Instance;
        NameLabel.Text = acc.Name;
        MetaLabel.Text = string.IsNullOrWhiteSpace(acc.City)
            ? acc.Phone
            : $"{acc.Phone} · {acc.City}";
        InitialsLabel.Text = acc.Initials;

        int count = EventService.Instance.Events.Count;
        EventsCaptionLabel.Text = $"{count} en cours";
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnAccountTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MonComptePage());
    }

    private async void OnMyEventsTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MesEvenementsPage());
    }

    private async void OnNotificationsTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NotificationsPage());
    }

    private async void OnSettingsTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ParametresPage());
    }

    private async void OnHelpTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AidePage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Déconnexion", "Voulez-vous vraiment vous déconnecter de FêteCam ?", "Oui", "Non");
        if (confirm)
        {
            ApiService.Instance.Logout();
            await App.EnterLoginAsync();
        }
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
        // Already on ProfilPage
    }

    private async void OnNavAccueilTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}

namespace FeteCam.Views;

public partial class BilanPage : ContentPage
{
    public BilanPage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Export", "Bilan financier et logistique exporté en PDF avec succès !", "OK");
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Partage", "Lien de partage du rapport copié dans le presse-papier !", "OK");
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

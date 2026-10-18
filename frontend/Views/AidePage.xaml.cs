namespace FeteCam.Views;

public partial class AidePage : ContentPage
{
    public AidePage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnContactSupportClicked(object? sender, EventArgs e)
    {
        await DisplayAlert(
            "Assistance FêteCam",
            "Pour toute question ou assistance personnalisée :\n\nWhatsApp & Téléphone : +237 671 00 00 00\nEmail : support@fetecam.cm\n\nNos équipes sont disponibles 7j/7.",
            "Fermer");
    }
}

namespace FeteCam.Views;

public partial class EnregistrerContributionPage : ContentPage
{
    public EnregistrerContributionPage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Succès", "Contribution enregistrée avec succès !", "OK");
        await Navigation.PopAsync();
    }
}

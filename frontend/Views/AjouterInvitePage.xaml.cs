namespace FeteCam.Views;

public partial class AjouterInvitePage : ContentPage
{
    public AjouterInvitePage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Succès", "Invité enregistré avec succès !", "OK");
        await Navigation.PopAsync();
    }
}

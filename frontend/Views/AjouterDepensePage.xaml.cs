namespace FeteCam.Views;

public partial class AjouterDepensePage : ContentPage
{
    public AjouterDepensePage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Succès", "Dépense enregistrée avec succès !", "OK");
        await Navigation.PopAsync();
    }
}

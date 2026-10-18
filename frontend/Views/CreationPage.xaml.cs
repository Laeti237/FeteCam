namespace FeteCam.Views;

public partial class CreationPage : ContentPage
{
    public CreationPage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Succès", "Événement créé avec succès !", "OK");
        await Navigation.PopAsync();
    }
}

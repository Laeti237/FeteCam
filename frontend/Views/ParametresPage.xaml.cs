namespace FeteCam.Views;

public partial class ParametresPage : ContentPage
{
    public ParametresPage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSavePreferencesClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Paramètres", "Vos préférences ont été enregistrées avec succès.", "OK");
        await Navigation.PopAsync();
    }
}

namespace FeteCam.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnMarkAllReadClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Notifications", "Toutes les notifications ont été marquées comme lues.", "OK");
    }
}

namespace FeteCam.Views;

public partial class DetailInvitePage : ContentPage
{
    private string _guestName;
    private string _guestPhone;
    private string _status;
    private string _places;
    private string _table;

    public DetailInvitePage(string name = "Marie Ngono", string phone = "+237 671 22 33 44", string status = "Confirmé", string places = "2 personnes", string table = "Table d'honneur", string eventName = "Mariage Alain & Rose")
    {
        InitializeComponent();

        _guestName = name;
        _guestPhone = phone;
        _status = status;
        _places = places;
        _table = table;

        if (!string.IsNullOrWhiteSpace(eventName))
            SubtitleLabel.Text = eventName;

        HeaderNameLabel.Text = _guestName;
        GuestNameLabel.Text = _guestName;
        GuestPhoneLabel.Text = _guestPhone;
        StatusLabel.Text = _status;
        PlacesLabel.Text = _places;
        TableLabel.Text = _table;

        InitialsLabel.Text = string.IsNullOrWhiteSpace(_guestName) ? "?" : (_guestName.Length <= 2 ? _guestName.ToUpper() : _guestName.Substring(0, 2).ToUpper());
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnCallClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Appel téléphonique", $"Lancement de l'appel vers {_guestName} ({_guestPhone})...", "OK");
    }

    private async void OnMessageClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Message", $"Ouverture de la messagerie pour {_guestName}...", "OK");
    }

    private async void OnDeleteGuestClicked(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmation", $"Voulez-vous retirer {_guestName} de la liste des invités ?", "Retirer", "Annuler");
        if (confirm)
        {
            await DisplayAlert("Succès", $"{_guestName} a été retiré(e) de la liste.", "OK");
            await Navigation.PopAsync();
        }
    }
}
